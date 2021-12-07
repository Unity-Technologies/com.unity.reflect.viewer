using System;
using System.Linq;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible for managing the Account Dialog
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class AccountUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_LogoutButton;
        [SerializeField]
        Button m_LoginButton;
        [SerializeField]
        SlideToggle m_PrivateModeButton;
        [SerializeField]
        TextMeshProUGUI m_UserNameText;
        [SerializeField]
        GameObject m_ToggleGreyOut;
#pragma warning restore CS0649
        IUISelector<LoginState> m_LoginStateSelector;
        IUISelector<UnityUser> m_UserSelector;

        bool m_PrivateModeInteractable;
        bool m_LogOutInteractable;
        IUISelector<NetworkReachability> m_NetworkReachabilitySelector;
        IUISelector<bool> m_ProjectServerConnectionSelector;
        IUISelector<AccessToken> m_AccessTokenSelector;

        const string k_Bim360Link = "https://dashboard.reflect.unity3d.com/";

        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        UnityProject.AccessType[] m_UserPermissions = { };

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_PrivateModeInteractable = true;
            m_LogOutInteractable = true;
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable), OnVREnableChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonInteractable>(AppBarContext.current, nameof(IAppBarDataProvider.buttonInteractable), OnButtonInteractableChanged));
            m_DisposeOnDestroy.Add(m_LoginStateSelector = UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState), OnLoggedStateDataChanged));
            m_DisposeOnDestroy.Add(m_UserSelector = UISelectorFactory.createSelector<UnityUser>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.user)));

            m_DisposeOnDestroy.Add(m_NetworkReachabilitySelector = UISelectorFactory.createSelector<NetworkReachability>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.networkReachability), NetworkReachabilityChanged));
            m_DisposeOnDestroy.Add(m_ProjectServerConnectionSelector = UISelectorFactory.createSelector<bool>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.projectServerConnection), ProjectServerConnectionChanged));
            m_DisposeOnDestroy.Add(m_AccessTokenSelector = UISelectorFactory.createSelector<AccessToken>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.accessToken)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), OnActiveProjectChanged));
        }

        void ProjectServerConnectionChanged(bool _)
        {
            UpdateProfile();
        }

        void NetworkReachabilityChanged(NetworkReachability _)
        {
            UpdateProfile();
        }

        void OnVREnableChanged(bool newData)
        {
            m_LogoutButton.interactable = !newData;
            m_LoginButton.interactable = !newData;
        }

        void OnButtonInteractableChanged(IButtonInteractable data)
        {
            switch (data?.type)
            {
                case (int)ButtonType.PrivateMode:
                    m_PrivateModeInteractable = data.interactable;
                    UpdateToggleInteractable();
                    break;
                case (int)ButtonType.LogOff:
                    m_LogOutInteractable = data.interactable;
                    m_LogoutButton.interactable = data.interactable;
                    break;
            }
        }

        void Start()
        {
            m_LogoutButton.onClick.AddListener(OnLogoutButtonClick);
            m_LoginButton.onClick.AddListener(OnLoginButtonClick);
            m_PrivateModeButton.onValueChanged.AddListener(OnPrivateModeToggle);
        }

        void OnLoggedStateDataChanged(LoginState data)
        {
            UpdateProfile();
            m_LogoutButton.gameObject.SetActive(data == LoginState.LoggedIn);
            m_LoginButton.gameObject.SetActive(data != LoginState.LoggedIn);
        }

        void UpdateProfile()
        {
            var user = m_UserSelector?.GetValue();
            if (user != null)
            {
                m_UserNameText.text = user.DisplayName;
                var accessToken = user.AccessToken;
                m_LogoutButton.gameObject.SetActive(m_LogOutInteractable &&
                    !string.IsNullOrWhiteSpace(accessToken));
            }

            UpdateToggleInteractable();
        }

        void OnPrivateModeToggle(bool isPrivate)
        {
            Dispatcher.Dispatch(SetPrivateModeAction.From(isPrivate));
        }

        void OnBIM360ButtonClick()
        {
            Dispatcher.Dispatch(OpenURLActions<Project>.From(k_Bim360Link));
        }

        void OnLogoutButtonClick()
        {
            if (m_LoginStateSelector.GetValue() == LoginState.LoggedIn)
            {
                Dispatcher.Dispatch(SetStatusMessage.From("Logging out..."));
                Dispatcher.Dispatch(CloseAllDialogsAction.From(null));
                Dispatcher.Dispatch(SetWalkEnableAction.From(false));
                Dispatcher.Dispatch(SetLoginAction.From(LoginState.LoggingOut));
                Dispatcher.Dispatch(SetLoginAction.From(LoginState.LoggedOut));
            }
        }

        void OnLoginButtonClick()
        {
            if (m_LoginStateSelector.GetValue() == LoginState.LoggedOut)
            {
                Dispatcher.Dispatch(SetStatusMessage.From("Logging in..."));
                Dispatcher.Dispatch(SetLoginAction.From(LoginState.LoggingIn));
            }
        }

        void SetToggleInteractable(bool interactable)
        {
            m_PrivateModeButton.isInteractable = interactable;
            m_ToggleGreyOut.SetActive(!interactable);
        }

        void UpdateToggleInteractable()
        {
            bool interactable = m_PrivateModeInteractable &&
                (m_UserPermissions.Contains(UnityProject.AccessType.GoOffline) &&
                UIStateManager.current.IsNetworkConnected || m_PrivateModeButton.on);
            SetToggleInteractable(interactable);
        }

        void OnActiveProjectChanged(Project project)
        {
            m_UserPermissions = project?.UnityProject?.AccessSet != null ? project.UnityProject.AccessSet.ToArray() : new UnityProject.AccessType[] { };
            UpdateToggleInteractable();
        }
    }
}
