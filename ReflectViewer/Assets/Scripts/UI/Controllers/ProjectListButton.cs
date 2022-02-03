using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class ProjectListButton: MonoBehaviour
    {
        [SerializeField, Tooltip("Project List Button")]
        ToolButton m_ProjectListButton;
        [SerializeField, Tooltip("Refresh Button")]
        Button m_RefreshButton;

        IUISelector<Project> m_ActiveProjectSelector;
        bool m_RefreshVisibility;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;
        IUISelector<SetDialogModeAction.DialogMode> m_DialogModeSelector;
        IUISelector<LoginState> m_LoginStateSelector;
        bool m_Interactable;

        List<IDisposable> m_DisposeOnDisable = new List<IDisposable>();
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonVisibility>(AppBarContext.current, nameof(IAppBarDataProvider.buttonVisibility), OnButtonVisibilityChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonInteractable>(AppBarContext.current, nameof(IAppBarDataProvider.buttonInteractable), OnButtonInteractableChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetHelpModeIDAction.HelpModeEntryID>(UIStateContext.current, nameof(IHelpModeDataProvider.helpModeEntryId), OnHelpModeEntryChanged));

            m_RefreshVisibility = true;
            m_Interactable = true;
        }

        void OnEnable()
        {
            m_DisposeOnDisable.Add(m_ActiveProjectSelector = UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), OnActiveProjectChanged));
            m_DisposeOnDisable.Add(m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged));
            m_DisposeOnDisable.Add(m_DialogModeSelector = UISelectorFactory.createSelector<SetDialogModeAction.DialogMode>(UIStateContext.current, nameof(IDialogDataProvider.dialogMode), OnDialogModeChanged));
            m_DisposeOnDestroy.Add(m_LoginStateSelector = UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState)));
        }

        void OnDisable()
        {
            m_DisposeOnDisable.ForEach(x => x.Dispose());
            m_DisposeOnDisable.Clear();
        }

        void Start()
        {
            m_ProjectListButton.button.interactable = false;
            m_RefreshButton.onClick.AddListener(OnRefreshClicked);
            m_ProjectListButton.buttonClicked += OnProjectListButtonClick;
        }

        void OnRefreshClicked()
        {
            Dispatcher.Dispatch(RefreshProjectListAction.From(ProjectListState.AwaitingUserData));
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType newData)
        {
            UpdateButtons();
        }

        void OnDialogModeChanged(SetDialogModeAction.DialogMode newData)
        {
            UpdateButtons();
        }

        public void UpdateButtons()
        {
            if (m_ActiveDialogSelector?.GetValue() == OpenDialogAction.DialogType.LandingScreen && m_DialogModeSelector?.GetValue() == SetDialogModeAction.DialogMode.Normal)
            {
                if (m_DialogModeSelector.GetValue() == SetDialogModeAction.DialogMode.Normal)
                {
                    m_ProjectListButton.button.interactable = m_Interactable &&
                        m_ActiveProjectSelector.GetValue() != null &&
                        m_ActiveProjectSelector.GetValue() != Project.Empty &&
                        m_LoginStateSelector.GetValue() == LoginState.LoggedIn;
                    m_RefreshButton.transform.parent.gameObject.SetActive(m_RefreshVisibility);
                }

                m_ProjectListButton.selected = true;
            }
            else
            {
                m_ProjectListButton.button.interactable = m_Interactable;
                m_RefreshButton.transform.parent.gameObject.SetActive(false);
                m_ProjectListButton.selected = false;
            }
        }

        void OnActiveProjectChanged(Project newData)
        {
            UpdateButtons();
        }

        void OnProjectListButtonClick()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Projects))
            {
                m_ProjectListButton.selected = true;
                return;
            }

            Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.None));
            var dialogType = m_ActiveDialogSelector.GetValue() == OpenDialogAction.DialogType.LandingScreen ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.LandingScreen;
            Dispatcher.Dispatch(OpenDialogAction.From(dialogType));
            if (dialogType == OpenDialogAction.DialogType.None)
            {
                Dispatcher.Dispatch(CloseAllDialogsAction.From(null));
                Dispatcher.Dispatch(ResetToolBarAction.From(null));
                Dispatcher.Dispatch(ToggleMeasureToolAction.From(ToggleMeasureToolAction.ToggleMeasureToolData.defaultData));
                Dispatcher.Dispatch(ClearStatusAction.From(true));
                Dispatcher.Dispatch(ClearStatusAction.From(false));
            }
        }

        void OnButtonVisibilityChanged(IButtonVisibility data)
        {
            if (data?.type == (int)ButtonType.ProjectList)
            {
                m_ProjectListButton.transform.parent.gameObject.SetActive(data.visible);
            }
            else if (data?.type == (int)ButtonType.Refresh)
            {
                m_RefreshVisibility = data.visible;
                m_RefreshButton.transform.parent.gameObject.SetActive(m_RefreshVisibility &&
                    m_ActiveDialogSelector.GetValue() == OpenDialogAction.DialogType.LandingScreen &&
                    m_DialogModeSelector.GetValue() == SetDialogModeAction.DialogMode.Normal
                    );
            }
        }


        void OnButtonInteractableChanged(IButtonInteractable data)
        {
            if (data?.type == (int)ButtonType.ProjectList)
            {
                m_Interactable = data.interactable;
                UpdateButtons();
            }
        }

        void OnHelpModeEntryChanged(SetHelpModeIDAction.HelpModeEntryID newData)
        {
            if (newData == SetHelpModeIDAction.HelpModeEntryID.None)
            {
                m_ProjectListButton.selected = false;
            }
        }
    }
}
