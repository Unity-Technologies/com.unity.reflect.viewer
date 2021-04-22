using System;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class UserInfoDialogController : UserDetailsUIController
    {
        [SerializeField, Tooltip("User's Name [Optional]")]
        TMPro.TMP_Text m_MuteText;

        DialogWindow m_DialogWindow;
        UserInfoDialogData m_UserDialogData;

        void Awake()
        {
            m_DialogWindow = GetComponent<DialogWindow>();
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.roomConnectionStateChanged += OnConnectionStateChanged;
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
            if (stateData.SelectedUserData != m_UserDialogData)
            {
                m_UserDialogData = stateData.SelectedUserData;
                MatchmakerId = m_UserDialogData.matchmakerId;
                if (UIStateManager.current.stateData.navigationState.navigationMode != NavigationMode.VR)
                {
                    transform.position = m_UserDialogData.dialogPosition;
                }

                UpdateUser(m_UserDialogData.matchmakerId, true);
            }
        }

        void OnConnectionStateChanged(RoomConnectionStateData applicationData)
        {
            UpdateUser(MatchmakerId);
        }
    }
}
