using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class CollaborationUserListController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        UserUIController m_ListItemPrefab;

        [Space(10)]
        [SerializeField]
        public TMP_Text m_DialogTitleText;
        [SerializeField]
        public Transform m_List;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        List<UserUIController> m_Users = new List<UserUIController>();
        string[] m_MatchmakerIds;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void Awake()
        {
            m_DialogWindow = GetComponent<DialogWindow>();
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<List<NetworkUserData>>(RoomConnectionContext.current, nameof(IRoomConnectionDataProvider<NetworkUserData>.users), OnUsersChanged));
        }

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType data)
        {
            switch (data)
            {
                case OpenDialogAction.DialogType.CollaborationUserInfo:
                case OpenDialogAction.DialogType.CollaborationUserList:
                    UpdateList(m_MatchmakerIds);
                    break;
            }
        }

        void OnUsersChanged(List<NetworkUserData> users)
        {
            m_MatchmakerIds = users.Select( u => u.matchmakerId).ToArray();
            if (m_DialogWindow.open)
            {
                UpdateList(m_MatchmakerIds);
            }
        }

        void UpdateList(string[] connectedUsers)
        {
            m_DialogTitleText.text = $"{connectedUsers.Length.ToString()} Total Users";
            for (int i = 0; i < m_Users.Count || i < connectedUsers.Length; i++)
            {
                if (i >= m_Users.Count)
                {
                    InstantiateNewItem(connectedUsers[i]);
                }
                else if (i < connectedUsers.Length)
                {
                    m_Users[i].UpdateUser(connectedUsers[i]);
                    m_Users[i].gameObject.SetActive(true);
                }
                else
                {
                    m_Users[i].gameObject.SetActive(false);
                }
            }
        }

        void InstantiateNewItem(string userId)
        {
            var avatarController = Instantiate(m_ListItemPrefab, m_List);
            if(!ReferenceEquals(avatarController, null))
            {
                avatarController.UpdateUser(userId);
                m_Users.Add(avatarController);
                avatarController.gameObject.SetActive(true);
            }
        }
    }
}
