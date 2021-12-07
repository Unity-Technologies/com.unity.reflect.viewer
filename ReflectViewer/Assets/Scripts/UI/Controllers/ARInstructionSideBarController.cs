using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

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

        SetActiveToolAction.ToolType? m_CurrentActiveTool;
        bool? m_CachedPlacementGesturesEnabled;
        IUISelector<IARInstructionUI> m_CurrentARInstructionUISelector;

        SetARToolStateAction.IUIButtonValidator m_Validator;
        LeftSideBarController m_LeftSideBarController;

        IUISelector<bool> m_ToolBarEnabledSelector;
        List<IDisposable> m_DisposableSelectors = new List<IDisposable>();

        void Awake()
        {
            m_BackButton.buttonClicked += OnBackButtonClicked;
            m_OkButton.buttonClicked += OnOkButtonClicked;
            m_CancelButton.buttonClicked += OnCancelButtonClicked;
            m_ScaleButton.buttonClicked += OnScaleButtonClicked;
            m_LeftSideBarController = GameObject.FindObjectOfType<LeftSideBarController>();

            m_DisposableSelectors.Add(m_ToolBarEnabledSelector = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled)));
            m_DisposableSelectors.Add(m_CurrentARInstructionUISelector = UISelectorFactory.createSelector<IARInstructionUI>(ARContext.current, nameof(IARModeDataProvider.currentARInstructionUI)));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.previousStepEnabled),
                data =>
                {
                    m_BackButton.button.interactable = m_ToolBarEnabledSelector.GetValue() && data;
                } ));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.okEnabled),
                data =>
                {
                    m_OkButton.button.interactable = m_ToolBarEnabledSelector.GetValue() && data;
                    m_OkButton.selected = m_OkButton.button.interactable;
                } ));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.cancelEnabled),
                data =>
                {
                    m_CancelButton.transform.parent.gameObject.SetActive(m_ToolBarEnabledSelector.GetValue() && data);
                    m_LeftSideBarController.UpdateLayout();
                } ));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.scaleEnabled),
                data =>
                {
                    m_ScaleButton.transform.parent.gameObject.SetActive(m_ToolBarEnabledSelector.GetValue() && data);
                    m_LeftSideBarController.UpdateLayout();
                } ));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<SetARToolStateAction.IUIButtonValidator>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.okButtonValidator),
                data =>
                {
                    m_Validator = data;
                    CheckButtonValidations();
                } ));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(ARPlacementContext.current, nameof(IARPlacementDataProvider.validTarget),
                data =>
                {
                    CheckButtonValidations();
                } ));
        }

        void OnDestroy()
        {
            foreach(var disposable in m_DisposableSelectors)
            {
                disposable.Dispose();
            }
            m_DisposableSelectors.Clear();
        }

        void CheckButtonValidations()
        {
            if (m_Validator == null)
                return;

            if (m_Validator is SetARToolStateAction.EmptyUIButtonValidator)
                return;

            m_OkButton.button.interactable = m_ToolBarEnabledSelector.GetValue() && m_Validator.ButtonValidate();
            m_OkButton.selected = m_OkButton.button.interactable;
        }

        void OnCancelButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Cancel)) return;
            Dispatcher.Dispatch(CancelAction.From(true));
        }

        void OnOkButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Ok)) return;
            m_CurrentARInstructionUISelector.GetValue().Next();
        }

        void OnBackButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Back)) return;
            m_CurrentARInstructionUISelector.GetValue().Back();
        }

        void OnScaleButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Scale)) return;
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARScaleDial));
            ARScaleRadialUIController.m_previousToolbar = SetActiveToolBarAction.ToolbarType.ARInstructionSidebar;

            var radialPosition = m_ScaleRadial.transform.position;
            radialPosition.y = m_ScaleButton.transform.position.y;
            m_ScaleRadial.transform.position = radialPosition;
        }
    }
}
