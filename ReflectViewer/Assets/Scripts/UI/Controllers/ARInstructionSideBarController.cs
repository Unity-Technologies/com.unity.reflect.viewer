using System;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public class ARInstructionSideBarController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_BackButton;

        [SerializeField]
        ToolButton m_OkButton;

        [SerializeField]
        ToolButton m_CancelButton;

        [SerializeField]
        ToolButton m_ScaleButton;

        [SerializeField]
        GameObject m_ScaleRadial;
#pragma warning restore CS0649

        bool m_ToolbarsEnabled;
        ToolType? m_CurrentActiveTool;
        NavigationMode? m_CurrentNavigationMode;
        InstructionUIState? m_CurrentInstructionUI;
        bool? m_CachedPlacementGesturesEnabled;
        ARToolStateData? m_CachedARToolStateData;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.arStateChanged += OnARStateDataChanged;

            m_BackButton.buttonClicked += OnBackButtonClicked;
            m_OkButton.buttonClicked += OnOkButtonClicked;
            m_CancelButton.buttonClicked += OnCancelButtonClicked;
            m_ScaleButton.buttonClicked += OnScaleButtonClicked;

            OnStateDataChanged(UIStateManager.current.stateData);
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

        void OnARStateDataChanged(UIARStateData arData)
        {
            if (m_CachedARToolStateData != arData.arToolStateData)
            {
                m_BackButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.previousStepEnabled;
                m_OkButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.okEnabled;
                m_OkButton.selected = m_OkButton.button.interactable;
                m_CancelButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.cancelEnabled;
                m_ScaleButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.scaleEnabled;

                m_CachedARToolStateData = arData.arToolStateData;
            }
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

        void OnCancelButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.Cancel)) return;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Cancel, true ));
        }

        void OnOkButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.Ok)) return;
            UIStateManager.current.arStateData.currentInstructionUI.Next();
        }

        void OnBackButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.Back)) return;
            UIStateManager.current.arStateData.currentInstructionUI.Back();
        }

        void OnScaleButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.Scale)) return;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARScaleDial));
            ARScaleRadialUIController.m_previousToolbar = ToolbarType.ARInstructionSidebar;

            var radialPosition = m_ScaleRadial.transform.position;
            radialPosition.y = m_ScaleButton.transform.position.y;
            m_ScaleRadial.transform.position = radialPosition;
        }
    }
}
