using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible for managing the profile button
    /// </summary>
    public class ProfileUIController : UserUIButton
    {
        UserIdentity m_LocalUserIdentity;
        IUISelector<Project> m_ActiveProjectGetter;
        IUISelector<OpenDialogAction.DialogType> m_ActiveSubDialogGetter;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogGetter;
        IUISelector<bool> m_IsPrivateModeGetter;
        IUISelector<IUserIdentity> m_UserIdentityGetter;
        IUISelector<LoginState> m_LoggedStateGetter;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        protected override void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
            base.OnDestroy();
        }

        public override void Awake()
        {
            base.Awake();

            UIStateContext.current.stateChanged += OnStateDataChanged;

            m_DisposeOnDestroy.Add(m_ActiveProjectGetter = UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject)));

			m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonVisibility>(AppBarContext.current, nameof(IAppBarDataProvider.buttonVisibility), OnButtonVisibilityChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonInteractable>(AppBarContext.current, nameof(IAppBarDataProvider.buttonInteractable), OnButtonInteractableChanged));

            m_DisposeOnDestroy.Add(m_ActiveSubDialogGetter = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog)));
            m_DisposeOnDestroy.Add(m_ActiveDialogGetter = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog)));

            m_DisposeOnDestroy.Add(m_IsPrivateModeGetter = UISelectorFactory.createSelector<bool>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.isInPrivateMode)));
            m_DisposeOnDestroy.Add(m_UserIdentityGetter = UISelectorFactory.createSelector<IUserIdentity>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.userIdentity), OnSessionStateDataChanged));
            m_DisposeOnDestroy.Add(m_LoggedStateGetter = UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState)));

        }

        protected override void Start()
        {
            base.Start();
        }

        void OnStateDataChanged()
        {
            UpdateUser(m_LocalUserIdentity.matchmakerId);
        }

        protected override bool IsSelected()
        {
            return m_ActiveSubDialogGetter.GetValue() == OpenDialogAction.DialogType.Account;
        }

        protected override void OnUserClick()
        {
            var dialogType = m_ActiveSubDialogGetter.GetValue() == OpenDialogAction.DialogType.Account ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.Account;
            if (m_ActiveDialogGetter.GetValue() != OpenDialogAction.DialogType.LandingScreen)
            {
                Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
            }

            Dispatcher.Dispatch(OpenSubDialogAction.From(dialogType));
        }

        void OnSessionStateDataChanged(IUserIdentity data)
        {
            if (data != null && m_LocalUserIdentity != (UserIdentity)data)
            {
                m_LocalUserIdentity = (UserIdentity)data;
                switch (m_LoggedStateGetter.GetValue())
                {
                    case LoginState.LoggedIn:
                        UpdateUser(m_LocalUserIdentity.matchmakerId, true);
                        break;
                    case LoginState.LoggedOut:
                        Clear();
                        break;
                    case LoginState.LoggingIn:
                    case LoginState.LoggingOut:
                        break;
                }
            }
        }

        protected override void UpdateIcons()
        {
            for (var index = 0; index < m_Icons.Length; index++)
            {
                switch (index)
                {
                    case 0:
                        m_Icons[index].SetActive(m_IsPrivateModeGetter.GetValue());
                        break;
                    case 1:
                        m_Icons[index].SetActive(!m_IsPrivateModeGetter.GetValue() && !IsConnected());
                        break;
                }
            }
        }

        bool IsConnected()
        {
            return string.IsNullOrEmpty(m_ActiveProjectGetter.GetValue()?.projectId)
                || !string.IsNullOrEmpty(((UserIdentity)m_UserIdentityGetter.GetValue()).matchmakerId);
        }

        void OnButtonVisibilityChanged(IButtonVisibility data)
        {
            if (data?.type == (int)ButtonType.Profile)
            {
                m_Button.transform.parent.gameObject.SetActive(data.visible);
            }
        }

        void OnButtonInteractableChanged(IButtonInteractable data)
        {
            if (data?.type == (int)ButtonType.Profile)
            {
                m_Button.interactable = data.interactable;
            }
        }
    }
}
