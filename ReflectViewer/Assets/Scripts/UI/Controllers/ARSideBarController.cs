using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.Core.Actions;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    public class ARSideBarController: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_BackButton;

        [SerializeField]
        ToolButton m_ScaleButton;

        [SerializeField]
        ToolButton m_TargetButton;

        [SerializeField]
        GameObject m_ScaleRadial;
#pragma warning restore CS0649

        bool m_ToolbarsEnabled;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        IUISelector<IARInstructionUI> m_CurrentARInstructionUIGetter;
        LeftSideBarController m_LeftSideBarController;


        void Awake()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled),
                data =>
                {
                    m_ToolbarsEnabled = data;
                }));

            m_LeftSideBarController = GameObject.FindObjectOfType<LeftSideBarController>();

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.scaleEnabled),
                data =>
                {
                    m_ScaleButton.transform.parent.gameObject.SetActive(m_ToolbarsEnabled && data);
                    m_LeftSideBarController.UpdateLayout();
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.previousStepEnabled),
                data =>
                {
                    m_BackButton.button.interactable = m_ToolbarsEnabled && data;
                }));


            m_DisposeOnDestroy.Add(m_CurrentARInstructionUIGetter = UISelectorFactory.createSelector<IARInstructionUI>(ARContext.current, nameof(IARModeDataProvider.currentARInstructionUI)));

            m_BackButton.buttonClicked += OnBackButtonClicked;
            m_ScaleButton.buttonClicked += OnScaleButtonClicked;
            m_TargetButton.buttonClicked += OnTargetButtonClicked;
        }

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void OnTargetButtonClicked()
        {
        }

        void OnScaleButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Scale))
                return;
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARScaleDial));
            ARScaleRadialUIController.m_previousToolbar = SetActiveToolBarAction.ToolbarType.ARSidebar;

            var radialPosition = m_ScaleRadial.transform.position;
            radialPosition.y = m_ScaleButton.transform.position.y;
            m_ScaleRadial.transform.position = radialPosition;
        }

        void OnBackButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Back))
                return;
            m_CurrentARInstructionUIGetter.GetValue().Back();
        }
    }
}
