using System;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.Reflect.Viewer;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class RightSideBarController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_OrbitButton;

        [SerializeField]
        ToolButton m_LookAroundButton;

        [SerializeField]
        ToolButton m_SelectButton;

        [SerializeField]
        ToolButton m_SunStudyButton;

        [SerializeField]
        ToolButton m_MeasureToolButton;

        [SerializeField]
        Sprite m_OrbitImage;
        [SerializeField]
        Sprite m_ZoomImage;
        [SerializeField]
        Sprite m_PanImage;

#pragma warning restore CS0649

        ToolType m_CurrentOrbitButtonType;

        bool m_ToolbarsEnabled;
        ToolState? m_CachedToolState;
        ExternalToolStateData? m_CachedExternalToolStateData;
        MeasureToolStateData? m_CachedMeasureToolStateData;

        SpatialSelector m_ObjectSelector;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.externalToolChanged += OnExternalToolStateDataChanged;

            m_OrbitButton.buttonClicked += OnOrbitButtonClicked;
            m_OrbitButton.buttonLongPressed += OnOrbitButtonLongPressed;
            m_LookAroundButton.buttonClicked += OnLookAroundButtonClicked;
            m_SelectButton.buttonClicked += OnSelectButtonClicked;
            m_SunStudyButton.buttonClicked += OnSunStudyButtonClicked;
            m_MeasureToolButton.buttonClicked += OnMeasureToolButtonClicked;

            m_ObjectSelector = new SpatialSelector();
        }

        void OnExternalToolStateDataChanged(ExternalToolStateData data)
        {
            if (m_CachedMeasureToolStateData != data.measureToolStateData)
            {
                m_MeasureToolButton.selected = data.measureToolStateData.toolState;
                m_CachedMeasureToolStateData = data.measureToolStateData;
            }
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_ToolbarsEnabled != data.toolbarsEnabled)
            {
                m_OrbitButton.button.interactable = data.toolbarsEnabled;
                m_LookAroundButton.button.interactable = data.toolbarsEnabled;
                m_SelectButton.button.interactable = data.toolbarsEnabled;
                m_SunStudyButton.button.interactable = data.toolbarsEnabled;
                m_MeasureToolButton.button.interactable = data.toolbarsEnabled;
                m_ToolbarsEnabled = data.toolbarsEnabled;
            }

            if (m_CachedToolState != data.toolState)
            {
                m_OrbitButton.selected = false;

                m_LookAroundButton.selected = false;
                m_SelectButton.selected = false;

                if (data.toolState.activeTool == ToolType.SelectTool)
                {
                    m_SelectButton.selected = true;
                }
                else if (data.toolState.activeTool == ToolType.OrbitTool)
                {
                    if (data.toolState.orbitType == OrbitType.OrbitAtPoint)
                    {
                        m_CurrentOrbitButtonType = data.toolState.activeTool;
                        m_OrbitButton.selected = true;
                        m_OrbitButton.SetIcon(m_OrbitImage);
                    }
                    else if (data.toolState.orbitType == OrbitType.WorldOrbit)
                    {
                        m_LookAroundButton.selected = true;
                    }
                }
                else if (data.toolState.activeTool == ToolType.ZoomTool)
                {
                    m_CurrentOrbitButtonType = data.toolState.activeTool;
                    m_OrbitButton.selected = true;
                    m_OrbitButton.SetIcon(m_ZoomImage);
                }
                else if (data.toolState.activeTool == ToolType.PanTool)
                {
                    m_CurrentOrbitButtonType = data.toolState.activeTool;
                    m_OrbitButton.selected = true;
                    m_OrbitButton.SetIcon(m_PanImage);
                }
                m_CachedToolState = data.toolState;
            }
        }

        void OnSelectButtonClicked()
        {
            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = m_SelectButton.selected ? ToolType.None : ToolType.SelectTool;

            var dialogType = m_SelectButton.selected ? DialogType.None : DialogType.BimInfo;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetObjectPicker, m_ObjectSelector));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, dialogType));
        }

        void OnLookAroundButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.LookAround)) return;

            if (m_SelectButton.selected)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));

            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool =  m_LookAroundButton.selected ? ToolType.None : ToolType.OrbitTool;
            toolState.orbitType = OrbitType.WorldOrbit;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
        }

        void OnOrbitButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.OrbitSelect)) return;

            if (m_SelectButton.selected)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));

            var toolState = UIStateManager.current.stateData.toolState;

            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            if (m_OrbitButton.selected)
            {
                toolState.activeTool = ToolType.None;
            }
            else
            {
                toolState.activeTool = m_CurrentOrbitButtonType;
                toolState.orbitType = OrbitType.OrbitAtPoint;
            }
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
        }

        void OnOrbitButtonLongPressed()
        {
            if (m_SelectButton.selected)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.OrbitSelect));
        }

        void OnSunStudyButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.SunStudyDial)) return;

            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
            if (m_SelectButton.selected)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudyMode, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.TimeOfDayYearDial));
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.SunstudyTool;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            var sunStudyData = UIStateManager.current.stateData.sunStudyData;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, TimeRadialUIController.GetTimeStatusMessage(sunStudyData)));
        }

        void OnMeasureToolButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.MeasureTool)) return;

            var data = UIStateManager.current.externalToolStateData.measureToolStateData;
            data.toolState = !data.toolState;

            if (data.toolState)
            {
                if (UIStateManager.current.stateData.toolState.activeTool == ToolType.SelectTool && UIStateManager.current.projectStateData.objectSelectionInfo.CurrentSelectedObject() == null)
                {
                    var toolState = UIStateManager.current.stateData.toolState;
                    toolState.activeTool = ToolType.None;
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));
                }

                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                    new StatusMessageData() { text = UIMeasureToolController.instructionStart, type = StatusMessageType.Instruction }));
            }
            else
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, ""));
            }

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetMeasureToolOptions, data));

            // To initialize Anchor
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetObjectPicker, m_ObjectSelector));
        }
    }
}
