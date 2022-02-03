using System;
using System.Linq;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer
{
    /// <summary>
    /// Provides Marker system components with current project data from the Reflect Viewer system
    /// </summary>
    public class MarkerProjectManager : MonoBehaviour
    {
        [SerializeField]
        MarkerController m_MarkerController;
        [SerializeField]
        MarkerGraphicManager m_GraphicManager;
        MarkerSyncStoreManager m_SyncStoreManager;
        IUISelector<AccessToken> m_AccessTokenSelector;
        IUISelector<Project> m_ActiveProjectSelector;

        void Start()
        {
            m_AccessTokenSelector = UISelectorFactory.createSelector<AccessToken>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.accessToken), OnAccessToken);
            m_ActiveProjectSelector = UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), OnActiveProjectChanged);
            if (m_GraphicManager == null)
                m_GraphicManager = FindObjectOfType<MarkerGraphicManager>();
        }

        void OnDestroy()
        {
            m_AccessTokenSelector?.Dispose();
        }

        void OnActiveProjectChanged(Project activeProject)
        {
            if ((activeProject == null || string.IsNullOrWhiteSpace(activeProject.projectId)) && m_MarkerController != null)
            {
                m_MarkerController.Available = false;
                m_MarkerController.UnsupportedMessage = null;
                m_MarkerController.LoadingComplete = false;
            }
        }

        void OnAccessToken(AccessToken accessToken)
        {
            if (accessToken == null)
                return;

            if (m_SyncStoreManager == null)
                m_SyncStoreManager = FindObjectOfType<MarkerSyncStoreManager>();
            try
            {
                if (m_SyncStoreManager)
                    m_SyncStoreManager.UpdateProject(m_ActiveProjectSelector.GetValue(), accessToken.UnityUser);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MarkerProjectManager.SetProject] Exception thrown updating project: {e}");
            }
            m_GraphicManager.UnityProject = accessToken.UnityProject;
            m_MarkerController.ReadOnly = ReadOnly(accessToken.UnityProject);
            // Now the project is set, mark as available.
            if (m_MarkerController.UnsupportedMessage == null)
                m_MarkerController.Available = true;
        }

        bool ReadOnly(UnityProject project)
        {
            // Check if there's publish permission in the access set, if so than mark as not read only.
            return !project.AccessSet.Contains(UnityProject.AccessType.Publish);
        }
    }
}
