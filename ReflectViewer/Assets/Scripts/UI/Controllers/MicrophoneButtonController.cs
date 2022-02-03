using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.Core.Actions;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class MicrophoneButtonController : MonoBehaviour
    {
        [SerializeField, Tooltip("Microphone On Image [Optional]")]
        public GameObject m_MicToggleOnImage = null;
        [SerializeField, Tooltip("Microphone Off Image [Optional]")]
        public GameObject m_MicToggleOffImage = null;
        [SerializeField, Tooltip("Microphone volume [Optional]")]
        public Image m_MicLevel = null;

        Button m_Button;
        IUISelector<NetworkUserData> m_LocalUserGetter;
        bool m_Interactable;
        IUISelector<bool> m_ToolBarEnabledGetter;
        IUISelector<bool> m_IsPrivateModeGetter;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_Button = GetComponent<Button>();
            m_DisposeOnDestroy.Add(m_LocalUserGetter = UISelectorFactory.createSelector<NetworkUserData>(RoomConnectionContext.current, nameof(IRoomConnectionDataProvider<NetworkUserData>.localUser), OnLocalUserChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonVisibility>(AppBarContext.current, nameof(IAppBarDataProvider.buttonVisibility), OnButtonVisibilityChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonInteractable>(AppBarContext.current, nameof(IAppBarDataProvider.buttonInteractable), OnButtonInteractableChanged));
            m_Interactable = true;
            m_DisposeOnDestroy.Add(m_ToolBarEnabledGetter = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled),
                b =>
                {
                    UpdateButtonInteractable();
                }));
            m_DisposeOnDestroy.Add(m_IsPrivateModeGetter = UISelectorFactory.createSelector<bool>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.isInPrivateMode),
                b =>
                {
                    UpdateButtonInteractable();
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<NetworkReachability>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.networkReachability), NetworkReachabilityChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.projectServerConnection), ProjectServerConnectionChanged));
        }

        void Start()
        {
            m_Button.onClick.AddListener(OnButtonClick);
            HasPermission();
        }

        bool HasPermission()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
                return false;
            }
#endif
            return true;
        }

        void OnLocalUserChanged(NetworkUserData localUser)
        {
            var voiceData = localUser.voiceStateData;
            var muted = voiceData.isServerMuted;
            m_MicToggleOnImage.SetActive(!muted);
            m_MicToggleOffImage.SetActive(muted);
            if (!muted && m_MicLevel != null)
            {
                m_MicLevel.fillAmount = voiceData.micVolume;
            }

        }

        void OnButtonClick()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Microphone))
                return;

            if (HasPermission())
            {
                var matchmakerId = m_LocalUserGetter.GetValue().matchmakerId;
                Dispatcher.Dispatch(ToggleMicrophoneAction.From(matchmakerId));
                Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"MicrophoneMuteToggle_{m_MicToggleOffImage.activeSelf}"));
            }
        }

        void OnButtonVisibilityChanged(IButtonVisibility data)
        {
            if (data?.type == (int)ButtonType.Audio)
            {
                m_Button.transform.parent.gameObject.SetActive(data.visible);
            }
        }

        void OnButtonInteractableChanged(IButtonInteractable data)
        {
            if (data?.type == (int)ButtonType.Audio)
            {
                m_Interactable = data.interactable;
                UpdateButtonInteractable();
            }
        }


        void UpdateButtonInteractable()
        {
            m_Button.interactable = (m_ToolBarEnabledGetter != null && m_IsPrivateModeGetter != null)
                                    && m_ToolBarEnabledGetter.GetValue() && m_Interactable
                                    && UIStateManager.current.IsNetworkConnected && !m_IsPrivateModeGetter.GetValue();
        }

        void ProjectServerConnectionChanged(bool _)
        {
            UpdateButtonInteractable();
        }

        void NetworkReachabilityChanged(NetworkReachability _)
        {
            UpdateButtonInteractable();
        }
    }
}
