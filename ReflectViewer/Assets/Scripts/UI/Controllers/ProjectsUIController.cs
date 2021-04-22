using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
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

        List<ProjectListItem> m_ProjectListItems = new List<ProjectListItem>();

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
                InstantiateButtons(data.sessionState.rooms);
            }
        }

        void InstantiateButtons(ProjectRoom[] rooms)
        {
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
        }

        ProjectListItem GetProjectListItemAt(int index)
        {
            ProjectListItem item;
            if (index >= m_ProjectListItems.Count)
            {
                item = Instantiate(m_ProjectListItemPrefab, m_ScrollViewContent.transform);
                item.projectItemClicked += OnProjectSelectButtonClick;
                item.optionButtonClicked += OnProjectOptionButtonClick;
                m_ProjectListItems.Add(item);
            }
            else
            {
                item = m_ProjectListItems[index];
            }

            return item;
        }

        void OnDialogButtonClick()
        {
            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.Projects;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));

            if(dialogType == DialogType.None)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.None));
        }

        void OnProjectSelectButtonClick(Project project)
        {
            var projectData = UIStateManager.current.projectStateData;
            projectData.activeProject = project;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenProject, projectData));
        }

        void OnProjectOptionButtonClick(Project project)
        {
            var model = UIStateManager.current.stateData;
            if (model.activeOptionDialog == OptionDialogType.ProjectOptions && model.selectedProjectOption == project)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.None));
            }
            else
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetOptionProject, project));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.ProjectOptions));
            }
        }
    }
}
