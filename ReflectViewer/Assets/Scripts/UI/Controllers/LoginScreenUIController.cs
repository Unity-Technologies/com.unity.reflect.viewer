using System;
using System.Linq;
using SharpFlux.Dispatching;
using TMPro;
using Unity.Reflect.Utils;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.UI;
using Unity.Reflect.Runtime;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class LoginScreenUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        TextMeshProUGUI m_RegionText;
        [SerializeField]
        TextMeshProUGUI m_WelcomeText;
        [SerializeField]
        TextMeshProUGUI m_HeaderText;
        [SerializeField]
        Button m_LoginButton;
        [SerializeField]
        Button m_RegionButton;
        [SerializeField]
        GameObject m_RegionOptionArrow;
        [SerializeField]
        Button m_HiddenButton;
        [SerializeField]
        Button m_WelcomeButton;

        [SerializeField]
        GameObject m_RegionPopup;
        [SerializeField]
        GameObject m_CloudPopup;

        [SerializeField]
        OptionItemButton[] m_RegionButtons;
        [SerializeField]
        OptionItemButton[] m_CloudButtons;

        [SerializeField]
        TMP_InputField m_CloudURLInput;

        [SerializeField]
        Button m_BackgroundButton;
        [SerializeField]
        Button m_CloudCancelButton;
        [SerializeField]
        Button m_CloudOKButton;

        [SerializeField]
        TextMeshProUGUI m_CloudSettingDebugInfo;

#pragma warning restore CS0649

        bool m_IsRegionPopupOpen;
        int m_ClickCount;

        bool m_CloudOtherSelected;
        IUISelector<bool> m_VREnableSelector;
        IUISelector<LoginState> m_LoggedStateSelector;
        IUISelector<bool> m_LinkShareLoggedOutSelector;

        void Awake()
        {
            m_BackgroundButton.onClick.AddListener(OnBackgroundButtonClicked);
            m_LoginButton.onClick.AddListener(OnLoginButtonClicked);
            m_RegionButton.onClick.AddListener(OnRegionButtonClicked);
            m_HiddenButton.onClick.AddListener(OnHiddenButtonClicked);
            m_WelcomeButton.onClick.AddListener(OnWelcomeButtonClicked);

            m_CloudURLInput.onValueChanged.AddListener(OnCustomURLChanged);

            foreach (var button in m_RegionButtons)
            {
                button.regionButtonClicked += OnRegionOptionButtonClicked;
            }

            foreach (var button in m_CloudButtons)
            {
                button.cloudButtonClicked += OnCloudOptionButtonClicked;
            }

            m_CloudOKButton.onClick.AddListener(OnCloudOKButtonClicked);
            m_CloudCancelButton.onClick.AddListener(OnCloudCancelButtonClicked);

            UIStateManager.loginSettingChanged += OnLoginSettingChanged;

            m_VREnableSelector = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable), OnVREnableChanged);
            m_LoggedStateSelector = UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState), OnLoggedStateChanged);
            m_LinkShareLoggedOutSelector = UISelectorFactory.createSelector<bool>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.linkShareLoggedOut));

            UpdateSettings();

            // Init to LoginState.LoginSessionFromCache state.
            // If no previous session is found, LoginState.LoggedOut will be dispatched and UI will get unlocked.
            OnLoginSessionFromCache();
        }

        void OnDestroy()
        {
            m_VREnableSelector?.Dispose();
            m_LoggedStateSelector?.Dispose();
            m_LinkShareLoggedOutSelector?.Dispose();
        }

        void OnVREnableChanged(bool newData)
        {
            m_LoginButton.interactable = !newData;
        }

        void OnLoginSessionFromCache()
        {
            m_LoginButton.gameObject.SetActive(false);
            m_LoginButton.enabled = false;
            m_HeaderText.text = "Welcome to Reflect";
            m_WelcomeText.text = "Looking for previous user session...";
        }

        void OnIncompleteLoginFlow()
        {
            Debug.Log($"Login flow incomplete, user can now try again.");
            m_LoginButton.gameObject.SetActive(true);
            m_LoginButton.enabled = true;
            m_LoginButton.GetComponentInChildren<TextMeshProUGUI>().text = "Try again";
            m_HeaderText.text = "Awaiting browser login response";
            m_WelcomeText.text = "Please complete the sign-in form in your browser.";
        }

        void OnLoggedStateChanged(LoginState sessionStateData)
        {
            switch (sessionStateData)
            {
                case LoginState.LoginSessionFromCache:
                    OnLoginSessionFromCache();
                    break;
                case LoginState.LoggedIn:
                    m_LoginButton.gameObject.SetActive(false);
                    m_LoginButton.enabled = false;
                    break;
                case LoginState.LoggingIn:
#if UNITY_IOS
                    m_LoginButton.gameObject.SetActive(true);
                    m_LoginButton.enabled = true;
                    m_LoginButton.GetComponentInChildren<TextMeshProUGUI>().text = "Try again";
                    m_WelcomeText.text = "Please complete the sign-in form in your browser.";
#else
                    m_LoginButton.gameObject.SetActive(false);
                    m_LoginButton.enabled = false;
                    m_WelcomeText.text = "";
#endif
                    m_HeaderText.text = "Awaiting browser login response";
                    break;
                case LoginState.ProcessingToken:
                    m_LoginButton.gameObject.SetActive(false);
                    m_LoginButton.enabled = false;
                    m_HeaderText.text = "Fetching user information...";
                    m_WelcomeText.text = "";
                    break;
                case LoginState.LoggedOut:
                    m_LoginButton.gameObject.SetActive(true);
                    m_LoginButton.enabled = true;
                    m_LoginButton.GetComponentInChildren<TextMeshProUGUI>().text = "Login";
                    m_HeaderText.text = "Welcome to Reflect";
                    m_WelcomeText.text = m_LinkShareLoggedOutSelector.GetValue() ? "Log in with your Unity account to open the shared project" : "Login with your Unity account to get started";
                    break;
            }
        }

        void UpdateSettings()
        {
            var environmentInfo = LocaleUtils.GetEnvironmentInfo();

            RegionOption regionOption = RegionOption.None;
            switch (environmentInfo.provider)
            {
                case RegionUtils.Provider.GCP:
                    regionOption = RegionOption.Default;
                    break;
                case RegionUtils.Provider.Tencent:
                    regionOption = RegionOption.China;
                    break;
            }

            OptionItemButton selectedButton = null;
            foreach (var button in m_RegionButtons)
            {
                button.SelectButton(button.regionOption == regionOption);
                if (button.regionOption == regionOption)
                    selectedButton = button;
            }

            if (selectedButton != null)
                m_RegionText.text = $"Region: {selectedButton.label.text}";

            CloudOption cloudOption = CloudOption.Default;
            if (PlayerPrefs.HasKey(LocaleUtils.SettingsKeys.CloudEnvironment))
            {
                switch (environmentInfo.cloudEnvironment)
                {
                    case CloudEnvironment.Other:
                        cloudOption = CloudOption.Other;
                        break;
                    case CloudEnvironment.Local:
                        cloudOption = CloudOption.Local;
                        break;
                    case CloudEnvironment.Test:
                        cloudOption = CloudOption.Test;
                        break;
                    case CloudEnvironment.Staging:
                        cloudOption = CloudOption.Staging;
                        break;
                    case CloudEnvironment.Production:
                        cloudOption = CloudOption.Production;
                        break;
                }
            }

            m_CloudOtherSelected = cloudOption == CloudOption.Other;
            EnableCustomInput(m_CloudOtherSelected);
            m_CloudURLInput.text = environmentInfo.customUrl;

            foreach (var button in m_CloudButtons)
            {
                button.SelectButton(button.cloudOption == cloudOption);
            }

            if (environmentInfo.cloudEnvironment != CloudEnvironment.Production)
            {
                m_CloudSettingDebugInfo.gameObject.SetActive(true);

                if (environmentInfo.cloudEnvironment == CloudEnvironment.Other)
                {
                    if (PlayerPrefs.HasKey(LocaleUtils.SettingsKeys.CloudEnvironment))
                        m_CloudSettingDebugInfo.text = $"Environment: {environmentInfo.customUrl}";
                    else
                        m_CloudSettingDebugInfo.text =
                            $"Environment: {ProjectServerClient.ProjectServerAddress(environmentInfo.provider, Protocol.Http)}";
                }
                else
                {
                    m_CloudSettingDebugInfo.text = $"Environment: {environmentInfo.cloudEnvironment}";
                }
            }
            else
            {
                m_CloudSettingDebugInfo.gameObject.SetActive(false);
            }
        }

        void OnLoginButtonClicked()
        {
            switch (m_LoggedStateSelector.GetValue())
            {
                case LoginState.LoggingIn:
                case LoginState.LoggedOut:
                    Dispatcher.Dispatch(SetStatusMessage.From("Logging in..."));
                    Dispatcher.Dispatch(SetLoginAction.From(LoginState.LoggingIn));
                    break;
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (SessionStateContext<UnityUser, LinkPermission>.current != null)
            {
                if (hasFocus)
                {
                    switch (m_LoggedStateSelector.GetValue())
                    {
                        case LoginState.LoggingIn:
                            // User refocus the viewer before browser login was completed.
                            OnIncompleteLoginFlow();
                            break;
                        case LoginState.ProcessingToken:
                            // OS level refocus after user completed sign in.
                            Debug.Log($"Sign in completed. Now processing token...");
                            break;
                    }
                }
            }
        }

        void OnRegionButtonClicked()
        {
            m_ClickCount = 0;
            SetActiveRegionPopup(!m_IsRegionPopupOpen);
        }

        void SetActiveRegionPopup(bool active)
        {
            if (active)
                m_RegionOptionArrow.transform.eulerAngles = new Vector3(0, 0, 0);
            else
                m_RegionOptionArrow.transform.eulerAngles = new Vector3(0, 0, 180);

            m_RegionPopup.SetActive(active);
            m_IsRegionPopupOpen = active;
        }

        void SetActiveCloudPopup(bool active)
        {
            m_CloudPopup.SetActive(active);
            m_LoginButton.interactable = !active && !m_VREnableSelector.GetValue();
            m_RegionButton.interactable = !active;
        }

        void SetRegionOption(RegionUtils.Provider provider)
        {
            EnvironmentInfo environmentInfo = LocaleUtils.GetEnvironmentInfo();
            environmentInfo.provider = provider;
            environmentInfo.cloudEnvironment = CloudEnvironment.Production;
            Dispatcher.Dispatch(SetLoginSettingActions<EnvironmentInfo>.From(environmentInfo));

            SetActiveRegionPopup(false);
        }

        void OnCloudOKButtonClicked()
        {
            SetActiveCloudPopup(false);

            var optionItemButton = m_CloudButtons.FirstOrDefault(e => e.selected);
            if (optionItemButton == null)
                return;

            // if set to Default, remove player pref
            if (optionItemButton.cloudOption == CloudOption.Default)
            {
                Dispatcher.Dispatch(DeleteCloudEnvironmentSetting<EnvironmentInfo>.From(true));
                Dispatcher.Dispatch(DeleteCloudEnvironmentSetting<EnvironmentInfo>.From(false));
                return;
            }

            EnvironmentInfo environmentInfo = LocaleUtils.GetEnvironmentInfo();
            environmentInfo.cloudEnvironment = CloudEnvironment.Production;

            switch (optionItemButton.cloudOption)
            {
                case CloudOption.Production:
                    environmentInfo.cloudEnvironment = CloudEnvironment.Production;
                    break;
                case CloudOption.Local:
                    environmentInfo.cloudEnvironment = CloudEnvironment.Local;
                    break;
                case CloudOption.Staging:
                    environmentInfo.cloudEnvironment = CloudEnvironment.Staging;
                    break;
                case CloudOption.Test:
                    environmentInfo.cloudEnvironment = CloudEnvironment.Test;
                    break;
                case CloudOption.Other:
                    environmentInfo.cloudEnvironment = CloudEnvironment.Other;
                    break;
            }

            environmentInfo.customUrl = m_CloudURLInput.text;

            Dispatcher.Dispatch(SetLoginSettingActions<EnvironmentInfo>.From(environmentInfo));
        }

        void OnCloudCancelButtonClicked()
        {
            SetActiveCloudPopup(false);
        }

        void OnRegionOptionButtonClicked(RegionOption regionOption)
        {
            if (regionOption == RegionOption.China)
            {
                SetRegionOption(RegionUtils.Provider.Tencent);
            }
            else if (regionOption == RegionOption.Default)
            {
                SetRegionOption(RegionUtils.Provider.GCP);
            }
        }

        void OnCloudOptionButtonClicked(CloudOption cloudOption)
        {
            m_CloudOtherSelected = cloudOption == CloudOption.Other;
            EnableCustomInput(m_CloudOtherSelected);

            CheckCustomUrl(m_CloudURLInput.text);
            foreach (var button in m_CloudButtons)
            {
                button.SelectButton(button.cloudOption == cloudOption);
            }
        }

        void EnableCustomInput(bool enable)
        {
            m_CloudURLInput.interactable = enable;
            m_CloudURLInput.textComponent.color = enable ? Color.white : Color.gray;
        }

        void OnBackgroundButtonClicked()
        {
            m_ClickCount = 0;
            SetActiveCloudPopup(false);
            SetActiveRegionPopup(false);
        }

        // For debug purpose, tap region label more than 5times and tap the welcome image, then we open Cloud setting popup.
        void OnHiddenButtonClicked()
        {
            m_ClickCount++;
        }

        void OnWelcomeButtonClicked()
        {
            if (m_ClickCount > 4)
            {
                SetActiveCloudPopup(true);
            }

            m_ClickCount = 0;
        }

        void OnLoginSettingChanged()
        {
            UpdateSettings();
        }

        void OnCustomURLChanged(string url)
        {
            CheckCustomUrl(url);
        }

        void CheckCustomUrl(string url)
        {
            if (m_CloudOtherSelected && !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
            {
                m_CloudOKButton.interactable = false;
            }
            else
            {
                m_CloudOKButton.interactable = true;
            }
        }
    }
}
