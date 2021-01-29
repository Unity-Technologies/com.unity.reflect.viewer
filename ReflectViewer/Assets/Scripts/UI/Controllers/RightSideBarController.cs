using System;
using SharpFlux;
using UnityEngine;
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
        Sprite m_OrbitImage;
        [SerializeField]
        Sprite m_ZoomImage;
        [SerializeField]
        Sprite m_PanImage;

#pragma warning restore CS0649

        ToolType m_CurrentOrbitButtonType;

        bool m_ToolbarsEnabled;
        ToolState m_CurrentToolState;

        SpatialSelector m_ObjectSelector;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_OrbitButton.buttonClicked += OnOrbitButtonClicked;
            m_OrbitButton.buttonLongPressed += OnOrbitButtonLongPressed;
            m_LookAroundButton.buttonClicked += OnLookAroundButtonClicked;
            m_SelectButton.buttonClicked += OnSelectButtonClicked;
            m_SunStudyButton.buttonClicked += OnSunStudyButtonClicked;

            m_ObjectSelector = new SpatialSelector();
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_ToolbarsEnabled != data.toolbarsEnabled)
            {
                m_OrbitButton.button.interactable = data.toolbarsEnabled;
                m_LookAroundButton.button.interactable = data.toolbarsEnabled;
                m_SelectButton.button.interactable = data.toolbarsEnabled;
                m_SunStudyButton.button.interactable = data.toolbarsEnabled;
                m_ToolbarsEnabled = data.toolbarsEnabled;
            }

            if (m_CurrentToolState != data.toolState)
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
                m_CurrentToolState = data.toolState;
            }
        }

        void OnSelectButtonClicked()
        {
            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = m_SelectButton.selected ? ToolType.None : ToolType.SelectTool;

            var dialogType = m_SelectButton.selected ? DialogType.None : DialogType.BimInfo;

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetObjectPicker, m_ObjectSelector));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, dialogType));
        }

        void OnLookAroundButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.LookAround)) return;

            if (m_SelectButton.selected)
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));

            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool =  m_LookAroundButton.selected ? ToolType.None : ToolType.OrbitTool;
            toolState.orbitType = OrbitType.WorldOrbit;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
        }

        void OnOrbitButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.OrbitSelect)) return;

            if (m_SelectButton.selected)
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));

            var toolState = UIStateManager.current.stateData.toolState;

            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            if (m_OrbitButton.selected)
            {
                toolState.activeTool = ToolType.None;
            }
            else
            {
                toolState.activeTool = m_CurrentOrbitButtonType;
                toolState.orbitType = OrbitType.OrbitAtPoint;
            }
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
        }

        void OnOrbitButtonLongPressed()
        {
            if (m_SelectButton.selected)
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.OrbitSelect));
        }

        void OnSunStudyButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.SunStudyDial)) return;

            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
            if (m_SelectButton.selected)
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudyMode, false));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.TimeOfDayYearDial));
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.SunstudyTool;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            var sunStudyData = UIStateManager.current.stateData.sunStudyData;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatus, TimeRadialUIController.GetTimeStatusMessage(sunStudyData)));
        }
    }
}
