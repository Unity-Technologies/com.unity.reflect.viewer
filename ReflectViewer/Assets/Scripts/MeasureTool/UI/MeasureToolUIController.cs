using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.Reflect.Viewer.UI;
using UnityEngine.InputSystem;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace UnityEngine.Reflect.MeasureTool
{
    public class MeasureToolUIController: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_ResetButton;
        [SerializeField]
        Image m_ModeSwitchImage;
        [SerializeField]
        Sprite m_RegularModeSprite;
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
        ToolButton m_MeasureToolButton;
#pragma warning restore CS0649

        UIAnchorSelection m_AnchorSelection;
        Camera m_MainCamera;
        Material m_SelectedCursorMaterial;
        Material m_PlainLineMaterial;
        List<Tuple<int, MeshRenderer>> m_CachedCursorsMeshRenderer = new List<Tuple<int, MeshRenderer>>();
        VRMeasureToolController m_VRMeasureTool;
        GameObject m_CurrentCursorA;
        GameObject m_CurrentCursorB;
        bool m_OnDrag;
        Sprite m_PerpendicularModeSprite;
        Button m_ModeSwitchButton;
        SpatialSelector m_ObjectSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        IUISelector<ToggleMeasureToolAction.MeasureMode> m_MeasureModeGetter;
        IUISelector<int> m_HashObjectDraggedSelector;
        IUISelector<bool> m_VREnableSelector;
        IUISelector<Transform> m_VRControllerSelector;
        IUISelector<bool> m_MeasureToolStateGetter;
        IUISelector<SelectObjectAction.IObjectSelectionInfo> m_ObjectSelectionInfoGetter;

        IUISelector<SetActiveToolAction.ToolType> m_ActiveToolGetter;

        public const string instructionStart = "Tap anywhere on the model to place a measurement point.";
        public const string instructionTapOnSurface = "/!\\ The point did not register. Tap on a surface to add a measurement point.";

        void Awake()
        {
            m_MeasureToolButton.buttonClicked += OnMeasureToolButtonClicked;
            SetCurrentCursor(m_CursorA, m_CursorB);
            m_VRMeasureTool = GetComponent<VRMeasureToolController>();
            m_ObjectSelector = new SpatialSelector();

            m_PlainLineMaterial = m_Line.material;
            m_PerpendicularModeSprite = m_ModeSwitchImage.sprite;
            m_ModeSwitchButton = m_ModeSwitchImage.transform.parent.GetComponent<Button>();

            m_DisposeOnDestroy.Add(m_MeasureToolStateGetter = UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState), OnToolStateDataChanged));
            m_DisposeOnDestroy.Add(m_ObjectSelectionInfoGetter = UISelectorFactory.createSelector<SelectObjectAction.IObjectSelectionInfo>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectSelectionInfo)));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<float>(UIStateContext.current, nameof(IUIStateDisplayProvider<DisplayData>.display) + "." + nameof(IDisplayDataProvider.scaleFactor), OnScaleFactorChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState), OnToolStateDataChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<ToggleMeasureToolAction.MeasureFormat>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.measureFormat), OnMeasureFormatDataChanged));
            m_DisposeOnDestroy.Add(m_MeasureModeGetter = UISelectorFactory.createSelector<ToggleMeasureToolAction.MeasureMode>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.measureMode), OnMeasureModeDataChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<ToggleMeasureToolAction.AnchorType>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.selectionType), OnSelectionTypeDataChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SelectObjectMeasureToolAction.IAnchor>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.selectedAnchor), OnSelectedAnchorsDataChanged));

            m_DisposeOnDestroy.Add(m_VREnableSelector = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable)));
            m_DisposeOnDestroy.Add(m_VRControllerSelector = UISelectorFactory.createSelector<Transform>(VRContext.current, nameof(IVREnableDataProvider.RightController)));

            m_AnchorSelection = new UIAnchorSelection(m_MeasureText, m_DraggablePad, m_Line, m_MeasureModeGetter.GetValue, m_VREnableSelector.GetValue, m_VRControllerSelector.GetValue);

            m_DisposeOnDestroy.Add(m_HashObjectDraggedSelector = UISelectorFactory.createSelector<int>(DragStateContext.current, nameof(IDragStateData.hashObjectDragged)));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<DragState>(DragStateContext.current, nameof(IDragStateData.dragState), OnDragStateDataChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<Vector3>(DragStateContext.current, nameof(IDragStateData.position), OnDragPositionDataChanged));

            m_DisposeOnDestroy.Add(m_ActiveToolGetter =  UISelectorFactory.createSelector<SetActiveToolAction.ToolType>(ToolStateContext.current, nameof(IToolStateDataProvider.activeTool)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetHelpModeIDAction.HelpModeEntryID>(UIStateContext.current, nameof(IHelpModeDataProvider.helpModeEntryId), OnHelpModeEntryChanged));

            m_DraggablePad.onBeginDrag.AddListener(OnBeginDrag);
            m_DraggablePad.onDrag.AddListener(OnDrag);
            m_DraggablePad.onEndDrag.AddListener(OnEndDrag);
            m_ResetButton.onClick.AddListener(OnReset);
            m_ModeSwitchButton.onClick.AddListener(OnSwitchMode);
        }

        void OnDestroy()
        {
            m_AnchorSelection?.Dispose();
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void OnMeasureToolButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.MeasureTool))
            {
                m_MeasureToolButton.selected = true;
                return;
            }

            var toggleData = ToggleMeasureToolAction.ToggleMeasureToolData.defaultData;
            toggleData.toolState = !m_MeasureToolStateGetter.GetValue();

            if (toggleData.toolState)
            {
                Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));

                if (m_ActiveToolGetter.GetValue() == SetActiveToolAction.ToolType.SelectTool && ((ObjectSelectionInfo)m_ObjectSelectionInfoGetter.GetValue()).CurrentSelectedObject() == null)
                {
                    Dispatcher.Dispatch(SetActiveToolAction.From(SetActiveToolAction.ToolType.None));
                }

                Dispatcher.Dispatch(SetStatusMessageWithType.From(new StatusMessageData { text = instructionStart, type = StatusMessageType.Instruction }));
            }
            else
            {
                Dispatcher.Dispatch(ClearStatusAction.From(true));
                Dispatcher.Dispatch(ClearStatusAction.From(false));
            }

            Dispatcher.Dispatch(ToggleMeasureToolAction.From(toggleData));

            // To initialize Anchor
            Dispatcher.Dispatch(SetSpatialSelectorAction.From(m_ObjectSelector));
        }

        void OnScaleFactorChanged(float newData)
        {
            if (newData > 0f)
            {
                var currentScaleFactor = 1 / newData;
                m_CurrentCursorA.transform.localScale = new Vector3(currentScaleFactor, currentScaleFactor, currentScaleFactor);
                m_CurrentCursorB.transform.localScale = new Vector3(currentScaleFactor, currentScaleFactor, currentScaleFactor);
            }
        }

        void OnToolStateDataChanged(bool newData)
        {
            if (newData)
            {
                InitCursor();
                m_ResetButton.gameObject.SetActive(true);
                m_ModeSwitchButton.gameObject.SetActive(true);
                m_InputActionAsset["MeasureTool/Select"].performed += OnPointerUp;
                m_InputActionAsset["VR/Select"].performed += OnPointerUp;
                m_InputActionAsset["MeasureTool/Select"].Enable();
                m_MeasureToolButton.selected = true;
            }
            else
            {
                m_ResetButton.gameObject.SetActive(false);
                m_ModeSwitchButton.gameObject.SetActive(false);
                m_InputActionAsset["MeasureTool/Select"].performed -= OnPointerUp;
                m_InputActionAsset["VR/Select"].performed -= OnPointerUp;
                OnReset();
                m_VRMeasureTool.OnReset();
                m_MeasureToolButton.selected = false;
            }
        }

        void OnMeasureFormatDataChanged(ToggleMeasureToolAction.MeasureFormat newData)
        {
            //TODO: update the current value of the measure
        }

        void OnMeasureModeDataChanged(ToggleMeasureToolAction.MeasureMode newData)
        {
            //TODO: better UX implementation - this is temporary UX feedback
            switch (newData)
            {
                case ToggleMeasureToolAction.MeasureMode.RawDistance:
                    m_ModeSwitchImage.sprite = m_PerpendicularModeSprite;
                    break;
                case ToggleMeasureToolAction.MeasureMode.PerpendicularDistance:
                    m_ModeSwitchImage.sprite = m_RegularModeSprite;
                    break;
            }
        }

        void OnSelectionTypeDataChanged(ToggleMeasureToolAction.AnchorType newData)
        {
            m_AnchorSelection?.SetAnchorPickerSelectionType(newData);

            //TODO: update the Cursors style with the current selectionType
        }

        void OnSelectedAnchorsDataChanged(SelectObjectMeasureToolAction.IAnchor newData)
        {
            // Create Point
            if (!m_OnDrag)
            {
                if (!m_CurrentCursorA.activeSelf)
                {
                    m_AnchorSelection?.OnStateDataChanged(newData, m_CurrentCursorA);
                    return;
                }

                if (!m_CurrentCursorB.activeSelf)
                {
                    m_AnchorSelection.OnStateDataChanged(newData, m_CurrentCursorB);
                    m_AnchorSelection.SetLineUI();
                    return;
                }
            }

            // Drag Existing Point
            m_AnchorSelection?.OnStateDataChanged(newData);
            if (m_CurrentCursorB.activeSelf)
            {
                m_AnchorSelection.SetLineUI();
            }
        }

        void Update()
        {
            m_AnchorSelection.Update();
        }

        void OnDragStateDataChanged(DragState newData)
        {
            if (m_HashObjectDraggedSelector.GetValue() != m_DraggablePad.gameObject.GetHashCode())
                return;

            switch (newData)
            {
                case DragState.OnStart:
                    m_Line.material = m_DotedLineMaterial;
                    break;
                case DragState.OnEnd:
                    m_Line.material = m_PlainLineMaterial;
                    break;
            }
        }

        void OnDragPositionDataChanged(Vector3 newData)
        {
            if (m_HashObjectDraggedSelector.GetValue() != m_DraggablePad.gameObject.GetHashCode())
                return;

            m_DraggablePad.gameObject.transform.position = newData;
        }

        void InitCursor()
        {
            if (m_VREnableSelector.GetValue())
            {
                m_VRMeasureTool.InitVR();
                m_InputActionAsset["MeasureTool/Reset"].performed += OnVRReset;
            }
            else
            {
                m_InputActionAsset["MeasureTool/Reset"].performed -= OnVRReset;
                SetCurrentCursor(m_CursorA, m_CursorB);
            }
        }

        void OnVRReset(InputAction.CallbackContext obj)
        {
            OnReset();
        }

        void OnPointerUp(InputAction.CallbackContext input)
        {
            if (OrphanUIController.isTouchBlockedByUI && !m_VREnableSelector.GetValue())
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
            m_AnchorSelection?.ResetSelector();

            foreach (var tuple in m_CachedCursorsMeshRenderer)
            {
                tuple.Item2.material = m_SelectedCursorMaterial;
            }
        }

        void OnSwitchMode()
        {
            switch (m_MeasureModeGetter.GetValue())
            {
                case ToggleMeasureToolAction.MeasureMode.RawDistance:
                    Dispatcher.Dispatch(ChangeMeasureModeMeasureToolAction.From(ToggleMeasureToolAction.MeasureMode.PerpendicularDistance));
                    break;
                case ToggleMeasureToolAction.MeasureMode.PerpendicularDistance:
                    Dispatcher.Dispatch(ChangeMeasureModeMeasureToolAction.From(ToggleMeasureToolAction.MeasureMode.RawDistance));
                    break;
            }
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

        void OnManageCursor(Vector3 position)
        {
            // Create Cursor
            if (m_CurrentCursorA.gameObject.activeSelf == false || m_CurrentCursorB.gameObject.activeSelf == false)
            {
                m_AnchorSelection.UnselectCursor(m_UnselectedCursorMaterial, m_CachedCursorsMeshRenderer);
                m_AnchorSelection.OnPointerUp(position);

                return;
            }

            // Selection
            if (m_MainCamera == null || !m_MainCamera.gameObject.activeInHierarchy)
            {
                m_MainCamera = Camera.main;
                if (m_MainCamera == null)
                {
                    Debug.LogError($"[{nameof(MeasureToolUIController)}] active main camera not found!");
                    return;
                }
            }

            RaycastHit hitInfo;
            Ray ray = m_MainCamera.ScreenPointToRay(position);

            if (m_VREnableSelector.GetValue() && m_VRControllerSelector.GetValue() != null)
            {
                ray.origin = m_VRControllerSelector.GetValue().position;
                ray.direction = m_VRControllerSelector.GetValue().forward;
            }

            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.transform.gameObject.Equals(m_CurrentCursorA))
                {
                    Dispatcher.Dispatch(ClearStatusAction.From(true));
                    Dispatcher.Dispatch(ClearStatusAction.From(false));
                    m_AnchorSelection.UnselectCursor(m_UnselectedCursorMaterial, m_CachedCursorsMeshRenderer);
                    m_AnchorSelection.SelectCursor(m_CurrentCursorA, m_SelectedCursorMaterial, m_CachedCursorsMeshRenderer);
                    return;
                }

                if (hitInfo.transform.gameObject.Equals(m_CurrentCursorB))
                {
                    Dispatcher.Dispatch(ClearStatusAction.From(true));
                    Dispatcher.Dispatch(ClearStatusAction.From(false));
                    m_AnchorSelection.UnselectCursor(m_UnselectedCursorMaterial, m_CachedCursorsMeshRenderer);
                    m_AnchorSelection.SelectCursor(m_CurrentCursorB, m_SelectedCursorMaterial, m_CachedCursorsMeshRenderer);
                    return;
                }
            }

            // Unselect
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
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

        public void SetCurrentCursor(GameObject cursorA, GameObject cursorB)
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

        void OnHelpModeEntryChanged(SetHelpModeIDAction.HelpModeEntryID newData)
        {
            if (newData == SetHelpModeIDAction.HelpModeEntryID.None)
            {
                m_MeasureToolButton.selected = false;
            }
        }
    }
}
