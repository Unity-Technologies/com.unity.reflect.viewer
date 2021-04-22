using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class AppBarUIController: MonoBehaviour
    {
        [SerializeField]
        CanvasGroup m_AppBarPanel;
        [SerializeField]
        CollaborationUIController m_CollaboratorsList;

        LoginState? m_LoginState;
        IEnumerable<string> m_UserIds = new string[0];

        void Awake()
        {
            UIStateManager.roomConnectionStateChanged += OnConnectionStateChanged;
            UIStateManager.sessionStateChanged += OnSessionStateDataChanged;
            UIStateManager.stateChanged += OnUIStateDataChanged;
        }

        void SetPanelActive(bool isActive)
        {
            if (m_AppBarPanel != null)
            {
                m_AppBarPanel.alpha = isActive ? 1 : 0;
                m_AppBarPanel.interactable = isActive;
                m_AppBarPanel.blocksRaycasts = isActive;
            }
        }

        void OnConnectionStateChanged(RoomConnectionStateData connectionState)
        {
            var userIds = connectionState.users.Select(u => u.matchmakerId);

            if(EnumerableExtension.SafeSequenceEquals(m_UserIds, userIds))
            {
                m_UserIds = userIds;
                m_CollaboratorsList.UpdateUserList(m_UserIds.ToArray());
            }
        }

        void OnUIStateDataChanged(UIStateData stateData)
        {
            m_CollaboratorsList.UpdateUserList(m_UserIds.ToArray());
        }

        void OnSessionStateDataChanged(UISessionStateData data)
        {
            if (!m_LoginState.HasValue || m_LoginState != data.sessionState.loggedState)
            {
                switch (data.sessionState.loggedState)
                {
                    case LoginState.LoggedIn:
                        SetPanelActive(true);
                        break;
                    case LoginState.LoggedOut:
                        SetPanelActive(false);
                        break;
                    case LoginState.LoggingIn:
                        break;
                    case LoginState.LoggingOut:
                        break;
                }
                m_LoginState = data.sessionState.loggedState;
            }
        }
    }
}
