using System;
using Unity.Reflect;
using Unity.Reflect.IO;
using Unity.Reflect.Viewer.UI;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    public class ViewerReflectPipeline : MonoBehaviour, IUpdateDelegate
    {
#pragma warning disable CS0649
        [SerializeField]
        ReflectPipeline m_ReflectPipeline;
#pragma warning restore CS0649

        public event Action<float> update;

        bool m_SyncEnabled = false;
        bool m_IsManifestDirty = false;

        Project m_SelectedProject;

        ReflectClient m_Client;
        AuthClient m_AuthClient;

        public bool TryGetNode<T>(out T node) where T : class, IReflectNode
        {
            return m_ReflectPipeline.TryGetNode(out node);
        }

        public bool HasPipelineAsset => m_ReflectPipeline != null && m_ReflectPipeline.pipelineAsset != null;

        // Streaming Events
        public void OpenProject(Project project)
        {
            if (m_AuthClient == null)
            {
                Debug.LogError("Unable to open project without a Authentication Client.");
                return;
            }

            if (m_SelectedProject != null)
            {
                Debug.LogError("Only one project can be opened at a time.");
                return;
            }

            m_SelectedProject = project;

            m_Client = new ReflectClient(this, m_AuthClient.user, m_AuthClient.storage, m_SelectedProject);
            m_Client.manifestUpdated += OnManifestUpdated;
            m_IsManifestDirty = false;

            m_ReflectPipeline.InitializeAndRefreshPipeline(m_Client);

            // TODO : SaveProjectData(project) saves "project.data" for the offline project.
            // Maybe we need to move/remove this code depends on the design.
            // "project.data" file is using to get "Offline Project List" and "Enable Delete Button" in the project option dialog now
            var storage = new PlayerStorage(ProjectServer.ProjectDataPath, true,false);
            storage.SaveProjectData(project);
        }

        public void SetSync(bool enabled)
        {
            m_SyncEnabled = enabled;
            if (enabled && m_IsManifestDirty)
            {
                m_ReflectPipeline.RefreshPipeline();
                m_IsManifestDirty = false;
            }
        }

        void OnManifestUpdated()
        {
            if (m_SyncEnabled)
            {
                m_ReflectPipeline.RefreshPipeline();
                m_IsManifestDirty = false;
            }
            else
            {
                m_IsManifestDirty = true;
            }
        }

        public void CloseProject()
        {
            if (m_ReflectPipeline == null)
            {
                return;
            }

            m_ReflectPipeline.ShutdownPipeline();

            if (m_Client != null)
            {
                m_Client.manifestUpdated -= OnManifestUpdated;
                m_Client.Dispose();
            }

            m_SelectedProject = null;
        }

        public void SetUser(UnityUser user)
        {
            if (user == null || string.IsNullOrEmpty(user.UserId))
            {
                Debug.LogError("Invalid User");
            }

            // Storage
            var storage = new PlayerStorage(ProjectServer.ProjectDataPath, true, false);

            // Client
            m_AuthClient = new AuthClient(user, storage);

            ReflectPipelineFactory.SetUser(user, this, m_AuthClient, storage);
        }

        public void ClearUser()
        {
            ReflectPipelineFactory.ClearUser();
        }

        void OnEnable()
        {
            m_ReflectPipeline.onException += OnPipelineException;
        }

        void OnDisable()
        {
            ClearUser();
            CloseProject();
            m_ReflectPipeline.ShutdownPipeline();
            m_ReflectPipeline.onException -= OnPipelineException;
            update = null;
        }

        static void OnPipelineException(Exception exception)
        {
            var ex = ExtractInnerException(exception);
            var errorMessage = UIStateManager.current.popUpManager.GetModalPopUpData();
            errorMessage.title = "Processing Error";
            errorMessage.text = $"An error occured while processing the Reflect model: {ex.Message}";
            UIStateManager.current.popUpManager.DisplayModalPopUp(errorMessage);
        }

        static Exception ExtractInnerException(Exception exception)
        {
            return exception.InnerException == null ? exception : ExtractInnerException(exception.InnerException);
        }

        void Update()
        {
            update?.Invoke(Time.unscaledDeltaTime);
        }
    }
}
