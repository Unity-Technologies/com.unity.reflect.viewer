using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.Core.Actions;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class UserDetailsUIController : UserUIController
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Follow camera Button [Optional]")]
        public Button m_FollowCameraButton = null;
        [SerializeField, Tooltip("Microphone Toggle [Optional]")]
        public Button m_MicToggleButton = null;
        [SerializeField, Tooltip("Microphone On Icon [Optional]")]
        public GameObject m_MicOnIcon = null;
        [SerializeField, Tooltip("Microphone Off Icon [Optional]")]
        public GameObject m_MicOffIcon = null;
        [SerializeField, Tooltip("User's Name [Optional]")]
        public TMPro.TMP_Text m_FullName = null;
#pragma warning restore CS0649

        bool m_CachedMicEnabled;
        protected IUISelector<SetNavigationModeAction.NavigationMode> m_NavigationModeSelector;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        protected override void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
            base.OnDestroy();
        }

        public override void Awake()
        {
            base.Awake();
            m_DisposeOnDestroy.Add(m_NavigationModeSelector = UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode)));
            m_DisposeOnDestroy.Add(m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog)));
        }

        void Start()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonInteractable>(AppBarContext.current, nameof(IAppBarDataProvider.buttonInteractable), OnButtonInteractableChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonVisibility>(AppBarContext.current, nameof(IAppBarDataProvider.buttonVisibility), OnButtonVisibilityChanged));

            if(m_FollowCameraButton != null)
            {
                m_FollowCameraButton.onClick.AddListener(()=>
                {
                    ToggleFollowUserTool();
                    CloseUserInfoDialog();
                });
            }
            if(m_MicToggleButton != null)
            {
                m_MicToggleButton.onClick.AddListener(()=>
                {
                    ToggleMicrophone();
                    CloseUserInfoDialog();
                });
            }
        }

        protected override void UpdateUser(UserIdentity identity)
        {
            base.UpdateUser(identity);
            if (!ReferenceEquals(m_FullName, null))
            {
                m_FullName.text = identity.fullName;
            }
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();

            var muted = IsMuted();
            if (m_MicOnIcon != null)
            {
                m_MicOnIcon.SetActive(!muted);
            }
            if (m_MicOffIcon != null)
            {
                m_MicOffIcon.SetActive(muted);
            }
        }

        void ToggleMicrophone()
        {
            Dispatcher.Dispatch(ToggleMicrophoneAction.From(MatchmakerId));
        }

        void ToggleFollowUserTool()
        {
            var networkUserData = m_UsersSelector.GetValue().Find(user => user.matchmakerId == MatchmakerId);
            if (m_NavigationModeSelector.GetValue() == SetNavigationModeAction.NavigationMode.VR)
            {
                if (!ReferenceEquals(networkUserData.visualRepresentation, null))
                {
                    Dispatcher.Dispatch(TeleportAction.From(networkUserData.visualRepresentation.transform.position));
                }
            }
            else
            {
                Dispatcher.Dispatch(SetWalkEnableAction.From(false));

                var followUserData = new FollowUserAction.FollowUserData();
                followUserData.matchmakerId = networkUserData.matchmakerId;
                followUserData.visualRepresentationGameObject = networkUserData.visualRepresentation.gameObject;
                Dispatcher.Dispatch(FollowUserAction.From(followUserData));
                Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"FollowUser"));
            }
        }

        public void CloseUserInfoDialog()
        {
            if (m_ActiveDialogSelector.GetValue() == OpenDialogAction.DialogType.CollaborationUserInfo)
            {
                Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
            }
        }

        void OnButtonVisibilityChanged(IButtonVisibility data)
        {
            if (data != null && (ButtonType)data.type == ButtonType.Follow)
            {
                m_FollowCameraButton.gameObject.SetActive(data.visible);
            }
        }

        void OnButtonInteractableChanged(IButtonInteractable data)
        {
            if (data != null && (ButtonType)data.type == ButtonType.Follow)
            {
                m_FollowCameraButton.interactable = data.interactable;
            }
        }
    }
}
