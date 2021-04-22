using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public class CollaborationUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        GameObject m_AvatarPrefab;
        [SerializeField]
        public int maxHorizontalAvatars;

        [Space(10)]
        [SerializeField]
        Transform m_AvatarList;
        [SerializeField]
        GameObject m_GroupBubble;
        [SerializeField]
        TMP_Text m_GroupBubbleText;
#pragma warning restore CS0649

        List<UserUIController> m_Users = new List<UserUIController>();

        void CreateAvatarPool()
        {
            for (int i = m_Users.Count; i < maxHorizontalAvatars; ++i)
            {
                var prefab = Instantiate(m_AvatarPrefab, m_AvatarList);
                if(prefab.TryGetComponent(out UserUIController avatarController))
                {
                    m_Users.Add(avatarController);
                    avatarController.gameObject.SetActive(false);
                    avatarController.gameObject.transform.SetSiblingIndex(i);
                }
            }
        }

        public void UpdateUserList(string[] matchmakerIds)
        {
            if (m_Users.Count < maxHorizontalAvatars)
            {
                CreateAvatarPool();
            }

            bool isGrouping = matchmakerIds.Length > maxHorizontalAvatars;
            int nbUngroupedAvatars = maxHorizontalAvatars - 1;
            for (int i = 0; i < m_Users.Count; i++)
            {
                if (i < m_Users.Count)
                {
                    if (i < matchmakerIds.Length && (!isGrouping || i < nbUngroupedAvatars))
                    {
                        m_Users[i].UpdateUser(matchmakerIds[i]);
                        m_Users[i].gameObject.SetActive(true);
                        m_Users[i].gameObject.transform.SetSiblingIndex(i);
                    }
                    else
                    {
                        m_Users[i].gameObject.SetActive(false);
                    }
                }
            }

            if (UIStateManager.current.stateData.activeDialog == DialogType.CollaborationUserInfo &&
                Array.FindIndex(matchmakerIds, (user) => user == UIStateManager.current.stateData.SelectedUserData.matchmakerId) == -1)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
            }

            UpdateUserGroupBubble(matchmakerIds);
        }

        void UpdateUserGroupBubble(string[] connectedIds)
        {
            int nbUngroupedAvatars = maxHorizontalAvatars - 1;
            if (connectedIds.Length > maxHorizontalAvatars)
            {
                m_GroupBubble.SetActive(true);
                m_GroupBubbleText.text = $"+{(connectedIds.Length - nbUngroupedAvatars).ToString()}";
            }
            else
            {
                m_GroupBubble.SetActive(false);
                if (UIStateManager.current.stateData.activeDialog == DialogType.CollaborationUserList)
                {
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
                }
            }
        }
    }
}
