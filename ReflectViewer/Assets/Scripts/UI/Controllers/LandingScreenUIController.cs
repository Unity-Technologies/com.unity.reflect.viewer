using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Dispatching;
using TMPro;
using Unity.Reflect.Runtime;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect;
using UnityEngine.UI;
using Unity.Reflect.Utils;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class LandingScreenUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        GameObject m_Suspending;
        [SerializeField]
        GameObject m_NoProjectPanel;
        [SerializeField]
        GameObject m_FetchingProjectsPanel;

        [SerializeField]
        ProjectListItem m_ProjectListItemPrefab;
        [SerializeField]
        GameObject m_ScrollViewContent;
        [SerializeField]
        LandingScreenProjectOptionsUIController m_ProjectOption;

        [SerializeField]
        ScrollRect m_ScrollRect;
        [SerializeField]
        GameObject m_TapDetector;

        [SerializeField]
        TMP_Dropdown m_SortDropdown;

        [SerializeField]
        TMP_InputField m_SearchInput;

        [SerializeField]
        ProjectTabController m_ProjectTabController;

        [SerializeField]
        TextMeshProUGUI m_CloudSettingDebugInfo;

        public RectTransform tableContainer;
#pragma warning restore CS0649

        const float k_VRLayoutOffsetUp = 500;
        RectTransform m_RectTransform;
        readonly List<ProjectListItem> m_ProjectListItems = new List<ProjectListItem>();
        ProjectListItem m_ActiveProjectListItem;
        RectTransform m_ScrollRectTransform;
        LoginState? m_LoginState;
        ProjectListState? m_ProjectListState;
        Coroutine m_SearchCoroutine;
        IUISelector<Project> m_ActiveProjectGetter;
        IUISelector<Sprite> m_ActiveProjectThumbnailGetter;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogGetter;
        IUISelector<SetNavigationModeAction.NavigationMode> m_NavigationModeGetter;
        IUISelector<SetLandingScreenFilterProjectServerAction.ProjectServerType> m_LandingScreenProjectServerTypeGetter;
        static IUISelector<SetProgressStateAction.ProgressState> s_ProgressStateGetter;
        IUISelector<string> m_LandingScreenSearchStringGetter;
        IUISelector<LoginState> m_LoggedStateGetter;
        IUISelector<ProjectListState> m_ProjectListStateGetter;
        static IUISelector<IProjectRoom[]> s_RoomsGetter;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<ProjectListSortData>(ProjectContext.current, nameof(IProjectSortDataProvider.projectSortData), OnProjectSortDataChanged));
            m_DisposeOnDestroy.Add(m_ActiveProjectGetter = UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), OnActiveProjectChanged));
            m_DisposeOnDestroy.Add(m_ActiveProjectThumbnailGetter = UISelectorFactory.createSelector<Sprite>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProjectThumbnail), OnActiveProjectThumbnailChanged));
            m_DisposeOnDestroy.Add(m_LoggedStateGetter = UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState), OnLoggedStateDataChanged));
            m_DisposeOnDestroy.Add(m_ProjectListStateGetter = UISelectorFactory.createSelector<ProjectListState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.projectListState)));
            m_DisposeOnDestroy.Add(s_RoomsGetter = UISelectorFactory.createSelector<IProjectRoom[]>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.rooms), OnProjectRoomUpdate));
            m_DisposeOnDestroy.Add(m_LandingScreenProjectServerTypeGetter = UISelectorFactory.createSelector<SetLandingScreenFilterProjectServerAction.ProjectServerType>(LandingScreenContext.current, nameof(IProjectListFilterDataProvider.projectServerType), OnProjectServerTypeChanged));
            m_DisposeOnDestroy.Add(m_LandingScreenSearchStringGetter = UISelectorFactory.createSelector<string>(LandingScreenContext.current, nameof(IProjectListFilterDataProvider.searchString), OnFilterSearchStringChanged));
            m_DisposeOnDestroy.Add(m_ActiveDialogGetter = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog)));
            m_DisposeOnDestroy.Add(m_NavigationModeGetter = UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode), OnNavigationModeChanged));
            m_DisposeOnDestroy.Add(s_ProgressStateGetter = UISelectorFactory.createSelector<SetProgressStateAction.ProgressState>(ProgressContext.current, nameof(IProgressDataProvider.progressState)));

            m_ScrollRectTransform = m_ScrollRect.GetComponent<RectTransform>();

            m_RectTransform = GetComponent<RectTransform>();

            // Auto-close Option dialog events
            m_ScrollRect.onValueChanged.AddListener((pos) => HideProjectOptionDialog());
            m_ProjectOption.downloadButtonClicked += HideProjectOptionDialog;
            m_ProjectOption.deleteButtonClicked += HideProjectOptionDialog;
        }

        void OnNavigationModeChanged(SetNavigationModeAction.NavigationMode newData)
        {
            if (m_ActiveDialogGetter.GetValue() == OpenDialogAction.DialogType.LandingScreen)
            {
                var top = (newData == SetNavigationModeAction.NavigationMode.VR) ? k_VRLayoutOffsetUp : 0f;
                var bottom = (newData == SetNavigationModeAction.NavigationMode.VR) ? m_RectTransform.offsetMin.y : 0f;
                m_RectTransform.offsetMax = new Vector2(m_RectTransform.offsetMax.x, top);
                m_RectTransform.offsetMin = new Vector2(m_RectTransform.offsetMin.x, bottom);
            }
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                EventTriggerUtility.CreateEventTrigger(m_TapDetector, OnTapDetectorDown,
                    EventTriggerType.PointerDown);
            }

            m_ProjectTabController.projectTabButtonClicked += OnProjectTabButtonClicked;
            m_SearchInput.onValueChanged.AddListener(OnSearchInputTextChanged);
            m_SearchInput.onSelect.AddListener(OnSearchInputSelected);
            m_SearchInput.onDeselect.AddListener(OnSearchInputDeselected);
            m_SortDropdown.onValueChanged.AddListener(OnSortMethodValueChanged);
            m_Suspending.SetActive(true);
            m_NoProjectPanel.SetActive(false);
            m_ProjectListItemPrefab.gameObject.SetActive(false);
        }

        static void OnSortMethodValueChanged(int value)
        {
            ProjectSortField sortField = (ProjectSortField)value;
            switch (sortField)
            {
                case ProjectSortField.SortByDate:
                    Dispatcher.Dispatch(SetProjectSortMethodAction.From(ProjectSortField.SortByDate));
                    break;
                case ProjectSortField.SortByName:
                    Dispatcher.Dispatch(SetProjectSortMethodAction.From(ProjectSortField.SortByName));
                    break;
                default:
                    Dispatcher.Dispatch(SetProjectSortMethodAction.From(ProjectSortField.SortByDate));
                    break;
            }
        }

        void OnSearchInputTextChanged(string search)
        {
            if (m_SearchCoroutine != null)
            {
                StopCoroutine(m_SearchCoroutine);
                m_SearchCoroutine = null;
            }
            m_SearchCoroutine = StartCoroutine(SearchStringChanged(search));
        }

        static void OnSearchInputSelected(string input)
        {
            DisableMovementMapping(true);
        }

        static void OnSearchInputDeselected(string input)
        {
            DisableMovementMapping(false);
        }

        static void DisableMovementMapping(bool disableWASD)
        {
            Dispatcher.Dispatch(SetMoveEnabledAction.From(!disableWASD));
        }

        IEnumerator SearchStringChanged(string search)
        {
            yield return new WaitForSeconds(0.2f);
            Dispatcher.Dispatch(SetLandingScreenFilterSearchStringAction.From(search));
        }

        void OnProjectTabButtonClicked(SetLandingScreenFilterProjectServerAction.ProjectServerType projectServerType)
        {
            Dispatcher.Dispatch(SetLandingScreenFilterProjectServerAction.From(projectServerType));
        }

        void OnFilterSearchStringChanged(string newData)
        {
            m_ScrollRect.verticalNormalizedPosition = 1;
            FilterProjectList();
        }

        void OnProjectServerTypeChanged(SetLandingScreenFilterProjectServerAction.ProjectServerType newData)
        {
            m_ScrollRect.verticalNormalizedPosition = 1;
            FilterProjectList();
            m_ProjectTabController.SelectButtonType(newData);
        }

        void FilterProjectList()
        {
            if (m_LandingScreenProjectServerTypeGetter == null)
                return;

            var stringFilter = m_LandingScreenSearchStringGetter != null && !string.IsNullOrWhiteSpace(m_LandingScreenSearchStringGetter.GetValue());

            foreach (var projectListItem in m_ProjectListItems)
            {
                var visible = m_LandingScreenProjectServerTypeGetter.GetValue().HasFlag(projectListItem.projectServerType);
                if (stringFilter)
                {
                    visible = visible && projectListItem.room.project.name.IndexOf(m_LandingScreenSearchStringGetter.GetValue(), StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (visible)
                {
                    projectListItem.SetHighlightString(m_LandingScreenSearchStringGetter.GetValue());
                }

                projectListItem.gameObject.SetActive(visible);
            }

            var isLoggedIn = m_LoggedStateGetter.GetValue() == LoginState.LoggedIn;
            m_NoProjectPanel.SetActive(!isLoggedIn || HasNoProjectsAvailable());
        }

        void OnActiveProjectChanged(Project newData)
        {
            m_ActiveProjectListItem = m_ProjectListItems.FirstOrDefault(item => item.room.project.projectId == newData.projectId);
        }

        void OnActiveProjectThumbnailChanged(Sprite newData)
        {
            if (m_ActiveProjectListItem == null || m_ActiveProjectListItem.projectThumbnail == newData)
                return;

            if (m_ActiveProjectListItem.room.project is EmbeddedProject && m_ActiveProjectListItem.projectThumbnail != null)
                return; // Do not override the thumbnail of embedded projects

            m_ActiveProjectListItem.projectThumbnail = newData;
        }

        void OnProjectSortDataChanged(ProjectListSortData newData)
        {
            SortProjects(newData);
        }

        void OnProjectRoomUpdate(IProjectRoom[] data)
        {
            if (data == null)
                return;

            UpdateProjectItems(data);
            if (HasNoProjectsAvailable())
            {
                m_NoProjectPanel.SetActive(true);
                m_FetchingProjectsPanel.SetActive(false);
            }
        }

        void OnLoggedStateDataChanged(LoginState newData)
        {
            if (m_LoginState != newData)
            {
                switch (newData)
                {
                    case LoginState.ProcessingToken:
                        ClearProjectListItem();
                        m_NoProjectPanel.SetActive(false);
                        m_FetchingProjectsPanel.SetActive(false);
                        break;
                    case LoginState.LoggingIn:
                        m_NoProjectPanel.SetActive(false);
                        m_FetchingProjectsPanel.SetActive(false);
                        break;
                    case LoginState.LoggedIn:
                        m_NoProjectPanel.SetActive(false);
                        m_FetchingProjectsPanel.SetActive(true);
                        break;
                    case LoginState.LoggedOut:
                        ClearProjectListItem();
                        m_NoProjectPanel.SetActive(false);
                        m_FetchingProjectsPanel.SetActive(false);
                        break;
                    case LoginState.LoggingOut:
                        // todo put spinner
                        break;
                }

                m_LoginState = newData;
            }

            if (m_LoginState == LoginState.LoggedIn)
            {
                // Display Cloud environment debug info when it's not "Production"
                var environmentInfo = LocaleUtils.GetEnvironmentInfo();
                if (environmentInfo.cloudEnvironment != CloudEnvironment.Production)
                {
                    m_CloudSettingDebugInfo.gameObject.SetActive(true);

                    if (environmentInfo.cloudEnvironment == CloudEnvironment.Other)
                    {
                        if (PlayerPrefs.HasKey(LocaleUtils.SettingsKeys.CloudEnvironment))
                            m_CloudSettingDebugInfo.text = $"Environment: {environmentInfo.customUrl}";
                        else
                            m_CloudSettingDebugInfo.text =
                                $"Environment: {ProjectServerClient.ProjectServerAddress(environmentInfo.provider, Protocol.Http)}";
                    }
                    else
                    {
                        m_CloudSettingDebugInfo.text = $"Environment: {environmentInfo.cloudEnvironment}";
                    }
                }
                else
                {
                    m_CloudSettingDebugInfo.gameObject.SetActive(false);
                }
            }
        }

        static bool IsLoadingProjects()
        {
            return s_ProgressStateGetter != null && s_RoomsGetter != null
                && s_ProgressStateGetter?.GetValue() != SetProgressStateAction.ProgressState.NoPendingRequest && s_RoomsGetter.GetValue().Length == 0;
        }

        static bool HasNoProjectsAvailable()
        {
            return (s_ProgressStateGetter == null || s_ProgressStateGetter == null)
                || (s_ProgressStateGetter.GetValue() == SetProgressStateAction.ProgressState.NoPendingRequest && s_RoomsGetter.GetValue().Length == 0);
        }

        void ClearProjectListItem()
        {
            foreach (var projectListItem in m_ProjectListItems)
            {
                Destroy(projectListItem.gameObject);
            }
            m_ProjectListItems.Clear();
        }

        void UpdateProjectItems(IProjectRoom[] rooms)
        {
            if(m_FetchingProjectsPanel.activeSelf)
                m_FetchingProjectsPanel.SetActive(IsLoadingProjects());

            Array.Sort(rooms, (room1, room2) => ((ProjectRoom)room2).project.lastPublished.CompareTo(((ProjectRoom)room1).project.lastPublished));
            for (var index = 0; index < rooms.Length || index < m_ProjectListItems.Count; index++)
            {
                var listItem = GetProjectListItemAt(index);
                if (index < rooms.Length)
                {
                    var room = rooms[index];
                    listItem.gameObject.SetActive(true);
                    listItem.OnProjectRoomChanged((ProjectRoom)room);
                }
                else
                {
                    listItem.gameObject.SetActive(false);
                }
            }

            m_ProjectListItemPrefab.gameObject.SetActive(false);

            FilterProjectList();
        }

        ProjectListItem GetProjectListItemAt(int index)
        {
            ProjectListItem item;
            if (index >= m_ProjectListItems.Count)
            {
                item = Instantiate(m_ProjectListItemPrefab, m_ScrollViewContent.transform);
                item.projectItemClicked += OnProjectOpenButtonClick;
                item.optionButtonClicked += OnProjectOptionButtonClick;
                item.downloadButtonClicked += OnProjectDownloadButtonClick;
                item.GetComponentInChildren<ProjectListColumnController>().tableContainer = tableContainer;
                m_ProjectListItems.Add(item);
            }
            else
            {
                item = m_ProjectListItems[index];
            }

            return item;
        }

        static void OnProjectDownloadButtonClick(Project project)
        {
            Dispatcher.Dispatch(DownloadProjectAction.From(project));
        }

        void OnProjectOpenButtonClick(Project project)
        {
            if (!ReflectProjectsManager.IsReadyForOpening(project))
                return;

            var activeProject = m_ActiveProjectGetter.GetValue();

            if (activeProject?.serverProjectId == project.serverProjectId)
            {
                Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
                //if the project already opened, just close landing screen
                return;
            }
            Dispatcher.Dispatch(SetWalkEnableAction.From(false));

            activeProject = project;

            if (activeProject != Project.Empty)
            {
                if (m_NavigationModeGetter.GetValue() != SetNavigationModeAction.NavigationMode.VR)
                {
                    // switch to orbit mode
                    var data = new SetForceNavigationModeAction.ForceNavigationModeTrigger((int)SetNavigationModeAction.NavigationMode.Orbit);
                    Dispatcher.Dispatch(SetForceNavigationModeAction.From(data));
                }

                // first close current Project if open
                Dispatcher.Dispatch(SetStatusMessage.From("Closing {UIStateManager.current.projectStateData.activeProject.name}..."));
            }
            Dispatcher.Dispatch(SetStatusMessage.From($"Opening {activeProject.name}..."));
            Dispatcher.Dispatch(CloseAllDialogsAction.From(null));

            Dispatcher.Dispatch(SetIsOpenWithLinkSharingAction.From(false));
            Dispatcher.Dispatch(SetCachedLinkTokenAction.From(string.Empty));

            Dispatcher.Dispatch(OpenProjectActions<Project>.From(activeProject));
            Dispatcher.Dispatch(ToggleMeasureToolAction.From(ToggleMeasureToolAction.ToggleMeasureToolData.defaultData));
        }

        void OnProjectOptionButtonClick(ProjectListItem item, Project project)
        {
            ShowProjectOptionDialog(item, project);
        }

        void OnTapDetectorDown(BaseEventData data)
        {
            HideProjectOptionDialog();
        }

        void ShowProjectOptionDialog(ProjectListItem item, Project project)
        {
            if (!m_ProjectOption.IsVisible())
            {
                var contentRectTransform = m_ScrollViewContent.GetComponent<RectTransform>();
                var itemRectTransform = item.GetComponent<RectTransform>();

                var desiredHeight = itemRectTransform.anchoredPosition.y + contentRectTransform.anchoredPosition.y + itemRectTransform.rect.height;

                m_ProjectOption.Show(project, desiredHeight, m_ScrollRectTransform.rect.height);

                ProjectListItem.ShowOptionButtonHighlight(item);

                m_TapDetector.SetActive(true);
            }
        }

        void HideProjectOptionDialog()
        {
            if (m_ProjectOption.IsVisible())
            {
                m_ProjectOption.Hide();
                ProjectListItem.HideOptionButtonHighlight();

                m_TapDetector.SetActive(false);
            }
        }

        void SortProjects(ProjectListSortData sortData)
        {
            m_ScrollRect.verticalNormalizedPosition = 1;
            List<ProjectListItem> childrenProjectItems = m_ScrollViewContent.GetComponentsInChildren<ProjectListItem>(true).ToList();
            foreach (var projectItem in childrenProjectItems.ToList())
            {
                if (!projectItem.gameObject.activeInHierarchy)
                {
                    childrenProjectItems.Remove(projectItem);
                }
            }

            if (childrenProjectItems.Count == 0)
            {
                return;
            }

            var method = (sortData.method == ProjectSortMethod.Ascending) ? 1 : -1;
            switch (sortData.sortField)
            {
                case ProjectSortField.SortByDate:
                    childrenProjectItems.Sort((project1, project2) => method * project2.room.project.lastPublished.CompareTo(project1.room.project.lastPublished));
                    break;
                case ProjectSortField.SortByName:
                    childrenProjectItems.Sort((project1, project2) => method * project1.room.project.name.CompareTo(project2.room.project.name));
                    break;
                case ProjectSortField.SortByOrganization:
                    childrenProjectItems.Sort((project1, project2) =>
                    {
                        var org1 = project1.room.project?.UnityProject?.Organization?.Name ?? string.Empty;
                        var org2 = project2.room.project?.UnityProject?.Organization?.Name ?? string.Empty;
                        return method * org1.CompareTo(org2);
                    });
                    break;
                case ProjectSortField.SortByServer:
                    childrenProjectItems.Sort((project1, project2) => method * project1.room.project.host.ServerName.CompareTo(project2.room.project.host.ServerName));
                    break;
                case ProjectSortField.SortByCollaborators:
                    childrenProjectItems.Sort((project1, project2) => method * -project1.room.users.Count.CompareTo(project2.room.users.Count));
                    break;
                default:
                    childrenProjectItems.Sort((project1, project2) => method * project2.room.project.lastPublished.CompareTo(project1.room.project.lastPublished));
                    break;
            }
            for (int i = 0; i < childrenProjectItems.Count; i++)
            {
                childrenProjectItems[i].gameObject.transform.SetSiblingIndex(i);
            }

        }
    }
}
