using System;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    public class ProjectTabController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ProjectTabButton[]  m_ProjectTabs;
#pragma warning restore CS0649

        public ProjectTabButton[] projectTabs => m_ProjectTabs;

        public event Action<SetLandingScreenFilterProjectServerAction.ProjectServerType> projectTabButtonClicked;

        void Start()
        {
            foreach (var projectTabButton in m_ProjectTabs)
            {
                projectTabButton.projectTabButtonClicked += OnProjectTabButtonClicked;
            }
        }

        void OnProjectTabButtonClicked(SetLandingScreenFilterProjectServerAction.ProjectServerType projectServerType)
        {
            projectTabButtonClicked?.Invoke(projectServerType);
        }

        public void SelectButtonType(SetLandingScreenFilterProjectServerAction.ProjectServerType projectServerType)
        {
            foreach (var projectTabButton in m_ProjectTabs)
            {
                projectTabButton.SelectButton(projectTabButton.type == projectServerType);
            }
        }
    }
}
