using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class HelpToggleButton : MonoBehaviour
    {
        Button m_Button;
        Image m_ButtonImage;
        bool m_Visibility;

        IUISelector<bool> m_MeasureToolStateGetter;
        bool m_Interactable;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;
        IUISelector<OpenDialogAction.DialogType> m_ActiveSubDialogSelector;
        IUISelector<SetNavigationModeAction.NavigationMode> m_NavigationModeSelector;
        IUISelector<SetDialogModeAction.DialogMode> m_DialogModeSelector;
        IUISelector<SetActiveToolBarAction.ToolbarType> m_ActiveToolBarSelector;
        IUISelector<bool> m_ToolBarEnabledSelector;

        List<IDisposable> m_Disposables= new List<IDisposable>();

        void Awake()
        {
            m_Button = GetComponent<Button>();
            m_ButtonImage = m_Button.GetComponent<Image>();

            m_Disposables.Add(m_MeasureToolStateGetter = UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState)));
            m_Disposables.Add(UISelectorFactory.createSelector<IButtonVisibility>(AppBarContext.current, nameof(IAppBarDataProvider.buttonVisibility), OnButtonVisibilityChanged));
            m_Disposables.Add(UISelectorFactory.createSelector<IButtonInteractable>(AppBarContext.current, nameof(IAppBarDataProvider.buttonInteractable), OnButtonInteractableChanged));
            m_Disposables.Add(m_ActiveToolBarSelector = UISelectorFactory.createSelector<SetActiveToolBarAction.ToolbarType>(UIStateContext.current, nameof(IToolBarDataProvider.activeToolbar)));
            m_Visibility = true;
            m_Interactable = true;

            m_Disposables.Add(m_ToolBarEnabledSelector = UISelectorFactory.createSelector<bool>(UIStateContext.current,
                nameof(IToolBarDataProvider.toolbarsEnabled), UpdateButtonInteractable));

            m_Disposables.Add(m_DialogModeSelector = UISelectorFactory.createSelector<SetDialogModeAction.DialogMode>(UIStateContext.current, nameof(IDialogDataProvider.dialogMode),
                data =>
                {
                    m_ButtonImage.enabled = data == SetDialogModeAction.DialogMode.Help;
                } ));

            m_Disposables.Add(m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog),
                data =>
                {
                    m_Button.transform.parent.gameObject.SetActive(m_Visibility && data != OpenDialogAction.DialogType.LandingScreen &&
                                                                   m_NavigationModeSelector?.GetValue() != SetNavigationModeAction.NavigationMode.AR && m_NavigationModeSelector?.GetValue() != SetNavigationModeAction.NavigationMode.VR);
                } ));

            m_Disposables.Add(m_ActiveSubDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog)));

            m_Disposables.Add(m_NavigationModeSelector = UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode),
                data =>
                {
                    m_Button.transform.parent.gameObject.SetActive(m_ActiveDialogSelector?.GetValue() != OpenDialogAction.DialogType.LandingScreen &&
                                                                   data != SetNavigationModeAction.NavigationMode.AR && data != SetNavigationModeAction.NavigationMode.VR);
                } ));
        }

        void OnDestroy()
        {
            foreach(var disposable in m_Disposables)
            {
                disposable.Dispose();
            }
            m_Disposables.Clear();
        }

        void Start()
        {
            m_Button.onClick.AddListener(OnButtonClick);
        }

        void OnButtonClick()
        {
            var dialogMode = m_DialogModeSelector.GetValue();
            dialogMode = (dialogMode == SetDialogModeAction.DialogMode.Help) ? SetDialogModeAction.DialogMode.Normal : SetDialogModeAction.DialogMode.Help;
            Dispatcher.Dispatch(SetDialogModeAction.From(dialogMode));

            // close all (sub)dialogs, and sunstudy dial (a ToolbarType)
            if (dialogMode == SetDialogModeAction.DialogMode.Help)
            {
                Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"HelpMode"));
                Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
                Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.None));

                var activeToolbar = m_ActiveToolBarSelector.GetValue();
                if (activeToolbar == SetActiveToolBarAction.ToolbarType.TimeOfDayYearDial || activeToolbar == SetActiveToolBarAction.ToolbarType.AltitudeAzimuthDial)
                {
                    Dispatcher.Dispatch(ClearStatusAction.From(true));
                    Dispatcher.Dispatch(ClearStatusAction.From(false));
                    Dispatcher.Dispatch(SetActiveToolBarAction.From(TimeRadialUIController.m_previousToolbar));
                }

                if (m_MeasureToolStateGetter.GetValue())
                {
                    Dispatcher.Dispatch(ClearStatusAction.From(true));
                    Dispatcher.Dispatch(ClearStatusAction.From(false));
                    Dispatcher.Dispatch(ToggleMeasureToolAction.From(ToggleMeasureToolAction.ToggleMeasureToolData.defaultData));
                }
            }
        }

        void OnButtonVisibilityChanged(IButtonVisibility data)
        {
            if (data?.type == (int)ButtonType.Help)
            {
                m_Visibility = data.visible;
                m_Button.transform.parent.gameObject.SetActive(m_ActiveDialogSelector.GetValue() != OpenDialogAction.DialogType.LandingScreen &&
                    m_NavigationModeSelector.GetValue() != SetNavigationModeAction.NavigationMode.AR &&
                    m_NavigationModeSelector.GetValue() != SetNavigationModeAction.NavigationMode.VR &&
                    m_Visibility
                );
            }
        }


        void UpdateButtonInteractable(bool toolbarsEnabled)
        {
            m_Button.interactable = toolbarsEnabled && m_Interactable;
        }

        void OnButtonInteractableChanged(IButtonInteractable data)
        {
            if (data?.type == (int)ButtonType.Help)
            {
                m_Interactable = data.interactable;
                UpdateButtonInteractable(m_ToolBarEnabledSelector.GetValue());
            }
        }
    }
}
