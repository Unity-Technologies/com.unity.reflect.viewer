using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using TMPro;
using Unity.TouchFramework;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class LandingScreenUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        GameObject m_Suspending;
        [SerializeField]
        GameObject m_WelcomePanel;
        [SerializeField]
        GameObject m_NoProjectPanel;
        [SerializeField]
        GameObject m_FetchingProjectsPanel;

        [SerializeField]
        Button m_DialogButton;
        [SerializeField]
        ToolButton m_RefreshButton;
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
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
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
        Project[] m_Projects;

        ProjectSortMethod m_CurrentProjectSortMethod = ProjectSortMethod.SortByDate;

        void Awake()
        {
            m_ProjectOption.gameObject.SetActive(false);

            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.sessionStateChanged += OnSessionStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

            m_OptionPopupRectTransform = m_ProjectOption.GetComponent<RectTransform>();
            m_ScrollRectTransform = m_ScrollRect.GetComponent<RectTransform>();

            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();
            m_DialogWindow = GetComponent<DialogWindow>();
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                EventTriggerUtility.CreateEventTrigger(m_TapDetector, OnTapDetectorDown,
                    EventTriggerType.PointerDown);
            }

            m_DialogButton.interactable = false;
            m_DialogButton.onClick.AddListener(OnDialogButtonClick);
            m_ProjectTabController.projectTabButtonClicked += OnProjectTabButtonClicked;
            m_SearchInput.onValueChanged.AddListener(OnSearchInputTextChanged);
            m_SortDropdown.onValueChanged.AddListener(OnSortMethodValueChanged);

            m_RefreshButton.buttonClicked += OnRefreshButtonClicked;
            SuspendingPopup();
        }

        Coroutine m_SuspendingCoroutine;
        void SuspendingPopup()
        {
            if (m_SuspendingCoroutine != null)
            {
                StopCoroutine(m_SuspendingCoroutine);
                m_SuspendingCoroutine = null;
            }
            m_SuspendingCoroutine = StartCoroutine(SuspendingToShowPopup());
        }

        IEnumerator SuspendingToShowPopup()
        {
            m_Suspending.SetActive(false);
            yield return new WaitForSeconds(1.0f);
            m_Suspending.SetActive(true);
        }

        void OnSortMethodValueChanged(int value)
        {
            ProjectSortMethod sortMethod = (ProjectSortMethod)value;
            switch (sortMethod)
            {
                case ProjectSortMethod.SortByDate:
                    UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetProjectSortMethod, ProjectSortMethod.SortByDate));
                    break;
                case ProjectSortMethod.SortByName:
                    UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetProjectSortMethod, ProjectSortMethod.SortByName));
                    break;
                default:
                    UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetProjectSortMethod, ProjectSortMethod.SortByDate));
                    break;
            }
        }

        Coroutine m_SearchCoroutine;
        void OnSearchInputTextChanged(string search)
        {
            if (m_SearchCoroutine != null)
            {
                StopCoroutine(m_SearchCoroutine);
                m_SearchCoroutine = null;
            }
            m_SearchCoroutine = StartCoroutine(SearchStringChanged(search));
        }

        IEnumerator SearchStringChanged(string search)
        {
            yield return new WaitForSeconds(0.2f);
            var data = UIStateManager.current.stateData.landingScreenFilterData;
            data.searchString = search;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetLandingScreenFilter, data));
        }

        void OnProjectTabButtonClicked(ProjectServerType projectServerType)
        {
            var data = UIStateManager.current.stateData.landingScreenFilterData;
            data.projectServerType = projectServerType;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetLandingScreenFilter, data));
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
            m_DialogButtonImage.enabled = data.activeDialog == DialogType.LandingScreen && m_DialogButton.interactable;

            if (data.selectedProjectOption != m_CurrentSelectedProject)
            {
                if (m_LastHighlightedItem != null)
                {
                    m_LastHighlightedItem.SetHighlight(false);
                    m_LastHighlightedItem = null;
                }

                if (data.selectedProjectOption != Project.Empty)
                {
                    var projectListItem = m_ProjectListItems.SingleOrDefault(e => e.project == data.selectedProjectOption);
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
                FilterProjectList(data.landingScreenFilterData);
                m_CurrentFilterData = data.landingScreenFilterData;

                if (data.landingScreenFilterData.projectServerType != m_CurrentServerType)
                {
                    m_ProjectTabController.SelectButtonType(data.landingScreenFilterData.projectServerType);
                    m_CurrentServerType = data.landingScreenFilterData.projectServerType;
                }
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
                    visible = visible && projectListItem.project.name.IndexOf(filterData.searchString, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (visible)
                {
                    projectListItem.SetHighlightString(filterData.searchString);
                    visibleCount++;
                }
                projectListItem.gameObject.SetActive(visible);
            }

            m_NoProjectPanel.SetActive(!m_WelcomePanel.activeSelf && visibleCount == 0);
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (data.activeProject != m_CurrentActiveProject)
            {
                m_DialogButton.interactable = data.activeProject != Project.Empty;
                m_CurrentActiveProject = data.activeProject;
                m_ActiveProjectListItem = m_ProjectListItems.FirstOrDefault(item => item.project == m_CurrentActiveProject);
            }

            if (m_ActiveProjectListItem != null && data.activeProjectThumbnail != m_ActiveProjectListItem.projectThumbnail)
            {
                m_ActiveProjectListItem.projectThumbnail = data.activeProjectThumbnail;
            }

            if (data.projectSortMethod != m_CurrentProjectSortMethod)
            {
                m_CurrentProjectSortMethod = data.projectSortMethod;
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
                        UIStateManager.current.Dispatcher.Dispatch(
                            Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.LandingScreen));
                        m_WelcomePanel.SetActive(false);
                        m_NoProjectPanel.SetActive(false);
                        m_FetchingProjectsPanel.SetActive(true);
                        m_RefreshButton.gameObject.SetActive(true);
                        break;
                    case LoginState.LoggedOut:
                        ClearProjectListItem();
                        UIStateManager.current.Dispatcher.Dispatch(
                            Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.LandingScreen));
                        UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseProject,
                            UIStateManager.current.projectStateData.activeProject));
                        m_WelcomePanel.SetActive(true);
                        m_NoProjectPanel.SetActive(false);
                        m_FetchingProjectsPanel.SetActive(false);
                        m_RefreshButton.gameObject.SetActive(false);
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
                if (m_Projects != data.sessionState.projects && !EnumerableExtension.SafeSequenceEquals(m_Projects, data.sessionState.projects))
                {
                    InstantiateProjectItems(data.sessionState.projects);
                    m_Projects = data.sessionState.projects;
                }
                else if (UIStateManager.current.stateData.progressData.progressState == ProgressData.ProgressState.NoPendingRequest && data.sessionState.projects.Length == 0)
                {
                    m_NoProjectPanel.SetActive(true);
                    m_FetchingProjectsPanel.SetActive(false);
                }
            }
           
        }

        void ClearProjectListItem()
        {
            foreach (var projectListItem in m_ProjectListItems)
            {
                Destroy(projectListItem.gameObject);
            }
            m_ProjectListItems.Clear();
        }

        void InstantiateProjectItems(Project[] projects)
        {
            ClearProjectListItem();
            m_ScrollRect.verticalNormalizedPosition = 1;

            if(m_FetchingProjectsPanel.activeSelf)
                m_FetchingProjectsPanel.SetActive(false);

            Array.Sort(projects, (project1, project2) => project2.lastPublished.CompareTo(project1.lastPublished));
            foreach (var project in projects)
            {
                var listItem = Instantiate(m_ProjectListItemPrefab, m_ScrollViewContent.transform);
                listItem.gameObject.SetActive(true);
                listItem.InitProjectItem(project, ThumbnailController.LoadThumbnailForProject(project));

                listItem.projectItemClicked += OnProjectOpenButtonClick;
                listItem.optionButtonClicked += OnProjectOptionButtonClick;
                m_ProjectListItems.Add(listItem);
            }

            m_ProjectListItemPrefab.gameObject.SetActive(false);

            FilterProjectList(m_CurrentFilterData);
        }

        void OnDialogButtonClick()
        {
            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.LandingScreen;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));

            if(dialogType == DialogType.None)
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.None));
        }

        void OnProjectOpenButtonClick(Project project)
        {
            var projectData = UIStateManager.current.projectStateData;

            if (projectData.activeProject == project)
            {
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
                //if the project already opened, just close landing screen

                return;
            }

            projectData.activeProject = project;
            projectData.activeProjectThumbnail = ThumbnailController.LoadThumbnailForProject(project);

            if (UIStateManager.current.projectStateData.activeProject != Project.Empty)
            {
                // first close current Project if open
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatus, "Closing {UIStateManager.current.projectStateData.activeProject.name}..."));
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseProject, UIStateManager.current.projectStateData.activeProject));
            }
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatus, $"Opening {projectData.activeProject.name}..."));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenProject, projectData));

        }

        void OnProjectOptionButtonClick(Project project)
        {
            var model = UIStateManager.current.stateData;
            if (model.activeOptionDialog == OptionDialogType.ProjectOptions && model.selectedProjectOption == project)
            {
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetOptionProject, Project.Empty));
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.None));
            }
            else
            {
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetOptionProject, project));
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.ProjectOptions));
            }
        }

        void OnTapDetectorDown(BaseEventData data)
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetOptionProject, Project.Empty));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.None));
        }


        void OnRefreshButtonClicked()
        {
            m_SearchInput.text = "";
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.RefreshProjectList, null));
        }

        void SortProjects()
        {
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

            switch (m_CurrentProjectSortMethod)
            {
                case ProjectSortMethod.SortByDate:
                    childrenProjectItems.Sort((project1, project2) => project2.project.lastPublished.CompareTo(project1.project.lastPublished));
                    break;
                case ProjectSortMethod.SortByName:
                    childrenProjectItems.Sort((project1, project2) => project1.project.name.CompareTo(project2.project.name));
                    break;
                default:
                    childrenProjectItems.Sort((project1, project2) => project2.project.lastPublished.CompareTo(project1.project.lastPublished));
                    break;
            }
            for (int i = 0; i < childrenProjectItems.Count; i++)
            {
                childrenProjectItems[i].gameObject.transform.SetSiblingIndex(i);
            }

        }
    }
}
