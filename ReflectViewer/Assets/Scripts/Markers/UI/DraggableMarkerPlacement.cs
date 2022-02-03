using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.Reflect.Markers.Placement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// UI Tool for placing and moving Marker points using raycasts and a draggable knob to select surface points.
    /// </summary>
    public class DraggableMarkerPlacement : MonoBehaviour
    {
        [SerializeField]
        GameObject m_Cursor;
        [SerializeField]
        DraggableButton m_DraggablePad;
        [SerializeField]
        Material m_UnselectedCursorMaterial;
        [SerializeField]
        InputActionAsset m_InputActionAsset;
        [SerializeField]
        TransformVariable m_RightController;
        MarkerAnchorSelector m_AnchorSelection;

        IUISelector<int> m_HashObjectDraggedSelector;
        IUISelector<bool> m_VREnableSelector;
        IUISelector<Vector3> m_DragStatePositionSelector;
        GameObject m_CurrentCursor;
        List<MeshRenderer> m_CachedCursorsMeshRenderer = new List<MeshRenderer>();

        Material m_SelectedCursorMaterial;
        Material m_PlainLineMaterial;

        bool m_OnDrag = false;

        const string k_InstructionStart = "Tap anywhere on the model to place your marker.";
        const string k_InstructionTapOnSurface = "The point did not register to a surface. Tap on model to place marker.";
        const string k_InstructionDragPoint = "Drag handle to place marker.";


        public Pose Value => m_Value;
        Pose m_Value = Pose.identity;
        public event Action<Pose> OnValueUpdate;
        public bool Active => m_Active;
        bool m_Active = false;
        bool m_Dragging = false;

        void Awake()
        {
            m_HashObjectDraggedSelector = UISelectorFactory.createSelector<int>(DragStateContext.current, nameof(IDragStateData.hashObjectDragged));

            m_VREnableSelector = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable));
            m_DragStatePositionSelector = UISelectorFactory.createSelector<Vector3>(DragStateContext.current, nameof(IDragStateData.position), OnDragPositionDataChanged);

            m_AnchorSelection = new MarkerAnchorSelector(m_DraggablePad, m_VREnableSelector.GetValue);
            m_AnchorSelection.OnAnchorDataChanged += OnSelectedAnchorsDataChanged;

            m_DraggablePad.onBeginDrag.AddListener(OnBeginDrag);
            m_DraggablePad.onDrag.AddListener(OnDrag);
            m_DraggablePad.onEndDrag.AddListener(OnEndDrag);
        }

        void OnDestroy()
        {
            m_HashObjectDraggedSelector?.Dispose();
            m_VREnableSelector?.Dispose();
            m_DragStatePositionSelector?.Dispose();
            m_AnchorSelection?.Dispose();
        }

        public void Open()
        {
            if (m_Active)
                Close();
            SetCurrentCursor(m_Cursor);
            m_InputActionAsset["MeasureTool/Select"].performed += OnPointerUp;
            m_InputActionAsset["VR/Select"].performed += OnPointerUp;
            m_InputActionAsset["MeasureTool/Select"].Enable();
            InitCursor();
            m_Active = true;
        }

        public void Open(Pose initPose)
        {
            Open();
            m_AnchorSelection.OnPosePick(initPose);
        }

        public void UpdatePose(Pose updatedPose)
        {
            if (!m_Active)
            {
                Open(updatedPose);
                return;
            }
            m_AnchorSelection.OnPosePick(updatedPose);
        }

        public void Close()
        {
            m_InputActionAsset["MeasureTool/Select"].performed -= OnPointerUp;
            m_InputActionAsset["VR/Select"].performed -= OnPointerUp;
            m_InputActionAsset["MeasureTool/Reset"].performed -= OnVRReset;

            Reset();
            m_Active = false;
        }

        void InitCursor()
        {
            if (m_VREnableSelector.GetValue())
            {
                m_AnchorSelection.SetControllerTransform(m_RightController);
                m_InputActionAsset["MeasureTool/Reset"].performed += OnVRReset;
            }
        }

        void OnVRReset(InputAction.CallbackContext obj)
        {
            Reset();
        }


        public void Reset()
        {
            m_DraggablePad.gameObject.SetActive(false);
            if (m_CurrentCursor)
                m_CurrentCursor.gameObject.SetActive(false);
            m_AnchorSelection.ResetSelector();

            foreach (var item in m_CachedCursorsMeshRenderer)
            {
                item.material = m_SelectedCursorMaterial;
            }
        }

        void OnPointerUp(InputAction.CallbackContext input)
        {
            if (OrphanUIController.isTouchBlockedByUI && !m_VREnableSelector.GetValue())
                return;

            OnManageCursor(Pointer.current.position.ReadValue());
        }

        void OnManageCursor(Vector3 position)
        {
            // Create Cursor
            if (m_CurrentCursor.gameObject.activeSelf == false)
            {
                m_Dragging = true;
                m_AnchorSelection.UnselectCursor(m_UnselectedCursorMaterial, m_CachedCursorsMeshRenderer);
                m_AnchorSelection.OnPointerUp(position);
            }
        }

        void Update()
        {
            m_AnchorSelection.Update();
            if (m_CurrentCursor)
            {
                // Update exposed value
                m_Value.position = m_CurrentCursor.transform.position;
                m_Value.rotation = m_CurrentCursor.transform.rotation;
                if (m_Dragging && !PivotTransform.Similar(Vector3.zero, m_Value.position))
                {
                    OnValueUpdate?.Invoke(m_Value);
                    m_Dragging = false;
                }
            }
        }

        void OnDragPositionDataChanged(Vector3 newData)
        {
            if (m_HashObjectDraggedSelector.GetValue() != m_DraggablePad.gameObject.GetHashCode())
                return;

            m_DraggablePad.gameObject.transform.position = newData;
            m_Dragging = true;
        }

        void OnSelectedAnchorsDataChanged(SelectObjectDragToolAction.IAnchor newData)
        {
            if (newData == null)
            {
                if (m_DraggablePad.isActiveAndEnabled)
                {
                    Dispatcher.Dispatch(SetStatusMessageWithType.From(new StatusMessageData { text = k_InstructionDragPoint, type = StatusMessageType.Instruction }));
                }
                else
                {
                    Dispatcher.Dispatch(SetStatusMessageWithType.From(new StatusMessageData { text = k_InstructionStart, type = StatusMessageType.Instruction }));
                }

                return;
            }
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));

            // Create Point
            if (!m_OnDrag)
            {
                if (!m_CurrentCursor.activeSelf)
                {
                    m_AnchorSelection.OnStateDataChanged(newData, m_CurrentCursor);
                    return;
                }
            }

            // Drag Existing Point
            m_AnchorSelection.OnStateDataChanged(newData);
        }

        void OnBeginDrag(Vector3 position)
        {
            m_OnDrag = true;

            DragStateData buttonStateData = new DragStateData();
            buttonStateData.position = position;
            buttonStateData.dragState = DragState.OnStart;
            buttonStateData.hashObjectDragged = m_DraggablePad.gameObject.GetHashCode();

            Dispatcher.Dispatch(SetDragStateAction.From(buttonStateData));

            m_AnchorSelection.OnBeginDragPad();
        }

        public void OnDrag(Vector3 position)
        {
            DragStateData buttonStateData = new DragStateData();
            buttonStateData.position = position;
            buttonStateData.dragState = DragState.OnUpdate;
            buttonStateData.hashObjectDragged = m_DraggablePad.gameObject.GetHashCode();

            Dispatcher.Dispatch(SetDragStateAction.From(buttonStateData));

            m_AnchorSelection.OnDragPad(buttonStateData);
        }

        void OnEndDrag(Vector3 position)
        {
            m_OnDrag = false;

            DragStateData buttonStateData = new DragStateData();
            buttonStateData.position = position;
            buttonStateData.dragState = DragState.OnEnd;
            buttonStateData.hashObjectDragged = m_DraggablePad.gameObject.GetHashCode();

            Dispatcher.Dispatch(SetDragStateAction.From(buttonStateData));

            m_AnchorSelection.OnEndDragPad();
        }

        public void SetCurrentCursor(GameObject cursor)
        {
            if (cursor == m_CurrentCursor)
            {
                m_CurrentCursor.transform.position = Vector3.zero;
                m_CurrentCursor.transform.rotation = Quaternion.identity;
                return;
            }

            m_CurrentCursor = cursor;
            m_CurrentCursor.transform.position = Vector3.zero;
            m_CurrentCursor.transform.rotation = Quaternion.identity;

            // Cache MeshRenderers
            m_CachedCursorsMeshRenderer.Clear();
            foreach (var mesh in m_CurrentCursor.GetComponentsInChildren<MeshRenderer>())
            {
                m_CachedCursorsMeshRenderer.Add(mesh);
            }

            if (m_CachedCursorsMeshRenderer.Count > 0)
                m_SelectedCursorMaterial = m_CachedCursorsMeshRenderer[0].material;
        }
    }
}

