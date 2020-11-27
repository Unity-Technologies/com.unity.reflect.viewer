using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using SharpFlux.Stores;
using Unity.MARS;
using Unity.MARS.Data;
using Unity.MARS.Providers;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Pipeline;
using UnityEngine.SceneManagement;
using Bounds = UnityEngine.Bounds;
using Unity.TouchFramework;
#if UNITY_EDITOR
using UnityEditor.MARS.Simulation;
#endif

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Component that hold the state of the UI.
    /// </summary>
    public class UIStateManager : MonoBehaviour,
        IStore<UIStateData>, IStore<UISessionStateData>, IStore<UIProjectStateData>, IStore<UIARStateData>, IStore<UIDebugStateData>, IStore<ApplicationStateData>,
        IUsesSessionControl, IUsesPointCloud, IUsesPlaneFinding
    {
        readonly object syncRoot = new object();
        static UIStateManager s_Current;

#pragma warning disable CS0649
        [SerializeField]
        ViewerReflectPipeline m_ReflectPipeline;
        [SerializeField]
        PopUpManager m_PopUpManager;
        [SerializeField]
        float m_WaitingDelayToCloseStreamIndicator = 1f;
#pragma warning restore CS0649

        OrbitModeUIController m_OrbitModeUIController;
        ThumbnailController m_ThumbnailController;
        const float k_Timeout = 0.5f;
        Coroutine m_WaitStreamIndicatorCoroutine;
        WaitForSeconds m_WaitDelay;

        public static UIStateManager current => s_Current;

        IDispatcher dispatcher;
        public IDispatcher Dispatcher => dispatcher ?? (dispatcher = new Dispatcher());

        /// <summary>
        /// Event signaled when the state of the UI has changed
        /// </summary>
        public static event Action<UIStateData> stateChanged = delegate {};

        /// <summary>
        /// Event signaled when the state of the session has changed
        /// </summary>
        public static event Action<UISessionStateData> sessionStateChanged = delegate {};

        /// <summary>
        /// Event signaled when the state of the Project has changed
        /// </summary>
        public static event Action<UIProjectStateData> projectStateChanged = delegate {};

        /// <summary>
        /// Event signaled when the state of the AR Simulation has changed
        /// </summary>
        public static event Action<UIARStateData> arStateChanged = delegate {};

        public static event Action<UIDebugStateData> debugStateChanged = delegate {};

        /// <summary>
        /// Event signaled when the state of the Application has changed
        /// </summary>
        public static event Action<ApplicationStateData> applicationStateChanged = delegate { };

        [SerializeField, Tooltip("Reflect Session Manager")]
        public LoginManager m_LoginManager;
        public SunStudy.SunStudy m_SunStudy;
        public GameObject m_RootNode;
        public GameObject m_BoundingBoxRootNode;
        public GameObject m_PlacementRoot;
        public List<GameObject> m_PlacementRulesPrefabs;
        [SerializeField, Tooltip("State of the UI")]
        UIStateData m_UIStateData;
        [SerializeField, Tooltip("State of the Session")]
        UISessionStateData m_UISessionStateData;
        [SerializeField, Tooltip("State of the Project")]
        UIProjectStateData m_UIProjectStateData;
        [SerializeField, Tooltip("State of the AR Simulation")]
        UIARStateData m_ARStateData;
        [SerializeField, Tooltip("State of the Debug Data")]
        UIDebugStateData m_UIDebugStateData;
        [SerializeField, Tooltip("State of the Application Data")]
        ApplicationStateData m_ApplicationStateData;
        [SerializeField]
        bool m_VerboseLogging;

        public PopUpManager popUpManager => m_PopUpManager;
        /// <summary>
        /// State of the UI
        /// </summary>
        public UIStateData stateData { get => m_UIStateData; }

        /// <summary>
        /// State of the Session
        /// </summary>
        public UISessionStateData sessionStateData { get  => m_UISessionStateData; }

        /// <summary>
        /// State of the Project
        /// </summary>
        public UIProjectStateData projectStateData { get => m_UIProjectStateData; }

        /// <summary>
        /// State of the Project
        /// </summary>
        public UIARStateData arStateData { get => m_ARStateData; }

        public UIDebugStateData debugStateData { get => m_UIDebugStateData; }

        /// <summary>
        /// State of the Application
        /// </summary>
        public ApplicationStateData applicationStateData { get => m_ApplicationStateData; }


        public GameObject m_PlacementRules;
        MetadataFilterNode m_MetadataFilter;
        LightFilterNode m_LightFilterNode;

        void Awake()
        {
            DetectCapabilities();

            // set Project.Empty for the NonSerialized value
            m_UIStateData.selectedProjectOption = Project.Empty;
            m_UIProjectStateData.activeProject = Project.Empty;
            m_UIProjectStateData.activeProjectThumbnail = null;
            m_ApplicationStateData.qualityStateData = QualityState.defaultData;

            m_LoginManager.userLoggedIn.AddListener(OnUserLoggedIn);
            m_LoginManager.userLoggedOut.AddListener(OnUserLoggedOut);

            if (s_Current == null)
            {
                s_Current = this;
            }

            DispatchToken = Subscribe();

            m_OrbitModeUIController = GetComponent<OrbitModeUIController>();
            m_ThumbnailController = GetComponent<ThumbnailController>();


            m_WaitDelay = new WaitForSeconds(m_WaitingDelayToCloseStreamIndicator);
#if UNITY_EDITOR
            SimulationSettings.instance.ShowSimulatedEnvironment = false;
            SimulationSettings.instance.ShowSimulatedData = false;
#endif
        }

        void DetectCapabilities()
        {
            m_UIStateData.deviceCapability = DeviceCapability.None;
#if UNITY_EDITOR
            m_UIStateData.deviceCapability |= DeviceCapability.VRCapability | DeviceCapability.ARCapability;
#elif UNITY_IOS || UNITY_ANDROID
            m_UIStateData.deviceCapability |= DeviceCapability.ARCapability;
#elif UNITY_STANDALONE_WIN
            m_UIStateData.deviceCapability |= DeviceCapability.VRCapability;
#endif
        }

        IProvidesSessionControl IFunctionalitySubscriber<IProvidesSessionControl>.provider { get; set; }
        IProvidesPointCloud IFunctionalitySubscriber<IProvidesPointCloud>.provider { get; set; }
        IProvidesPlaneFinding IFunctionalitySubscriber<IProvidesPlaneFinding>.provider { get; set; }

        IEnumerator Start()
        {
            while (!this.SessionReady() && Time.time < k_Timeout)
            {
                yield return null;
            }

            yield return null;

            this.StopDetectingPlanes();
            this.StopDetectingPoints();
            this.PauseSession();
            m_OrbitModeUIController.ResetCamera();

            yield return null;

            if (s_Current == this)
            {
                OnAppStateChanged();
                OnSessionStateChanged();
                OnProjectStateChanged();
                OnARStateChanged();
            }
        }

        public bool verboseLogging
        {
            get => m_VerboseLogging;
            set => m_VerboseLogging = value;
        }

        void OnAppStateChanged()
        {
            ForceSendStateChangedEvent();
        }

        void OnSessionStateChanged()
        {

        }

        void OnProjectStateChanged()
        {
            ForceSendProjectStateChangedEvent();
        }

        void OnARStateChanged()
        {
            ForceSendARStateChangedEvent();
        }

        void OnUserLoggedIn(UnityUser changedUser)
        {
            // clear status message
            m_UIStateData.LogStatusMessage(String.Empty);

            // update session state
            m_UISessionStateData.sessionState.loggedState = LoginState.LoggedIn;
            m_UISessionStateData.sessionState.user = changedUser;
            //  prepare the pipeline
            m_ReflectPipeline.SetUser(changedUser);
            // connect Pipeline events
            ConnectPipelineEvents();
            // connect all Pipeline Factory events
            ConnectPipelineFactoryEvents();
            // refreshProjects
            ReflectPipelineFactory.RefreshProjects();

            m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingIndeterminate;
            stateChanged?.Invoke(m_UIStateData);

            sessionStateChanged?.Invoke(sessionStateData);
        }

        void ConnectPipelineFactoryEvents()
        {
            // listing projects events
            ReflectPipelineFactory.projectsRefreshCompleted.AddListener(OnProjectsRefreshCompleted);
            ReflectPipelineFactory.projectLocalDataDeleted +=  OnProjectLocalDataDeleted;
            ReflectPipelineFactory.projectDeleteProgressChanged +=  OnProjectDeleteProgressChanged;

            ReflectPipelineFactory.projectDataDownloaded +=  OnProjectDataDownloaded;
            ReflectPipelineFactory.projectDownloadProgressChanged +=  OnProjectDownloadProgressChanged;
        }

        void ConnectPipelineEvents()
        {
            if (m_ReflectPipeline == null)
                return;

            // initial bounds
            if (m_ReflectPipeline.TryGetNode<SpatialFilterNode>(out var spatialFilterNode))
            {
                spatialFilterNode.settings.onGlobalBoundsCalculated.AddListener(OnBoundsChanged);
            }
            // use BoundingBoxFilter as a fallback if there is no SpatialFilter in this pipeline
            else if (m_ReflectPipeline.TryGetNode<BoundingBoxFilterNode>(out var boundingBoxFilterNode))
            {
                boundingBoxFilterNode.settings.onBoundsCalculated.AddListener(OnBoundsChanged);
            }

            if (m_ReflectPipeline.TryGetNode<MetadataFilterNode>(out var metadataFilterNode))
            {
                metadataFilterNode.settings.groupsChanged.AddListener(OnMetadataGroupsChanged);
                metadataFilterNode.settings.categoriesChanged.AddListener(OnMetadataCategoriesChanged);
            }

            if (m_ReflectPipeline.TryGetNode<StreamIndicatorNode>(out var streamIndicatorNode))
            {
                streamIndicatorNode.settings.instanceStreamBegin.AddListener(OnInstanceStreamBegin);
                streamIndicatorNode.settings.instanceStreamEnd.AddListener(OnInstanceStreamEnd);
                streamIndicatorNode.settings.gameObjectStreamEvent.AddListener(OnGameObjectStreamEvent);
                streamIndicatorNode.settings.gameObjectStreamEnd.AddListener(OnGameObjectStreamEnd);

                streamIndicatorNode.settings.assetCountModified.AddListener(OnAssetCountModified);
                streamIndicatorNode.settings.instanceCountModified.AddListener(OnInstanceCountModified);
                streamIndicatorNode.settings.gameObjectCountModified.AddListener(OnGameObjectCountModified);
            }
        }

        void OnAssetCountModified(StreamCountData streamCountData)
        {
            m_UIDebugStateData.statsInfoData.assetsCountData = streamCountData;
            debugStateChanged?.Invoke(m_UIDebugStateData);
        }

        void OnInstanceCountModified(StreamCountData streamCountData)
        {
            m_UIDebugStateData.statsInfoData.instancesCountData = streamCountData;
            debugStateChanged?.Invoke(m_UIDebugStateData);
        }

        void OnGameObjectCountModified(StreamCountData streamCountData)
        {
            m_UIDebugStateData.statsInfoData.gameObjectsCountData = streamCountData;
            debugStateChanged?.Invoke(m_UIDebugStateData);
        }

        bool m_InstanceStreamEnd;

        void OnGameObjectStreamEvent(int currentCount, int totalCount)
        {
            if (m_InstanceStreamEnd)
            {
                m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingDeterminate;
                m_UIStateData.progressData.currentProgress = Math.Min(currentCount, totalCount);
                m_UIStateData.progressData.totalCount = totalCount;
                m_UIStateData.progressData.message = "Streaming...";
                m_UIStateData.LogStatusMessage($"Streaming... {currentCount}/{totalCount}");
                stateChanged?.Invoke(m_UIStateData);

                if (m_WaitStreamIndicatorCoroutine != null)
                {
                    StopCoroutine(m_WaitStreamIndicatorCoroutine);
                }

                m_WaitStreamIndicatorCoroutine = StartCoroutine(WaitCloseStreamIndicator());
            }
        }

        IEnumerator WaitCloseStreamIndicator()
        {
            yield return m_WaitDelay;

            if(!m_ARStateData.arEnabled || m_ARStateData.instructionUIState == InstructionUIState.Completed)
            {
                m_UIProjectStateData.activeProjectThumbnail = m_ThumbnailController.CaptureActiveProjectThumbnail(current.projectStateData);
                projectStateChanged?.Invoke(m_UIProjectStateData);
            }

            m_UIStateData.progressData.progressState = ProgressData.ProgressState.NoPendingRequest;
            m_UIStateData.LogStatusMessage(String.Empty);
            stateChanged?.Invoke(m_UIStateData);

            m_WaitStreamIndicatorCoroutine = null;
        }

        void OnInstanceStreamEnd()
        {
            m_InstanceStreamEnd = true;
        }

        void OnInstanceStreamBegin()
        {
            m_InstanceStreamEnd = false;
            m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingIndeterminate;
            stateChanged?.Invoke(m_UIStateData);
        }

        void OnGameObjectStreamEnd()
        {
        }

        void OnBoundsChanged(Bounds bb)
        {
            m_UIProjectStateData.rootBounds = bb;
            projectStateChanged?.Invoke(projectStateData);
        }

        void OnMetadataGroupsChanged(IEnumerable<string> groups)
        {
            if(!EnumerableExtension.SafeSequenceEquals(groups, m_UIProjectStateData.filterGroupList))
            {
                m_UIProjectStateData.filterGroupList = new List<string>(groups);
                projectStateChanged?.Invoke(m_UIProjectStateData);
            }
        }

        void OnMetadataCategoriesChanged(string group, IEnumerable<string> categories)
        {
            if(m_UIStateData.filterGroup == group)
            {
                m_UIProjectStateData.filterItemInfos = new List<FilterItemInfo>(GetFilterItemInfos(m_UIStateData.filterGroup));
                projectStateChanged?.Invoke(m_UIProjectStateData);
            }
        }

        IEnumerable<FilterItemInfo> GetFilterItemInfos(string groupKey)
        {
            var filterKeys = m_MetadataFilter.processor.GetFilterKeys(m_UIStateData.filterGroup);
            var filterItemInfo = new FilterItemInfo
            {
                groupKey = groupKey
            };
            foreach (var filterKey in filterKeys)
            {
                filterItemInfo.filterKey = filterKey;
                filterItemInfo.visible = m_MetadataFilter.processor.IsVisible(groupKey, filterKey);
                filterItemInfo.highlight = m_MetadataFilter.processor.IsHighlighted(groupKey, filterKey);
                yield return filterItemInfo;
            }
        }

        int m_maxObjectLeft = -1;
        int m_lastObjectCount = -1;
        void OnProgressChanged(int objectsLeft)
        {
            int ol = objectsLeft;
            if (objectsLeft > m_maxObjectLeft)
            {
                m_maxObjectLeft = objectsLeft;
            }

            if (objectsLeft > m_lastObjectCount)
            {
                m_maxObjectLeft = objectsLeft;
            }

            m_lastObjectCount = objectsLeft;
            if (ol == 0)
            {
                m_UIStateData.LogStatusMessage(String.Empty);
            }
            else
            {
                m_UIStateData.LogStatusMessage($"Streaming Object {m_maxObjectLeft - ol}/{m_maxObjectLeft}");
            }
            stateChanged?.Invoke(stateData);
        }

        void OnUserLoggedOut()
        {
            // clear status message
            m_UIStateData.LogStatusMessage(String.Empty);
            stateChanged?.Invoke(stateData);
            // update session state
            m_UISessionStateData.sessionState.loggedState = LoginState.LoggedOut;
            m_UISessionStateData.sessionState.user = null;
            sessionStateChanged?.Invoke(sessionStateData);
        }

        void OnProjectsRefreshCompleted(List<Project> projects)
        {
            m_UIStateData.progressData.progressState = ProgressData.ProgressState.NoPendingRequest;
            ForceSendStateChangedEvent();

            m_UISessionStateData.sessionState.projects = projects.ToArray();
            ForceSendSessionStateChangedEvent();

            // TODO move to new deepLink data state
            if (m_LoginManager.deepLinkRoute.Equals(DeepLinkRoute.openprojectrequest) && m_LoginManager.deepLinkArgs.Count >= 4)
            {
                var openProjectRequested = projects.Select(x => x).Where(x => x.host.ServerId.Equals(m_LoginManager.deepLinkArgs[1]) && x.projectId.Equals(m_LoginManager.deepLinkArgs[3])).FirstOrDefault();
                // Re-initialise deepLink state
                m_LoginManager.deepLinkRoute = DeepLinkRoute.none;
                m_LoginManager.deepLinkArgs.Clear();

                if (openProjectRequested != null)
                {
                    OpenProject(openProjectRequested);
                }
            }
        }

        void OnProjectLocalDataDeleted(Project project)
        {
            m_UIStateData.progressData.progressState = ProgressData.ProgressState.NoPendingRequest;
            m_UIStateData.selectedProjectOption = project;
            m_UIStateData.projectOptionIndex++;
            stateChanged?.Invoke(m_UIStateData);
        }

        void OnProjectDeleteProgressChanged(int progress, int total, string message)
        {
            m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingDeterminate;
            m_UIStateData.progressData.currentProgress = progress;
            m_UIStateData.progressData.totalCount = total;
            m_UIStateData.progressData.message = message;
            stateChanged?.Invoke(m_UIStateData);
        }

        void OnProjectDataDownloaded(Project project)
        {
            m_UIStateData.progressData.progressState = ProgressData.ProgressState.NoPendingRequest;
            m_UIStateData.selectedProjectOption = project;
            m_UIStateData.projectOptionIndex++;
            stateChanged?.Invoke(m_UIStateData);
        }

        void OnProjectDownloadProgressChanged(int progress, int total, string message)
        {
            m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingDeterminate;
            m_UIStateData.progressData.currentProgress = progress;
            m_UIStateData.progressData.totalCount = total;
            m_UIStateData.progressData.message = message;
            stateChanged?.Invoke(m_UIStateData);
        }

        /// <summary>
        /// Invoke model changed event.
        /// </summary>
        public void ForceSendStateChangedEvent()
        {
            stateChanged.Invoke(m_UIStateData);
        }

        /// <summary>
        /// Invoke model changed event.
        /// </summary>
        public void ForceSendSessionStateChangedEvent()
        {
            sessionStateChanged.Invoke(m_UISessionStateData);
        }

        /// <summary>
        /// Invoke Project changed event.
        /// </summary>
        public void ForceSendProjectStateChangedEvent()
        {
            projectStateChanged.Invoke(m_UIProjectStateData);
        }

        /// <summary>
        /// Invoke AR Simulation changed event.
        /// </summary>
        public void ForceSendARStateChangedEvent()
        {
            arStateChanged.Invoke(m_ARStateData);
        }

        IEnumerator LoadAsyncScene(string scenePath)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            stateChanged.Invoke(m_UIStateData);
            projectStateChanged.Invoke(m_UIProjectStateData);
        }

        IEnumerator UnloadAsyncScene(string scenePath)
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(scenePath);

            // Wait until the asynchronous scene fully loads
            while (!asyncUnload.isDone)
            {
                yield return null;
            }
        }

        void CloseProject()
        {
            if (m_UIProjectStateData.activeProject != Project.Empty)
            {
                m_UIStateData.statusMessage = $"Closing {m_UIProjectStateData.activeProject.name}...";

                m_ReflectPipeline?.CloseProject();
                m_UIStateData.toolbarsEnabled = false;
                m_UIStateData.navigationState.EnableAllNavigation(false);
                stateChanged?.Invoke(m_UIStateData);
                m_UIProjectStateData.activeProject = Project.Empty;
                m_UIProjectStateData.activeProjectThumbnail = null;
                m_UIProjectStateData.objectSelectionInfo = default;
                projectStateChanged?.Invoke(projectStateData);
            }
        }

        void CloseAllDialogs()
        {
            m_UIStateData.activeDialog = DialogType.None;
            m_UIStateData.activeOptionDialog = OptionDialogType.None;
            m_UIStateData.activeSubDialog = DialogType.None;
            stateChanged?.Invoke(m_UIStateData);
        }

        void OpenProject(Project project)
        {
            CloseProject();

            CloseAllDialogs();

            m_UIProjectStateData.activeProject = project;

            m_UIStateData.statusMessage = $"Opening {m_UIProjectStateData.activeProject.name}...";
            stateChanged?.Invoke(m_UIStateData);

            m_UIProjectStateData.activeProjectThumbnail = ThumbnailController.LoadThumbnailForProject(m_UIProjectStateData.activeProject);
            projectStateChanged?.Invoke(projectStateData);

            if (m_ReflectPipeline != null)
            {
                m_ReflectPipeline.OpenProject(projectStateData.activeProject);
                m_ReflectPipeline.TryGetNode(out m_MetadataFilter);
                m_ReflectPipeline.TryGetNode(out m_LightFilterNode);
            }

            m_ReflectPipeline.TryGetNode(out m_SpatialFilter);

            // set enable texture and light
            if (m_UIStateData.sceneOptionData.enableTexture)
                Shader.SetGlobalFloat(k_UseTexture, 1);
            else
                Shader.SetGlobalFloat(k_UseTexture, 0);

            if (m_LightFilterNode != null)
            {
                m_UIStateData.sceneOptionData.enableLightData = m_LightFilterNode.settings.enableLights;
            }

            m_BoundingBoxRootNode.SetActive(true);
            if (m_ReflectPipeline.TryGetNode<SpatialFilterNode>(out var spatialFilterNode))
            {
                spatialFilterNode.settings.displayOnlyBoundingBoxes = false;
                m_UIProjectStateData.teleportPicker = new SpatialSelector
                {
                    SpatialPicker = m_SpatialFilter.SpatialPicker,
                    WorldRoot = m_RootNode.transform
                };
            }

            // reset the toolbars
            m_UIStateData.toolbarsEnabled = true;
            m_UIStateData.toolState.activeTool = ToolType.OrbitTool;
            m_UIStateData.toolState.orbitType = OrbitType.OrbitAtPoint;
            m_UIStateData.navigationState.EnableAllNavigation(true);

            m_UIStateData.settingsToolStateData = new SettingsToolStateData
            {
                bimFilterEnabled = true,
                sceneOptionEnabled = true,
                sunStudyEnabled = true
            };

            stateChanged?.Invoke(m_UIStateData);
        }


        UIStateData IStore<UIStateData>.Data => m_UIStateData;

        UISessionStateData IStore<UISessionStateData>.Data => m_UISessionStateData;

        UIProjectStateData IStore<UIProjectStateData>.Data => m_UIProjectStateData;

        UIARStateData IStore<UIARStateData>.Data => m_ARStateData;

        UIDebugStateData IStore<UIDebugStateData>.Data => m_UIDebugStateData;

        ApplicationStateData IStore<ApplicationStateData>.Data => m_ApplicationStateData;

        public string DispatchToken { get; private set; }

        //Returns whether the store has changed during the most recent dispatch
        bool hasChanged;
        static readonly int k_UseTexture = Shader.PropertyToID("_UseTexture");
        SpatialFilterNode m_SpatialFilter;

        public bool HasChanged {
            get { return hasChanged; }
            set
            {
                if (!Dispatcher.IsDispatching)
                    throw new InvalidOperationException("Must be invoked while dispatching.");
                hasChanged = value;
            }
        }

        //Dispatcher-forwarded methods so the API users do not have to care about the Dispatcher
        protected string Subscribe()
        {
            return Dispatcher.Register<Payload<ActionTypes>>(InvokeOnDispatch);
        }

        protected void Unsubscribe(string dispatchToken)
        {
            Dispatcher.Unregister(dispatchToken);
        }

        protected void WaitFor(IEnumerable<string> dispatchTokens)
        {
            Dispatcher.WaitFor<ActionTypes>(dispatchTokens);
        }

        //This is the store's registered callback method and all the logic that will be executed is contained here
        //Only place where state's mutation should happen
        private void InvokeOnDispatch(Payload<ActionTypes> payload)
        {
            HasChanged = false;

            lock (syncRoot)
            {
                OnDispatch(payload);
            }
        }

        IEnumerator TakeDelayedThumbnail()
        {
            yield return new WaitForSeconds(1.0f);
            m_UIProjectStateData.activeProjectThumbnail = m_ThumbnailController.CaptureActiveProjectThumbnail(current.projectStateData);
            projectStateChanged?.Invoke(m_UIProjectStateData);
        }

        void OnDispatch(Payload<ActionTypes> payload)
        {
            switch (payload.ActionType)
            {
                case ActionTypes.Login:
                {
                    m_UISessionStateData.sessionState.loggedState = LoginState.LoggingIn;
                    m_LoginManager.Login();
                    sessionStateChanged?.Invoke(sessionStateData);
                    break;
                }
                case ActionTypes.Logout:
                {
                    m_UISessionStateData.sessionState.loggedState = LoginState.LoggingOut;
                    m_LoginManager.Logout();
                    sessionStateChanged?.Invoke(sessionStateData);
                    break;
                }
                case ActionTypes.OpenURL:
                {
                    Application.OpenURL((string) payload.Data);
                    break;
                }
                case ActionTypes.SetToolState:
                {
                    var toolState = (ToolState) payload.Data;
                    m_UIStateData.toolState = toolState;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.OpenDialog:
                {
                    var dialogType = (DialogType) payload.Data;
                    m_UIStateData.activeDialog = dialogType;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.OpenSubDialog:
                {
                    var dialogType = (DialogType) payload.Data;
                    m_UIStateData.activeSubDialog = dialogType;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetProgressIndicator:
                {
                    var progressIndicatorData = (ProgressData) payload.Data;
                    m_UIStateData.progressData = progressIndicatorData;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetSync:
                {
                    var enabled = (bool)payload.Data;
                    m_ReflectPipeline.SetSync(enabled);
                    m_UIStateData.syncEnabled = enabled;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetActiveToolbar:
                {
                    var toolbarType = (ToolbarType)payload.Data;
                    m_UIStateData.activeToolbar = toolbarType;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.OpenOptionDialog:
                {
                    var optionDialogType = (OptionDialogType) payload.Data;
                    m_UIStateData.activeOptionDialog = optionDialogType;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetProjectSortMethod:
                {
                    var sortMethod = (ProjectSortMethod) payload.Data;
                    m_UIProjectStateData.projectSortMethod = sortMethod;
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }
                case ActionTypes.SetOptionProject:
                {
                    var project = (Project) payload.Data;
                    m_UIStateData.selectedProjectOption = project;
                    m_UIStateData.projectOptionIndex = 0;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.CloseAllDialogs:
                {
                    CloseAllDialogs();
                    break;
                }
                case ActionTypes.SetDialogMode:
                {
                    var dialogMode = (DialogMode)payload.Data;
                    m_UIStateData.dialogMode = dialogMode;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetHelpModeID:
                {
                    var activeEntry = (HelpModeEntryID)payload.Data;
                    m_UIStateData.helpModeEntryId = activeEntry;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetStatus:
                {
                    m_UIStateData.LogStatusMessage((string) payload.Data);
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetStatusWithLevel:
                {
                    var message = (StatusMessageData)payload.Data;
                    m_UIStateData.LogStatusMessage(message.text, message.level);
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetStatusLevel:
                {
                    var level = (StatusMessageLevel) payload.Data;
                    m_UIStateData.statusMessageLevel = level;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.ClearStatus:
                {
                    m_UIStateData.statusMessage = String.Empty;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetObjectPicker:
                {
                    SpatialSelector picker = (SpatialSelector)payload.Data;
                    picker.SpatialPicker = m_SpatialFilter.SpatialPicker;
                    picker.WorldRoot = m_RootNode.transform;
                    m_UIProjectStateData.objectPicker = picker;
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }
                case ActionTypes.OpenProject:
                {
                    UIProjectStateData projectData = (UIProjectStateData)payload.Data;
                    OpenProject(projectData.activeProject);
                    break;
            }
                case ActionTypes.CloseProject:
                {
                    CloseProject();
                    break;
                }
                case ActionTypes.DownloadProject:
                {
                    var project = (Project) payload.Data;
                    ReflectPipelineFactory.DownloadProject(project);

                    m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingIndeterminate;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.RemoveProject:
                {
                    var project = (Project) payload.Data;
                    ReflectPipelineFactory.DeleteProjectLocally(project);

                    m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingIndeterminate;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetVisibleFilter:
                {
                    var filterItemInfo = (FilterItemInfo) payload.Data;
                    m_MetadataFilter.processor.SetVisibility(filterItemInfo.groupKey, filterItemInfo.filterKey, filterItemInfo.visible);

                    m_UIProjectStateData.lastChangedFilterItem = new FilterItemInfo
                    {
                        groupKey = filterItemInfo.groupKey,
                        filterKey = filterItemInfo.filterKey,
                        visible = filterItemInfo.visible,
                    };
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }

                case ActionTypes.SetHighlightFilter:
                {
                    var highlightFilterInfo = (HighlightFilterInfo) payload.Data;

                    if (m_MetadataFilter.processor.IsHighlighted(highlightFilterInfo.groupKey, highlightFilterInfo.filterKey))
                    {
                        m_UIProjectStateData.highlightFilter.groupKey = String.Empty;
                        m_UIProjectStateData.highlightFilter.filterKey = String.Empty;
                    }
                    else
                    {
                        m_UIProjectStateData.highlightFilter = highlightFilterInfo;
                    }

                    m_MetadataFilter.processor.SetHighlightFilter(highlightFilterInfo.groupKey, highlightFilterInfo.filterKey);
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }
                case ActionTypes.SetFilterGroup:
                {
                    m_UIStateData.filterGroup = (string) payload.Data;
                    stateChanged?.Invoke(m_UIStateData);

                    m_UIProjectStateData.filterItemInfos = new List<FilterItemInfo>(GetFilterItemInfos(m_UIStateData.filterGroup));
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }
                case ActionTypes.SetViewOption:
                {
                    var sceneOptionData = (SceneOptionData) payload.Data;

                    if (sceneOptionData.enableTexture != m_UIStateData.sceneOptionData.enableTexture)
                    {
                        // set enable texture
                        if (sceneOptionData.enableTexture)
                            Shader.SetGlobalFloat(k_UseTexture, 1);
                        else
                            Shader.SetGlobalFloat(k_UseTexture, 0);

                    }

                    if (m_LightFilterNode != null && sceneOptionData.enableLightData != m_UIStateData.sceneOptionData.enableLightData)
                    {
                        m_LightFilterNode.settings.enableLights = sceneOptionData.enableLightData;
                        m_LightFilterNode.processor.RefreshLights();
                    }

                    m_UIStateData.sceneOptionData = sceneOptionData;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetSkybox:
                {
                    var sceneOptionData = (SceneOptionData) payload.Data;

                    // set skybox option
                    // sceneOptionData.skyboxData

                    m_UIStateData.sceneOptionData = sceneOptionData;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetClimateOption:
                {
                    var sceneOptionData = (SceneOptionData) payload.Data;

                    // sceneOptionData.climateSimulation;
                    // sceneOptionData.weatherType;
                    // sceneOptionData.temperature;

                    m_UIStateData.sceneOptionData = sceneOptionData;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetSunStudy:
                {
                    var sunStudyData = (SunStudyData)payload.Data;
                    sunStudyData = SunStudyData.Format(sunStudyData);
                    SunStudyData.SetSunStudyData(m_SunStudy, sunStudyData);
                    m_UIStateData.sunStudyData = sunStudyData;
                    m_UIStateData.LogStatusMessage(SunStudyData.GetSunStatusMessage(m_UIStateData.activeToolbar, m_UIStateData.sunStudyData, m_UIStateData.statusMessage));
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetSunStudyMode:
                {
                    var isStatic = (bool)payload.Data;
                    m_UIStateData.sunStudyData.isStaticMode = isStatic;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetCameraOption:
                {
                    var cameraOptionData = (CameraOptionData)payload.Data;
                    if (m_UIStateData.cameraOptionData.cameraProjectionType != cameraOptionData.cameraProjectionType)
                    {
                        // set camera projection Type
                    }

                    if (m_UIStateData.cameraOptionData.cameraViewType != cameraOptionData.cameraViewType)
                    {
                        // set camera view Type
                        m_UIStateData.cameraOptionData.cameraViewType = cameraOptionData.cameraViewType;
                        stateChanged?.Invoke(m_UIStateData);
                        break;
                    }

                    m_UIStateData.cameraOptionData = cameraOptionData;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetJoystickOption:
                {
                    var cameraOptionData = (CameraOptionData)payload.Data;

                    // set Enable Joystick
                    // cameraOptionData.enableJoysticks;

                    // set Joystick Preference
                    // cameraOptionData.joystickPreference;

                    m_UIStateData.cameraOptionData = cameraOptionData;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetNavigationOption:
                {
                    var cameraOptionData = (CameraOptionData)payload.Data;

                    // cameraOptionData.autoNavigationSpeed;
                    // cameraOptionData.navigationSpeed;
                    break;
                }
                case ActionTypes.SetNavigationState:
                {
                    var navigationState = (NavigationState)payload.Data;

                    if (navigationState.navigationMode != m_UIStateData.navigationState.navigationMode && navigationState.navigationMode == NavigationMode.AR)
                    {
                        // Reset instruction UI Step when start AR mode
                        m_ARStateData.instructionUIStep = 0;
                    }
                    else if(navigationState.navigationMode != NavigationMode.AR)
                    {
                        // TODO: Move this as part of switching ot Orbit mode.
                        // Reset scale for most mode except AR
                        m_UIStateData.modelScale = ArchitectureScale.OneToOne;
                        float scalef = (float)m_UIStateData.modelScale;
                        // always scale these two the same
                        m_RootNode.gameObject.transform.localScale = new Vector3(1.0f / scalef, 1.0f / scalef, 1.0f / scalef);
                        m_BoundingBoxRootNode.gameObject.transform.localScale = new Vector3(1.0f / scalef, 1.0f / scalef, 1.0f / scalef);
                    }

                    m_UIStateData.navigationState = navigationState;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SelectObjects:
                {
                    m_UIProjectStateData.objectSelectionInfo = (ObjectSelectionInfo) payload.Data;
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }
                case ActionTypes.SetBimGroup:
                {
                    m_UIStateData.bimGroup = (string) payload.Data;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.Failure:
                {
                    break;
                }
                case ActionTypes.SetLandingScreenFilter:
                {
                    var filterData = (ProjectListFilterData)payload.Data;
                    m_UIStateData.landingScreenFilterData = filterData;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.LoadScene:
                {
                    var name = (string)payload.Data;
                    if (!string.IsNullOrEmpty(name) && !SceneManager.GetSceneByName(name).IsValid())
                    {
                        StartCoroutine(LoadAsyncScene(name));
                    }
                    break;
                }
                case ActionTypes.UnloadScene:
                {
                    var name = (string)payload.Data;
                    if (!string.IsNullOrEmpty(name) && SceneManager.GetSceneByName(name).IsValid())
                    {
                        StartCoroutine(UnloadAsyncScene(name));
                    }
                    break;
                }
                case ActionTypes.SetInstructionUIState:
                {
                    m_ARStateData.instructionUIState = (InstructionUIState) payload.Data;
                    arStateChanged?.Invoke(m_ARStateData);

                    if(m_ARStateData.instructionUIState == InstructionUIState.Completed && m_UIStateData.progressData.progressState == ProgressData.ProgressState.NoPendingRequest)
                    {
                        // This has to be delayed because the renderers are
                        // not activated instantly after the instruction is complete
                        StartCoroutine(TakeDelayedThumbnail());
                    }
                    break;
                }
                case ActionTypes.SetInstructionUI:
                {
                    m_ARStateData.currentInstructionUI = (IInstructionUI) payload.Data;
                    arStateChanged?.Invoke(m_ARStateData);
                    break;
                }
                case ActionTypes.ResetHomeView:
                {
                    m_UIProjectStateData.cameraTransformInfo = m_OrbitModeUIController.ResetCamera();
                    m_UIStateData.cameraOptionData.cameraViewType = CameraViewType.Default;
                    m_RootNode.transform.position = Vector3.zero;
                    m_RootNode.transform.rotation = Quaternion.identity;
                    // Reset scale also
                    m_UIStateData.modelScale = ArchitectureScale.OneToOne;
                    // always scale these two the same
                    m_RootNode.gameObject.transform.localScale = Vector3.one;
                    m_BoundingBoxRootNode.gameObject.transform.localScale = Vector3.one;
                    stateChanged?.Invoke(m_UIStateData);
                    projectStateChanged?.Invoke(m_UIProjectStateData);
				    break;
			    }
                case ActionTypes.Teleport:
                {
                    m_UIProjectStateData.teleportTarget = (Vector3) payload.Data;
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }
                case ActionTypes.SetStatsInfo:
                {
                    var statsInfoData = (StatsInfoData)payload.Data;
                    m_UIDebugStateData.statsInfoData = statsInfoData;
                    debugStateChanged?.Invoke(m_UIDebugStateData);
                    break;
                }
                case ActionTypes.SetQuality:
                {
                    var qualityData = (QualityState)payload.Data;
                    m_ApplicationStateData.qualityStateData = qualityData;
                    applicationStateChanged?.Invoke(m_ApplicationStateData);
                    break;
                }
                case ActionTypes.ShowModel:
                {
                    var active = (bool)payload.Data;
                    m_RootNode.SetActive(active);

                    if (active)
                    {
                        if (m_ReflectPipeline.TryGetNode<SpatialFilterNode>(out var spatialFilterNode))
                        {
                            spatialFilterNode.settings.displayOnlyBoundingBoxes = false;
                        }
                    }

                    break;
                }
                case ActionTypes.ShowBoundingBoxModel:
                {
                    var active = (bool)payload.Data;
                    m_BoundingBoxRootNode.SetActive(active);

                    if (m_ReflectPipeline.TryGetNode<SpatialFilterNode>(out var spatialFilterNode))
                    {
                        spatialFilterNode.settings.displayOnlyBoundingBoxes = active;
                    }

                    break;
                }
                case ActionTypes.SetModelScale:
                {
                    var scale = (ArchitectureScale)payload.Data;
                    m_UIStateData.modelScale = scale;
                    // TODO: Use Mars World Scale to scale objects
                    float scalef = (float)m_UIStateData.modelScale;
                    // always scale these two the same
                    m_RootNode.gameObject.transform.localScale = new Vector3(1.0f / scalef, 1.0f / scalef, 1.0f / scalef);
                    m_BoundingBoxRootNode.gameObject.transform.localScale = new Vector3(1.0f / scalef, 1.0f / scalef, 1.0f / scalef);
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetModelRotation:
                {
                    var rotation = (Vector3)payload.Data;
                    m_PlacementRoot.transform.Rotate(rotation);
                    m_RootNode.transform.Rotate(rotation);
                    break;
                }

                case ActionTypes.RefreshProjectList:
                {
                    ReflectPipelineFactory.RefreshProjects();
                    m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingIndeterminate;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetPlacementRules:
                {
                    var value = (PlacementRule)payload.Data;

                    if (m_PlacementRules != null && value == m_ARStateData.placementStateData.placementRule)
                    {
                        if (value != PlacementRule.None)
                        {
                            m_PlacementRules.SetActive(true);
                        }
                        return;
                    }

                    m_ARStateData.placementStateData.placementRule = value;

                    if (m_PlacementRules != null)
                    {
                        Destroy(m_PlacementRules);
                        m_PlacementRules = null;
                    }

                    if (value != PlacementRule.None)
                    {
                        m_PlacementRules = Instantiate(m_PlacementRulesPrefabs[(int)value - 1], m_BoundingBoxRootNode.transform);
                        ModuleLoaderCore.instance.GetModule<FunctionalityInjectionModule>().activeIsland.InjectFunctionalitySingle(m_PlacementRules.gameObject.GetComponent<Replicator>());
                        m_PlacementRules.transform.parent = null;
                        m_PlacementRules.transform.localScale = Vector3.one;
                        m_PlacementRules.SetActive(true);
                    }
                    arStateChanged?.Invoke(m_ARStateData);
                    break;
                }
                case ActionTypes.Cancel:
                {
                    var value = (bool) payload.Data;
                    m_UIStateData.operationCancelled = value;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.EnablePlacement:
                {
                    var value = (bool) payload.Data;
                    m_ARStateData.placementGesturesEnabled = value;
                    arStateChanged?.Invoke(m_ARStateData);
                    break;
                }
                case ActionTypes.SetTheme:
                {
                    var value = (string) payload.Data;
                    m_UIStateData.themeName = value;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.EnableAR:
                {
                    var value = (bool) payload.Data;
                    m_ARStateData.arEnabled = value;
                    arStateChanged?.Invoke(m_ARStateData);
                    break;
                }
                case ActionTypes.SetARToolState:
                {
                    var value = (ARToolStateData) payload.Data;
                    m_ARStateData.arToolStateData = value;
                    arStateChanged?.Invoke(m_ARStateData);
                    break;
                }
                case ActionTypes.SetPlacementState:
                {
                    var value = (ARPlacementStateData)payload.Data;
                    m_ARStateData.placementStateData = value;
                    arStateChanged?.Invoke(m_ARStateData);
                    break;
                }
                case ActionTypes.SetARMode:
                {
                    var value = (ARMode)payload.Data;
                    m_ARStateData.arMode = value;
                    arStateChanged?.Invoke(m_ARStateData);
                    break;
                }
                case ActionTypes.SetSettingsToolState:
                {
                    var value = (SettingsToolStateData)payload.Data;
                    m_UIStateData.settingsToolStateData = value;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetDebugOptions:
                {
                    var value = (DebugOptionsData)payload.Data;
                    m_UIDebugStateData.debugOptionsData = value;
                    debugStateChanged?.Invoke(m_UIDebugStateData);
                    break;
                }
            }
        }
    }
}
