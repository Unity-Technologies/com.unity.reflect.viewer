using System;
using System.Text.RegularExpressions;
using SharpFlux;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible for managing the selection of the type of orbit tool
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class AccountUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Button to toggle the dialog.")]
        Button m_DialogButton;
        [SerializeField]
        Button m_LogoutButton;
        [SerializeField]
        Button m_BIM360Button;
        [SerializeField]
        TextMeshProUGUI m_UserNameText;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;

        Image m_ProfileButtonImage;
        Image m_ProfileButtonIcon;

        TextMeshProUGUI m_ProfileButtonText;

        const string k_Bim360Link = "https://dashboard.reflect.unity3d.com/";

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.sessionStateChanged += OnSessionStateDataChanged;

            m_DialogWindow = GetComponent<DialogWindow>();
            m_ProfileButtonImage = m_DialogButton.GetComponent<Image>();
            m_ProfileButtonIcon = m_DialogButton.transform.GetChild(0).GetComponent<Image>();
            m_ProfileButtonText = m_DialogButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
#if UNITY_EDITOR
            m_LogoutButton.interactable = false;
#endif
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (stateData.activeDialog == DialogType.Account)
            {
                m_ProfileButtonImage.color = UIConfig.propertySelectedColor;
            }
            else
            {
                if (UIStateManager.current.sessionStateData.sessionState.loggedState == LoginState.LoggedIn)
                {
                    m_ProfileButtonImage.color = UIConfig.propertyPressedColor;
                }
                else
                {
                    m_ProfileButtonImage.color = UIConfig.propertyBaseColor;
                }
            }
        }

        void OnSessionStateDataChanged(UISessionStateData data)
        {
            switch (data.sessionState.loggedState)
            {
                case LoginState.LoggedIn:
                    m_ProfileButtonImage.enabled = true;
                    m_ProfileButtonIcon.enabled = false;
                    m_ProfileButtonText.enabled = true;
                    m_ProfileButtonText.text = GetInitials(data.sessionState.user.DisplayName);
                    m_UserNameText.text = data.sessionState.user.DisplayName;
                    break;
                case LoginState.LoggedOut:
                    m_ProfileButtonImage.enabled = false;
                    m_ProfileButtonIcon.enabled = true;
                    m_ProfileButtonText.enabled = false;
                    m_ProfileButtonText.text = String.Empty;
                    m_UserNameText.text = String.Empty;
                    break;
                case LoginState.LoggingIn:
                case LoginState.LoggingOut:
                    // todo put spinner
                    break;
            }
        }

        void Start()
        {
            m_DialogButton.onClick.AddListener(OnDialogButtonClick);
            m_LogoutButton.onClick.AddListener(OnLogoutButtonClick);
            m_BIM360Button.onClick.AddListener(OnBIM360ButtonClick);
        }

        void OnBIM360ButtonClick()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenURL, k_Bim360Link));
        }

        void OnLogoutButtonClick()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatus, "Logging out..."));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Logout, null));
        }

        void OnDialogButtonClick()
        {
            var session = UIStateManager.current.sessionStateData;
            switch (session.sessionState.loggedState)
            {
                case LoginState.LoggedIn:
                    var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.Account;
                    UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, dialogType));
                    break;
                case LoginState.LoggingIn:
                case LoginState.LoggedOut:
#if !UNITY_EDITOR
                    UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatus, "Logging in..."));
                    UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Login, null));
#endif
                    break;
                case LoginState.LoggingOut:
                    break;
            }
        }

        public static string GetInitials(string fullName)
        {
            // first remove all: punctuation, separator chars, control chars, and numbers (unicode style regexes)
            string initials = Regex.Replace(fullName, @"[\p{P}\p{S}\p{C}\p{N}]+", "");

            // Replacing all possible whitespace/separator characters (unicode style), with a single, regular ascii space.
            initials = Regex.Replace(initials, @"\p{Z}+", " ");

            // Remove all Sr, Jr, I, II, III, IV, V, VI, VII, VIII, IX at the end of names
            initials = Regex.Replace(initials.Trim(), @"\s+(?:[JS]R|I{1,3}|I[VX]|VI{0,3})$", "", RegexOptions.IgnoreCase);

            // Extract up to 2 initials from the remaining cleaned name.
            initials = Regex.Replace(initials, @"^(\p{L})[^\s]*(?:\s+(?:\p{L}+\s+(?=\p{L}))?(?:(\p{L})\p{L}*)?)?$", "$1$2").Trim();

            if (initials.Length > 2)
            {
                // Worst case scenario, everything failed, just grab the first two letters of what we have left.
                initials = initials.Substring(0, 2);
            }

            return initials.ToUpperInvariant();
        }
    }
}
