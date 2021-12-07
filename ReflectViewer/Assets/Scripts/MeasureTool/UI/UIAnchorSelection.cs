using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Dispatching;
using TMPro;
using Unity.Reflect.Viewer.Core.Actions;
using Unity.Reflect.Viewer.UI;
using Unity.XRTools.Utils;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace UnityEngine.Reflect.MeasureTool
{
    public class UIAnchorSelection: IDisposable
    {
        AnchorSelector AnchorPicker { get; }
        int? m_CurrentSelectedAnchorIndex;
        float m_Offset = 70f;
        List<SelectObjectMeasureToolAction.IAnchor> m_AnchorsList;
        List<Tuple<int, GameObject>> m_AnchorIndexToCursorObject = new List<Tuple<int, GameObject>>();
        Camera m_MainCamera;
        Vector3? m_PreviousPadPosition;
        double m_Tolerance = 0.1f;
        float m_LineWidthMultiplier = 100f;
        float m_MeasureTextWidthMin = 150f;

        TextMeshProUGUI m_MeasureText;
        DraggableButton m_DraggablePad;
        LineRenderer m_Line;
        Func<ToggleMeasureToolAction.MeasureMode> m_MeasureModeGetter;
        Func<bool> m_VREnableGetter;
        Func<Transform> m_VRControllerGetter;

        public UIAnchorSelection(TextMeshProUGUI textMesh, DraggableButton draggablePad, LineRenderer line, Func<ToggleMeasureToolAction.MeasureMode> measureModeGetter,
            Func<bool> vrEnableGetter, Func<Transform> vrControllerGetter)
        {
            AnchorPicker = new AnchorSelector();
            AnchorPicker.CurrentAnchorTypeSelection = ToggleMeasureToolAction.AnchorType.Point;
            m_AnchorsList = new List<SelectObjectMeasureToolAction.IAnchor>();

            m_MeasureText = textMesh;
            m_DraggablePad = draggablePad;
            m_Line = line;

            m_MeasureModeGetter = measureModeGetter;
            m_VREnableGetter = vrEnableGetter;
            m_VRControllerGetter = vrControllerGetter;
        }

        void LabelFOV(Vector3 start, Vector3 end, Transform label)
        {
            if (m_MainCamera != null && GeometryUtils.ClosestTimesOnTwoLines(start, end - start, m_MainCamera.transform.position, m_MainCamera.transform.forward, out var t, out _))
            {
                var targetPositionOnLine = Vector3.Lerp(start, end, Mathf.Clamp(t, 0.1f, 0.9f));
                label.transform.position = targetPositionOnLine; // TODO easing or non-continuous movement towards target position
            }
        }

        public void Update()
        {
            if (m_MainCamera == null || !m_MainCamera.gameObject.activeInHierarchy)
            {
                m_MainCamera = Camera.main;
            }

            if (m_MainCamera != null)
            {
                if (m_AnchorsList.Count >= 2)
                {
                    // Keep measureText screen position between anchors when camera moving
                    var measureText = m_MeasureText.transform.parent.parent;
                    var pos1 = m_MainCamera.WorldToScreenPoint(((PointAnchor)m_AnchorsList[0]).position);
                    var pos2 = m_MainCamera.WorldToScreenPoint(((PointAnchor)m_AnchorsList[1]).position);

                    // Update the line width with the camera distance
                    m_Line.startWidth = Vector3.Distance(((PointAnchor)m_AnchorsList[0]).position, m_MainCamera.transform.position) / m_LineWidthMultiplier;
                    m_Line.endWidth = Vector3.Distance(((PointAnchor)m_AnchorsList[1]).position, m_MainCamera.transform.position) / m_LineWidthMultiplier;

                    if (m_VREnableGetter())
                    {
                        measureText.transform.gameObject.SetActive(true);
                        measureText.localScale = Vector3.one * 2f;

                        pos1 = ((PointAnchor)m_AnchorsList[0]).position;
                        pos2 = ((PointAnchor)m_AnchorsList[1]).position;
                        LabelFOV(pos1, pos2, measureText);
                    }
                    else if (pos1.z >= 0 && pos2.z >= 0)
                    {
                        var position = Vector3.Lerp(pos1, pos2, 0.5f);
                        measureText.localScale = Vector3.one;

                        Vector2 new2DPos = new Vector2(position.x, position.y);
                        Vector2 cur2DPos = new Vector2(measureText.position.x, measureText.position.y);

                        if ((new2DPos - cur2DPos).magnitude >= m_Tolerance)
                        {
                            // disable measureText when distance between points is not wide enough
                            // positions are checked to avoid back camera view
                            if ((pos2 - pos1).magnitude < m_MeasureTextWidthMin)
                            {
                                if (measureText.gameObject.activeSelf)
                                    measureText.gameObject.SetActive(false);
                            }
                            else
                            {
                                if (!measureText.gameObject.activeSelf)
                                    measureText.gameObject.SetActive(true);

                                measureText.transform.position = new Vector3(position.x, position.y, 0f);

                                if (pos2.x - pos1.x >= 0)
                                    measureText.transform.right = new Vector3(pos2.x - pos1.x, pos2.y - pos1.y, 0f);
                                else
                                    measureText.transform.right = new Vector3(pos1.x - pos2.x, pos1.y - pos2.y, 0f);
                            }
                        }
                    }
                }


                // Keep draggablePad screen position when camera moving
                if (m_CurrentSelectedAnchorIndex != null && !m_VREnableGetter())
                {
                    var currentCursor = m_AnchorIndexToCursorObject.Where(r => r.Item1 == m_CurrentSelectedAnchorIndex).Select(r => r.Item2).FirstOrDefault();
                    var position = m_MainCamera.WorldToScreenPoint(currentCursor.transform.position) + new Vector3(0f, -m_Offset, 0f);

                    if ((m_DraggablePad.transform.position - position).magnitude >= m_Tolerance)
                    {
                        // z position is checked to avoid back camera view
                        if (position.z <= 0 && m_DraggablePad.isActiveAndEnabled)
                        {
                            m_DraggablePad.gameObject.SetActive(false);
                        }
                        else if (position.z > 0)
                        {
                            if (!m_DraggablePad.isActiveAndEnabled)
                                m_DraggablePad.gameObject.SetActive(true);
                            m_DraggablePad.transform.position = position;
                        }
                    }
                }
            }
        }

        public void OnStateDataChanged(SelectObjectMeasureToolAction.IAnchor selectedAnchor, GameObject cursor = null)
        {
            if (cursor != null && selectedAnchor != null)
            {
                SetCursorUI(selectedAnchor, cursor);

                if (!cursor.activeSelf)
                {
                    cursor.gameObject.SetActive(true);
                    m_AnchorsList.Add(selectedAnchor);
                }

                m_AnchorIndexToCursorObject.Add(new Tuple<int, GameObject>(m_AnchorsList.Count - 1, cursor));
                m_CurrentSelectedAnchorIndex = m_AnchorIndexToCursorObject[m_AnchorIndexToCursorObject.Count - 1].Item1;

                if (!m_VREnableGetter())
                    m_DraggablePad.gameObject.SetActive(true);
            }
            else if ((m_VREnableGetter() || m_DraggablePad.button.IsActive()) && m_CurrentSelectedAnchorIndex != null)
            {
                var currentCursor = m_AnchorIndexToCursorObject.Where(r => r.Item1 == m_CurrentSelectedAnchorIndex).Select(r => r.Item2).FirstOrDefault();

                SetCursorUI(selectedAnchor, currentCursor);

                m_AnchorsList[m_CurrentSelectedAnchorIndex.Value] = selectedAnchor;
            }
        }

        void SetCursorUI(SelectObjectMeasureToolAction.IAnchor selectedAnchor, GameObject cursor)
        {
            switch (selectedAnchor.type)
            {
                case ToggleMeasureToolAction.AnchorType.Point:
                default:
                    cursor.transform.position = ((PointAnchor)selectedAnchor).position;
                    cursor.transform.forward = ((PointAnchor)selectedAnchor).normal;
                    break;
            }
        }

        public void SetLineUI()
        {
            if (m_AnchorsList.Count < 2)
                return;

            if (m_Line != null)
            {
                if (!m_Line.gameObject.activeSelf)
                    m_Line.gameObject.SetActive(true);

                m_Line.SetPosition(0, ((PointAnchor)m_AnchorsList[0]).position);
                m_Line.SetPosition(1, ((PointAnchor)m_AnchorsList[1]).position);
            }

            if (m_MeasureText != null)
            {
                var distance = RawMeasure.GetDistanceBetweenAnchors(m_AnchorsList[0], m_AnchorsList[1]);
                m_MeasureText.text = $"{distance.ToString("0.00")} m";
            }
        }

        public void OnBeginDragPad()
        {
            Dispatcher.Dispatch(SetSpatialSelectorAction.From(AnchorPicker));
        }

        public void OnDragPad(DragStateData dragState)
        {
            if (m_PreviousPadPosition.HasValue && (dragState.position - m_PreviousPadPosition.Value).magnitude < m_Tolerance && !m_VREnableGetter())
                return;

            m_PreviousPadPosition = dragState.position;
            var position =  dragState.position;
            position.y += m_Offset;

            OnPick(position, OnPickDragAsyncCallback);
        }

        public void OnEndDragPad()
        {
            m_PreviousPadPosition = null;
        }

        public void OnPointerUp(Vector3 position)
        {
            Dispatcher.Dispatch(SetSpatialSelectorAction.From(AnchorPicker));

            OnPick(position, OnPickPointerAsyncCallback);
        }

        void OnPick(Vector3 position, Action<List<Tuple<GameObject, RaycastHit>>> onPickCallBack)
        {
            if (m_MainCamera == null || !m_MainCamera.gameObject.activeInHierarchy)
            {
                m_MainCamera = Camera.main;
                if (m_MainCamera == null)
                {
                    Debug.LogError($"[{nameof(UISelectionController)}] active main camera not found!");
                    return;
                }
            }

            Ray ray = m_MainCamera.ScreenPointToRay(position);

            if (m_VREnableGetter())
            {
                ray.origin = m_VRControllerGetter().position;
                ray.direction = m_VRControllerGetter().forward;
            }

            AnchorPicker.Pick(ray, onPickCallBack);
        }

        void OnPickDragAsyncCallback(List<Tuple<GameObject, RaycastHit>> results)
        {
            if (results == null)
                return;

            var selectedObjects = results.Select(x => x.Item1).Where(x => x.layer != MetadataFilter.k_OtherLayer);
            var selectedAnchorsContext = selectedObjects.Select(r => r.GetComponent<AnchorSelectionContext>()).Where(g => g != null).ToList();

            SelectObjectMeasureToolAction.IAnchor selectedAnchor = null;
            if (selectedAnchorsContext.Count > 0)
                selectedAnchor = selectedAnchorsContext[0].LastContext.selectedAnchor;

            Dispatcher.Dispatch(SelectObjectMeasureToolAction.From(selectedAnchor));
        }

        void OnPickPointerAsyncCallback(List<Tuple<GameObject, RaycastHit>> results)
        {
            if (results == null || results.Count == 0)
            {
                Dispatcher.Dispatch(SetStatusMessageWithType.From(
                    new StatusMessageData() { text = MeasureToolUIController.instructionTapOnSurface, type = StatusMessageType.Instruction }));
                return;
            }

            SelectObjectMeasureToolAction.IAnchor selectedAnchor = null;
            var selectedObjects = results.Select(x => x.Item1).Where(x => x.layer != MetadataFilter.k_OtherLayer);
            var selectedAnchorsContext = selectedObjects.Select(r => r.GetComponent<AnchorSelectionContext>()).Where(g => g != null).ToList();
            if (selectedAnchorsContext.Count > 0)
                selectedAnchor = selectedAnchorsContext[0].LastContext.selectedAnchor;

            Dispatcher.Dispatch(SelectObjectMeasureToolAction.From(selectedAnchor));
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));

            if (m_MeasureModeGetter() == ToggleMeasureToolAction.MeasureMode.PerpendicularDistance && m_AnchorsList.Count == 1)
            {
                Ray ray = new Ray();
                var firstAnchor = selectedAnchorsContext[0].LastContext.selectedAnchor;
                if (firstAnchor.type == ToggleMeasureToolAction.AnchorType.Point)
                {
                    var anc = ((PointAnchor) firstAnchor);
                    ray.origin = anc.position;
                    ray.direction = anc.normal;
                }

                AnchorPicker.Pick(ray, OnPickPerpendicularAsyncCallback);
            }
        }

        void OnPickPerpendicularAsyncCallback(List<Tuple<GameObject, RaycastHit>> results)
        {
            if (results != null && results.Count > 0)
            {
                SelectObjectMeasureToolAction.IAnchor selectedAnchor = null;
                var selectedObjects = results.Select(x => x.Item1).Where(x => x.layer != MetadataFilter.k_OtherLayer);
                var selectedAnchorsContext = selectedObjects.Select(r => r.GetComponent<AnchorSelectionContext>()).Where(g => g != null).ToList();
                if (selectedAnchorsContext.Count > 0)
                    selectedAnchor = selectedAnchorsContext[0].LastContext.selectedAnchor;

                Dispatcher.Dispatch(SelectObjectMeasureToolAction.From(selectedAnchor));
            }
        }

        public void SelectCursor(GameObject cursor, Material selectedMaterial, List<Tuple<int, MeshRenderer>> cachedRenderers)
        {
            if (m_CurrentSelectedAnchorIndex != null)
            {
                var currentCursor = m_AnchorIndexToCursorObject.Where(r => r.Item1 == m_CurrentSelectedAnchorIndex).Select(r => r.Item2).FirstOrDefault();
                if (currentCursor != null && currentCursor.Equals(cursor))
                    return;
            }

            m_CurrentSelectedAnchorIndex = m_AnchorIndexToCursorObject.Where(r => r.Item2 == cursor).Select(r => r.Item1).FirstOrDefault();

            var currRenderers = cachedRenderers.Where(r => r.Item1 == m_CurrentSelectedAnchorIndex).Select(r => r.Item2).ToList();

            foreach (var mesh in currRenderers)
            {
                mesh.material = selectedMaterial;
            }

            if (m_VREnableGetter())
                return;

            if (!m_DraggablePad.isActiveAndEnabled)
            {
                m_DraggablePad.gameObject.SetActive(true);
            }
        }

        public void UnselectCursor(Material unselectedMaterial, List<Tuple<int, MeshRenderer>> cachedRenderers)
        {
            m_DraggablePad.gameObject.SetActive(false);
            if (m_CurrentSelectedAnchorIndex != null)
            {
                var currRenderers = cachedRenderers.Where(r => r.Item1 == m_CurrentSelectedAnchorIndex).Select(r => r.Item2).ToList();

                foreach (var mesh in currRenderers)
                {
                    mesh.material = unselectedMaterial;
                }
            }

            m_CurrentSelectedAnchorIndex = null;
        }

        public void ResetSelector()
        {
            m_CurrentSelectedAnchorIndex = null;
            m_AnchorIndexToCursorObject.Clear();
            m_AnchorsList.Clear();
        }

        public void SetAnchorPickerSelectionType(ToggleMeasureToolAction.AnchorType anchorType)
        {
            AnchorPicker.CurrentAnchorTypeSelection = anchorType;
        }

        public void Dispose()
        {
            AnchorPicker?.Dispose();
        }
    }
}
