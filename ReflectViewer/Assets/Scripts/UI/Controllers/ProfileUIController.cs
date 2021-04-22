using System;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible for managing the profile button
    /// </summary>
    public class ProfileUIController : UserUIButton
    {
        UserIdentity m_LocalUserIdentity;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.sessionStateChanged += OnSessionStateDataChanged;
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            UpdateUser(m_LocalUserIdentity.matchmakerId);
        }

        protected override bool IsSelected()
        {
            return UIStateManager.current.stateData.activeSubDialog == DialogType.Account;
        }

        protected override void OnUserClick()
        {
            var dialogType = UIStateManager.current.stateData.activeSubDialog == DialogType.Account ? DialogType.None : DialogType.Account;
            if (UIStateManager.current.stateData.activeDialog != DialogType.LandingScreen)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog,  DialogType.None));
            }
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog,  dialogType));
        }

        void OnSessionStateDataChanged(UISessionStateData data)
        {
            if (m_LocalUserIdentity != data.sessionState.userIdentity)
            {
                m_LocalUserIdentity = data.sessionState.userIdentity;
                switch (data.sessionState.loggedState)
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
                        m_Icons[index].SetActive(IsInPrivateMode());
                        break;
                    case 1:
                        m_Icons[index].SetActive(!IsInPrivateMode() && !IsConnected());
                        break;
                }
            }
        }

        bool IsInPrivateMode()
        {
            return UIStateManager.current.sessionStateData.sessionState.isInPrivateMode;
        }

        bool IsConnected()
        {
            return string.IsNullOrEmpty(UIStateManager.current.projectStateData.activeProject?.projectId)
                || !string.IsNullOrEmpty(UIStateManager.current.sessionStateData.sessionState.userIdentity.matchmakerId);
        }
    }
}
