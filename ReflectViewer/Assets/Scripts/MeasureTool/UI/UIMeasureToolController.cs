using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using Unity.TouchFramework;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UnityEngine.Reflect.MeasureTool
{
    public class UIMeasureToolController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_ResetButton;

        // Anchor Selection
        [SerializeField]
        LineRenderer m_Line;
        [SerializeField]
        GameObject m_CursorA;
        [SerializeField]
        GameObject m_CursorB;
        [SerializeField]
        DraggableButton m_DraggablePad;
        [SerializeField]
        TextMeshProUGUI m_MeasureText;
        [SerializeField]
        Material m_UnselectedCursorMaterial;
        [SerializeField]
        Material m_DotedLineMaterial;
        [SerializeField]
        InputActionAsset m_InputActionAsset;
        [SerializeField]
        TransformVariable m_RightController;
#pragma warning restore CS0649

        MeasureToolStateData? m_CachedMeasureToolStateData;
        DragStateData? m_CachedDragStateData;
        UIAnchorSelection m_AnchorSelection;
        bool m_CreatePointOnScreen;
        Camera m_MainCamera;
        Material m_SelectedCursorMaterial;
        Material m_PlainLineMaterial;
        List<Tuple<int, MeshRenderer>> m_CachedCursorsMeshRenderer = new List<Tuple<int, MeshRenderer>>();
        VRMeasureToolController m_VRMeasureTool;

        GameObject m_CurrentCursorA;
        GameObject m_CurrentCursorB;

        public const string instructionStart = "Tap anywhere on the model to place a measurement point.";
        const string m_InstructionTapOnSurface = "/!\\ The point did not register. Tap on a surface to add a measurement point.";

        void Awake()
        {
            SetCurrentCursor(ref m_CursorA, ref m_CursorB);
            m_VRMeasureTool = GetComponent<VRMeasureToolController>();

            m_PlainLineMaterial = m_Line.material;

            m_AnchorSelection = new UIAnchorSelection(ref m_MeasureText, ref m_DraggablePad, ref m_Line);

            UIStateManager.externalToolChanged += OnStateDataChanged;
            UIStateManager.dragStateChanged += OnDragStateChanged;

            m_DraggablePad.onBeginDrag.AddListener(OnBeginDrag);
            m_DraggablePad.onDrag.AddListener(OnDrag);
            m_DraggablePad.onEndDrag.AddListener(OnEndDrag);
            m_ResetButton.onClick.AddListener(OnReset);

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetMeasureToolOptions, MeasureToolStateData.defaultData));
        }

        void Update()
        {
            // Anchor Selection
            m_AnchorSelection.Update();
        }

        void OnDragStateChanged(DragStateData dragStateData)
        {
            if (m_CachedDragStateData != dragStateData && dragStateData.hashObjectDragged == m_DraggablePad.gameObject.GetHashCode())
            {
                if (m_CachedDragStateData == null || m_CachedDragStateData.Value.position != dragStateData.position)
                {
                    m_DraggablePad.gameObject.transform.position = dragStateData.position;
                }

                if (m_CachedDragStateData == null || m_CachedDragStateData.Value.dragState != dragStateData.dragState)
                {
                    switch (dragStateData.dragState)
                    {
                        case DragState.OnStart:
                            m_Line.material = m_DotedLineMaterial;
                            break;
                        case DragState.OnEnd:
                            m_Line.material = m_PlainLineMaterial;
                            break;
                    }
                }

                m_CachedDragStateData = dragStateData;
            }
        }

        void OnStateDataChanged(ExternalToolStateData externalToolData)
        {
            var data = externalToolData.measureToolStateData;

            if (m_CachedMeasureToolStateData != data)
            {
                if (m_CachedMeasureToolStateData == null || m_CachedMeasureToolStateData.Value.toolState != data.toolState)
                {
                    if (data.toolState)
                    {
                        InitCursor();
                        m_ResetButton.gameObject.SetActive(true);
                        m_InputActionAsset["MeasureTool/Select"].performed += OnPointerUp;
                        m_InputActionAsset["VR/Select"].performed += OnPointerUp;
                        m_InputActionAsset["MeasureTool/Select"].Enable();
                    }
                    else
                    {
                        m_ResetButton.gameObject.SetActive(false);
                        m_InputActionAsset["MeasureTool/Select"].performed -= OnPointerUp;
                        m_InputActionAsset["VR/Select"].performed -= OnPointerUp;
                        OnReset();
                        m_VRMeasureTool.OnReset();
                    }
                }

                if (m_CachedMeasureToolStateData == null || m_CachedMeasureToolStateData.Value.measureFormat != data.measureFormat)
                {
                    //TODO: reset the current selection
                }

                if (m_CachedMeasureToolStateData == null || m_CachedMeasureToolStateData.Value.measureMode != data.measureMode)
                {
                    //TODO: update the current value of the measure
                }

                if (m_CachedMeasureToolStateData == null || m_CachedMeasureToolStateData.Value.selectionType != data.selectionType)
                {
                    m_AnchorSelection.SetAnchorPickerSelectionType(data.selectionType);

                    if (data.toolState)
                        OnReset();

                    //TODO: update the Cursors style with the current selectionType
                }

                if (m_CachedMeasureToolStateData == null || m_CachedMeasureToolStateData.Value.selectedAnchorsContext != data.selectedAnchorsContext)
                {
                    // Create Point
                    if (m_CreatePointOnScreen)
                    {
                        if (!m_CurrentCursorA.activeSelf)
                        {
                            m_AnchorSelection.OnStateDataChanged(data, m_CurrentCursorA);
                        }
                        else
                        {
                            m_AnchorSelection.OnStateDataChanged(data, m_CurrentCursorB);
                            m_AnchorSelection.SetLineUI();
                        }

                        m_CreatePointOnScreen = false;
                        return;
                    }

                    // Drag Existing Point
                    m_AnchorSelection.OnStateDataChanged(data);
                    if (m_CurrentCursorB.activeSelf)
                    {
                        m_AnchorSelection.SetLineUI();
                    }
                }

                m_CachedMeasureToolStateData = data;
            }
        }

        void InitCursor()
        {
            if (UIStateManager.current.stateData.VREnable)
            {
                m_VRMeasureTool.InitVR();
                m_AnchorSelection.SetControllerTransform(m_RightController);
                m_InputActionAsset["MeasureTool/Reset"].performed += OnVRReset;
            }
            else
            {
                m_InputActionAsset["MeasureTool/Reset"].performed -= OnVRReset;
                SetCurrentCursor(ref m_CursorA, ref m_CursorB);
            }
        }

        void OnVRReset(InputAction.CallbackContext obj)
        {
            OnReset();
        }

        void OnPointerUp(InputAction.CallbackContext input)
        {
            if (m_CachedMeasureToolStateData == null || m_CachedMeasureToolStateData.Value.toolState == false ||
                (OrphanUIController.isTouchBlockedByUI && !UIStateManager.current.stateData.VREnable))
                return;

            OnManageCursor(Pointer.current.position.ReadValue());
        }

        void OnReset()
        {
            m_DraggablePad.gameObject.SetActive(false);
            m_CurrentCursorA.gameObject.SetActive(false);
            m_CurrentCursorB.gameObject.SetActive(false);
            m_MeasureText.transform.parent.parent.gameObject.SetActive(false);
            m_Line.gameObject.SetActive(false);
            m_AnchorSelection.ResetSelector();

            foreach (var tuple in m_CachedCursorsMeshRenderer)
            {
                tuple.Item2.material = m_SelectedCursorMaterial;
            }
        }

        void OnBeginDrag(Vector3 position)
        {
            DragStateData stateData = new DragStateData();
            stateData.position = position;
            stateData.dragState = DragState.OnStart;
            stateData.hashObjectDragged = m_DraggablePad.gameObject.GetHashCode();

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.BeginDrag, stateData));

            m_AnchorSelection.OnBeginDragPad();
        }

        public void OnDrag(Vector3 position)
        {
            DragStateData stateData = new DragStateData();
            stateData.position = position;
            stateData.dragState = DragState.OnUpdate;
            stateData.hashObjectDragged = m_DraggablePad.gameObject.GetHashCode();

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OnDrag, stateData));

            m_AnchorSelection.OnDragPad(stateData);
        }

        void OnEndDrag(Vector3 position)
        {
            DragStateData stateData = new DragStateData();
            stateData.position = position;
            stateData.dragState = DragState.OnEnd;
            stateData.hashObjectDragged = m_DraggablePad.gameObject.GetHashCode();

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EndDrag, stateData));

            m_AnchorSelection.OnEndDragPad();
        }

        void OnManageCursor(Vector3 position)
        {
            // Create Cursor
            if (m_CurrentCursorA.gameObject.activeSelf == false || m_CurrentCursorB.gameObject.activeSelf == false)
            {
                m_CreatePointOnScreen = true;
                m_AnchorSelection.UnselectCursor(m_UnselectedCursorMaterial, m_CachedCursorsMeshRenderer);
                if (m_AnchorSelection.OnPointerUp(position, m_CachedMeasureToolStateData.Value))
                {
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, ""));
                }
                else
                {
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                        new StatusMessageData() { text = m_InstructionTapOnSurface, type = StatusMessageType.Instruction }));
                }

                return;
            }

            // Selection
            if (m_MainCamera == null || !m_MainCamera.gameObject.activeInHierarchy)
            {
                m_MainCamera = Camera.main;
                if (m_MainCamera == null)
                {
                    Debug.LogError($"[{nameof(UIMeasureToolController)}] active main camera not found!");
                    return;
                }
            }

            RaycastHit hitInfo;
            Ray ray = m_MainCamera.ScreenPointToRay(position);

            if (UIStateManager.current.stateData.VREnable)
            {
                ray.origin = m_RightController.Value.position;
                ray.direction = m_RightController.Value.forward;
            }

            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.transform.gameObject.Equals(m_CurrentCursorA))
                {
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, ""));
                    m_AnchorSelection.UnselectCursor(m_UnselectedCursorMaterial, m_CachedCursorsMeshRenderer);
                    m_AnchorSelection.SelectCursor(m_CurrentCursorA, m_SelectedCursorMaterial, m_CachedCursorsMeshRenderer);
                    return;
                }

                if (hitInfo.transform.gameObject.Equals(m_CurrentCursorB))
                {
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, ""));
                    m_AnchorSelection.UnselectCursor(m_UnselectedCursorMaterial, m_CachedCursorsMeshRenderer);
                    m_AnchorSelection.SelectCursor(m_CurrentCursorB, m_SelectedCursorMaterial, m_CachedCursorsMeshRenderer);
                    return;
                }
            }

            // Unselect
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, ""));
            m_AnchorSelection.UnselectCursor(m_UnselectedCursorMaterial, m_CachedCursorsMeshRenderer);
        }

        public void SelectVRCursor(GameObject cursor)
        {
            m_AnchorSelection.SelectCursor(cursor, m_SelectedCursorMaterial, m_CachedCursorsMeshRenderer);
        }

        public void UnselectVRCursor()
        {
            m_AnchorSelection.UnselectCursor(m_UnselectedCursorMaterial, m_CachedCursorsMeshRenderer);
        }

        public void SetCurrentCursor(ref GameObject cursorA, ref GameObject cursorB)
        {
            if (cursorA == m_CurrentCursorA && cursorB == m_CurrentCursorB)
                return;

            m_CurrentCursorA = cursorA;
            m_CurrentCursorB = cursorB;

            // Cache MeshRenderers
            m_CachedCursorsMeshRenderer.Clear();
            foreach (var mesh in m_CurrentCursorA.GetComponentsInChildren<MeshRenderer>())
            {
                m_CachedCursorsMeshRenderer.Add(new Tuple<int, MeshRenderer>(0, mesh));
            }

            foreach (var mesh in m_CurrentCursorB.GetComponentsInChildren<MeshRenderer>())
            {
                m_CachedCursorsMeshRenderer.Add(new Tuple<int, MeshRenderer>(1, mesh));
            }

            if (m_CachedCursorsMeshRenderer.Count > 0)
                m_SelectedCursorMaterial = m_CachedCursorsMeshRenderer[0].Item2.material;
        }
    }
}
