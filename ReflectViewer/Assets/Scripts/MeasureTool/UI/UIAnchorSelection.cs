using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using Unity.XRTools.Utils;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace UnityEngine.Reflect.MeasureTool
{
    public class UIAnchorSelection
    {
        AnchorSelector AnchorPicker { get; }
        int? m_CurrentSelectedAnchorIndex;
        MeasureToolStateData? m_CachedMeasureToolStateData;
        float m_Offset = 70f;
        List<IAnchor> m_AnchorsList;
        readonly List<Tuple<GameObject, RaycastHit>> m_Results = new List<Tuple<GameObject, RaycastHit>>();
        List<Tuple<int, GameObject>> m_AnchorIndexToCursorObject = new List<Tuple<int, GameObject>>();
        Camera m_MainCamera;
        Vector3? m_PreviousPadPosition;
        double m_Tolerance = 0.1f;
        Transform m_RightController;
        float m_LineWidthMultiplier = 100f;
        float m_MeasureTextWidthMin = 150f;

        TextMeshProUGUI m_MeasureText;
        DraggableButton m_DraggablePad;
        LineRenderer m_Line;

        public UIAnchorSelection(ref TextMeshProUGUI textMesh, ref DraggableButton draggablePad, ref LineRenderer line)
        {
            AnchorPicker = new AnchorSelector();
            AnchorPicker.CurrentAnchorTypeSelection = AnchorType.Point;
            m_AnchorsList = new List<IAnchor>();

            m_MeasureText = textMesh;
            m_DraggablePad = draggablePad;
            m_Line = line;
        }

        public void SetControllerTransform(TransformVariable rightController)
        {
            m_RightController = rightController.Value;
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

                    if (UIStateManager.current.stateData.VREnable)
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
                if (m_CurrentSelectedAnchorIndex != null && !UIStateManager.current.stateData.VREnable)
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

        public void OnStateDataChanged(MeasureToolStateData data, GameObject cursor = null)
        {
            if (m_CachedMeasureToolStateData != data)
            {
                if (m_CachedMeasureToolStateData == null || m_CachedMeasureToolStateData.Value.selectedAnchorsContext != data.selectedAnchorsContext)
                {
                    if (data.selectedAnchorsContext != null && data.selectedAnchorsContext.Count > 0)
                    {
                        var anchor = data.selectedAnchorsContext[0].LastContext.selectedAnchor;

                        if (cursor != null)
                        {
                            SetCursorUI(anchor, ref cursor);

                            if (!cursor.activeSelf)
                            {
                                cursor.gameObject.SetActive(true);
                                m_AnchorsList.Add(anchor);
                            }

                            m_AnchorIndexToCursorObject.Add(new Tuple<int, GameObject>(m_AnchorsList.Count - 1, cursor));
                            m_CurrentSelectedAnchorIndex = m_AnchorIndexToCursorObject[m_AnchorIndexToCursorObject.Count - 1].Item1;

                            if (!UIStateManager.current.stateData.VREnable)
                                m_DraggablePad.gameObject.SetActive(true);
                        }
                        else if ((UIStateManager.current.stateData.VREnable || m_DraggablePad.button.IsActive()) && m_CurrentSelectedAnchorIndex != null)
                        {
                            var currentCursor = m_AnchorIndexToCursorObject.Where(r => r.Item1 == m_CurrentSelectedAnchorIndex).Select(r => r.Item2).FirstOrDefault();

                            SetCursorUI(anchor, ref currentCursor);

                            m_AnchorsList[m_CurrentSelectedAnchorIndex.Value] = anchor;
                        }

                        data.selectedAnchorsContext[0].SelectionContextList.Clear();
                    }
                }

                m_CachedMeasureToolStateData = data;
            }
        }

        void SetCursorUI(IAnchor selectedAnchor, ref GameObject cursor)
        {
            switch (selectedAnchor.type)
            {
                case AnchorType.Point:
                default:
                    cursor.transform.position = ((PointAnchor)selectedAnchor).position;
                    cursor.transform.forward =  UIStateManager.current.m_RootNode.transform.localRotation * ((PointAnchor)selectedAnchor).normal;

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
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetObjectPicker, AnchorPicker));
        }

        public void OnDragPad(DragStateData dragState)
        {
            if (m_PreviousPadPosition.HasValue && (dragState.position - m_PreviousPadPosition.Value).magnitude < m_Tolerance)
                return;

            dragState.position.y += m_Offset;
            m_PreviousPadPosition = dragState.position;

            OnPick(dragState.position);

            if (m_Results == null)
                return;

            var stateData = m_CachedMeasureToolStateData == null ? MeasureToolStateData.defaultData : m_CachedMeasureToolStateData.Value;
            var selectedObjects = m_Results.Select(x => x.Item1).Where(x => x.layer != MetadataFilter.k_OtherLayer);
            stateData.selectedAnchorsContext = selectedObjects.Select(r => r.GetComponent<AnchorSelectionContext>()).Where(g => g != null).ToList();
            stateData.toolState = true;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetMeasureToolOptions, stateData));
        }

        public void OnEndDragPad()
        {
            m_PreviousPadPosition = null;
        }

        public bool OnPointerUp(Vector3 position, MeasureToolStateData data)
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetObjectPicker, AnchorPicker));

            OnPick(position);

            if (m_Results == null || m_Results.Count == 0)
                return false;

            var selectedObjects = m_Results.Select(x => x.Item1).Where(x => x.layer != MetadataFilter.k_OtherLayer);
            data.selectedAnchorsContext = selectedObjects.Select(r => r.GetComponent<AnchorSelectionContext>()).Where(g => g != null).ToList();
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetMeasureToolOptions, data));
            return true;
        }

        void OnPick(Vector3 position)
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

            if (UIStateManager.current.stateData.VREnable)
            {
                ray.origin = m_RightController.position;
                ray.direction = m_RightController.forward;
            }

            AnchorPicker.Pick(ray, m_Results);
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

            if (UIStateManager.current.stateData.VREnable)
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

        public void SetAnchorPickerSelectionType(AnchorType anchorType)
        {
            AnchorPicker.CurrentAnchorTypeSelection = anchorType;
        }
    }
}
