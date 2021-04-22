using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.UI;
using Unity.Reflect.Utils;

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
        Image m_DialogButtonImage;

        Project m_CurrentActiveProject = Project.Empty;
        Project m_CurrentSelectedProject = Project.Empty;
        int m_CurrentOptionIndex;

        List<ProjectListItem> m_ProjectListItems = new List<ProjectListItem>();
        ProjectListItem m_LastHighlightedItem;
        ProjectListItem m_ActiveProjectListItem;

        RectTransform m_OptionPopupRectTransform;
        RectTransform m_ScrollRectTransform;

        ProjectListFilterData m_CurrentFilterData;
        ProjectServerType m_CurrentServerType;

        LoginState? m_LoginState;
        ProjectRoom[] m_Rooms;

        ProjectListSortData m_CurrentProjectSortData;

        void Awake()
        {
            m_ProjectOption.gameObject.SetActive(false);

            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.sessionStateChanged += OnSessionStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

            m_OptionPopupRectTransform = m_ProjectOption.GetComponent<RectTransform>();
            m_ScrollRectTransform = m_ScrollRect.GetComponent<RectTransform>();

            m_RectTransform = GetComponent<RectTransform>();
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
        }

        void OnSortMethodValueChanged(int value)
        {
            ProjectSortField sortField = (ProjectSortField)value;
            switch (sortField)
            {
                case ProjectSortField.SortByDate:
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetProjectSortMethod, ProjectSortField.SortByDate));
                    break;
                case ProjectSortField.SortByName:
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetProjectSortMethod, ProjectSortField.SortByName));
                    break;
                default:
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetProjectSortMethod, ProjectSortField.SortByDate));
                    break;
            }
        }

        Coroutine m_SearchCoroutine;
        NavigationMode m_CurrentNavigationMode;

        void OnSearchInputTextChanged(string search)
        {
            if (m_SearchCoroutine != null)
            {
                StopCoroutine(m_SearchCoroutine);
                m_SearchCoroutine = null;
            }
            m_SearchCoroutine = StartCoroutine(SearchStringChanged(search));
        }

        void OnSearchInputSelected(string input)
        {
            DisableMovementMapping(true);
        }
        void OnSearchInputDeselected(string input)
        {
            DisableMovementMapping(false);
        }

        public static void DisableMovementMapping(bool disableWASD)
        {
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.moveEnabled = !disableWASD;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
        }

        IEnumerator SearchStringChanged(string search)
        {
            yield return new WaitForSeconds(0.2f);
            var data = UIStateManager.current.stateData.landingScreenFilterData;
            data.searchString = search;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetLandingScreenFilter, data));
        }

        void OnProjectTabButtonClicked(ProjectServerType projectServerType)
        {
            var data = UIStateManager.current.stateData.landingScreenFilterData;
            data.projectServerType = projectServerType;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetLandingScreenFilter, data));
        }

        Vector2 GetOptionPosition(ProjectListItem item)
        {
            var anchoredPositionY = m_ScrollViewContent.GetComponent<RectTransform>().anchoredPosition.y;
            var localPositionY = item.transform.localPosition.y + anchoredPositionY;

            var itemHeight = item.GetComponent<RectTransform>().rect.height;
            var adjustY = m_OptionPopupRectTransform.rect.height - itemHeight;
            if (localPositionY - m_OptionPopupRectTransform.rect.height < -m_ScrollRectTransform.rect.height)
            {
                return new Vector2(m_ProjectOption.transform.localPosition.x, localPositionY + adjustY);
            }
            else
            {
                return new Vector2(m_ProjectOption.transform.localPosition.x, localPositionY);
            }
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (data.selectedProjectOption != m_CurrentSelectedProject)
            {
                if (m_LastHighlightedItem != null)
                {
                    m_LastHighlightedItem.SetHighlight(false);
                    m_LastHighlightedItem = null;
                }

                if (data.selectedProjectOption != Project.Empty)
                {
                    var projectListItem = m_ProjectListItems.SingleOrDefault(e => e.room.project == data.selectedProjectOption);
                    if (projectListItem != null)
                    {
                        projectListItem.SetHighlight(true);
                        m_LastHighlightedItem = projectListItem;
                    }

                    m_ProjectOption.transform.SetParent(m_ScrollRect.transform);
                    m_ProjectOption.transform.localPosition = GetOptionPosition(projectListItem);
                    m_ProjectOption.transform.SetParent(m_ScrollRect.transform.parent);

                    m_ScrollRect.StopMovement();
                    m_ProjectOption.InitProjectOption(data.selectedProjectOption);
                    m_ProjectOption.gameObject.SetActive(true);
                    m_TapDetector.SetActive(true);
                }
                else
                {
                    m_ProjectOption.gameObject.SetActive(false);
                    m_TapDetector.SetActive(false);
                }
                m_CurrentSelectedProject = data.selectedProjectOption;
                m_CurrentOptionIndex = data.projectOptionIndex;
            }
            else if (data.projectOptionIndex != m_CurrentOptionIndex)
            {
                m_ProjectOption.InitProjectOption(data.selectedProjectOption);
                m_CurrentOptionIndex = data.projectOptionIndex;
            }

            if (data.landingScreenFilterData != m_CurrentFilterData)
            {
                m_ScrollRect.verticalNormalizedPosition = 1;
                FilterProjectList(data.landingScreenFilterData);
                m_CurrentFilterData = data.landingScreenFilterData;

                if (data.landingScreenFilterData.projectServerType != m_CurrentServerType)
                {
                    m_ProjectTabController.SelectButtonType(data.landingScreenFilterData.projectServerType);
                    m_CurrentServerType = data.landingScreenFilterData.projectServerType;
                }
            }

            UpdateLayout(data);
        }

        void UpdateLayout(UIStateData data)
        {
            if (data.activeDialog == DialogType.LandingScreen &&
                data.navigationState.navigationMode != m_CurrentNavigationMode)
            {
                m_CurrentNavigationMode = data.navigationState.navigationMode;
                var top = (m_CurrentNavigationMode == NavigationMode.VR) ? k_VRLayoutOffsetUp: 0f;
                var bottom = (m_CurrentNavigationMode == NavigationMode.VR) ? m_RectTransform.offsetMin.y: 0f;
                m_RectTransform.offsetMax = new Vector2(m_RectTransform.offsetMax.x, top);
                m_RectTransform.offsetMin = new Vector2(m_RectTransform.offsetMin.x, bottom);
            }
        }

        void FilterProjectList(ProjectListFilterData filterData)
        {
            bool stringFilter = !string.IsNullOrWhiteSpace(filterData.searchString);
            int visibleCount = 0;

            foreach (var projectListItem in m_ProjectListItems)
            {
                bool visible = filterData.projectServerType.HasFlag(projectListItem.projectServerType);
                if (stringFilter)
                {
                    visible = visible && projectListItem.room.project.name.IndexOf(filterData.searchString, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (visible)
                {
                    projectListItem.SetHighlightString(filterData.searchString);
                    visibleCount++;
                }

                projectListItem.gameObject.SetActive(visible);
            }

            var isLoggedIn= UIStateManager.current.sessionStateData.sessionState.loggedState == LoginState.LoggedIn;
            m_NoProjectPanel.SetActive(!isLoggedIn || HasNoProjectsAvailable());
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (data.activeProject != m_CurrentActiveProject)
            {
                m_CurrentActiveProject = data.activeProject;
                m_ActiveProjectListItem = m_ProjectListItems.FirstOrDefault(item => item.room.project == m_CurrentActiveProject);
            }

            if (m_ActiveProjectListItem != null && data.activeProjectThumbnail != m_ActiveProjectListItem.projectThumbnail)
            {
                m_ActiveProjectListItem.projectThumbnail = data.activeProjectThumbnail;
            }

            if (data.projectSortData != m_CurrentProjectSortData)
            {
                m_CurrentProjectSortData = data.projectSortData;
                SortProjects();
            }
        }

        void OnSessionStateDataChanged(UISessionStateData data)
        {
            if (m_LoginState != data.sessionState.loggedState)
            {
                switch (data.sessionState.loggedState)
                {
                    case LoginState.LoggedIn:
                        m_NoProjectPanel.SetActive(false);
                        m_FetchingProjectsPanel.SetActive(true);
                        break;
                    case LoginState.LoggedOut:
                        ClearProjectListItem();
                        m_NoProjectPanel.SetActive(false);
                        m_FetchingProjectsPanel.SetActive(false);
                        break;
                    case LoginState.LoggingIn:
                    case LoginState.LoggingOut:
                        // todo put spinner
                        break;
                }

                m_LoginState = data.sessionState.loggedState;
            }

            if(m_LoginState == LoginState.LoggedIn)
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
                                $"Environment: {ProjectServerClient.ProjectServerAddress(environmentInfo.provider)}";
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

                UpdateProjectItems(data.sessionState.rooms);

                if (m_Rooms != data.sessionState.rooms || !EnumerableExtension.SafeSequenceEquals(m_Rooms, data.sessionState.rooms))
                {
                    m_Rooms = data.sessionState.rooms;
                }
                else if (HasNoProjectsAvailable())
                {
                    m_NoProjectPanel.SetActive(true);
                    m_FetchingProjectsPanel.SetActive(false);
                }
            }

        }

        bool IsLoadingProjects()
        {
            return UIStateManager.current.stateData.progressData.progressState != ProgressData.ProgressState.NoPendingRequest && UIStateManager.current.sessionStateData.sessionState.rooms.Length == 0;
        }

        bool HasNoProjectsAvailable()
        {
            return UIStateManager.current.stateData.progressData.progressState == ProgressData.ProgressState.NoPendingRequest && UIStateManager.current.sessionStateData.sessionState.rooms.Length == 0;
        }

        void ClearProjectListItem()
        {
            foreach (var projectListItem in m_ProjectListItems)
            {
                Destroy(projectListItem.gameObject);
            }
            m_ProjectListItems.Clear();
        }

        void UpdateProjectItems(ProjectRoom[] rooms)
        {
            if(m_FetchingProjectsPanel.activeSelf)
                m_FetchingProjectsPanel.SetActive(IsLoadingProjects());

            Array.Sort(rooms, (room1, room2) => room2.project.lastPublished.CompareTo(room1.project.lastPublished));
            for (var index = 0; index < rooms.Length || index < m_ProjectListItems.Count; index++)
            {
                var listItem = GetProjectListItemAt(index);
                if (index < rooms.Length)
                {
                    var room = rooms[index];
                    listItem.gameObject.SetActive(true);
                    listItem.OnProjectRoomChanged(room);
                }
                else
                {
                    listItem.gameObject.SetActive(false);
                }
            }

            m_ProjectListItemPrefab.gameObject.SetActive(false);

            FilterProjectList(m_CurrentFilterData);
        }

        ProjectListItem GetProjectListItemAt(int index)
        {
            ProjectListItem item;
            if (index >= m_ProjectListItems.Count)
            {
                item = Instantiate(m_ProjectListItemPrefab, m_ScrollViewContent.transform);
                item.projectItemClicked += OnProjectOpenButtonClick;
                item.optionButtonClicked += OnProjectOptionButtonClick;
                item.GetComponentInChildren<ProjectListColumnController>().tableContainer = tableContainer;
                m_ProjectListItems.Add(item);
            }
            else
            {
                item = m_ProjectListItems[index];
            }

            return item;
        }

        void OnProjectOpenButtonClick(Project project)
        {
            var projectData = UIStateManager.current.projectStateData;

            if (projectData.activeProject == project)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
                //if the project already opened, just close landing screen
                return;
            }

            var navigationState = UIStateManager.current.stateData.navigationState;
            if (navigationState.navigationMode == NavigationMode.Walk)
            {
                UIStateManager.current.walkStateData.instruction.Cancel();
            }

            projectData.activeProject = project;
            projectData.activeProjectThumbnail = ThumbnailController.LoadThumbnailForProject(project);

            if (UIStateManager.current.projectStateData.activeProject != Project.Empty)
            {
                // first close current Project if open
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, "Closing {UIStateManager.current.projectStateData.activeProject.name}..."));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseProject, UIStateManager.current.projectStateData.activeProject));
            }
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, $"Opening {projectData.activeProject.name}..."));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenProject, projectData));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetMeasureToolOptions,  MeasureToolStateData.defaultData));
        }

        void OnProjectOptionButtonClick(Project project)
        {
            var model = UIStateManager.current.stateData;
            if (model.activeOptionDialog == OptionDialogType.ProjectOptions && model.selectedProjectOption == project)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetOptionProject, Project.Empty));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.None));
            }
            else
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetOptionProject, project));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.ProjectOptions));
            }
        }

        void OnTapDetectorDown(BaseEventData data)
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetOptionProject, Project.Empty));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.None));
        }

        void SortProjects()
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

            var method = (m_CurrentProjectSortData.method == ProjectSortMethod.Ascending) ? 1 : -1;
            switch (m_CurrentProjectSortData.sortField)
            {
                case ProjectSortField.SortByDate:
                    childrenProjectItems.Sort((project1, project2) => method * project2.room.project.lastPublished.CompareTo(project1.room.project.lastPublished));
                    break;
                case ProjectSortField.SortByName:
                    childrenProjectItems.Sort((project1, project2) => method * project1.room.project.name.CompareTo(project2.room.project.name));
                    break;
                case ProjectSortField.SortByOrganization:
                    //TODO update when organization is added to Project.cs in the Reflect package
                    childrenProjectItems.Sort((project1, project2) => method * project1.room.project.name.CompareTo(project2.room.project.name));
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
