using System;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
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

        void Start()
        {
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

            var muted = IsLocallyMuted();
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
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ToggleUserMicrophone, MatchmakerId));
        }

        void ToggleFollowUserTool()
        {
            var uiStateData = UIStateManager.current.stateData;
            var networkUserData = UIStateManager.current.roomConnectionStateData.users.Find(user => user.matchmakerId == MatchmakerId);
            if (uiStateData.navigationState.navigationMode == NavigationMode.VR)
            {
                if (!ReferenceEquals(networkUserData.visualRepresentation, null))
                {
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Teleport, networkUserData.visualRepresentation.transform.position));
                }
            }
            else
            {
                if(UIStateManager.current.walkStateData.walkEnabled)
                    UIStateManager.current.walkStateData.instruction.Cancel();
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.FollowUser, networkUserData));
            }
        }

        public void CloseUserInfoDialog()
        {
            if (UIStateManager.current.stateData.activeDialog == DialogType.CollaborationUserInfo)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
            }
        }
    }
}
