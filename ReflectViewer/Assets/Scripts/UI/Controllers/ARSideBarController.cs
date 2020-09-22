using System;
using SharpFlux;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ARSideBarController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_BackButton;

        [SerializeField]
        ToolButton m_ScaleButton;

        [SerializeField]
        ToolButton m_SelectButton;

        [SerializeField]
        ToolButton m_TargetButton;

#pragma warning restore CS0649

        bool m_ToolbarsEnabled;
        ToolType? m_CurrentActiveTool;
        NavigationMode? m_CurrentNavigationMode;
        InstructionUI? m_CurrentInstructionUI;
        ToolState? m_CurrentToolState;

        void Start()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.arStateChanged += OnARStateDataChanged;

            m_BackButton.buttonClicked += OnBackButtonClicked;
            m_SelectButton.buttonClicked += OnSelectButtonClicked;
            m_ScaleButton.buttonClicked += OnScaleButtonClicked;
            m_TargetButton.buttonClicked += OnTargetButtonClicked;
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_ToolbarsEnabled != data.toolbarsEnabled)
            {
                m_ToolbarsEnabled = data.toolbarsEnabled;
                OnARStateDataChanged(UIStateManager.current.arStateData);
            }

            if (m_CurrentNavigationMode != data.navigationState.navigationMode)
            {
                m_CurrentNavigationMode = data.navigationState.navigationMode;
                OnARStateDataChanged(UIStateManager.current.arStateData);
            }

            if (m_CurrentToolState != data.toolState)
            {
                m_SelectButton.selected = false;

                if (data.toolState.activeTool == ToolType.SelectTool)
                {
                    m_SelectButton.selected = true;
                }
                m_CurrentToolState = data.toolState;
            }
        }

        void OnARStateDataChanged(UIARStateData stateData)
        {
            if (m_CurrentNavigationMode !=  NavigationMode.AR)
            {
                m_BackButton.selected = false;
                m_SelectButton.selected = false;
                m_ScaleButton.selected = false;
                m_TargetButton.selected = false;
            }

            if (m_CurrentInstructionUI != stateData.instructionUI)
            {
                m_CurrentInstructionUI = stateData.instructionUI;
                if (m_CurrentInstructionUI == InstructionUI.OnBoardingComplete)
                {
                    m_BackButton.button.interactable = true;
                    m_SelectButton.button.interactable = true;
                    m_ScaleButton.button.interactable = true;
                }
                else
                {
                    m_BackButton.button.interactable = false;
                    m_SelectButton.button.interactable = false;
                    m_ScaleButton.button.interactable = false;
                }
            }
        }

        void OnTargetButtonClicked()
        {
        }

        void OnScaleButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARScaleDial));
            ARScaleRadialUIController.m_previousToolbar = ToolbarType.ARSidebar;
        }

        void OnSelectButtonClicked()
        {
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = m_SelectButton.selected ? ToolType.None : ToolType.SelectTool;

            var dialogType = m_SelectButton.selected ? DialogType.None : DialogType.BimInfo;

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, dialogType));
        }

        void OnBackButtonClicked()
        {
            // Back into Instruction Mode
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, InstructionUI.ConfirmPlacement));
        }
    }
}
