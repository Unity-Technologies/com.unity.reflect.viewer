using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    public class ARModelAlignSideBarController: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_OkButton;

        [SerializeField]
        ToolButton m_BackButton;

#pragma warning restore CS0649

        IUISelector<IARInstructionUI> m_CurrentARInstructionUISelector;
        SetARToolStateAction.IUIButtonValidator m_Validator;
        IUISelector<bool> m_ToolBarEnabledSelector;
        IUISelector<IARInstructionUI> m_CurrentARInstructionUIGetter;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DisposeOnDestroy.Add(m_ToolBarEnabledSelector = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled)));
            m_DisposeOnDestroy.Add(m_CurrentARInstructionUISelector = UISelectorFactory.createSelector<IARInstructionUI>(ARContext.current, nameof(IARModeDataProvider.currentARInstructionUI)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.previousStepEnabled),
                data =>
                {
                    m_BackButton.button.interactable = m_ToolBarEnabledSelector.GetValue() && data;
                    CheckButtonValidations();
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetARToolStateAction.IUIButtonValidator>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.okButtonValidator),
                data =>
                {
                    m_Validator = data;
                    CheckButtonValidations();
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetOrbitTypeAction.OrbitType>(ToolStateContext.current, nameof(IToolStateDataProvider.orbitType)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog)));

            ProjectContext.current.stateChanged += OnProjectStateDataChanged;

            m_OkButton.buttonClicked += OnOkButtonClicked;
            m_BackButton.buttonClicked += OnBackButtonClicked;
        }

        void OnProjectStateDataChanged()
        {
            CheckButtonValidations();
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

        void OnOkButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Ok))
                return;

            m_CurrentARInstructionUISelector.GetValue().Next();
        }

        void OnBackButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Back))
                return;

            m_CurrentARInstructionUISelector.GetValue().Back();
        }
    }
}
