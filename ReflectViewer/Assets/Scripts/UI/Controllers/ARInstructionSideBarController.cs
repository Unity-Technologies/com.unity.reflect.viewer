using System;
using SharpFlux;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ARInstructionSideBarController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_BackButton;

        [SerializeField]
        ToolButton m_CheckButton;

        [SerializeField]
        ToolButton m_CancelButton;

        [SerializeField]
        ToolButton m_ScaleButton;
#pragma warning restore CS0649

        bool m_ToolbarsEnabled;
        ToolType? m_CurrentActiveTool;
        NavigationMode? m_CurrentNavigationMode;
        InstructionUI? m_CurrentInstructionUI;

        void Start()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.arStateChanged += OnARStateDataChanged;

            m_BackButton.buttonClicked += OnBackButtonClicked;
            m_CheckButton.buttonClicked += OnCheckButtonClicked;
            m_CancelButton.buttonClicked += OnCancelButtonClicked;
            m_ScaleButton.buttonClicked += OnScaleButtonClicked;
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
        }

        void OnARStateDataChanged(UIARStateData stateData)
        {
            if (m_CurrentNavigationMode ==  NavigationMode.AR && m_ToolbarsEnabled)
            {
                if (m_CurrentInstructionUI != stateData.instructionUI)
                {
                    m_CurrentInstructionUI = stateData.instructionUI;
                    switch(m_CurrentInstructionUI)
                    {
                        case InstructionUI.Init:
                        {
                            m_BackButton.button.interactable = false;
                            m_CheckButton.button.interactable = false;
                            m_CancelButton.button.interactable = false;
                            m_ScaleButton.button.interactable = false;
                            break;
                        }

                        case InstructionUI.CrossPlatformFindAPlane:
                        {
                            m_BackButton.button.interactable = false;
                            m_CheckButton.button.interactable = false;
                            m_CancelButton.button.interactable = false;
                            m_ScaleButton.button.interactable = false;
                            break;
                        }

                        case InstructionUI.AimToPlaceBoundingBox:
                        {
                            m_BackButton.button.interactable = true;
                            m_CheckButton.button.interactable = true;
                            m_CancelButton.button.interactable = false;
                            m_ScaleButton.button.interactable = false;
                            break;
                        }

                        case InstructionUI.ConfirmPlacement:
                        {
                            m_BackButton.button.interactable = true;
                            m_CheckButton.button.interactable = true;
                            m_CancelButton.button.interactable = true;
                            m_ScaleButton.button.interactable = true;
                            break;
                        }
                    }
                }
            }
        }

        void OnCancelButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Cancel, true ));
        }

        void OnCheckButtonClicked()
        {
            InstructionUI next = UIStateManager.current.arStateData.instructionUI + 1;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, next));
        }

        void OnBackButtonClicked()
        {
            InstructionUI previous = UIStateManager.current.arStateData.instructionUI - 1;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, previous));
        }

        void OnScaleButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARScaleDial));
            ARScaleRadialUIController.m_previousToolbar = ToolbarType.ARInstructionSidebar;
        }
    }
}
