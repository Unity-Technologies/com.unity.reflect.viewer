using System;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ARModelAlignSideBarController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_SelectButton;

        [SerializeField]
        ToolButton m_OrbitButton;

        [SerializeField]
        ToolButton m_LookAroundButton;

        [SerializeField]
        ToolButton m_OkButton;

        [SerializeField]
        ToolButton m_BackButton;

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
        ARToolStateData? m_CachedARToolStateData;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.arStateChanged += OnARStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

            m_SelectButton.buttonClicked += OnSelectButtonClicked;
            m_OrbitButton.buttonClicked += OnOrbitButtonClicked;
            m_OrbitButton.buttonLongPressed += OnOrbitButtonLongPressed;
            m_LookAroundButton.buttonClicked += OnLookAroundButtonClicked;
            m_OkButton.buttonClicked += OnOkButtonClicked;
            m_BackButton.buttonClicked += OnBackButtonClicked;

            OnStateDataChanged(UIStateManager.current.stateData);
            OnARStateDataChanged(UIStateManager.current.arStateData);
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_ToolbarsEnabled != data.toolbarsEnabled)
            {
                m_ToolbarsEnabled = data.toolbarsEnabled;
            }

            if (m_CurrentToolState != data.toolState)
            {
                m_SelectButton.selected = false;
                m_OrbitButton.selected = false;
                m_LookAroundButton.selected = false;

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

            CheckButtonValidations();
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            CheckButtonValidations();
        }

        void CheckButtonValidations()
        {
            if (UIStateManager.current.arStateData.arToolStateData.okButtonValidator != null)
            {
                m_OkButton.button.interactable = m_ToolbarsEnabled && UIStateManager.current.arStateData.arToolStateData.okButtonValidator.ButtonValidate();
                m_OkButton.selected = m_OkButton.button.interactable;
            }
        }

        void OnARStateDataChanged(UIARStateData arData)
        {
            if (m_CachedARToolStateData != arData.arToolStateData)
            {
                m_SelectButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.selectionEnabled;
                m_OrbitButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.navigationEnabled;
                m_LookAroundButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.navigationEnabled;
                m_OkButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.okEnabled;
                m_OkButton.selected = m_OkButton.button.interactable;
                m_BackButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.previousStepEnabled;
                m_CachedARToolStateData = arData.arToolStateData;
            }
            CheckButtonValidations();
        }

        void OnSelectButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.ARSelect)) return;

            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = m_SelectButton.selected ? ToolType.None : ToolType.SelectTool;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
        }

        void OnOkButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.Ok)) return;

            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            UIStateManager.current.arStateData.currentInstructionUI.Next();
        }

        void OnLookAroundButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.LookAround)) return;

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
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.OrbitSelect));
        }

        void OnBackButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.Back)) return;

            if (UIStateManager.current.stateData.activeDialog == DialogType.OrbitSelect)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));

            UIStateManager.current.arStateData.currentInstructionUI.Back();
        }
    }
}
