using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class AppBarUIController: MonoBehaviour
    {
        [SerializeField]
        CanvasGroup m_AppBarPanel;
        [SerializeField]
        CollaborationUIController m_CollaboratorsList;

        IEnumerable<string> m_UserIds = new string[0];
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void Awake()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<List<NetworkUserData>>(RoomConnectionContext.current, nameof(IRoomConnectionDataProvider<NetworkUserData>.users), OnUsersChanged));
            SetPanelActive(true);
        }

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
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

        void OnUsersChanged(List<NetworkUserData> users)
        {
            var userIds = users.Select(u => u.matchmakerId);

            if (!EnumerableExtension.SafeSequenceEquals(m_UserIds, userIds))
            {
                m_UserIds = new List<string>(userIds);
                m_CollaboratorsList.UpdateUserList(m_UserIds.ToArray());
            }

            m_CollaboratorsList.UpdateUsers(m_UserIds.ToArray());
        }
    }
}
