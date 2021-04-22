using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Stores;
using Unity.MARS.Providers;
using Unity.Reflect.IO;
using Unity.TouchFramework;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Pipeline;
using UnityEngine.SceneManagement;
using Unity.Reflect.Multiplayer;
using Unity.Reflect.Streaming;
#if UNITY_EDITOR
using UnityEditor.MARS.Simulation;
#endif

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Component that hold the state of the UI.
    /// Partial Class with non SharpFlux Connexions, Pipeline, ProjectLister and LoginManager
    /// </summary>
    public partial class UIStateManager : MonoBehaviour,
        IStore<UIStateData>, IStore<UISessionStateData>, IStore<UIProjectStateData>, IStore<UIARStateData>,
        IStore<UIDebugStateData>, IStore<ApplicationStateData>, IStore<RoomConnectionStateData>, IStore<UIWalkStateData>,
        IUsesSessionControl, IUsesPointCloud, IUsesPlaneFinding
    {
#pragma warning disable CS0649
        [SerializeField]
        ViewerReflectPipeline m_ReflectPipeline;
        [SerializeField]
        RuntimeReflectBootstrapper m_Reflect;
        [SerializeField]
        PopUpManager m_PopUpManager;
        [SerializeField]
        NavigationModeUIController m_NavigationModeUIController;
        [SerializeField]
        float m_WaitingDelayToCloseStreamIndicator = 1f;
        [SerializeField]
        ViewerMessageManager m_MessageManager;
        [SerializeField]
        DisplayController m_DisplayController;
#pragma warning restore CS0649

        [SerializeField, Tooltip("Reflect Session Manager")]
        public LoginManager m_LoginManager;
        public LinkSharingManager m_LinkSharingManager;
        public ArgumentParser m_ArgumentParser;
        public SunStudy.SunStudy m_SunStudy;
        public GameObject m_RootNode;
        public GameObject m_BoundingBoxRootNode;
        public GameObject m_PlacementRoot;
        public GameObject m_PlacementRules;
        public List<GameObject> m_PlacementRulesPrefabs;

        static readonly int k_UseTexture = Shader.PropertyToID("_UseTexture");
        readonly object syncRoot = new object();

        const float k_Timeout = 0.5f;

        OrbitModeUIController m_OrbitModeUIController;
        ThumbnailController m_ThumbnailController;

        Coroutine m_WaitStreamIndicatorCoroutine;
        WaitForSeconds m_WaitDelay;

        SpatialFilterNode m_SpatialFilter;
        MetadataFilterNode m_MetadataFilter;
        LightFilterNode m_LightFilterNode;

        bool m_UseExperimentalActorSystem;
        BridgeActor.Proxy m_Bridge;

        const string k_BadVRConfigurationTitle = "Missing OpenXR Setup";
        const string k_BadVRConfiguration = "Please setup an OpenXR compatible VR device before switching to VR navigation mode.";
        const string k_VRDeviceDisconnectedTitle = "Disconnected VR Device detected";
        const string k_VRDeviceDisconnected = "Please connect your OpenXR compatible VR device before switching to VR navigation mode.";
        const string k_CloseProjectTitle = "Close Project";
        const string k_CloseProjectText = "Do you want to close the current project?";
        const string k_AccessDeniedTitle = "Access Denied";
        const string k_AccessDeniedText = "You don't have access to the project you are trying to open.";
        const string k_ConnectionErrorTitle = "Connection Failed";
        const string k_ConnectionErrorText = "Connection to cloud services failed.";
        string m_CachedLinkToken = "";
        OpenInViewerInfo m_CachedOpenInViewerInfo = null;

        IProvidesSessionControl IFunctionalitySubscriber<IProvidesSessionControl>.provider { get; set; }
        IProvidesPointCloud IFunctionalitySubscriber<IProvidesPointCloud>.provider { get; set; }
        IProvidesPlaneFinding IFunctionalitySubscriber<IProvidesPlaneFinding>.provider { get; set; }

        [SerializeField]
        bool m_VerboseLogging;

        public bool verboseLogging
        {
            get => m_VerboseLogging;
            set => m_VerboseLogging = value;
        }

        void AwakePipeline()
        {
            m_UseExperimentalActorSystem = m_Reflect != null && m_Reflect.EnableExperimentalActorSystem;

            m_OrbitModeUIController = GetComponent<OrbitModeUIController>();
            m_ThumbnailController = GetComponent<ThumbnailController>();

            m_LoginManager.userLoggedIn.AddListener(OnUserLoggedIn);
            m_LoginManager.userLoggedOut.AddListener(OnUserLoggedOut);

            m_NavigationModeUIController.badVRConfigurationEvent.AddListener(OnBadVRConfiguration);

            m_LoginManager.linkSharingDetected.AddListener(OnLinkSharingDetected);
            m_LoginManager.openInViewerDetected.AddListener(OnOpenInViewerDetected);

            m_LinkSharingManager.linkSharingProjectInfoEvent.AddListener(OnLinkSharingProjectInfo);
            m_LinkSharingManager.sharingLinkCreated.AddListener(OnSharingLinkCreated);

            m_LinkSharingManager.linkCreatedExceptionEvent.AddListener(OnLinkCreatedException);
            m_LinkSharingManager.projectInfoExceptionEvent.AddListener(OnProjectInfoException);

            m_ArgumentParser = new ArgumentParser();
            m_ArgumentParser.Parse();

            m_WaitDelay = new WaitForSeconds(m_WaitingDelayToCloseStreamIndicator);

            m_DisplayController.OnDisplayChanged += OnDisplayChanged;
#if UNITY_EDITOR
            SimulationSettings.instance.ShowSimulatedEnvironment = false;
            SimulationSettings.instance.ShowSimulatedData = false;
#endif
        }

        IEnumerator StartPipeline()
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
        }

        void OnUserLoggedIn(UnityUser changedUser)
        {

            PlayerClientBridge.MatchmakerManager.Connect( changedUser.AccessToken, m_MultiplayerController.connectToLocalServer);

            m_UIStateData.colorPalette = PlayerClientBridge.MatchmakerManager.Palette.Select(c =>
               new Color(c.R / (float)255, c.G / (float)255, c.B / (float)255)
           ).ToArray();

            // clear status message
            m_MessageManager.ClearAllMessage();

            // show landing screen when log in
            m_UIStateData.activeDialog = DialogType.LandingScreen;

            // update session state
            m_UISessionStateData.sessionState.loggedState = LoginState.LoggedIn;
            m_UISessionStateData.sessionState.user = changedUser;
            m_UISessionStateData.sessionState.userIdentity = new UserIdentity(null, -1, changedUser?.DisplayName, DateTime.MinValue, null);
            m_UISessionStateData.sessionState.linkShareLoggedOut = false;

            var useExperimentalActorSystem = m_Reflect != null && m_Reflect.EnableExperimentalActorSystem;
            if (!useExperimentalActorSystem)
                m_ReflectPipeline.SetUser(changedUser);
            else
            {
                // Hack: Pipeline is in its own assembly, not available in Unity.Reflect
                var storage = new PlayerStorage(UnityEngine.Reflect.ProjectServer.ProjectDataPath, true, false);
                var auth = new AuthClient(changedUser, storage);
                ReflectPipelineFactory.SetUser(changedUser, m_Reflect.Hook, auth, storage);
            }
            // connect Pipeline events
            ConnectPipelineEvents();
            // connect all Pipeline Factory events
            ConnectPipelineFactoryEvents();

            ConnectMultiplayerEvents();
            // refreshProjects
            ReflectPipelineFactory.RefreshProjects();

            m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingIndeterminate;
            stateChanged?.Invoke(m_UIStateData);
            sessionStateChanged?.Invoke(m_UISessionStateData);
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
            var useExperimentalActorSystem = m_Reflect != null && m_Reflect.EnableExperimentalActorSystem;

            if (!useExperimentalActorSystem && m_ReflectPipeline.HasPipelineAsset)
            {
                if (m_ReflectPipeline.TryGetNode<SpatialFilterNode>(out var spatialFilterNode))
                {
                    spatialFilterNode.settings.memoryLevelChanged.AddListener(OnMemoryLevelChanged);
                }
                // initial bounds
                if (m_ReflectPipeline.TryGetNode<BoundingBoxControllerNode>(out var boundingBoxControllerNode))
                {
                    boundingBoxControllerNode.settings.onGlobalBoundsCalculated.AddListener(OnBoundsChanged);
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
            else if (useExperimentalActorSystem)
            {
                // This function is called in OnUserLoggedIn, which doesn't make any sense because the pipeline is not instantiated yet.
                // The hooking for actor system is done when the actor system is instantiated instead in OpenProject (see way below...)
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
                m_MessageManager.SetStatusMessage($"Streaming... {currentCount}/{totalCount}");
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

        void OnMemoryLevelChanged(MemoryLevel memoryLevel)
        {
            Debug.Log($"OnMemoryLevelChanged = {memoryLevel}");
            switch (memoryLevel)
            {
                case MemoryLevel.Unknown:
                case MemoryLevel.Low:
                    break;
                case MemoryLevel.Medium:
                case MemoryLevel.High:
                case MemoryLevel.Critical:
                    m_MessageManager.SetStatusMessage("Objects have stop loading due to memory limitations", StatusMessageType.Warning);
                    break;
            }
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
                GetFilterItemInfos(m_UIStateData.filterGroup, filters =>
                {
                    m_UIProjectStateData.filterItemInfos = filters;
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                });
            }
        }

        void GetFilterItemInfos(string groupKey, Action<List<FilterItemInfo>> callback)
        {
            var useExperimentalActorSystem = m_Reflect != null && m_Reflect.EnableExperimentalActorSystem;

            if (!useExperimentalActorSystem)
            {
                var filterKeys = m_MetadataFilter.processor.GetFilterKeys(groupKey);
                var filterItemInfo = new FilterItemInfo
                {
                    groupKey = groupKey
                };

                var result = new List<FilterItemInfo>();
                foreach (var filterKey in filterKeys)
                {
                    filterItemInfo.filterKey = filterKey;
                    filterItemInfo.visible = m_MetadataFilter.processor.IsVisible(groupKey, filterKey);
                    filterItemInfo.highlight = m_MetadataFilter.processor.IsHighlighted(groupKey, filterKey);
                    result.Add(filterItemInfo);
                }

                callback(result);
            }
            else
            {
                var bridge = m_Reflect.Hook.systems.ActorRunner.Bridge;
                bridge.GetFilterStates(groupKey, filters =>
                {
                    var result = new List<FilterItemInfo>();
                    var filterItemInfo = new FilterItemInfo { groupKey = groupKey };
                    foreach (var filter in filters)
                    {
                        filterItemInfo.filterKey = filter.Key;
                        filterItemInfo.visible = filter.IsVisible;
                        filterItemInfo.highlight = filter.isHighlighted;
                        result.Add(filterItemInfo);
                    }

                    callback(result);
                });
            }
        }

        void OnUserLoggedOut()
        {
            CloseProject();

            // clear status message
            m_MessageManager.ClearAllMessage();

            // show login screen when log out
            m_UIStateData.activeDialog = DialogType.LoginScreen;

            stateChanged?.Invoke(stateData);
            // update session state
            m_UISessionStateData.sessionState.loggedState = LoginState.LoggedOut;
            m_UISessionStateData.sessionState.user = null;
            m_UISessionStateData.sessionState.userIdentity = default;
            sessionStateChanged?.Invoke(sessionStateData);
            PlayerClientBridge.MatchmakerManager.LeaveRoom();
            PlayerClientBridge.MatchmakerManager.Disconnect();

        }

        void OnProjectsRefreshCompleted(List<Project> projects)
        {

            m_UIStateData.progressData.progressState = ProgressData.ProgressState.NoPendingRequest;
            ForceSendStateChangedEvent();

            List<ProjectRoom> newProjects = new List<ProjectRoom>();
            foreach(var proj in projects)
            {
                ProjectRoom toAdd;
                var existingProjectIndex = Array.FindIndex(m_UISessionStateData.sessionState.rooms, pr => pr.project.serverProjectId == proj.serverProjectId);
                if(existingProjectIndex != -1)
                {
                    toAdd = new ProjectRoom(proj, m_UISessionStateData.sessionState.rooms[existingProjectIndex].users.ToArray());
                }
                else
                {
                    toAdd = new ProjectRoom(proj);
                }
                newProjects.Add(toAdd);
            }

            m_UISessionStateData.sessionState.rooms = newProjects.ToArray();
            ForceSendSessionStateChangedEvent();

            PlayerClientBridge.MatchmakerManager.MonitorRooms(projects.Select(p => p.serverProjectId));

            if (m_CachedOpenInViewerInfo != null)
            {
                if (!TryOpenProject(m_CachedOpenInViewerInfo))
                {
                    PopupAccessDeniedMessage();
                }
                m_CachedOpenInViewerInfo = null;
            }

            if (!string.IsNullOrWhiteSpace(m_CachedLinkToken))
            {
                m_LinkSharingManager.ProcessSharingToken(m_UISessionStateData.sessionState.user.AccessToken, m_CachedLinkToken);
                m_CachedLinkToken = string.Empty;
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

        void OnBadVRConfiguration(bool VRDeviceDisconnected, Action onDismiss)
        {
            var data = m_PopUpManager.GetModalPopUpData();
            if (VRDeviceDisconnected)
            {
                data.title = k_VRDeviceDisconnectedTitle;
                data.text = k_VRDeviceDisconnected;
                data.negativeText = "Dismiss";
                // Let user continue since he could connect the device later on
                data.negativeCallback = delegate
                {
                    onDismiss();
                };
            }
            else
            {
                data.title = k_BadVRConfigurationTitle;
                data.text = k_BadVRConfiguration;
            }
            m_PopUpManager.DisplayModalPopUp(data);
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

        void OnDisplayChanged(DisplayData data)
        {
            UpdateDisplayData(data);
        }

        protected void UpdateDisplayData(DisplayData data)
        {
            m_UIStateData.display = data;
            stateChanged?.Invoke(m_UIStateData);
        }

        void CloseProject()
        {
            if (m_UIProjectStateData.activeProject != Project.Empty)
            {
                m_MessageManager.SetStatusMessage($"Closing {m_UIProjectStateData.activeProject.name}...");

                m_ReflectPipeline?.CloseProject();
                m_UIStateData.toolbarsEnabled = false;
                m_UIStateData.navigationState.EnableAllNavigation(false);
                stateChanged?.Invoke(m_UIStateData);
                m_UIProjectStateData.activeProject = Project.Empty;
                m_UIProjectStateData.activeProjectThumbnail = null;
                m_UIProjectStateData.objectSelectionInfo = default;
                projectStateChanged?.Invoke(projectStateData);

                PlayerClientBridge.MatchmakerManager.LeaveRoom();

                //If it was a link shared project open
                if(m_UISessionStateData.sessionState.linkSharedProjectRoom.project.UnityProject != null && m_UISessionStateData.sessionState.linkSharedProjectRoom.project.serverProjectId == m_UIProjectStateData.activeProject.serverProjectId)
                {
                    m_UISessionStateData.sessionState.linkSharedProjectRoom = default;
                }

            }
        }

        void CloseAllDialogs()
        {
            m_UIStateData.activeDialog = DialogType.None;
            m_UIStateData.activeOptionDialog = OptionDialogType.None;
            m_UIStateData.activeSubDialog = DialogType.None;
            stateChanged?.Invoke(m_UIStateData);
        }

        void ResetToolBars()
        {
            m_UIStateData.toolbarsEnabled = true;
            m_UIStateData.toolState.activeTool = ToolType.OrbitTool;
            m_UIStateData.toolState.orbitType = OrbitType.OrbitAtPoint;
            m_UIStateData.activeToolbar = ToolbarType.OrbitSidebar;
            stateChanged?.Invoke(m_UIStateData);
        }

        void ResetExternalTools()
        {
            m_ExternalToolStateData.measureToolStateData.toolState = false;
            externalToolChanged?.Invoke(m_ExternalToolStateData);
        }

        void OpenProject(Project project)
        {
            CloseProject();

            CloseAllDialogs();


            m_UIProjectStateData.activeProject = project;

            var projectIndex = Array.FindIndex(m_UISessionStateData.sessionState.rooms, (room) => room.project.serverProjectId == project.serverProjectId);
            if (projectIndex == -1) // Opening a project not in the project list
            {
                m_UISessionStateData.sessionState.linkSharedProjectRoom = new ProjectRoom(project);
            }


            m_MessageManager.SetStatusMessage($"Opening {m_UIProjectStateData.activeProject.name}...");
            stateChanged?.Invoke(m_UIStateData);

            m_UIProjectStateData.activeProjectThumbnail = ThumbnailController.LoadThumbnailForProject(m_UIProjectStateData.activeProject);
            projectStateChanged?.Invoke(projectStateData);

            var useExperimentalActorSystem = m_Reflect != null && m_Reflect.EnableExperimentalActorSystem;

            if (!useExperimentalActorSystem)
            {
                m_ReflectPipeline.OpenProject(projectStateData.activeProject);
                m_ReflectPipeline.TryGetNode(out m_MetadataFilter);
                m_ReflectPipeline.TryGetNode(out m_LightFilterNode);
                m_ReflectPipeline.TryGetNode(out m_SpatialFilter);
            }
            else
            {
                var runner = m_Reflect.Hook.systems.ActorRunner;
                runner.Instantiate(m_Reflect.Asset, project, m_Reflect, m_UISessionStateData.sessionState.user);
                runner.StartActorSystem();
                m_Bridge = runner.Bridge;
                m_Bridge.Subscribe<GlobalBoundsUpdated>(ctx => OnBoundsChanged(ctx.Data.GlobalBounds));

                m_Bridge.Subscribe<MetadataGroupsChanged>(ctx => OnMetadataGroupsChanged(ctx.Data.GroupKeys));
                m_Bridge.Subscribe<MetadataCategoriesChanged>(ctx => OnMetadataCategoriesChanged(ctx.Data.GroupKey, ctx.Data.FilterKeys));

                m_Bridge.Subscribe<AssetCountChanged>(ctx =>
                {
                    m_UIDebugStateData.statsInfoData.assetsCountData.addedCount = ctx.Data.ItemCount.NbAdded;
                    m_UIDebugStateData.statsInfoData.assetsCountData.changedCount = ctx.Data.ItemCount.NbChanged;
                    m_UIDebugStateData.statsInfoData.assetsCountData.removedCount = ctx.Data.ItemCount.NbRemoved;
                    debugStateChanged?.Invoke(m_UIDebugStateData);
                });

                m_Bridge.Subscribe<InstanceCountChanged>(ctx =>
                {
                    if (m_UIDebugStateData.statsInfoData.instancesCountData.addedCount == 0 && ctx.Data.ItemCount.NbAdded > 0)
                    {
                        OnInstanceStreamBegin();
                        OnInstanceStreamEnd();
                    }

                    m_UIDebugStateData.statsInfoData.instancesCountData.addedCount = ctx.Data.ItemCount.NbAdded;
                    m_UIDebugStateData.statsInfoData.instancesCountData.changedCount = ctx.Data.ItemCount.NbChanged;
                    m_UIDebugStateData.statsInfoData.instancesCountData.removedCount = ctx.Data.ItemCount.NbRemoved;
                    debugStateChanged?.Invoke(m_UIDebugStateData);
                });

                m_Bridge.Subscribe<GameObjectCountChanged>(ctx =>
                {
                    m_UIDebugStateData.statsInfoData.gameObjectsCountData.addedCount = ctx.Data.ItemCount.NbAdded;
                    m_UIDebugStateData.statsInfoData.gameObjectsCountData.changedCount = ctx.Data.ItemCount.NbChanged;
                    m_UIDebugStateData.statsInfoData.gameObjectsCountData.removedCount = ctx.Data.ItemCount.NbRemoved;
                    debugStateChanged?.Invoke(m_UIDebugStateData);
                });

                m_Bridge.Subscribe<GameObjectCountChanged>(ctx =>
                {
                    m_UIDebugStateData.statsInfoData.gameObjectsCountData.addedCount = ctx.Data.ItemCount.NbAdded;
                    m_UIDebugStateData.statsInfoData.gameObjectsCountData.changedCount = ctx.Data.ItemCount.NbChanged;
                    m_UIDebugStateData.statsInfoData.gameObjectsCountData.removedCount = ctx.Data.ItemCount.NbRemoved;
                    debugStateChanged?.Invoke(m_UIDebugStateData);
                });

                m_Bridge.Subscribe<StreamingProgressed>(ctx => OnGameObjectStreamEvent(ctx.Data.NbStreamed, ctx.Data.Total));

                runner.Bridge.SendUpdateManifests();
            }

            // set enable texture and light
            if (m_UIStateData.sceneOptionData.enableTexture)
                Shader.SetGlobalFloat(k_UseTexture, 1);
            else
                Shader.SetGlobalFloat(k_UseTexture, 0);

            if (!useExperimentalActorSystem)
            {
                if (m_LightFilterNode != null)
                {
                    m_UIStateData.sceneOptionData.enableLightData = m_LightFilterNode.settings.enableLights;
                }
            }
            else
            {
                var bridge = m_Reflect.Hook.systems.ActorRunner.Bridge;
                m_UIStateData.sceneOptionData.enableLightData = bridge.GetFirstMatchingSettings<LightActor.Settings>().EnableLights;
            }

            m_BoundingBoxRootNode.SetActive(true);
            if (!useExperimentalActorSystem)
            {
                if (m_ReflectPipeline.TryGetNode<BoundingBoxControllerNode>(out var boundingBoxControllerNode))
                {
                    boundingBoxControllerNode.settings.displayOnlyBoundingBoxes = false;
                    m_UIDebugStateData.debugOptionsData.useDebugBoundingBoxMaterials =
                        boundingBoxControllerNode.settings.useDebugMaterials;
                    m_UIDebugStateData.debugOptionsData.useStaticBatching =
                        boundingBoxControllerNode.settings.useStaticBatching;
                }
                if (m_ReflectPipeline.TryGetNode<SpatialFilterNode>(out var spatialFilterNode))
                {
                    m_UIProjectStateData.teleportPicker = new SpatialSelector
                    {
                        SpatialPicker = spatialFilterNode.SpatialPicker,
                        WorldRoot = m_RootNode.transform
                    };
                    m_UIDebugStateData.debugOptionsData.spatialPriorityWeights = new Vector3(
                        spatialFilterNode.settings.priorityWeightAngle,
                        spatialFilterNode.settings.priorityWeightDistance,
                        spatialFilterNode.settings.priorityWeightSize);
                    // ensure depth culling is off by default on devices that don't support AsycGPUReadback
                    spatialFilterNode.settings.cullingSettings.useDepthCulling &= m_UIStateData.deviceCapability.HasFlag(DeviceCapability.SupportsAsyncGPUReadback);
                }

                // [AEC-2238] force StreamLimiter usage on Android to mitigate slowdown on second project load
#if UNITY_ANDROID && !UNITY_EDITOR
                if (m_ReflectPipeline.TryGetNode<StreamInstanceLimiterNode>(out var streamInstanceLimiterNode))
                {
                    streamInstanceLimiterNode.settings.bypass = false;
                }
#endif
            }
            else
            {
                var bridge = m_Reflect.Hook.systems.ActorRunner.Bridge;

                var boxSettings = bridge.GetFirstMatchingSettings<BoundingBoxActor.Settings>();
                if (boxSettings != null)
                {
                    boxSettings.DisplayOnlyBoundingBoxes = false;
                    m_UIDebugStateData.debugOptionsData.useDebugBoundingBoxMaterials = boxSettings.UseDebugMaterials;
                    m_UIDebugStateData.debugOptionsData.useStaticBatching = boxSettings.UseStaticBatching;
                }

                var spatialSettings = bridge.GetFirstMatchingSettings<SpatialActor.Settings>();
                if (spatialSettings != null)
                {
                    // Todo: Fix SpatialPicker
                    //m_UIProjectStateData.teleportPicker = new SpatialSelector
                    //{
                    //    SpatialPicker = spatialFilterNode.SpatialPicker,
                    //    WorldRoot = m_RootNode.transform
                    //};
                    m_UIDebugStateData.debugOptionsData.spatialPriorityWeights = new Vector3(
                        spatialSettings.PriorityWeightAngle,
                        spatialSettings.PriorityWeightDistance,
                        spatialSettings.PriorityWeightSize);
                    // ensure depth culling is off by default on devices that don't support AsycGPUReadback
                    spatialSettings.UseDepthCulling &= m_UIStateData.deviceCapability.HasFlag(DeviceCapability.SupportsAsyncGPUReadback);
                }
            }

            // reset the toolbars
            ResetToolBars();
            // reset the external tools
            ResetExternalTools();
            m_UIStateData.navigationState.EnableAllNavigation(true);

            m_UIStateData.settingsToolStateData = new SettingsToolStateData
            {
                bimFilterEnabled = true,
                sceneOptionEnabled = true,
                sunStudyEnabled = true
            };

            stateChanged?.Invoke(m_UIStateData);
            debugStateChanged?.Invoke(m_UIDebugStateData);

            PlayerClientBridge.MatchmakerManager.JoinRoom(m_UIProjectStateData.activeProject.serverProjectId);

        }

        void OnOpenInViewerDetected(OpenInViewerInfo openInViewerInfo)
        {
            // Can only process one at a time
            if (m_CachedOpenInViewerInfo == null)
            {
                var session = m_UISessionStateData;
                switch (session.sessionState.loggedState)
                {
                    case LoginState.LoggedIn:
                        // Can we match the request with a valid project?
                        if (GetProjectToOpen(openInViewerInfo.ServerId, openInViewerInfo.ProjectId) != null)
                        {
                            if (m_UIProjectStateData.activeProject == Project.Empty)
                            {
                                TryOpenProject(openInViewerInfo);
                            }
                            else
                            {
                                // Ask to close current project only if not the same
                                if (m_UIProjectStateData.activeProject.projectId != openInViewerInfo.ProjectId && m_UIProjectStateData.activeProject.serverProjectId != openInViewerInfo.ServerId)
                                {
                                    PopupCloseProjectDialog(openInViewerInfo, UserApprovedOpenInViewer);
                                }
                            }
                        }
                        else
                        {
                            PopupAccessDeniedMessage();
                        }
                        break;
                    case LoginState.LoggingIn:
                    case LoginState.LoggedOut:
                        m_CachedOpenInViewerInfo = openInViewerInfo;
                        m_UISessionStateData.sessionState.linkShareLoggedOut = true;
                        sessionStateChanged?.Invoke(sessionStateData);
                        break;
                    case LoginState.LoggingOut:
                        break;
                }
            }
        }

        void PopupCloseProjectDialog(OpenInViewerInfo openInViewerInfo, Action<OpenInViewerInfo> OnApprove)
        {
            // open dialog
            var data = m_PopUpManager.GetModalPopUpData();
            data.title = k_CloseProjectTitle;
            data.text = k_CloseProjectText;
            data.negativeText = "Cancel";
            data.positiveCallback = delegate
            {
                OnApprove(openInViewerInfo);
            };
            data.negativeCallback = delegate { };
            m_PopUpManager.DisplayModalPopUp(data);
        }

        void UserApprovedOpenInViewer(OpenInViewerInfo openInViewerInfo)
        {
            var session = m_UISessionStateData;
            switch (session.sessionState.loggedState)
            {
                case LoginState.LoggedIn:
                    if (!TryOpenProject(openInViewerInfo))
                    {
                        PopupAccessDeniedMessage();
                    }
                    break;
                case LoginState.LoggingIn:
                case LoginState.LoggedOut:
                    m_CachedOpenInViewerInfo = openInViewerInfo;
                    break;
                case LoginState.LoggingOut:
                    break;
            }
        }

        bool TryOpenProject(OpenInViewerInfo openInViewerInfo)
        {
            var project = GetProjectToOpen(openInViewerInfo.ServerId, openInViewerInfo.ProjectId);
            if (project != null)
            {
                OpenProject(project);
                return true;
            }
            return false;
        }

        void OnLinkSharingDetected(string linkToken)
        {
            var session = m_UISessionStateData;
            switch (session.sessionState.loggedState)
            {
                case LoginState.LoggedIn:
                    // process token
                    m_LinkSharingManager.ProcessSharingToken(session.sessionState.user.AccessToken, linkToken);

                    break;
                case LoginState.LoggingIn:
                case LoginState.LoggedOut:
                    m_CachedLinkToken = linkToken;
                    m_UISessionStateData.sessionState.linkShareLoggedOut = true;
                    sessionStateChanged?.Invoke(sessionStateData);
                    break;
                case LoginState.LoggingOut:
                    break;
            }
        }

        IEnumerator StartLogin()
        {
            yield return null;
            m_MessageManager.SetStatusMessage("Logging in...");
            stateChanged?.Invoke(m_UIStateData);

            m_UISessionStateData.sessionState.loggedState = LoginState.LoggingIn;
            m_LoginManager.Login();
            sessionStateChanged?.Invoke(sessionStateData);
        }

        Project GetProjectToOpen(string serverId, string projectId)
        {
            return m_UISessionStateData.sessionState.rooms
                .Select(x => x.project)
                .FirstOrDefault(x => x.host.ServerId.Equals(serverId)
                                     && x.projectId.Equals(projectId));
        }

        void OnLinkSharingProjectInfo(UnityProject projectInfo)
        {
            var project = GetProjectToOpen(projectInfo.Host.ServerId, projectInfo.ProjectId) ?? new Project(projectInfo);

            if (m_UIProjectStateData.activeProject != Project.Empty)
            {
                // close project popup.
                var data = m_PopUpManager.GetModalPopUpData();
                data.title = k_CloseProjectTitle;
                data.text = k_CloseProjectText;
                data.negativeText = "Cancel";
                data.positiveCallback = delegate
                {
                    StarOpenProjectFromModal(project);
                };
                m_PopUpManager.DisplayModalPopUp(data);
            }
            else
            {
                OpenProject(project);
            }
        }

        void StarOpenProjectFromModal(Project project)
        {
            StartCoroutine(OpenProjectFromModal(project));
        }

        IEnumerator OpenProjectFromModal(Project project)
        {
            // Avoid sticky modal popup
            yield return new WaitForSeconds(0.5f);
            OpenProject(project);
        }

        void PopupAccessDeniedMessage()
        {
            // error popup. don't exist the project on your project list
            var data = m_PopUpManager.GetModalPopUpData();
            data.title = k_AccessDeniedTitle;
            data.text = k_AccessDeniedText;
            data.positiveText = "Close";
            m_PopUpManager.DisplayModalPopUp(data);
        }

        void OnSharingLinkCreated(SharingLinkInfo sharingLinkInfo)
        {
            Debug.Log(sharingLinkInfo.Domain);
            Debug.Log(sharingLinkInfo.LinkToken);
        }

        void OnLinkCreatedException(Exception exception)
        {
            // todo
        }
        void OnProjectInfoException(Exception exception)
        {
            // open dialog
            var data = m_PopUpManager.GetModalPopUpData();
            data.positiveText = "Close";
            if (exception is ConnectionException)
            {
                data.title = k_ConnectionErrorTitle;
                data.text = k_ConnectionErrorText;
            }
            else
            {
                data.title = k_AccessDeniedTitle;
                data.text = k_AccessDeniedText;
            }
            m_PopUpManager.DisplayModalPopUp(data);
        }
    }
}
