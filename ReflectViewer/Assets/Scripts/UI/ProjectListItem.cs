using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.Utils;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ProjectListItem: MonoBehaviour
    {
#pragma warning disable CS0649
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
        GameObject m_OptionButtonBackground;
        [SerializeField]
        Image m_ThumbnailImage;
        [SerializeField]
        CollaborationUIController m_CollaboratorsList;

        [Space]
        [SerializeField]
        ProjectListItemStatus m_ProjectListItemStatus;
#pragma warning restore CS0649

        ProjectRoom m_Room;

        static ProjectListItem s_OptionButtonHighlightOwner;

        SetLandingScreenFilterProjectServerAction.ProjectServerType m_ProjectServerType = SetLandingScreenFilterProjectServerAction.ProjectServerType.None;
        string[] m_UserList = new string[0];

        const string k_OrganizationFallback = "-";

        List<IDisposable> m_DisposeOnDisable = new List<IDisposable>();

        public ProjectRoom room => m_Room;
        public Sprite projectThumbnail
        {
            get => m_ThumbnailImage.sprite;
            set
            {
                if (value != null)
                    m_ThumbnailImage.sprite = value;
            }
        }

        public SetLandingScreenFilterProjectServerAction.ProjectServerType projectServerType => m_ProjectServerType;

        public event Action<Project> projectItemClicked;
        public event Action<ProjectListItem, Project> optionButtonClicked;
        public event Action<Project> downloadButtonClicked;

        void Awake()
        {
            m_ItemButton.onClick.AddListener(OnItemButtonClicked);
            m_OptionButton.onClick.AddListener(OnOptionButtonClicked);

            m_ProjectListItemStatus.RegisterDownloadButtonClick(OnDownloadButtonClicked);
        }

        void OnEnable()
        {
            ReflectProjectsManager.projectStatusChanged += OnProjectStatusChanged;
            ReflectProjectsManager.projectDownloadProgressChanged += OnProjectDownloadProgressChanged;

            m_DisposeOnDisable.Add(UISelectorFactory.createSelector<SetDisplayAction.ScreenSizeQualifier>(UIStateContext.current, nameof(IUIStateDisplayProvider<DisplayData>.display) + "." + nameof(IDisplayDataProvider.screenSizeQualifier), OnDisplayChanged));
        }

        void OnDisplayChanged(SetDisplayAction.ScreenSizeQualifier data)
        {
            m_CollaboratorsList.maxHorizontalAvatars = (data < SetDisplayAction.ScreenSizeQualifier.Medium) ? 2 : 3;
            m_CollaboratorsList.UpdateUserList(m_UserList);
        }

        void OnDisable()
        {
            ReflectProjectsManager.projectStatusChanged -= OnProjectStatusChanged;
            ReflectProjectsManager.projectDownloadProgressChanged -= OnProjectDownloadProgressChanged;

            m_DisposeOnDisable.ForEach(x => x.Dispose());
            m_DisposeOnDisable.Clear();
        }

        void OnProjectStatusChanged(Project project, ProjectsManager.Status status)
        {
            if (m_Room.project != project)
                return;

            switch (status)
            {
                case ProjectsManager.Status.QueuedForDownload:
                    m_ProjectListItemStatus.SetStatus(ProjectListItemStatus.Status.QueuedForDownload);
                    break;

                case ProjectsManager.Status.Downloading:
                    m_ProjectListItemStatus.SetStatus(ProjectListItemStatus.Status.Downloading);
                    break;

                case ProjectsManager.Status.QueuedForDelete:
                    m_ProjectListItemStatus.SetStatus(ProjectListItemStatus.Status.QueuedForDelete);
                    break;

                case ProjectsManager.Status.Deleting:
                    m_ProjectListItemStatus.SetStatus(ProjectListItemStatus.Status.Deleting);
                    break;

                case ProjectsManager.Status.Downloaded:
                case ProjectsManager.Status.Deleted:
                default:
                    if (project.IsLocal)
                    {
                        m_ProjectListItemStatus.SetStatus(project.HasUpdate ?
                            ProjectListItemStatus.Status.UpdateAvailable : ProjectListItemStatus.Status.AvailableOffline);
                    }
                    else
                    {
                        m_ProjectListItemStatus.SetStatus(project.IsConnectedToServer ?
                            ProjectListItemStatus.Status.AvailableForDownload : ProjectListItemStatus.Status.ConnectionError);
                    }
                    break;
            }

            m_ItemButton.interactable = ReflectProjectsManager.IsReadyForOpening(project);
        }


        void OnProjectDownloadProgressChanged(Project project, int progress, int total)
        {
            if (m_Room.project != project)
                return;

            m_ProjectListItemStatus.SetProgress((float)progress / total);
        }

        public void OnProjectRoomChanged(ProjectRoom projectRoom)
        {
            var project = projectRoom.project;
            m_Room = projectRoom;
            m_TitleText.text = project.name;

            m_ServerText.text = project.description;
            if (project.description == "Local")
            {
                m_ProjectServerType = SetLandingScreenFilterProjectServerAction.ProjectServerType.Local;
            }
            else if (project.description == "Cloud")
            {
                m_ProjectServerType = SetLandingScreenFilterProjectServerAction.ProjectServerType.Cloud;
            }
            else if (project.description == "Network")
            {
                m_ProjectServerType = SetLandingScreenFilterProjectServerAction.ProjectServerType.Network;
            }

            if (project is EmbeddedProject embeddedProject)
            {
                m_OrganizationText.text = k_OrganizationFallback;
                m_DateText.text = "-";
                m_ProjectListItemStatus.SetStatus(ProjectListItemStatus.Status.Unknown);

                projectThumbnail = embeddedProject.thumbnailOverride != null ? embeddedProject.thumbnailOverride : ThumbnailController.LoadThumbnailForProject(project);
            }
            else
            {
                m_OrganizationText.text = ((UnityProject)project).Organization?.Name ?? k_OrganizationFallback;
                m_DateText.text = UIUtils.GetTimeIntervalSinceNow(project.lastPublished.ToLocalTime());

                OnProjectStatusChanged(project, ReflectProjectsManager.GetStatus(project));

                projectThumbnail = ThumbnailController.LoadThumbnailForProject(project);
            }

            m_UserList = projectRoom.users.Select(u => u.matchmakerId).ToArray();
            m_CollaboratorsList.UpdateUserList(m_UserList);
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

        public static void ShowOptionButtonHighlight(ProjectListItem item)
        {
            if (item == null)
                return;

            if (s_OptionButtonHighlightOwner != item)
                HideOptionButtonHighlight();

            item.m_OptionButtonBackground.SetActive(true);
            s_OptionButtonHighlightOwner = item;
        }

        public static void HideOptionButtonHighlight()
        {
            if (s_OptionButtonHighlightOwner == null)
                return;

            s_OptionButtonHighlightOwner.m_OptionButtonBackground.SetActive(false);
            s_OptionButtonHighlightOwner = null;
        }

        void OnItemButtonClicked()
        {
            projectItemClicked?.Invoke(m_Room.project);
        }

        void OnOptionButtonClicked()
        {
            optionButtonClicked?.Invoke(this, m_Room.project);
        }

        void OnDownloadButtonClicked()
        {
            downloadButtonClicked?.Invoke(m_Room.project);
        }
    }
}
