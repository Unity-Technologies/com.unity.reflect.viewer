using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Reflect;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.Viewer
{
    public static class ReflectProjectsManager
    {
        static ProjectsLister s_ProjectLister;
        static ProjectsManager s_ProjectsManager;

        public static ProjectListerSettings.ProjectsEvents projectsRefreshCompleted = new ProjectListerSettings.ProjectsEvents();
        public static event Action<ProjectListRefreshException> projectsRefreshException;

        public static event StatusChanged projectStatusChanged;
        public static event ProgressChanged projectDownloadProgressChanged;
        public static event ProgressChanged projectDeleteProgressChanged;

        static ReflectProjectsManager()
        {
        }

        public static void Init(UnityUser user, IUpdateDelegate updater, IProjectProvider client) // TODO Rename or move into a ProjectLister manager class
        {
            if (s_ProjectLister != null || s_ProjectsManager != null)
            {
                Dispose();
            }

            // ProjectLister
            s_ProjectLister = new ProjectsLister(client);
            s_ProjectLister.projectListingCompleted += OnProjectListingCompleted;
            s_ProjectLister.projectListingException += OnProjectListingException;

            s_ProjectLister.SetUpdateDelegate(updater);

            s_ProjectsManager = new ProjectsManager(updater, user, Storage.main);
            s_ProjectsManager.projectStatusChanged += OnProjectStatusChanged;
            s_ProjectsManager.projectDownloadProgressChanged += OnProjectDownloadProgressChanged;
            s_ProjectsManager.projectDeleteProgressChanged += OnProjectDeleteProgressChanged;
        }

        static void OnProjectListingException(ProjectListRefreshException exception)
        {
            projectsRefreshException?.Invoke(exception);
        }

        static void OnProjectListingCompleted(IEnumerable<Project> projects)
        {
            projectsRefreshCompleted?.Invoke(projects.ToList());
        }

        static void OnProjectStatusChanged(Project project, ProjectsManager.Status status)
        {
            projectStatusChanged?.Invoke(project, status);
        }

        static void OnProjectDeleteProgressChanged(Project project, int progress, int total)
        {
            projectDeleteProgressChanged?.Invoke(project, progress, total);
        }

        static void OnProjectDownloadProgressChanged(Project project, int progress, int total)
        {
            projectDownloadProgressChanged?.Invoke(project, progress, total);
        }

        public static void Dispose()
        {
            if (s_ProjectLister != null)
            {
                s_ProjectLister.projectListingCompleted -= OnProjectListingCompleted;

                s_ProjectLister.Dispose();
                s_ProjectLister = null;
            }

            if (s_ProjectsManager != null)
            {
                s_ProjectsManager.projectStatusChanged -= OnProjectStatusChanged;
                s_ProjectsManager.projectDownloadProgressChanged -= OnProjectDownloadProgressChanged;
                s_ProjectsManager.projectDeleteProgressChanged -= OnProjectDeleteProgressChanged;

                s_ProjectsManager.Dispose();
                s_ProjectsManager = null;
            }
        }

        public static void RefreshProjects()
        {
            s_ProjectLister?.Run();
        }

        public static ProjectsManager.Status GetStatus(Project project)
        {
            return s_ProjectsManager.GetStatus(project);
        }

        public static bool IsReadyForOpening(Project project)
        {
            var status = s_ProjectsManager.GetStatus(project);

            if (status == ProjectsManager.Status.Unknown)
                return true;

            if (!project.IsConnectedToServer && !project.IsLocal)
                return false;

            return status != ProjectsManager.Status.Deleting && status != ProjectsManager.Status.QueuedForDelete
                && status != ProjectsManager.Status.Downloading && status != ProjectsManager.Status.QueuedForDownload;
        }

        public static void DownloadProject(Project project)
        {
            if (s_ProjectsManager == null)
                return;

            s_ProjectsManager.Download(project);
        }

        public static void DeleteProjectLocally(Project project)
        {
            s_ProjectsManager.Delete(project);
        }
    }
}
