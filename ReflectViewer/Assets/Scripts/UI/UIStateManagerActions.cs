using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using SharpFlux.Stores;
using Unity.MARS;
using Unity.Reflect.Actors;
using Unity.Reflect.Multiplayer;
using Unity.Reflect.Runtime;
using Unity.Reflect.Viewer.Actors;
using Unity.TouchFramework;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Component that hold the state of the UI.
    /// Partial Class with Actions and Dispatcher
    /// </summary>
    public partial class UIStateManager
    {
        //Returns whether the store has changed during the most recent dispatch
        bool hasChanged;

        IDispatcher dispatcher;
        public string DispatchToken { get; private set; }
        public string DispatchTokenClassAction { get; private set; }
        readonly object syncRoot = new object();

        [SerializeField, Tooltip("State of the UI"), UIContextProperties(nameof(UIStateContext))]
        [ContextButton("Value Changed", nameof(OnUIStateContextChanged))]
        UIStateData m_UIStateData;

        [SerializeField, Tooltip("State of the UI"), UIContextProperties(nameof(MessageManagerContext))]
        [ContextButton("Value Changed", nameof(OnMessageManagerContextChanged))]
        MessageManagerStateData m_MessageManagerStateData;

        [SerializeField, Tooltip("State of the Session"), UIContextProperties(nameof(SessionStateContext<UnityUser, LinkPermission>))]
        [ContextButton("Value Changed", nameof(OnSessionStateContextChanged))]
        UISessionStateData m_UISessionStateData;

        [SerializeField, Tooltip("State of the Project"), UIContextProperties(nameof(ProjectContext))]
        [ContextButton("Value Changed", nameof(OnProjectStateContextChanged))]
        UIProjectStateData m_UIProjectStateData;

        [SerializeField, Tooltip("State of the Project setting"), UIContextProperties(nameof(ProjectManagementContext<Project>))]
        [ContextButton("Value Changed", nameof(OnProjectSettingStateContextChanged))]
        ProjectSettingStateData m_ProjectSettingStateData;

        [SerializeField, Tooltip("State of the login setting"), UIContextProperties(nameof(LoginSettingContext<EnvironmentInfo>))]
        [ContextButton("Value Changed", nameof(OnLoginSettingStateContextChanged))]
        LoginSettingStateData m_LoginSettingStateData;
        bool m_CanUpdateEnvironmentInfo;

        [SerializeField, Tooltip("State of the Debug Data"), UIContextProperties(nameof(DebugStateContext))]
        [ContextButton("Value Changed", nameof(OnDebugStateContextChanged))]
        UIDebugStateData m_UIDebugStateData;

        [SerializeField, Tooltip("State of the Room Connexion Data"), UIContextProperties(nameof(RoomConnectionContext))]
        [ContextButton("Value Changed", nameof(OnRoomConnectionContextChanged))]
        RoomConnectionStateData m_RoomConnectionStateData;

        [SerializeField, Tooltip("State of the Application Settings Data"), UIContextProperties(nameof(ApplicationSettingsContext))]
        [ContextButton("Value Changed", nameof(OnApplicationSettingsContextChanged))]
        ApplicationSettingsStateData m_ApplicationSettingsStateData;

        [SerializeField, Tooltip("State of the SceneOption"), UIContextProperties(nameof(SceneOptionContext))]
        [ContextButton("Value Changed", nameof(OnSceneOptionContextChanged))]
        SceneOptionData m_SceneOptionData;

        [SerializeField, Tooltip("State of the Tool")]
        [ContextButton("Value Changed", nameof(OnExternalToolStateContextChanged))]
        ExternalToolStateData m_ExternalToolStateData;

        [SerializeField, Tooltip("State of the Drag"), UIContextProperties(nameof(DragStateContext))]
        [ContextButton("Value Changed", nameof(OnDragStateContextChanged))]
        DragStateData m_DragStateData;

        [SerializeField, Tooltip("State of the AR Simulation"), UIContextProperties(nameof(ARContext))]
        [ContextButton("Value Changed", nameof(OnARStateContextChanged))]
        UIARStateData m_ARStateData;

        [SerializeField, Tooltip("State of the Pipeline"), UIContextProperties(nameof(PipelineContext))]
        [ContextButton("Value Changed", nameof(OnPipelineStateContextChanged))]
        PipelineStateData m_PipelineStateData;

        [SerializeField, Tooltip("State of the VR"), UIContextProperties(nameof(VRContext))]
        [ContextButton("Value Changed", nameof(OnVRStateContextChanged))]
        VRStateData m_VRStateData;

        [SerializeField, Tooltip("State of the walk mode"), UIContextProperties(nameof(WalkModeContext))]
        [ContextButton("Value Changed", nameof(OnWalkStateContextChanged))]
        UIWalkStateData m_WalkStateData;

        [SerializeField, Tooltip("State of the Force Navigation Mode"), UIContextProperties(nameof(ForceNavigationModeContext))]
        [ContextButton("Value Changed", nameof(OnForceNavigationModeContextChanged))]
        ForceNavigationModeData m_ForceNavigationModeStateData;

        [SerializeField, Tooltip("State of the Top Bar Button Visibility"), UIContextProperties(nameof(AppBarContext))]
        [ContextButton("Value Changed", nameof(OnAppBarContextChanged))]
        AppBarStateData m_AppBarStateData;

        [SerializeField]
        MarkerUIPresenter m_MarkerUIPresenter;

        public PopUpManager popUpManager => m_PopUpManager;

        public bool IsNetworkConnected =>
            m_UISessionStateData.networkReachability != NetworkReachability.NotReachable &&
            m_UISessionStateData.projectServerConnection;

        // Only used by ProjectDrawerUIE
        public UIProjectStateData projectStateData
        {
            get => m_UIProjectStateData;
        }

        public ProjectSettingStateData projectSettingStateData
        {
            get => m_ProjectSettingStateData;
        }

        UIStateData IStore<UIStateData>.Data => m_UIStateData;
        IContextTarget m_UIStateContextTarget;
        IContextTarget m_ToolStateContextTarget;
        IContextTarget m_CameraOptionsContextTarget;
        IContextTarget m_NavigationContextTarget;
        IContextTarget m_SettingsToolContextTarget;
        IContextTarget m_FollowUserTarget;
        IContextTarget m_ProgressContextTarget;
        IContextTarget m_LandingScreenContextTarget;

        SceneOptionData IStore<SceneOptionData>.Data => m_SceneOptionData;
        IContextTarget m_SceneOptionContextTarget;

        UISessionStateData IStore<UISessionStateData>.Data => m_UISessionStateData;
        IContextTarget m_SessionStateContextTarget;

        MessageManagerStateData IStore<MessageManagerStateData>.Data => m_MessageManagerStateData;
        IContextTarget m_MessageManagerContextTarget;

        UIProjectStateData IStore<UIProjectStateData>.Data => m_UIProjectStateData;
        IContextTarget m_ProjectStateContextTarget;

        ProjectSettingStateData IStore<ProjectSettingStateData>.Data => m_ProjectSettingStateData;
        IContextTarget m_ProjectManagementContextTarget;

        LoginSettingStateData IStore<LoginSettingStateData>.Data => m_LoginSettingStateData;
        IContextTarget m_LoginSettingContextTarget;
        UIDebugStateData IStore<UIDebugStateData>.Data => m_UIDebugStateData;
        IContextTarget m_DebugStateContextTarget;
        IContextTarget m_DebugOptionContextTarget;
        IContextTarget m_StateInfoContextTarget;

        ApplicationSettingsStateData IStore<ApplicationSettingsStateData>.Data => m_ApplicationSettingsStateData;
        IContextTarget m_ApplicationSettingsContextTarget;
        RoomConnectionStateData IStore<RoomConnectionStateData>.Data => m_RoomConnectionStateData;
        IContextTarget m_RoomConnectionContextTarget;

        ExternalToolStateData IStore<ExternalToolStateData>.Data => m_ExternalToolStateData;
        IContextTarget m_MeasureToolContextTarget;
        IContextTarget m_SunStudyContextTarget;

        UIWalkStateData IStore<UIWalkStateData>.Data => m_WalkStateData;
        IContextTarget m_WalkStateContextTarget;

        DragStateData IStore<DragStateData>.Data => m_DragStateData;
        IContextTarget m_DragStateContextTarget;

        UIARStateData IStore<UIARStateData>.Data => m_ARStateData;
        IContextTarget m_ARStateContextTarget;
        IContextTarget m_ARPlacementContextTarget;
        IContextTarget m_ARToolStateContextTarget;

        PipelineStateData IStore<PipelineStateData>.Data => m_PipelineStateData;
        IContextTarget m_PipelineContextTarget;

        VRStateData IStore<VRStateData>.Data => m_VRStateData;
        IContextTarget m_VRStateContextTarget;

        ForceNavigationModeData IStore<ForceNavigationModeData>.Data => m_ForceNavigationModeStateData;
        IContextTarget m_ForceNavigationModeContextTarget;

        AppBarStateData IStore<AppBarStateData>.Data => m_AppBarStateData;
        IContextTarget m_AppBarContextTarget;

        /// <summary>
        /// Event signaled when the Login Setting has changed
        /// </summary>
        ///
        public static event Action loginSettingChanged = delegate { };
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void AwakeActions()
        {
            m_DragStateContextTarget = DragStateContext.BindTarget(m_DragStateData);
            m_MeasureToolContextTarget = MeasureToolContext.BindTarget(m_ExternalToolStateData.measureToolStateData);
            m_SunStudyContextTarget = SunStudyContext.BindTarget(m_ExternalToolStateData.sunStudyData);
            m_SceneOptionContextTarget = SceneOptionContext.BindTarget(m_SceneOptionData);
            m_ARStateContextTarget = ARContext.BindTarget(m_ARStateData);
            m_ARPlacementContextTarget = ARPlacementContext.BindTarget(m_ARStateData.placementStateData);
            m_ARToolStateContextTarget = ARToolStateContext.BindTarget(m_ARStateData.arToolStateData);
            m_PipelineContextTarget = PipelineContext.BindTarget(m_PipelineStateData);
            m_DebugStateContextTarget = DebugStateContext.BindTarget(m_UIDebugStateData);
            m_StateInfoContextTarget = StatsInfoContext.BindTarget(m_UIDebugStateData.statsInfoData);
            m_DebugOptionContextTarget = DebugOptionContext.BindTarget(m_UIDebugStateData.debugOptionsData);
            m_VRStateContextTarget = VRContext.BindTarget(m_VRStateData);
            m_WalkStateContextTarget = WalkModeContext.BindTarget(m_WalkStateData);
            m_ApplicationSettingsContextTarget = ApplicationSettingsContext.BindTarget(m_ApplicationSettingsStateData);
            m_ProjectStateContextTarget = ProjectContext.BindTarget(m_UIProjectStateData);
            m_ProjectManagementContextTarget = ProjectManagementContext<Project>.BindTarget(m_ProjectSettingStateData);
            m_LoginSettingContextTarget = LoginSettingContext<EnvironmentInfo>.BindTarget(m_LoginSettingStateData);
            m_ForceNavigationModeContextTarget = ForceNavigationModeContext.BindTarget(m_ForceNavigationModeStateData);
            m_AppBarContextTarget = AppBarContext.BindTarget(m_AppBarStateData);
            m_RoomConnectionContextTarget = RoomConnectionContext.BindTarget(m_RoomConnectionStateData);
            m_UIStateContextTarget = UIStateContext.BindTarget(m_UIStateData);
            m_ToolStateContextTarget = ToolStateContext.BindTarget(m_UIStateData.toolState);
            m_CameraOptionsContextTarget = CameraOptionsContext.BindTarget(m_UIStateData.cameraOptionData);
            m_NavigationContextTarget = NavigationContext.BindTarget(m_UIStateData.navigationStateData);
            m_SettingsToolContextTarget = SettingsToolContext.BindTarget(m_UIStateData.settingsToolStateData);
            m_ProgressContextTarget = ProgressContext.BindTarget(m_UIStateData.progressData);
            m_LandingScreenContextTarget = LandingScreenContext.BindTarget(m_UIStateData.landingScreenFilterData);
            m_FollowUserTarget = FollowUserContext.BindTarget(m_UIStateData.toolState.followUserTool);
            m_SessionStateContextTarget = SessionStateContext<UnityUser, LinkPermission>.BindTarget(m_UISessionStateData);
            m_MessageManagerContextTarget = MessageManagerContext.BindTarget(m_MessageManagerStateData);

            dispatcher = DispatcherFactory.GetDispatcher();
            m_ProjectSettingStateData.activeProject = Project.Empty;
            m_ProjectSettingStateData.activeProjectThumbnail = null;
            m_UIProjectStateData.highlightFilter = new HighlightFilterInfo();
            m_RoomConnectionStateData.localUser = NetworkUserData.defaultData;
            m_UISessionStateData = UISessionStateData.defaultData;
            m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData);
            m_UIStateData.SelectedUserData = new UserInfoDialogData();
            m_WalkStateData.instruction ??= new WalkModeInstruction();

            m_MarkerUIPresenter.Setup(dispatcher);

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetInstructionUIStateAction.InstructionUIState>(ARContext.current, nameof(IARModeDataProvider.instructionUIState), OnInstructionUIStateChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.enableLightData), OnEnableLightDataChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetModelScaleAction.ArchitectureScale>(ARPlacementContext.current, nameof(IARPlacementDataProvider.modelScale), OnModelScaleChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable), OnVREnableChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(WalkModeContext.current, nameof(IWalkModeDataProvider.walkEnabled), OnWalkEnableChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<Vector3>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.spatialPriorityWeights), OnSpatialPriorityWeightsChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.useDebugBoundingBoxMaterials), OnUseDebugBoundingBoxChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.useCulling), OnUseCullingChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.useSpatialManifest), OnUseSpatialManifestChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.useHlods), OnUseHlodsChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.hlodDelayMode), OnHlodDelayModeChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.hlodPrioritizer), OnHlodPrioritizerChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.targetFps), OnTargetFpsChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.showActorDebug), OnShowActorDebugChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(ARPlacementContext.current, nameof(IARPlacementDataProvider.showModel), OnShowModelChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(ARPlacementContext.current, nameof(IARPlacementDataProvider.showBoundingBoxModelAction), OnShowBoundingBoxModeChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.placementRulesGameObject), OnPlacementRulesGameObjectChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<FilterItemInfo>(ProjectContext.current, nameof(IProjectSortDataProvider.lastChangedFilterItem), OnProjectFilterInfoDataChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<Vector3>(ProjectContext.current, nameof(ITeleportDataProvider.teleportTarget), OnTeleportDataChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<ObjectSelectionInfo>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectSelectionInfo), OnObjectSelectionChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<HighlightFilterInfo>(ProjectContext.current, nameof(IProjectSortDataProvider.highlightFilter), OnHighlightFilterInfoChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), OnOpenProjectChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.loadSceneName), OnLoadSceneChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.unloadSceneName), OnUnloadSceneChanged));

            try
            {
                m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<EnvironmentInfo>(LoginSettingContext<EnvironmentInfo>.current, nameof(ILoginSettingDataProvider.environmentInfo), OnEnvironmentInfoChanged));
            }
            finally
            {
                m_CanUpdateEnvironmentInfo = true;
            }

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(LoginSettingContext<EnvironmentInfo>.current, nameof(ILoginSettingDataProvider.deleteCloudEnvironmentSetting), OnDeleteCloudEnvironmentChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(RoomConnectionContext.current, nameof(IRoomConnectionDataProvider<NetworkUserData>.userToMute), OnMuteUserChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<ProjectListState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.projectListState), OnRefreshProjectListStateChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<LinkPermission>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.linkSharePermission), OnLinkSharePermissionStateChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState), OnLoggedStateChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.isInPrivateMode), OnPrivateModeChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(ISyncModeDataProvider.syncEnabled), OnSyncModeEnabledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode), OnNavigationModeChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.filterHlods), OnFilterHLODsChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(WalkModeContext.current, nameof(IWalkModeDataProvider.isTeleportFinish), isFinish =>
            {
                if (isFinish)
                    m_WalkStateData.instructionUIState = SetInstructionUIStateAction.InstructionUIState.Started;
            }));

            // Sun Study
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.timeOfDay), (val) =>
            {
                m_SunStudy.MinuteOfDay = val;
            }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.timeOfYear), (val) =>
            {
                m_SunStudy.DayOfYear = val;
            }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.utcOffset), (val) =>
            {
                m_SunStudy.UtcOffset = val / 100f;
            }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.latitude), (val) =>
            {
                m_SunStudy.CoordLatitude = val;
            }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.longitude), (val) =>
            {
                m_SunStudy.CoordLongitude = val;
            }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.northAngle), (val) =>
            {
                m_SunStudy.NorthAngle = val;
            }));

            Subscribe();
        }

        void OnLoggedStateChanged(LoginState newData)
        {
            if (newData == LoginState.LoggingIn)
            {
                m_LoginManager.Login();
            }
            else if (newData == LoginState.LoggingOut)
            {
                CloseProject();
                m_UISessionStateData.projectListState = ProjectListState.AwaitingUser;
#if UNITY_EDITOR
                m_LoginManager.userLoggedOut?.Invoke();
#else
                    m_LoginManager.Logout();
#endif

                m_UISessionStateData.rooms = new IProjectRoom[] { };
                m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
                EnableMARSSession(false);
            }
        }

        void OnPrivateModeChanged(bool newData)
        {
            if (newData)
            {
                PlayerClientBridge.MatchmakerManager.LeaveRoom();

                // We dont disconnect from the room so the player identity is kept on the matchmaker when we reconnect
                PlayerClientBridge.MatchmakerManager.Disconnect();
            }
            else if (m_UISessionStateData.user != null)
            {
                PlayerClientBridge.MatchmakerManager.Connect(m_UISessionStateData.user.AccessToken, m_MultiplayerController.connectToLocalServer);
                PlayerClientBridge.MatchmakerManager.MonitorRooms(m_UISessionStateData.rooms.Select(r => ((ProjectRoom)r).project.serverProjectId));
                if (m_ProjectSettingStateData.activeProject != Project.Empty)
                {
                    PlayerClientBridge.MatchmakerManager.JoinRoom(m_ProjectSettingStateData.activeProject.serverProjectId, () => m_ProjectSettingStateData.accessToken.CloudServicesAccessToken);
                }
            }
        }

        void OnLinkSharePermissionStateChanged(LinkPermission newData)
        {
            m_UISessionStateData.linkSharePermission = newData;
        }

        void OnRefreshProjectListStateChanged(ProjectListState newData)
        {
            if (ProjectListState.AwaitingUserData == newData)
            {
                m_UISessionStateData.projectListState = newData;
                ReflectProjectsManager.RefreshProjects();
                m_UIStateData.progressData.progressState = SetProgressStateAction.ProgressState.PendingIndeterminate;
                m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);
            }
        }

        void OnNavigationModeChanged(SetNavigationModeAction.NavigationMode newData)
        {
            if (newData != SetNavigationModeAction.NavigationMode.AR && m_Bridge.IsInitialized)
            {
                var settings = m_Bridge.GetFirstMatchingSettings<BoundingBoxActor.Settings>();
                m_Bridge.UpdateSetting<BoundingBoxActor.Settings>(settings.Id, nameof(BoundingBoxActor.Settings.DisplayOnlyBoundingBoxes), false);
            }
        }

        void OnSyncModeEnabledChanged(bool newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var settings = m_Bridge.GetFirstMatchingSettings<SyncTreeActor.Settings>();
            m_Bridge.UpdateSetting<SyncTreeActor.Settings>(settings.Id, nameof(SyncTreeActor.Settings.IsLiveSyncEnabled), newData);
        }

        void OnWalkEnableChanged(bool newData)
        {
            if (newData)
            {
                m_WalkStateData.instructionUIState = SetInstructionUIStateAction.InstructionUIState.Init;
                m_WalkStateData.instruction ??= new WalkModeInstruction();
            }
        }

        void OnLoadSceneChanged(string newData)
        {
            if (!string.IsNullOrEmpty(newData) && !SceneManager.GetSceneByName(newData).IsValid())
            {
                StartCoroutine(LoadAsyncScene(newData));
            }
        }

        void OnUnloadSceneChanged(string newData)
        {
            if (!string.IsNullOrEmpty(newData) && SceneManager.GetSceneByName(newData).IsValid())
            {
                StartCoroutine(UnloadAsyncScene(newData));
            }
        }

        void OnEnvironmentInfoChanged(EnvironmentInfo newData)
        {
            if (m_CanUpdateEnvironmentInfo)
            {
                LocaleUtils.SaveEnvironmentInfo(newData);
                UnityEngine.Reflect.ProjectServer.Cleanup();
                UnityEngine.Reflect.ProjectServer.Init();
                loginSettingChanged?.Invoke();
            }
        }

        void OnDeleteCloudEnvironmentChanged(bool newData)
        {
            if (newData)
            {
                LocaleUtils.DeleteCloudEnvironmentSetting();
                UnityEngine.Reflect.ProjectServer.Cleanup();
                UnityEngine.Reflect.ProjectServer.Init();
                loginSettingChanged?.Invoke();
            }
        }

        void OnMuteUserChanged(string newData)
        {
            if (!string.IsNullOrEmpty(newData))
            {
                MuteUser(newData);
            }
        }

        void OnOpenProjectChanged(Project newData)
        {
            if (newData == null)
                return;

            if (newData == Project.Empty)
                CloseProject();
            else
                RequestOpenProject(newData);
        }

        void OnHighlightFilterInfoChanged(HighlightFilterInfo newData)
        {
            if (m_Bridge.IsInitialized)
            {
                var highlightActorHandle = m_Reflect.Hook.Systems.ActorRunner.GetActorHandle<HighlightActor>();
                if (newData.IsValid)
                {
                    var metadataActor = m_Reflect.Hook.Systems.ActorRunner.GetActorHandle<ObjectMetadataCacheActor>();
                    m_Bridge.ForwardRpc(metadataActor, new MetadataCategoryGroup { Group = newData.groupKey, FilterKey = newData.filterKey },
                        (List<DynamicGuid> ids) =>
                        {
                            m_Bridge.ForwardNet(highlightActorHandle, new SetHighlight() { HighlightedInstances = ids });
                        },
                        (ex) =>
                        {
                            Debug.LogError(ex.Message);
                        });
                }
                else
                {
                    m_Bridge.ForwardNet(highlightActorHandle, new CancelHighlight());
                }
            }
        }

        void OnObjectSelectionChanged(ObjectSelectionInfo newData)
        {
            SetNetworkSelected(newData);
        }

        void OnTeleportDataChanged(Vector3 newData)
        {
            if (m_UIStateData.navigationStateData.navigationMode == SetNavigationModeAction.NavigationMode.VR)
            {
                var teleportationProviders = Resources.FindObjectsOfTypeAll<TeleportationProvider>();
                if (teleportationProviders.Length > 0)
                {
                    var teleportationProvider = teleportationProviders[0];
                    if (!ReferenceEquals(teleportationProvider, null))
                    {
                        teleportationProvider.QueueTeleportRequest(new TeleportRequest()
                        {
                            destinationPosition = newData
                        });
                    }
                }
            }
        }

        List<FilterItemInfo> m_InvisibleFilterItemInfos = new List<FilterItemInfo>();

        void OnProjectFilterInfoDataChanged(FilterItemInfo newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            if (newData.visible)
                m_InvisibleFilterItemInfos.RemoveAll(x => x.groupKey == newData.groupKey && x.filterKey == newData.filterKey);
            else
                m_InvisibleFilterItemInfos.Add(newData);

            var metadataActor = m_Reflect.Hook.Systems.ActorRunner.GetActorHandle<ObjectMetadataCacheActor>();
            m_Bridge.ForwardNet(metadataActor, new MetadataCategoryGroupToggle(newData));
        }

        static void OnPlacementRulesGameObjectChanged(GameObject newPlacementGO)
        {
            if (newPlacementGO == null)
                return;

            ModuleLoaderCore.instance.GetModule<FunctionalityInjectionModule>().activeIsland
                .InjectFunctionalitySingle(newPlacementGO.gameObject.GetComponent<Replicator>());

            newPlacementGO.transform.parent = null;
            newPlacementGO.transform.localScale = Vector3.one;
            newPlacementGO.SetActive(true);
        }

        void OnVREnableChanged(bool newData)
        {
            if (m_Bridge.IsInitialized && m_PipelineStateData.deviceCapability.HasFlag(SetVREnableAction.DeviceCapability.SupportsAsyncGPUReadback))
            {
                var settings = m_Bridge.GetFirstMatchingSettings<SpatialActor.Settings>();
                m_Bridge.UpdateSetting<SpatialActor.Settings>(settings.Id, nameof(SpatialActor.Settings.UseDepthCulling), !newData);
            }
        }

        void OnUseCullingChanged(bool newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var settings = m_Bridge.GetFirstMatchingSettings<SpatialActor.Settings>();
            m_Bridge.UpdateSetting<SpatialActor.Settings>(settings.Id, nameof(SpatialActor.Settings.UseCulling), newData);
        }

        void OnUseDebugBoundingBoxChanged(bool newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var settings = m_Bridge.GetFirstMatchingSettings<BoundingBoxActor.Settings>();
            m_Bridge.UpdateSetting<BoundingBoxActor.Settings>(settings.Id, nameof(BoundingBoxActor.Settings.UseDebugMaterials), newData);
        }

        void OnSpatialPriorityWeightsChanged(Vector3 newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var settings = m_Bridge.GetFirstMatchingSettings<SpatialActor.Settings>();
            m_Bridge.UpdateSetting<SpatialActor.Settings>(settings.Id, nameof(SpatialActor.Settings.PriorityWeightAngle), newData.x);
            m_Bridge.UpdateSetting<SpatialActor.Settings>(settings.Id, nameof(SpatialActor.Settings.PriorityWeightDistance), newData.y);
            m_Bridge.UpdateSetting<SpatialActor.Settings>(settings.Id, nameof(SpatialActor.Settings.PriorityWeightSize), newData.z);
        }

        void OnUseSpatialManifestChanged(bool newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var settings = m_Bridge.GetFirstMatchingSettings<SyncTreeActor.Settings>();
            m_Bridge.UpdateSetting<SyncTreeActor.Settings>(settings.Id, nameof(SyncTreeActor.Settings.UseSpatialManifest), newData);
        }

        void OnUseHlodsChanged(bool newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var settings = m_Bridge.GetFirstMatchingSettings<SyncTreeActor.Settings>();
            m_Bridge.UpdateSetting<SyncTreeActor.Settings>(settings.Id, nameof(SyncTreeActor.Settings.UseHlods), newData);
        }

        void OnHlodDelayModeChanged(int newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var settings = m_Bridge.GetFirstMatchingSettings<SyncTreeActor.Settings>();
            m_Bridge.UpdateSetting<SyncTreeActor.Settings>(settings.Id, nameof(SyncTreeActor.Settings.HlodDelayMode), newData);
        }

        void OnHlodPrioritizerChanged(int newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var settings = m_Bridge.GetFirstMatchingSettings<SyncTreeActor.Settings>();
            m_Bridge.UpdateSetting<SyncTreeActor.Settings>(settings.Id, nameof(SyncTreeActor.Settings.Prioritizer), newData);
        }

        void OnTargetFpsChanged(int newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var settings = m_Bridge.GetFirstMatchingSettings<SyncTreeActor.Settings>();
            m_Bridge.UpdateSetting<SyncTreeActor.Settings>(settings.Id, nameof(SyncTreeActor.Settings.TargetFps), newData);
        }

        void OnShowActorDebugChanged(bool newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var settings = m_Bridge.GetFirstMatchingSettings<DebugActor.Settings>();
            m_Bridge.UpdateSetting<DebugActor.Settings>(settings.Id, nameof(DebugActor.Settings.ShowGui), newData);
        }

        void OnShowModelChanged(bool newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            m_PipelineStateData.rootNode.gameObject.SetActive(newData);

            if (newData)
            {
                var settings = m_Bridge.GetFirstMatchingSettings<BoundingBoxActor.Settings>();
                m_Bridge.UpdateSetting<BoundingBoxActor.Settings>(settings.Id, nameof(BoundingBoxActor.Settings.DisplayOnlyBoundingBoxes), false);
            }
        }

        void OnShowBoundingBoxModeChanged(bool newData)
        {
            if (!m_Bridge.IsInitialized)
                return;
            m_ARStateData.placementStateData.boundingBoxRootNode.gameObject.SetActive(newData);

            var settings = m_Bridge.GetFirstMatchingSettings<BoundingBoxActor.Settings>();
            m_Bridge.UpdateSetting<BoundingBoxActor.Settings>(settings.Id, nameof(BoundingBoxActor.Settings.DisplayOnlyBoundingBoxes), newData);
        }

        void OnInstructionUIStateChanged(SetInstructionUIStateAction.InstructionUIState newData)
        {
            if (newData == SetInstructionUIStateAction.InstructionUIState.Completed && m_UIStateData.progressData.progressState == SetProgressStateAction.ProgressState.NoPendingRequest)
            {
                // This has to be delayed because the renderers are
                // not activated instantly after the instruction is complete
                StartCoroutine(UIStateManager.current.TakeDelayedThumbnail());
            }
        }

        void OnEnableLightDataChanged(bool newData)
        {
            if (m_Bridge.IsInitialized)
            {
                var settings = m_Bridge.GetFirstMatchingSettings<LightActor.Settings>();
                if (settings != null)
                    m_Bridge.UpdateSetting<LightActor.Settings>(settings.Id, nameof(LightActor.Settings.EnableLights), newData);
            }
        }

        void OnModelScaleChanged(SetModelScaleAction.ArchitectureScale modelScale)
        {
            if (m_Bridge.IsInitialized)
            {
                var lightIntensity = Mathf.Pow(1 / (float)modelScale, 2);
                var settings = m_Bridge.GetFirstMatchingSettings<LightActor.Settings>();
                if (settings != null)
                    m_Bridge.UpdateSetting<LightActor.Settings>(settings.Id, nameof(LightActor.Settings.LightIntensity), lightIntensity);
            }
        }

        public bool HasChanged
        {
            get { return hasChanged; }
            set
            {
                if (!dispatcher.IsDispatching)
                    throw new InvalidOperationException("Must be invoked while dispatching.");
                hasChanged = value;
            }
        }

        //Dispatcher-forwarded methods so the API users do not have to care about the Dispatcher
        protected void Subscribe()
        {
            DispatchTokenClassAction = dispatcher.Register<Payload<IViewerAction>>(InvokeOnDispatchClassAction);
        }

        protected void Unsubscribe(string dispatchToken)
        {
            dispatcher.Unregister(dispatchToken);
        }

        public IEnumerator TakeDelayedThumbnail()
        {
            yield return new WaitForSeconds(1.0f);
            m_ProjectSettingStateData.activeProjectThumbnail = m_ThumbnailController.CaptureActiveProjectThumbnail(current.m_UIProjectStateData);
            m_ProjectManagementContextTarget.UpdateWith(ref m_ProjectSettingStateData);
        }

        T HasContextTargetChanged<T>(Payload<IViewerAction> viewerAction, IUIContext context, T stateData, IContextTarget contextTarget)
        {
            if (viewerAction.ActionType.RequiresContext(context, viewerAction.Data))
            {
                viewerAction.ActionType.ApplyPayload(viewerAction.Data, ref stateData, () =>
                {
                    contextTarget.UpdateWith(ref stateData);
                    HasChanged = true;
                });
            }

            return stateData;
        }

        ForceNavigationModeData HasForceNavigationModeContextTargetChanged(Payload<IViewerAction> viewerAction)
        {
            if (viewerAction.ActionType.RequiresContext(ForceNavigationModeContext.current, viewerAction.Data))
            {
                viewerAction.ActionType.ApplyPayload(viewerAction.Data, ref m_ForceNavigationModeStateData, () =>
                {
                    m_ForceNavigationModeContextTarget.UpdateWith(ref m_ForceNavigationModeStateData);
                    HasChanged = true;

                    if (m_ForceNavigationModeStateData.navigationMode.trigger)
                    {
                        var navigationMode = m_ForceNavigationModeStateData.navigationMode;
                        navigationMode.trigger = false;
                        m_ForceNavigationModeStateData.navigationMode = navigationMode;
                        m_ForceNavigationModeContextTarget.UpdateWith(ref m_ForceNavigationModeStateData);
                    }
                });
            }
            return m_ForceNavigationModeStateData;
        }

        void InvokeOnDispatchClassAction(Payload<IViewerAction> viewerAction)
        {
            HasChanged = false;

            lock (syncRoot)
            {
                m_ApplicationSettingsStateData = HasContextTargetChanged(viewerAction, ApplicationSettingsContext.current, m_ApplicationSettingsStateData, m_ApplicationSettingsContextTarget);
                m_PipelineStateData = HasContextTargetChanged(viewerAction, PipelineContext.current, m_PipelineStateData, m_PipelineContextTarget);
                m_RoomConnectionStateData = HasContextTargetChanged(viewerAction, RoomConnectionContext.current, m_RoomConnectionStateData, m_RoomConnectionContextTarget);
                m_UIProjectStateData = HasContextTargetChanged(viewerAction, ProjectContext.current, m_UIProjectStateData, m_ProjectStateContextTarget);
                m_ProjectSettingStateData = HasContextTargetChanged(viewerAction, ProjectManagementContext<Project>.current, m_ProjectSettingStateData, m_ProjectManagementContextTarget);
                m_LoginSettingStateData = HasContextTargetChanged(viewerAction, LoginSettingContext<EnvironmentInfo>.current, m_LoginSettingStateData, m_LoginSettingContextTarget);
                m_UIStateData = HasContextTargetChanged(viewerAction, UIStateContext.current, m_UIStateData, m_UIStateContextTarget);
                m_UIStateData.toolState = HasContextTargetChanged(viewerAction, ToolStateContext.current, m_UIStateData.toolState, m_ToolStateContextTarget);
                m_UIStateData.cameraOptionData = HasContextTargetChanged(viewerAction, CameraOptionsContext.current, m_UIStateData.cameraOptionData, m_CameraOptionsContextTarget);
                m_UIStateData.navigationStateData = HasContextTargetChanged(viewerAction, NavigationContext.current, m_UIStateData.navigationStateData, m_NavigationContextTarget);
                m_UIStateData.settingsToolStateData = HasContextTargetChanged(viewerAction, SettingsToolContext.current, m_UIStateData.settingsToolStateData, m_SettingsToolContextTarget);
                m_UIStateData.toolState.followUserTool = HasContextTargetChanged(viewerAction, FollowUserContext.current, m_UIStateData.toolState.followUserTool, m_FollowUserTarget);
                m_UIStateData.landingScreenFilterData = HasContextTargetChanged(viewerAction, LandingScreenContext.current, m_UIStateData.landingScreenFilterData, m_LandingScreenContextTarget);
                m_UIStateData.progressData = HasContextTargetChanged(viewerAction, ProgressContext.current, m_UIStateData.progressData, m_ProgressContextTarget);
                m_DragStateData = HasContextTargetChanged(viewerAction, DragStateContext.current, m_DragStateData, m_DragStateContextTarget);
                m_ExternalToolStateData.measureToolStateData = HasContextTargetChanged(viewerAction, MeasureToolContext.current, m_ExternalToolStateData.measureToolStateData, m_MeasureToolContextTarget);
                m_ExternalToolStateData.sunStudyData = HasContextTargetChanged(viewerAction, SunStudyContext.current, m_ExternalToolStateData.sunStudyData, m_SunStudyContextTarget);
                m_SceneOptionData = HasContextTargetChanged(viewerAction, SceneOptionContext.current, m_SceneOptionData, m_SceneOptionContextTarget);
                m_MessageManagerStateData = HasContextTargetChanged(viewerAction, MessageManagerContext.current, m_MessageManagerStateData, m_MessageManagerContextTarget);
                m_ARStateData.placementStateData = HasContextTargetChanged(viewerAction, ARPlacementContext.current, m_ARStateData.placementStateData, m_ARPlacementContextTarget);
                m_ARStateData.arToolStateData = HasContextTargetChanged(viewerAction, ARToolStateContext.current, m_ARStateData.arToolStateData, m_ARToolStateContextTarget);
                m_ARStateData = HasContextTargetChanged(viewerAction, ARContext.current, m_ARStateData, m_ARStateContextTarget);
                m_VRStateData = HasContextTargetChanged(viewerAction, VRContext.current, m_VRStateData, m_VRStateContextTarget);
                m_WalkStateData = HasContextTargetChanged(viewerAction, WalkModeContext.current, m_WalkStateData, m_WalkStateContextTarget);
                m_UIDebugStateData = HasContextTargetChanged(viewerAction, DebugStateContext.current, m_UIDebugStateData, m_DebugStateContextTarget);
                m_UIDebugStateData.statsInfoData = HasContextTargetChanged(viewerAction, StatsInfoContext.current, m_UIDebugStateData.statsInfoData, m_StateInfoContextTarget);
                m_UIDebugStateData.debugOptionsData = HasContextTargetChanged(viewerAction, DebugOptionContext.current, m_UIDebugStateData.debugOptionsData, m_DebugOptionContextTarget);
                m_ForceNavigationModeStateData = HasForceNavigationModeContextTargetChanged(viewerAction);
                m_AppBarStateData = HasContextTargetChanged(viewerAction, AppBarContext.current, m_AppBarStateData, m_AppBarContextTarget);
                m_UISessionStateData = HasContextTargetChanged(viewerAction, SessionStateContext<UnityUser, LinkPermission>.current, m_UISessionStateData, m_SessionStateContextTarget);
            }
        }

        void OnExternalToolStateContextChanged()
        {
            m_MeasureToolContextTarget.UpdateWith(ref m_ExternalToolStateData.measureToolStateData);
            m_SunStudyContextTarget.UpdateWith(ref m_ExternalToolStateData.sunStudyData);
        }

        void OnDragStateContextChanged()
        {
            m_DragStateContextTarget.UpdateWith(ref m_DragStateData);
        }

        void OnSceneOptionContextChanged()
        {
            m_SceneOptionContextTarget.UpdateWith(ref m_SceneOptionData);
        }

        void OnARStateContextChanged()
        {
            m_ARStateContextTarget.UpdateWith(ref m_ARStateData);
            var placementData = m_ARStateData.placementStateData;
            m_ARPlacementContextTarget.UpdateWith(ref placementData);
            m_ARToolStateContextTarget.UpdateWith(ref m_ARStateData.arToolStateData);
        }

        void OnPipelineStateContextChanged()
        {
            m_PipelineContextTarget.UpdateWith(ref m_DragStateData);
        }

        void OnDebugStateContextChanged()
        {
            m_DebugStateContextTarget.UpdateWith(ref m_UIDebugStateData);
            m_DebugOptionContextTarget.UpdateWith(ref m_UIDebugStateData.debugOptionsData);
            m_StateInfoContextTarget.UpdateWith(ref m_UIDebugStateData.statsInfoData);
        }

        void OnVRStateContextChanged()
        {
            m_VRStateContextTarget.UpdateWith(ref m_VRStateData);
        }

        void OnApplicationSettingsContextChanged()
        {
            m_ApplicationSettingsContextTarget.UpdateWith(ref m_ApplicationSettingsStateData);
        }

        void OnProjectStateContextChanged()
        {
            m_ProjectStateContextTarget.UpdateWith(ref m_UIProjectStateData);
        }

        void OnProjectSettingStateContextChanged()
        {
            m_ProjectManagementContextTarget.UpdateWith(ref m_ProjectSettingStateData);
        }

        void OnLoginSettingStateContextChanged()
        {
            m_LoginSettingContextTarget.UpdateWith(ref m_LoginSettingStateData);
        }

        void OnWalkStateContextChanged()
        {
            m_WalkStateContextTarget.UpdateWith(ref m_WalkStateData);
        }

        void OnForceNavigationModeContextChanged()
        {
            m_ForceNavigationModeContextTarget.UpdateWith(ref m_ForceNavigationModeStateData);
        }

        void OnFilterHLODsChanged(bool on)
        {
            if (!m_Bridge.IsInitialized)
                return;
            var spatialActor = m_Reflect.Hook.Systems.ActorRunner.GetActorHandle<SpatialActor>();
            if (on)
            {
                m_Bridge.ForwardNet(spatialActor, new AddVisibilityIgnoreFlag(SpatialActor.k_IsHlodFlag));
            }
            else
            {
                m_Bridge.ForwardNet(spatialActor, new RemoveVisibilityIgnoreFlag(SpatialActor.k_IsHlodFlag));
            }
        }

        void OnAppBarContextChanged()
        {
            m_AppBarContextTarget.UpdateWith(ref m_AppBarStateData);
        }

        void OnRoomConnectionContextChanged()
        {
            m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData);
        }

        void OnUIStateContextChanged()
        {
            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);
            m_ToolStateContextTarget.UpdateWith(ref m_UIStateData.toolState);
            m_CameraOptionsContextTarget.UpdateWith(ref m_UIStateData.cameraOptionData);
            m_NavigationContextTarget.UpdateWith(ref m_UIStateData.navigationStateData);
            m_SettingsToolContextTarget.UpdateWith(ref m_UIStateData.settingsToolStateData);
            m_FollowUserTarget.UpdateWith(ref m_UIStateData.toolState.followUserTool);
            m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);
            m_LandingScreenContextTarget.UpdateWith(ref m_UIStateData.landingScreenFilterData);
        }

        void OnSessionStateContextChanged()
        {
            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
        }

        void OnMessageManagerContextChanged()
        {
            m_MessageManagerContextTarget.UpdateWith(ref m_MessageManagerStateData);
        }
    }
}
