using System;
using System.Text.RegularExpressions;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Utils;
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
        SlideToggle m_PrivateModeButton;
        [SerializeField]
        TextMeshProUGUI m_UserNameText;
#pragma warning restore CS0649

        const string k_Bim360Link = "https://dashboard.reflect.unity3d.com/";

        void Awake()
        {
            UIStateManager.sessionStateChanged += OnSessionStateDataChanged;
            UIStateManager.stateChanged += OnStateChanged;
        }

        void OnStateChanged(UIStateData data)
        {
            m_LogoutButton.interactable = !data.VREnable;
        }

        void Start()
        {
            m_LogoutButton.onClick.AddListener(OnLogoutButtonClick);
            m_PrivateModeButton.onValueChanged.AddListener(OnPrivateModeToggle);
        }

        void OnSessionStateDataChanged(UISessionStateData data)
        {
            switch (data.sessionState.loggedState)
            {
                case LoginState.LoggedIn:
                    m_UserNameText.text = data.sessionState.user.DisplayName;
                    break;
                case LoginState.LoggedOut:
                    m_UserNameText.text = String.Empty;
                    break;
            }
        }

        private void OnPrivateModeToggle(bool isPrivate)
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPrivateMode, isPrivate));
        }

        void OnBIM360ButtonClick()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenURL, k_Bim360Link));
        }

        void OnLogoutButtonClick()
        {
            var session = UIStateManager.current.sessionStateData;
            if (session.sessionState.loggedState == LoginState.LoggedIn)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, "Logging out..."));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Logout, null));
            }
        }
    }
}
