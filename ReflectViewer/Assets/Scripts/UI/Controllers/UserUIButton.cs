using System;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class UserUIButton : UserUIController
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("User Selection Button")]
        public Button m_Button;
        [SerializeField, Tooltip("Position to display User Info Dialog [Optional]")]
        public Transform m_InfoAnchor;
        [SerializeField, Tooltip("Icon that indicates if the camera is following this user[Optional]")]
        public Image m_IsFollowingIcon;
        [SerializeField, Tooltip("Button background")]
        public Image m_ButtonBackground;
        [SerializeField, Tooltip("State Icons")]
        public GameObject[] m_Icons;
        [SerializeField, Tooltip("State icon mask")]
        public Image m_IconMask;
        [SerializeField, Tooltip("Microphone volume feedback")]
        public Image m_MicVolume;
#pragma warning restore CS0649

        protected virtual void Start()
        {
            m_Button.onClick.AddListener(()=>
            {
                OnUserClick();
            });
        }

        void UpdateBackground()
        {
            Color userColor = m_UserColor;
            if(IsSelected())
            {
                userColor = bubbleColorSelected;
                if (m_Initials != null)
                {
                    m_Initials.color = initialsTextColorAvatarSelected;
                }

                if (m_ButtonBackground != null)
                {
                    m_ButtonBackground.color = UIConfig.propertySelectedColor;
                }
            }
            else
            {
                if (m_ButtonBackground != null)
                {
                    m_ButtonBackground.color = Color.clear;
                }
            }
            if (!ReferenceEquals(m_IsFollowingIcon, null))
            {
                m_IsFollowingIcon.enabled = IsFollowing();
            }
            ColorImages(userColor);
        }

        protected virtual void UpdateIcons()
        {
            var statusIsOn = false;
            for (var index = 0; index < m_Icons.Length; index++)
            {
                switch (index)
                {
                    case 0:
                        m_Icons[index].SetActive(IsMuted());
                        break;
                    case 1:
                        var isSpeaking = IsSpeaking();
                        m_Icons[index].SetActive(isSpeaking);
                        if (isSpeaking && m_MicVolume != null)
                        {
                            var userData = UIStateManager.current.roomConnectionStateData.users.Find(data => data.matchmakerId == MatchmakerId);
                            if (userData != default)
                            {
                                m_MicVolume.fillAmount = userData.voiceStateData.micVolume;
                            }
                        }
                        break;
                }
                statusIsOn |= m_Icons[index].activeSelf;
            }

            if (m_IconMask != null)
            {
                m_IconMask.enabled = statusIsOn;
            }
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();
            UpdateBackground();
            UpdateIcons();
        }

        protected virtual void OnUserClick()
        {
            Vector3 anchorPosition = (ReferenceEquals(m_InfoAnchor, null))? transform.position: m_InfoAnchor.position;
            var dialogType =  DialogType.None;
            if (UIStateManager.current.stateData.activeDialog != DialogType.CollaborationUserInfo ||
                UIStateManager.current.stateData.SelectedUserData.matchmakerId != MatchmakerId)
            {
                dialogType =   DialogType.CollaborationUserInfo;
                var userInfo = UIStateManager.current.stateData.SelectedUserData;
                userInfo.matchmakerId = MatchmakerId;
                userInfo.dialogPosition = anchorPosition;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetUserInfo, userInfo));
            }
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
        }

        protected virtual bool IsSelected()
        {
            return UIStateManager.current.stateData.activeDialog == DialogType.CollaborationUserInfo &&
                UIStateManager.current.stateData.SelectedUserData.matchmakerId == MatchmakerId;
        }

        protected bool IsSpeaking()
        {
            var user = UIStateManager.current.roomConnectionStateData.users.Find(data => data.matchmakerId == MatchmakerId);
            return user.voiceStateData.micVolume > 0;
        }
    }
}
