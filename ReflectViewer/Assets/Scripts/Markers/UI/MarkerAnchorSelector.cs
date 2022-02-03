using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Dispatching;
using Unity.Reflect.Markers.Placement;
using Unity.Reflect.Markers.UI;
using Unity.Reflect.Viewer.Core.Actions;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Logic for selecting a single anchor point in 3D space from a 2D screen
    /// </summary>
    public class MarkerAnchorSelector: IDisposable
    {
        MarkerSpatialSelector AnchorPicker { get; }
        float m_Offset = 70f;
        SelectObjectDragToolAction.IAnchor m_Anchor;
        GameObject m_AnchorCursor;
        UnityEngine.Camera m_MainCamera;
        Vector3? m_PreviousPadPosition;
        double m_Tolerance = 0.1f;
        Transform m_RightController;

        DraggableButton m_DraggablePad;
        Func<bool> m_VREnableGetter;

        public Action<SelectObjectDragToolAction.IAnchor> OnAnchorDataChanged = null;

        public MarkerAnchorSelector(DraggableButton draggablePad, Func<bool> VREnableGetter)
        {
            AnchorPicker = new MarkerSpatialSelector();

            m_DraggablePad = draggablePad;
            m_VREnableGetter = VREnableGetter;
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
                m_MainCamera = UnityEngine.Camera.main;
            }

            if (m_MainCamera != null)
            {
                // Keep draggablePad screen position when camera moving
                if (m_AnchorCursor && !m_VREnableGetter())
                {
                    var position = m_MainCamera.WorldToScreenPoint(m_AnchorCursor.transform.position) + new Vector3(0f, -m_Offset, 0f);

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

        public void Dispose()
        {
            AnchorPicker.Dispose();
        }

        public void OnStateDataChanged(SelectObjectDragToolAction.IAnchor selectedAnchor, GameObject cursor = null)
        {
            if (cursor != null && selectedAnchor != null)
            {
                SetCursorUI(selectedAnchor, cursor);
                m_Anchor = selectedAnchor;
                m_AnchorCursor = cursor;
                if (!cursor.activeSelf)
                    cursor.SetActive(true);
                if (!m_VREnableGetter())
                    m_DraggablePad.gameObject.SetActive(true);
            }
            else if ((m_VREnableGetter() || m_DraggablePad.button.IsActive()) && m_AnchorCursor)
            {
                SetCursorUI(selectedAnchor, m_AnchorCursor);
            }
        }

        void SetCursorUI(SelectObjectDragToolAction.IAnchor selectedAnchor, GameObject cursor)
        {
            cursor.transform.position = selectedAnchor.position;
            cursor.transform.forward = selectedAnchor.normal;
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

        public void OnPosePick(Pose pose)
        {
            Dispatcher.Dispatch(SetSpatialSelectorAction.From(AnchorPicker));
            var start = pose.position + (pose.forward * 0.1f);
            var direction = pose.forward * -1;
            Ray ray = new Ray(start, direction);
            AnchorPicker.Pick(ray, OnPickPointerAsyncCallback);
        }

        void OnPick(Vector3 position, Action<List<Tuple<GameObject, RaycastHit>>> onPickCallBack)
        {
            if (m_MainCamera == null || !m_MainCamera.gameObject.activeInHierarchy)
            {
                m_MainCamera = UnityEngine.Camera.main;
                if (m_MainCamera == null)
                {
                    Debug.LogError($"[{nameof(MarkerAnchorSelector)}] active main camera not found!");
                    return;
                }
            }

            Ray ray = m_MainCamera.ScreenPointToRay(position);

            if (m_VREnableGetter())
            {
                ray.origin = m_RightController.position;
                ray.direction = m_RightController.forward;
            }
            AnchorPicker.Pick(ray, onPickCallBack);
        }

        void OnPickDragAsyncCallback(List<Tuple<GameObject, RaycastHit>> results)
        {
            if (results == null)
                return;

            var selectedObjects = results.Select(x => x.Item1).Where(x => x.layer != MetadataFilter.k_OtherLayer);
            var selectedAnchorsContext = selectedObjects.Select(r => r.GetComponent<MarkerAnchorSelectionContext>()).Where(g => g != null).ToList();

            SelectObjectDragToolAction.IAnchor selectedAnchor = null;
            if (selectedAnchorsContext.Count > 0)
                selectedAnchor = selectedAnchorsContext[0].LastContext.selectedAnchor;

            Dispatcher.Dispatch(SelectObjectDragToolAction.From(selectedAnchor));
            OnAnchorDataChanged?.Invoke(selectedAnchor);
        }

        void OnPickPointerAsyncCallback(List<Tuple<GameObject, RaycastHit>> results)
        {
            if (results == null || results.Count == 0)
            {
                OnAnchorDataChanged?.Invoke(null);
                return;
            }

            SelectObjectDragToolAction.IAnchor selectedAnchor = null;
            var selectedObjects = results.Select(x => x.Item1).Where(x => x.layer != MetadataFilter.k_OtherLayer);
            var selectedAnchorsContext = selectedObjects.Select(r => r.GetComponent<MarkerAnchorSelectionContext>()).Where(g => g != null).ToList();
            if (selectedAnchorsContext.Count > 0)
                selectedAnchor = selectedAnchorsContext[0].LastContext.selectedAnchor;

            Dispatcher.Dispatch(SelectObjectDragToolAction.From(selectedAnchor));
            OnAnchorDataChanged?.Invoke(selectedAnchor);
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
        }

        public void SelectCursor(GameObject cursor, Material selectedMaterial, List<MeshRenderer> cachedRenderers)
        {
            if (m_AnchorCursor == cursor)
            {
                return;
            }

            foreach (var mesh in cachedRenderers)
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

        public void UnselectCursor(Material unselectedMaterial, List<MeshRenderer> cachedRenderers)
        {
            m_DraggablePad.gameObject.SetActive(false);
            foreach (var mesh in cachedRenderers)
            {
                mesh.material = unselectedMaterial;
            }
            m_AnchorCursor = null;
        }

        public void ResetSelector()
        {
            m_AnchorCursor = null;
            m_Anchor = null;
        }
    }
}
