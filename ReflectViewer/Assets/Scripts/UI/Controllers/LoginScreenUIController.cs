using System;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.Reflect.Utils;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.UI;

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

        LoginState? m_LoginState;
        bool m_IsRegionPopupOpen;
        int m_ClickCount;

        bool m_CloudOtherSelected;

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
            UIStateManager.sessionStateChanged += OnSessionStateChanged;
            UIStateManager.stateChanged += OnStateChanged;

            UpdateSettings();
        }

        void OnStateChanged(UIStateData data)
        {
            m_LoginButton.interactable = !data.VREnable;
        }

        void OnSessionStateChanged(UISessionStateData sessionStateData)
        {
            m_WelcomeText.text = sessionStateData.sessionState.linkShareLoggedOut ?
            "Log in with your Unity account to open the shared project" : "Login with your Unity account to get started";
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
                            $"Environment: {ProjectServerClient.ProjectServerAddress(environmentInfo.provider)}";
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
            var session = UIStateManager.current.sessionStateData;
            switch (session.sessionState.loggedState)
            {
                case LoginState.LoggingIn:
                case LoginState.LoggedOut:
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, "Logging in..."));
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Login, null));
                    break;
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
                m_RegionOptionArrow.transform.eulerAngles = new Vector3(0,0,0);
            else
                m_RegionOptionArrow.transform.eulerAngles = new Vector3(0,0,180);

            m_RegionPopup.SetActive(active);
            m_IsRegionPopupOpen = active;
        }

        void SetActiveCloudPopup(bool active)
        {
            m_CloudPopup.SetActive(active);
            m_LoginButton.interactable = !active && !UIStateManager.current.stateData.VREnable;
            m_RegionButton.interactable = !active;
        }

        void SetRegionOption(RegionUtils.Provider provider)
        {
            EnvironmentInfo environmentInfo = LocaleUtils.GetEnvironmentInfo();
            environmentInfo.provider = provider;
            environmentInfo.cloudEnvironment = CloudEnvironment.Production;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetLoginSetting, environmentInfo));

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
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.DeleteCloudEnvironmentSetting, null));
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

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetLoginSetting, environmentInfo));

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
