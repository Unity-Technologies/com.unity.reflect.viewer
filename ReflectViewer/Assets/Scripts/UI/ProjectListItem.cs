using System;
using System.Linq;
using System.Text;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Utils;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ProjectListItem : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Image m_ItemImage;
        [SerializeField]
        TextMeshProUGUI m_TitleText;
        [SerializeField]
        TextMeshProUGUI m_ServerText;
        [SerializeField]
        TextMeshProUGUI m_OrganizationText;
        [SerializeField]
        TextMeshProUGUI m_DateText;
        [SerializeField]
        Button m_ItemButton;
        [SerializeField]
        Button m_OptionButton;
        [SerializeField]
        Image m_ConnectionStatusImage;
        [SerializeField]
        Image m_UpdatingImage;
        [SerializeField]
        Image m_ThumbnailImage;
        [SerializeField]
        CollaborationUIController m_CollaboratorsList;

        [Space(10)]
        [SerializeField]
        Sprite m_SyncedSprite;
        [SerializeField]
        Sprite m_DisconnectedSprite;
        [SerializeField]
        Sprite m_UpdatesAvailableSprite;

#pragma warning restore CS0649

        string m_ThumbnailPath;
        ProjectRoom m_Room;

        ProjectServerType m_ProjectServerType = ProjectServerType.None;
        ScreenSizeQualifier m_ScreenSizeQualifier;
        string[] m_UserList;

        private const string k_OrganizationFallback = "-";

        public ProjectRoom room => m_Room;
        public Sprite projectThumbnail
        {
            get => m_ThumbnailImage.sprite;
            set
            {
                if(value != null)
                    m_ThumbnailImage.sprite = value;
            }
        }

        public ProjectServerType projectServerType => m_ProjectServerType;

        public event Action<Project> projectItemClicked;
        public event Action<Project> optionButtonClicked;

        void Start()
        {
            m_ItemButton.onClick.AddListener(OnItemButtonClicked);
            m_OptionButton.onClick.AddListener(OnOptionButtonClicked);
        }

        void Awake()
        {
            m_UserList = new string[0];
            UIStateManager.stateChanged += UIStateManagerOnstateChanged;
        }

        void UIStateManagerOnstateChanged(UIStateData uiStateData)
        {
            if (uiStateData.display.screenSizeQualifier != m_ScreenSizeQualifier)
            {
                m_ScreenSizeQualifier = uiStateData.display.screenSizeQualifier;
                m_CollaboratorsList.maxHorizontalAvatars = (m_ScreenSizeQualifier < ScreenSizeQualifier.Medium)? 2 : 3;
                m_CollaboratorsList.UpdateUserList(m_UserList);
            }
        }

        public void OnProjectRoomChanged(ProjectRoom projectRoom)
        {
            m_Room = projectRoom;
            m_TitleText.text = projectRoom.project.name;
            projectThumbnail = ThumbnailController.LoadThumbnailForProject(projectRoom.project);

            m_ServerText.text = projectRoom.project.description;
            if (projectRoom.project.description == "Local")
            {
                m_ProjectServerType = ProjectServerType.Local;
            }
            else if (projectRoom.project.description == "Cloud")
            {
                m_ProjectServerType = ProjectServerType.Cloud;
            }
            else if (projectRoom.project.description == "Network")
            {
                m_ProjectServerType = ProjectServerType.Network;
            }

            m_OrganizationText.text = ((UnityProject)projectRoom.project).Organization?.Name ?? k_OrganizationFallback;
            m_DateText.text = UIUtils.GetTimeIntervalSinceNow(projectRoom.project.lastPublished.ToLocalTime());
            m_UserList = projectRoom.users.Select(u => u.matchmakerId).ToArray();
            m_CollaboratorsList.UpdateUserList(m_UserList);
            m_ConnectionStatusImage.sprite = projectRoom.project.isAvailableOnline ? m_SyncedSprite : m_DisconnectedSprite;
            m_UpdatingImage.gameObject.SetActive(false);
        }

        public void SetHighlightString(string search)
        {
            var projectName = m_Room.project.name;
            if (string.IsNullOrWhiteSpace(search))
            {
                m_TitleText.text = projectName;
            }
            else
            {
                var indexOf = projectName.IndexOf(search, StringComparison.OrdinalIgnoreCase);
                if (indexOf != -1)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<font=\"LiberationSans SDF\">");
                    int currentIndex = 0;

                    while (indexOf != -1)
                    {
                        sb.Append(projectName.Substring(currentIndex, indexOf - currentIndex));
                        sb.Append("<mark=#2096F3>");
                        sb.Append(projectName.Substring(indexOf, search.Length));
                        sb.Append("</mark>");
                        currentIndex = indexOf + search.Length;

                        indexOf = projectName.IndexOf(search, currentIndex, StringComparison.OrdinalIgnoreCase);
                    }
                    sb.Append(projectName.Substring(currentIndex));
                    m_TitleText.text = sb.ToString();
                }
            }
        }

        public void SetHighlight(bool highlight)
        {
            m_ItemImage.color = highlight ? UIConfig.projectItemSelectedColor : UIConfig.projectItemBaseColor;
        }

        void OnItemButtonClicked()
        {
            projectItemClicked?.Invoke(m_Room.project);
        }

        void OnOptionButtonClicked()
        {
            optionButtonClicked?.Invoke(m_Room.project);
        }
    }
}
