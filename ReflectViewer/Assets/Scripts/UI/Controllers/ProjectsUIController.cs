using System;
using SharpFlux;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Select new active project, download projects, manage projects
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class ProjectsUIController : MonoBehaviour
    {

#pragma warning disable CS0649
        [SerializeField]
        Button m_DialogButton;
        [SerializeField, Tooltip("Reference to the button prefab.")]
        ProjectListItem m_ProjectListItemPrefab;
        [SerializeField]
        GameObject m_ScrollViewContent;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        Image m_DialogButtonImage;

        void Awake()
        {
            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();

            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.sessionStateChanged += OnSessionStateDataChanged;

            m_DialogWindow = GetComponent<DialogWindow>();
        }

        void Start()
        {
            m_DialogButton.onClick.AddListener(OnDialogButtonClick);
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_DialogButtonImage.enabled = data.activeDialog == DialogType.Projects;
        }

        void OnSessionStateDataChanged(UISessionStateData data)
        {
            m_DialogButton.interactable = data.sessionState.loggedState == LoginState.LoggedIn;
            if (data.sessionState.loggedState == LoginState.LoggedIn)
            {
                InstantiateButtons(data.sessionState.projects);
            }
        }

        void InstantiateButtons(Project[] projects)
        {
            Array.Sort(projects, (project1, project2) => project2.lastPublished.CompareTo(project1.lastPublished));
            foreach (var project in projects)
            {
                var listItem = Instantiate(m_ProjectListItemPrefab, m_ScrollViewContent.transform);
                listItem.gameObject.SetActive(true);
                listItem.InitProjectItem(project, ThumbnailController.LoadThumbnailForProject(project));

                listItem.projectItemClicked += OnProjectSelectButtonClick;
                listItem.optionButtonClicked += OnProjectOptionButtonClick;
            }
        }

        void OnDialogButtonClick()
        {
            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.Projects;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));

            if(dialogType == DialogType.None)
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.None));
        }

        void OnProjectSelectButtonClick(Project project)
        {
            var projectData = UIStateManager.current.projectStateData;
            projectData.activeProject = project;

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenProject, projectData));
        }

        void OnProjectOptionButtonClick(Project project)
        {
            var model = UIStateManager.current.stateData;
            if (model.activeOptionDialog == OptionDialogType.ProjectOptions && model.selectedProjectOption == project)
            {
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.None));
            }
            else
            {
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetOptionProject, project));
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.ProjectOptions));
            }
        }
    }
}
