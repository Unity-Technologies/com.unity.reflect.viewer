using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
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

        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;
        IUISelector<IUserInfoDialogDataProvider> m_SelectedUserSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        protected override void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
            base.OnDestroy();
        }

        public override void Awake()
        {
            base.Awake();
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(FollowUserContext.current, nameof(IFollowUserDataProvider.userId), OnFollow));
            m_DisposeOnDestroy.Add(m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog)));
            m_DisposeOnDestroy.Add(m_SelectedUserSelector = UISelectorFactory.createSelector<IUserInfoDialogDataProvider>(UIStateContext.current, nameof(IUIStateDataProvider.SelectedUserData)));
        }

        void OnFollow(string newData)
        {
            if (!ReferenceEquals(m_IsFollowingIcon, null))
            {
                m_IsFollowingIcon.enabled = IsFollowing();
            }
        }

        protected virtual void Start()
        {
            m_Button.onClick.AddListener(() =>
            {
                OnUserClick();
            });
        }

        void UpdateBackground()
        {
            Color userColor = m_UserColor;
            if (IsSelected())
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
                        m_Icons[1].SetActive(!IsMuted());
                        break;
                    case 1:
                        var isSpeaking = IsSpeaking();
                        if (isSpeaking && m_MicVolume != null)
                        {
                            var userData = m_UsersSelector.GetValue().Find(data => data.matchmakerId == MatchmakerId);
                            if (userData != default)
                            {
                                m_MicVolume.fillAmount = userData.voiceStateData.micVolume;
                            }
                        }
                        else
                        {
                            m_MicVolume.fillAmount = 0;
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
            Vector3 anchorPosition = (ReferenceEquals(m_InfoAnchor, null)) ? transform.position : m_InfoAnchor.position;
            var dialogType = OpenDialogAction.DialogType.None;
            if (m_ActiveDialogSelector.GetValue() != OpenDialogAction.DialogType.CollaborationUserInfo ||
                m_SelectedUserSelector.GetValue().matchmakerId != MatchmakerId)
            {
                dialogType = OpenDialogAction.DialogType.CollaborationUserInfo;
                var userInfo = new SetUserInfoAction.SetUserInfoData();
                userInfo.matchmakerId = MatchmakerId;
                userInfo.dialogPosition = anchorPosition;
                Dispatcher.Dispatch(SetUserInfoAction.From(userInfo));
            }

            Dispatcher.Dispatch(OpenDialogAction.From(dialogType));
        }

        protected virtual bool IsSelected()
        {
            return m_ActiveDialogSelector.GetValue() == OpenDialogAction.DialogType.CollaborationUserInfo &&
                m_SelectedUserSelector.GetValue().matchmakerId == MatchmakerId;
        }

        protected bool IsSpeaking()
        {
            var user = m_UsersSelector.GetValue().Find(data => data.matchmakerId == MatchmakerId);
            return user.voiceStateData.micVolume > 0;
        }
    }
}
