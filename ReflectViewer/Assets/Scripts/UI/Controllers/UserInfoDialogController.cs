using System;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class UserInfoDialogController: UserDetailsUIController
    {
        [SerializeField, Tooltip("User's Name [Optional]")]
        TMP_Text m_MuteText;

        DialogWindow m_DialogWindow;
        TMP_Text m_FollowCameraText;
        const string k_FollowText = "Follow Camera";
        const string k_StopFollowText = "Stop Follow Camera";

        IUISelector<UserInfoDialogData> m_SelectedUserDataSelector;

        protected override void OnDestroy()
        {
            m_SelectedUserDataSelector?.Dispose();
            base.OnDestroy();
        }

        public override void Awake()
        {
            base.Awake();
            m_DialogWindow = GetComponent<DialogWindow>();
            if (m_FollowCameraButton != null)
            {
                m_FollowCameraText = m_FollowCameraButton.GetComponentInChildren<TMP_Text>();
            }
            m_DialogWindow.dialogOpen.AddListener(OnDialogOpen);
            RoomConnectionContext.current.stateChanged += OnConnectionStateChanged;
            m_SelectedUserDataSelector = UISelectorFactory.createSelector<UserInfoDialogData>(UIStateContext.current, nameof(IUIStateDataProvider.SelectedUserData), OnSelectedUserChanged);
        }

        void OnSelectedUserChanged(UserInfoDialogData data)
        {
            MatchmakerId = data.matchmakerId;

            if (m_NavigationModeSelector.GetValue() != SetNavigationModeAction.NavigationMode.VR)
            {
                transform.position = data.dialogPosition;
            }

            m_FollowCameraText.text = IsFollowing() ? k_StopFollowText : k_FollowText;

            UpdateUser(data.matchmakerId, true);
        }

        void OnDialogOpen()
        {
            if (m_FollowCameraText != null)
            {
                if (m_SelectedUserDataSelector.GetValue().matchmakerId == MatchmakerId && IsFollowing())
                {
                    m_FollowCameraText.text = k_StopFollowText;
                }
                else
                {
                    m_FollowCameraText.text = k_FollowText;
                }
            }
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();

            if (m_MuteText != null)
            {
                m_MuteText.text = IsLocallyMuted() ? "Unmute this user" : "Mute this user";
            }
        }

        void OnStateDataChanged(UIStateData stateData)
        {

        }

        void OnConnectionStateChanged()
        {
            UpdateUser(MatchmakerId);
        }
    }
}
