using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Stores;
using Unity.MARS;
using Unity.MARS.Providers;
using Unity.Reflect.ActorFramework;
using Unity.Reflect.Actors;
using Unity.Reflect.Collections;
using Unity.Reflect.Geometry;
using Unity.TouchFramework;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Pipeline;
using UnityEngine.SceneManagement;
using Unity.Reflect.Multiplayer;
using UnityEngine.Reflect.Viewer.Core;
using Unity.Reflect.Viewer.Actors;
using UnityEngine.Reflect.Viewer.Core.Actions;
using Unity.Reflect.Source.Utils.Errors;
#if UNITY_EDITOR
using UnityEditor.MARS.Simulation;
#endif

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Component that hold the state of the UI.
    /// Partial Class with non SharpFlux Connexions, Pipeline, ProjectLister and LoginManager
    /// </summary>
    public partial class UIStateManager: IStore<SceneOptionData>, IStore<VRStateData>
    {
#pragma warning disable CS0649
        [SerializeField]
        ViewerReflectBootstrapper m_Reflect;
        [SerializeField]
        PopUpManager m_PopUpManager;
        [SerializeField]
        NavigationModeUIController m_NavigationModeUIController;
        [SerializeField]
        float m_WaitingDelayToCloseStreamIndicator = 1f;
        [SerializeField]
        ViewerMessageManager m_MessageManager;
#pragma warning restore CS0649

        [SerializeField, Tooltip("Reflect Session Manager")]
        public LoginManager m_LoginManager;
        public LinkSharingManager m_LinkSharingManager;
        public AccessTokenManagerUpdater m_AccessTokenManagerUpdater;
        public ArgumentParser m_ArgumentParser;
        public SunStudy.SunStudy m_SunStudy;

        public EmbeddedProjectsComponent EmbeddedProjects;

        const float k_Timeout = 0.5f;

        OrbitModeUIController m_OrbitModeUIController;
        ThumbnailController m_ThumbnailController;

        Coroutine m_WaitStreamIndicatorCoroutine;
        WaitForSeconds m_WaitDelay;

        ActorSystemSetup m_LasLoadedAsset;

        BridgeActor.Proxy m_Bridge;
        ViewerBridgeActor.Proxy m_ViewerBridge;

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
        const string k_ProjectNotFoundTitle = "Linked project not found";
        const string k_ProjectNotFoundText = "The server hosting the linked project may be unavailable or the linked project no longer exists.";
        const string k_StopLoadDueToMemoryLimitationsText = "Objects have stopped loading due to memory limitations";
        const string k_OpenProjectList = "Open Project List";
        const string k_Login = "Login";
        const string k_Close = "Close";
        const string k_Error = "Error";
        const string k_NoSeats = "Cannot Assign Seat";
        const string k_MaxSeatsLoggedIn = "Maximum number of seats reached. Try again later or ask the owner to assign more seats to the project.";
        const string k_MaxSeatsLoggedOut = "Maximum number of seats reached. Try login to your Unity account to gain access or ask the owner to assign more seats to the project.";
        OpenInViewerInfo m_CachedOpenInViewerInfo = null;
        Dictionary<string, string> m_QueryArgs = new Dictionary<string, string>();
        MARSSession m_MarsSession = null;
        GameObject m_MarsSessionGameObject = null;

        IProvidesSessionControl IFunctionalitySubscriber<IProvidesSessionControl>.provider { get; set; }
        IProvidesPointCloud IFunctionalitySubscriber<IProvidesPointCloud>.provider { get; set; }
        IProvidesPlaneFinding IFunctionalitySubscriber<IProvidesPlaneFinding>.provider { get; set; }

        [SerializeField]
        bool m_VerboseLogging;

        Project m_RequestedProject = Project.Empty;
        Project m_CurrentOpenProject = Project.Empty;

        SpatialSelector m_TeleportSelector;

        public bool verboseLogging
        {
            get => m_VerboseLogging;
            set => m_VerboseLogging = value;
        }

        void AwakePipeline()
        {
            PipelineContext.current.ForceOnStateChanged();

            m_OrbitModeUIController = GetComponent<OrbitModeUIController>();
            m_ThumbnailController = GetComponent<ThumbnailController>();

            m_LoginManager.tokenUpdated.AddListener(OnTokenUpdated);
            m_LoginManager.userLoggedIn.AddListener(OnUserLoggedIn);
            m_LoginManager.userLoggedOut.AddListener(OnUserLoggedOut);
            m_LoginManager.authenticationFailed.AddListener(OnAuthenticationFailed);

            m_NavigationModeUIController.badVRConfigurationEvent.AddListener(OnBadVRConfiguration);

            m_LoginManager.linkSharingDetected.AddListener(OnLinkSharingDetected);
            m_LoginManager.openInViewerDetected.AddListener(OnOpenInViewerDetected);

            m_LoginManager.linkSharingDetectedWithArgs.AddListener(OnLinkSharingDetectedWithArgs);
            m_LoginManager.openInViewerDetectedWithArgs.AddListener(OnOpenInViewerDetectedWithArgs);

            m_LinkSharingManager.sharingLinkCreated.AddListener(OnSharingLinkCreated);
            m_LinkSharingManager.linkCreatedExceptionEvent.AddListener(OnLinkCreatedException);

            // License Manager
            m_AccessTokenManagerUpdater.createAccessTokenEvent.AddListener(OnCreateAccessToken);
            m_AccessTokenManagerUpdater.createAccessTokenExceptionEvent.AddListener(OnCreateAccessTokenException);

            m_AccessTokenManagerUpdater.createAccessTokenWithLinkTokenEvent.AddListener(OnAccessTokenCreatedWithLinkToken);
            m_AccessTokenManagerUpdater.createAccessTokenWithLinkTokenExceptionEvent.AddListener(OnCreateAccessTokenException);

            m_AccessTokenManagerUpdater.refreshAccessTokenEvent.AddListener(OnRefreshAccessToken);
            m_AccessTokenManagerUpdater.refreshAccessTokenExceptionEvent.AddListener(OnGeneralException);

            m_AccessTokenManagerUpdater.accessTokenExceptionEvent.AddListener(OnGeneralException);

            m_ArgumentParser = new ArgumentParser();
            m_ArgumentParser.Parse();

            m_WaitDelay = new WaitForSeconds(m_WaitingDelayToCloseStreamIndicator);

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

            // Deactivate mars session to avoid memory leak
            if (m_ProjectSettingStateData.activeProject == Project.Empty)
            {
                // if a project is already opened, we don't disable mars session
                EnableMARSSession(false);
            }

            yield return null;

            // for the Anonymous user with Public Link Sharing test in Editor
            // m_AccessTokenManagerUpdater.CreateAccessTokenWithLinkToken("Kd0H5qM6W4EKr5VuXJLQ2eLkATzEFfeIOmiQRewWpoUGQ",
            //     null, OpenProjectFromLinkSharing);
        }

        void EnableMARSSession(bool activate)
        {
            Debug.Log($"Enable MARSSession: {activate}");
            if (m_MarsSession == null)
            {
                m_MarsSession = FindObjectOfType<MARSSession>();
                if (m_MarsSession == null)
                {
                    Debug.Log($"Cannot find MARSSession Script");
                    return;
                }
            }

            m_MarsSession.enabled = activate;

            if (m_MarsSessionGameObject == null)
            {
                m_MarsSessionGameObject = GameObject.FindGameObjectWithTag("MarsSession");
                if (m_MarsSessionGameObject == null)
                {
                    Debug.Log($"Cannot find MARSSession GameObject");
                    return;
                }
            }

            m_MarsSessionGameObject.SetActive(activate);
        }

        void OnTokenUpdated(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                // update session state
                m_UISessionStateData.loggedState = LoginState.ProcessingToken;
            }

            RefreshProjectList();
        }

        void RefreshProjectList()
        {
            m_UISessionStateData.projectListState = ProjectListState.AwaitingUserData;
            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
        }

        void OnUserLoggedIn(UnityUser changedUser)
        {
            // if anonymous user and already opened a project
            if ((m_UISessionStateData.user == null ||
                    string.IsNullOrWhiteSpace(m_UISessionStateData.user.AccessToken)) &&
                m_ProjectSettingStateData.activeProject != Project.Empty)
            {
                Debug.Log("OnUserLoggedIn: exiting guest session and activating new logged in Unity account user");
                m_AppBarStateData.buttonInteractable = new ButtonInteractable { type = (int)ButtonType.ProjectList, interactable = true };
                m_AppBarContextTarget.UpdateWith(ref m_AppBarStateData);
                m_AppBarStateData.buttonInteractable = new ButtonInteractable { type = (int)ButtonType.LinkSharing, interactable = true };
                m_AppBarContextTarget.UpdateWith(ref m_AppBarStateData);

                PlayerClientBridge.MatchmakerManager.LeaveRoom();

                m_UISessionStateData.loggedState = LoginState.LoggedIn;
                m_UISessionStateData.projectListState = ProjectListState.AwaitingUserData;
                m_UISessionStateData.user = changedUser;
                m_UISessionStateData.userIdentity = new UserIdentity(null, -1, changedUser?.DisplayName, DateTime.MinValue, null);
                m_UISessionStateData.linkShareLoggedOut = false;
                m_UISessionStateData.rooms = new IProjectRoom[] { };

                ReflectProjectsManager.Dispose();
                var authClient = new AuthClient(changedUser);
                ReflectProjectsManager.Init(changedUser, m_Reflect.Hook, authClient);

                ConnectPipelineFactoryEvents();
                ReflectProjectsManager.RefreshProjects();
                m_UIStateData.progressData.progressState = SetProgressStateAction.ProgressState.PendingIndeterminate;
                m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);
                m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);

                m_AccessTokenManagerUpdater.ReleaseAccessTokenManager(m_ProjectSettingStateData.activeProject, () =>
                {
                    Debug.Log("Release AccessToken completed");
                    m_AccessTokenManagerUpdater.CreateAccessTokenWithLinkToken(m_UISessionStateData.cachedLinkToken, m_UISessionStateData.user.AccessToken, accessToken =>
                    {
                        Debug.Log("Create new AccessToken from Link completed");
                        OpenProjectFromLinkSharing(accessToken);
                    });
                });
                return;
            }

            PlayerClientBridge.MatchmakerManager.Connect(changedUser.AccessToken, m_MultiplayerController.connectToLocalServer);

            m_UIStateData.colorPalette = PlayerClientBridge.MatchmakerManager.Palette.Select(c =>
                new Color(c.R / (float)255, c.G / (float)255, c.B / (float)255)
            ).ToArray();

            // clear status message
            m_MessageManager.ClearAllMessage();

            // show landing screen when log in
            m_UIStateData.activeDialog = OpenDialogAction.DialogType.LandingScreen;

            // update session state
            m_UISessionStateData.loggedState = LoginState.LoggedIn;
            m_UISessionStateData.projectListState = ProjectListState.AwaitingUserData;
            m_UISessionStateData.user = changedUser;
            m_UISessionStateData.userIdentity = new UserIdentity(null, -1, changedUser?.DisplayName, DateTime.MinValue, null);
            m_UISessionStateData.linkShareLoggedOut = false;

            // Hack: Pipeline is in its own assembly, not available in Unity.Reflect
            var auth = new AuthClient(changedUser);
            ReflectProjectsManager.Init(changedUser, m_Reflect.Hook, auth);

            // connect all Pipeline Factory events
            ConnectPipelineFactoryEvents();

            ConnectMultiplayerEvents();

            // refreshProjects
            ReflectProjectsManager.RefreshProjects();

            m_UIStateData.progressData.progressState = SetProgressStateAction.ProgressState.PendingIndeterminate;
            m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);
            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);
        }

        void ConnectPipelineFactoryEvents()
        {
            // listing projects events
            ReflectProjectsManager.projectsRefreshCompleted.AddListener(OnProjectsRefreshCompleted);
            ReflectProjectsManager.projectsRefreshException +=  OnProjectsRefreshException;
            ReflectProjectsManager.projectDeleteProgressChanged +=  OnProjectDeleteProgressChanged;

            ReflectProjectsManager.projectStatusChanged += onProjectStatusChanged;
            ReflectProjectsManager.projectDownloadProgressChanged += OnProjectDownloadProgressChanged;
        }

        void OnAssetCountModified(StreamCountData streamCountData)
        {
            var statsInfoData = m_UIDebugStateData.statsInfoData;
            statsInfoData.assetsCountData = streamCountData;
            m_UIDebugStateData.statsInfoData = statsInfoData;
            m_DebugOptionContextTarget.UpdateWith(ref m_UIDebugStateData.debugOptionsData);
            m_StateInfoContextTarget.UpdateWith(ref m_UIDebugStateData.statsInfoData);
        }

        void OnInstanceCountModified(StreamCountData streamCountData)
        {
            var statsInfoData = m_UIDebugStateData.statsInfoData;
            statsInfoData.instancesCountData = streamCountData;
            m_UIDebugStateData.statsInfoData = statsInfoData;
            m_DebugOptionContextTarget.UpdateWith(ref m_UIDebugStateData.debugOptionsData);
            m_StateInfoContextTarget.UpdateWith(ref m_UIDebugStateData.statsInfoData);
        }

        void OnGameObjectCountModified(StreamCountData streamCountData)
        {
            var statsInfoData = m_UIDebugStateData.statsInfoData;
            statsInfoData.gameObjectsCountData = streamCountData;
            m_UIDebugStateData.statsInfoData = statsInfoData;
            m_DebugOptionContextTarget.UpdateWith(ref m_UIDebugStateData.debugOptionsData);
            m_StateInfoContextTarget.UpdateWith(ref m_UIDebugStateData.statsInfoData);
        }

        bool m_InstanceStreamEnd;

        Coroutine m_StreamingProgressedCoro;

        void OnGameObjectStreamEvent(int currentCount, int totalCount)
        {
            if (m_InstanceStreamEnd)
            {
                m_UIStateData.progressData.progressState = SetProgressStateAction.ProgressState.PendingDeterminate;
                m_UIStateData.progressData.currentProgress = Math.Min(currentCount, totalCount);
                m_UIStateData.progressData.totalCount = totalCount;
                m_UIStateData.progressData.message = "Streaming...";

                if (m_StreamingProgressedCoro != null)
                    return;

                m_StreamingProgressedCoro = StartCoroutine(DelayedUIUpdateForStreamingProgressed());
            }
        }

        IEnumerator DelayedUIUpdateForStreamingProgressed()
        {
            yield return new WaitForSeconds(0.2f);

            m_MessageManager.SetStatusMessage($"Streaming... {m_UIStateData.progressData.currentProgress}/{m_UIStateData.progressData.totalCount}");
            m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);


            if (m_WaitStreamIndicatorCoroutine != null)
                StopCoroutine(m_WaitStreamIndicatorCoroutine);

            m_WaitStreamIndicatorCoroutine = StartCoroutine(WaitCloseStreamIndicator());
            m_StreamingProgressedCoro = null;
        }

        IEnumerator WaitCloseStreamIndicator()
        {
            yield return m_WaitDelay;

            if (!m_ARStateData.arEnabled || m_ARStateData.instructionUIState == SetInstructionUIStateAction.InstructionUIState.Completed)
            {
                m_ProjectSettingStateData.activeProjectThumbnail = m_ThumbnailController.CaptureActiveProjectThumbnail(current.m_UIProjectStateData);
                m_ProjectManagementContextTarget.UpdateWith(ref m_ProjectSettingStateData);
            }

            m_UIStateData.progressData.progressState = SetProgressStateAction.ProgressState.NoPendingRequest;
            m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);

            m_WaitStreamIndicatorCoroutine = null;
        }

        void OnInstanceStreamEnd()
        {
            m_InstanceStreamEnd = true;
        }

        void OnInstanceStreamBegin()
        {
            m_InstanceStreamEnd = false;
            m_UIStateData.progressData.progressState = SetProgressStateAction.ProgressState.PendingIndeterminate;
            m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);
        }

        void OnGameObjectStreamEnd() { }

        void OnBoundsChanged(Bounds bb)
        {
            m_UIProjectStateData.rootBounds = bb;
            if (m_UIProjectStateData.zoneBounds == default)
            {
                m_UIProjectStateData.zoneBounds = bb;
            }

            m_ProjectStateContextTarget.UpdateWith(ref m_UIProjectStateData);
        }

        void OnSceneZonesChanged(Bounds bb)
        {
            m_UIProjectStateData.zoneBounds = bb;
            m_ProjectStateContextTarget.UpdateWith(ref m_UIProjectStateData);
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
                    m_MessageManager.SetStatusMessage(k_StopLoadDueToMemoryLimitationsText, StatusMessageType.Warning);
                    break;
            }
        }

        void OnMetadataGroupsChanged(IEnumerable<string> groups)
        {
            if (!EnumerableExtension.SafeSequenceEquals(groups, m_UIProjectStateData.filterGroupList))
            {
                m_UIProjectStateData.filterGroupList = new List<string>(groups);
                m_ProjectStateContextTarget.UpdateWith(ref m_UIProjectStateData, UpdateNotification.ForceNotify);
            }
        }

        void OnMetadataCategoriesChanged(string group, Diff<string> diff)
        {
            var items = new List<SetVisibleFilterAction.IFilterItemInfo>(m_UIProjectStateData.filterItemInfos);

            foreach(var category in diff.Added)
                items.Add(new MetadataGroupFilter(group, category, true));

            m_UIProjectStateData.filterItemInfos = items;

            m_LastReceivedFilterItemInfo = items;
            if (m_DelayedNotifyCoro != null)
                return;

            m_DelayedNotifyCoro = StartCoroutine(DelayedNotify());
        }

        Coroutine m_DelayedNotifyCoro;
        List<SetVisibleFilterAction.IFilterItemInfo> m_LastReceivedFilterItemInfo;

        IEnumerator DelayedNotify()
        {
            yield return new WaitForSeconds(0.2f);
            NotifyFilterItemInfos();
            m_LastReceivedFilterItemInfo = null;
            m_DelayedNotifyCoro = null;
        }

        void NotifyFilterItemInfos()
        {
            m_ProjectStateContextTarget.UpdateValueWith(nameof(m_UIProjectStateData.filterItemInfos), ref m_LastReceivedFilterItemInfo);
        }

        void OnObjectMetadataChanged(List<(DynamicGuid, Dictionary<string, string>)> idToGroupToFilterKeys)
        {
            if (!m_UIProjectStateData.highlightFilter.IsValid)
                return;

            var highlightActorHandle = m_Reflect.Hook.Systems.ActorRunner.GetActorHandle<HighlightActor>();
            var highlightedInstances = new List<DynamicGuid>();
            var otherInstances = new List<DynamicGuid>();

            foreach (var (id, groupToFilterKeys) in idToGroupToFilterKeys)
            {
                if (groupToFilterKeys.TryGetValue(m_UIProjectStateData.highlightFilter.groupKey, out var filterKey) && filterKey == m_UIProjectStateData.highlightFilter.filterKey)
                    highlightedInstances.Add(id);
                else
                    otherInstances.Add(id);
            }

            if (highlightedInstances.Count > 0)
                m_Bridge.ForwardNet(highlightActorHandle, new AddToHighlight { HighlightedInstances = highlightedInstances });

            if (otherInstances.Count > 0)
                m_Bridge.ForwardNet(highlightActorHandle, new RemoveFromHighlight { OtherInstances = otherInstances });
        }

        void OnUserLoggedOut()
        {
            CloseProject();

            // clear status message
            m_MessageManager.ClearAllMessage();

            // show login screen when log out
            m_UIStateData.activeDialog = OpenDialogAction.DialogType.LoginScreen;

            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);

            // update session state
            m_UISessionStateData.loggedState = LoginState.LoggedOut;
            m_UISessionStateData.user = null;
            m_UISessionStateData.userIdentity = default;
            m_UISessionStateData.rooms = new IProjectRoom[] { };
            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
            PlayerClientBridge.MatchmakerManager.LeaveRoom();
            PlayerClientBridge.MatchmakerManager.Disconnect();
        }

        void OnAuthenticationFailed(string errroMessage)
        {
#if UNITY_EDITOR
            m_LoginManager.userLoggedOut?.Invoke();
#else
            m_LoginManager.Logout();
#endif
        }

        void OnProjectsRefreshCompleted(IList<Project> projects)
        {
            m_UIStateData.progressData.progressState = SetProgressStateAction.ProgressState.NoPendingRequest;
            m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);

            var allProjects = new List<Project>(projects);

            if (EmbeddedProjects != null)
            {
                // Process embedded projects
                allProjects.AddRange(EmbeddedProjects.projectsData.Select(projectData => new EmbeddedProject(projectData)));
            }

            var newProjects = new List<IProjectRoom>();
            foreach (var proj in allProjects)
            {
                ProjectRoom toAdd;
                var existingProjectIndex = Array.FindIndex(m_UISessionStateData.rooms, pr => ((ProjectRoom)pr).project.serverProjectId == proj.serverProjectId);
                if (existingProjectIndex != -1)
                {
                    toAdd = new ProjectRoom(proj, ((ProjectRoom)m_UISessionStateData.rooms[existingProjectIndex]).users.ToArray());
                }
                else
                {
                    toAdd = new ProjectRoom(proj);
                }

                newProjects.Add(toAdd);
            }

            m_UISessionStateData.rooms = newProjects.ToArray();
            m_UISessionStateData.projectListState = ProjectListState.Ready;
            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData, UpdateNotification.ForceNotify);

            PlayerClientBridge.MatchmakerManager.MonitorRooms(projects.Select(p => p.serverProjectId));

            // Look for async ready condition
            Debug.Log("OnProjectsRefreshCompleted TryConsumeDeepLink");
            TryConsumeInteropRequest();
        }

        void TryConsumeInteropRequest()
        {
            if (m_UISessionStateData.projectListState.Equals(ProjectListState.Ready) &&
                m_UISessionStateData.collaborationState >= CollaborationState.ConnectedMatchmaker)
            {
                if (m_CachedOpenInViewerInfo != null)
                {
                    Debug.Log("Try consume cached 'Open in Project' request");
                    if (!TryOpenKnownProject(m_CachedOpenInViewerInfo))
                    {
                        PopupAccessDeniedMessage();
                    }

                    m_CachedOpenInViewerInfo = null;
                }

                if (m_UISessionStateData.isOpenWithLinkSharing && !string.IsNullOrWhiteSpace(m_UISessionStateData.cachedLinkToken))
                {
                    if (m_ProjectSettingStateData.activeProject != Project.Empty && !m_UISessionStateData.cachedLinkToken.Contains($"{m_ProjectSettingStateData.activeProject.UnityProject.LinkToken}"))
                    {
                        Debug.Log("Try processing cached 'Deep Link' request");
                        m_AccessTokenManagerUpdater.CreateAccessTokenWithLinkToken(m_UISessionStateData.cachedLinkToken, m_UISessionStateData.user.AccessToken, OpenProjectFromLinkSharing);
                    }
                }
            }
        }

        void OnProjectDeleteProgressChanged(Project project, int progress, int total)
        {
            m_UIStateData.progressData.progressState = SetProgressStateAction.ProgressState.PendingDeterminate;
            m_UIStateData.progressData.currentProgress = progress;
            m_UIStateData.progressData.totalCount = total;
            m_UIStateData.progressData.message = "Deleting";
            m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);
        }

        void onProjectStatusChanged(Project project, ProjectsManager.Status status)
        {
            if (status == ProjectsManager.Status.Downloaded || status == ProjectsManager.Status.Deleted)
            {
                m_UIStateData.progressData.progressState = SetProgressStateAction.ProgressState.NoPendingRequest;
                m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);
            }
        }

        void OnProjectDownloadProgressChanged(Project project, int progress, int total)
        {
            m_UIStateData.progressData.progressState = SetProgressStateAction.ProgressState.PendingDeterminate;
            m_UIStateData.progressData.currentProgress = progress;
            m_UIStateData.progressData.totalCount = total;
            m_UIStateData.progressData.message = "Downloading";
            m_ProgressContextTarget.UpdateWith(ref m_UIStateData.progressData);
        }

        void OnBadVRConfiguration(Action onRetry)
        {
            var data = m_PopUpManager.GetModalPopUpData();
            data.title = k_VRDeviceDisconnectedTitle;
            data.text = k_VRDeviceDisconnected;
            data.positiveText = "Retry";

            // Let user continue since he could connect the device later on
            data.positiveCallback = onRetry;
            data.negativeText = "Dismiss";
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

            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);
            m_ProjectStateContextTarget.UpdateWith(ref m_UIProjectStateData);
            m_ProjectManagementContextTarget.UpdateWith(ref m_ProjectSettingStateData);
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

        IEnumerator ReloadProject()
        {
            if (m_ProjectSettingStateData.activeProject != Project.Empty)
            {
                yield return null;
                OpenProject(m_ProjectSettingStateData.activeProject, m_ProjectSettingStateData.accessToken, true);
            }
        }

        void CloseProject(bool isRestarting = false)
        {
            if (m_CurrentOpenProject != Project.Empty)
            {
                if (!isRestarting)
                    m_MessageManager.SetStatusMessage($"Closing {m_ProjectSettingStateData.activeProject.name}...");

                m_AccessTokenManagerUpdater.ReleaseAccessTokenManager(m_ProjectSettingStateData.activeProject);

                m_Reflect.StreamingStarting -= OnStreamingStarting;
                var runner = m_Reflect.Hook.Systems.ActorRunner;
                runner.StopActorSystem();

                m_UIStateData.toolbarsEnabled = false;
                m_UIStateData.navigationStateData.EnableAllNavigation(false);
                m_UIStateContextTarget.UpdateWith(ref m_UIStateData);
                m_NavigationContextTarget.UpdateWith(ref m_UIStateData.navigationStateData);
                m_ProjectSettingStateData.activeProject = Project.Empty;
                m_CurrentOpenProject = Project.Empty;
                m_ProjectSettingStateData.activeProjectThumbnail = null;
                m_UIProjectStateData.objectSelectionInfo = default;
                m_UIProjectStateData.filterItemInfos = new List<SetVisibleFilterAction.IFilterItemInfo>();
                m_LastReceivedFilterItemInfo = new List<SetVisibleFilterAction.IFilterItemInfo>();
                if (m_DelayedNotifyCoro != null)
                {
                    StopCoroutine(m_DelayedNotifyCoro);
                    m_DelayedNotifyCoro = null;
                }

                m_ProjectManagementContextTarget.UpdateWith(ref m_ProjectSettingStateData);

                PlayerClientBridge.MatchmakerManager.LeaveRoom();

                //If it was a link shared project open
                if (((ProjectRoom)m_UISessionStateData.linkSharedProjectRoom).project?.UnityProject != null && ((ProjectRoom)m_UISessionStateData.linkSharedProjectRoom).project?.serverProjectId == m_ProjectSettingStateData.activeProject.serverProjectId)
                {
                    m_UISessionStateData.linkSharedProjectRoom = default;
                    m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
                }

                m_TeleportSelector?.Dispose();
                m_TeleportSelector = null;
            }
        }

        void CloseAllDialogs()
        {
            m_UIStateData.activeDialog = OpenDialogAction.DialogType.None;
            m_UIStateData.activeOptionDialog = CloseAllDialogsAction.OptionDialogType.None;
            m_UIStateData.activeSubDialog = OpenDialogAction.DialogType.None;
            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);
        }

        void ResetToolBars()
        {
            m_UIStateData.toolbarsEnabled = true;
            m_UIStateData.toolState.activeTool = SetActiveToolAction.ToolType.None;
            m_UIStateData.toolState.orbitType = SetOrbitTypeAction.OrbitType.OrbitAtPoint;
            m_UIStateData.activeToolbar = SetActiveToolBarAction.ToolbarType.OrbitSidebar;
            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);
            m_ToolStateContextTarget.UpdateWith(ref m_UIStateData.toolState);

            m_ARStateData.arToolStateData.selectionEnabled = true;
            m_ARStateData.arToolStateData.measureToolEnabled = true;
            m_ARToolStateContextTarget.UpdateWith( ref m_ARStateData.arToolStateData);
        }

        void RequestOpenProject(Project project)
        {
            if (m_RequestedProject == project || m_CurrentOpenProject.serverProjectId == project.serverProjectId)
            {
                return;
            }

            m_RequestedProject = project;

            if (project is EmbeddedProject || !project.IsConnectedToServer)
            {
                OpenProject(project);
                return;
            }

            m_AccessTokenManagerUpdater.CreateAccessToken(project,
                m_UISessionStateData.user.AccessToken,
                OpenProject);
        }

        void OnStreamingStarting(BridgeActor.Proxy bridge)
        {
            m_UIProjectStateData.rootBounds = default;
            m_UIProjectStateData.zoneBounds = default;
            m_ProjectStateContextTarget.UpdateWith(ref m_UIProjectStateData);

            bridge.Subscribe<GlobalBoundsUpdated>(ctx => OnBoundsChanged(ctx.Data.GlobalBounds));
            bridge.Subscribe<SceneZonesChanged>(ctx => OnSceneZonesChanged(ctx.Data.Zones[0].Bounds.ToUnity()));

            bridge.Subscribe<MetadataGroupsChanged>(ctx => OnMetadataGroupsChanged(ctx.Data.GroupKeys));
            bridge.Subscribe<MetadataCategoriesChanged>(ctx => OnMetadataCategoriesChanged(ctx.Data.GroupKey, ctx.Data.FilterKeys));
            bridge.Subscribe<ObjectMetadataChanged>(ctx => OnObjectMetadataChanged(ctx.Data.IdToGroupToFilterKeys));

            bridge.Subscribe<AssetCountChanged>(ctx =>
            {
                var statsInfoData = m_UIDebugStateData.statsInfoData;
                statsInfoData.assetsCountData = new StreamCountData
                {
                    addedCount = ctx.Data.ItemCount.NbAdded,
                    changedCount = ctx.Data.ItemCount.NbChanged,
                    removedCount = ctx.Data.ItemCount.NbRemoved
                };
                m_UIDebugStateData.statsInfoData = statsInfoData;
                m_DebugOptionContextTarget.UpdateWith(ref m_UIDebugStateData.debugOptionsData);
                m_StateInfoContextTarget.UpdateWith(ref m_UIDebugStateData.statsInfoData);
            });

            bridge.Subscribe<InstanceCountChanged>(ctx =>
            {
                if (m_UIDebugStateData.statsInfoData.instancesCountData.addedCount == 0 && ctx.Data.ItemCount.NbAdded > 0)
                {
                    OnInstanceStreamBegin();
                    OnInstanceStreamEnd();
                }

                var statsInfoData = m_UIDebugStateData.statsInfoData;
                statsInfoData.instancesCountData = new StreamCountData
                {
                    addedCount = ctx.Data.ItemCount.NbAdded,
                    changedCount = ctx.Data.ItemCount.NbChanged,
                    removedCount = ctx.Data.ItemCount.NbRemoved
                };
                m_UIDebugStateData.statsInfoData = statsInfoData;
                m_DebugOptionContextTarget.UpdateWith(ref m_UIDebugStateData.debugOptionsData);
                m_StateInfoContextTarget.UpdateWith(ref m_UIDebugStateData.statsInfoData);
            });

            bridge.Subscribe<GameObjectCountChanged>(ctx =>
            {
                var statsInfoData = m_UIDebugStateData.statsInfoData;
                statsInfoData.gameObjectsCountData = new StreamCountData
                {
                    addedCount = ctx.Data.ItemCount.NbAdded,
                    changedCount = ctx.Data.ItemCount.NbChanged,
                    removedCount = ctx.Data.ItemCount.NbRemoved
                };
                m_UIDebugStateData.statsInfoData = statsInfoData;
                m_DebugOptionContextTarget.UpdateWith(ref m_UIDebugStateData.debugOptionsData);
                m_StateInfoContextTarget.UpdateWith(ref m_UIDebugStateData.statsInfoData);
            });

            bridge.Subscribe<StreamingProgressed>(ctx => OnGameObjectStreamEvent(ctx.Data.NbStreamed, ctx.Data.Total));

            bridge.Subscribe<ReloadProject>(ctx => StartCoroutine(ReloadProject()));

            ConnectMultiplayerToActorSystem();
        }

        void OnOpenInViewerDetected(OpenInViewerInfo openInViewerInfo)
        {
            // Can only process one at a time
            if (m_CachedOpenInViewerInfo == null)
            {
                var isLogged = m_UISessionStateData.loggedState.Equals(LoginState.LoggedIn);
                m_UISessionStateData.linkShareLoggedOut = !isLogged;

                // Process openInViewerInfo only when user project list has been received
                if (isLogged && m_UISessionStateData.projectListState.Equals(ProjectListState.Ready))
                {
                    // Can we match the request with a valid project?
                    if (GetProjectToOpen(openInViewerInfo.ServerId, openInViewerInfo.ProjectId) != null)
                    {
                        if (m_ProjectSettingStateData.activeProject == Project.Empty)
                        {
                            TryOpenKnownProject(openInViewerInfo);
                        }
                        else
                        {
                            // Ask to close current project only if not the same
                            if (m_ProjectSettingStateData.activeProject.projectId != openInViewerInfo.ProjectId && m_ProjectSettingStateData.activeProject.serverProjectId != openInViewerInfo.ServerId)
                            {
                                PopupCloseProjectDialog(openInViewerInfo, UserApprovedOpenInViewer);
                            }
                        }
                    }
                    else
                    {
                        PopupAccessDeniedMessage();
                    }
                }
                else
                {
                    m_CachedOpenInViewerInfo = openInViewerInfo;
                }

                m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
            }
        }

        void OnOpenInViewerDetectedWithArgs(OpenInViewerInfo openInViewerInfo, Dictionary<string, string> queryArgs)
        {
            m_QueryArgs = queryArgs;
            OnOpenInViewerDetected(openInViewerInfo);
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
            data.negativeCallback = delegate
            { };
            m_PopUpManager.DisplayModalPopUp(data);
        }

        void UserApprovedOpenInViewer(OpenInViewerInfo openInViewerInfo)
        {
            if (m_UISessionStateData.loggedState.Equals(LoginState.LoggedIn) &&
                m_UISessionStateData.projectListState.Equals(ProjectListState.Ready))
            {
                if (!TryOpenKnownProject(openInViewerInfo))
                {
                    PopupAccessDeniedMessage();
                }
            }
            else
            {
                m_CachedOpenInViewerInfo = openInViewerInfo;
            }
        }

        bool TryOpenKnownProject(OpenInViewerInfo openInViewerInfo)
        {
            var project = GetProjectToOpen(openInViewerInfo.ServerId, openInViewerInfo.ProjectId);
            if (project != null)
            {
                RequestOpenProject(project);
                return true;
            }

            return false;
        }

        void OnLinkSharingDetected(string linkToken)
        {
            StartCoroutine(LinkSharingDetected(linkToken));
        }

        IEnumerator LinkSharingDetected(string linkToken)
        {
            while (!m_Initialized)
            {
                yield return null;
            }

            m_UISessionStateData.isOpenWithLinkSharing = true;
            m_UISessionStateData.cachedLinkToken = linkToken;

            if (m_UISessionStateData.loggedState.Equals(LoginState.LoggedIn) &&
                m_UISessionStateData.projectListState.Equals(ProjectListState.Ready))
            {
                m_AccessTokenManagerUpdater.CreateAccessTokenWithLinkToken(linkToken, m_UISessionStateData.user.AccessToken, OpenProjectFromLinkSharing);
                m_UISessionStateData.linkShareLoggedOut = false;
            }
            else if (!m_UISessionStateData.loggedState.Equals(LoginState.ProcessingToken) && !m_UISessionStateData.loggedState.Equals(LoginState.LoggedIn))
            {
                m_AccessTokenManagerUpdater.CreateAccessTokenWithLinkToken(linkToken, null, OpenProjectFromLinkSharing);
                m_UISessionStateData.linkShareLoggedOut = false;
            }
            else
            {
                m_UISessionStateData.linkShareLoggedOut = true;
            }

            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
        }

        IEnumerator StartLogin()
        {
            yield return null;
            m_MessageManager.SetStatusMessage("Logging in...");
            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);

            m_UISessionStateData.loggedState = LoginState.LoggingIn;
            m_LoginManager.Login();
            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
        }

        void OnLinkSharingDetectedWithArgs(string linkToken, Dictionary<string, string> queryArgs)
        {
            m_QueryArgs = queryArgs;
            OnLinkSharingDetected(linkToken);
        }

        Project GetProjectToOpen(string serverId, string projectId)
        {
            return m_UISessionStateData.rooms
                .Select(x => ((ProjectRoom)x).project)
                .FirstOrDefault(x => x.host.ServerId.Equals(serverId)
                    && x.projectId.Equals(projectId));
        }

        void StarOpenProjectFromModal(AccessToken accessToken)
        {
            StartCoroutine(OpenProjectFromModal(accessToken));
        }

        IEnumerator OpenProjectFromModal(AccessToken accessToken)
        {
            // Avoid sticky modal popup
            yield return new WaitForSeconds(0.5f);
            OpenProject(accessToken);
        }

        void PopupAccessDeniedMessage()
        {
            // error popup. don't exist the project on your project list
            var data = m_PopUpManager.GetModalPopUpData();
            data.title = k_AccessDeniedTitle;
            data.text = k_AccessDeniedText;
            data.positiveText = k_Close;
            m_PopUpManager.DisplayModalPopUp(data);
        }

        void OnSharingLinkCreated(SharingLinkInfo sharingLinkInfo)
        {
            Debug.Log(sharingLinkInfo.Domain);
            Debug.Log(sharingLinkInfo.LinkToken);
        }

        void OnLinkCreatedException(Exception exception)
        {
            Debug.Log("OnLinkCreatedException");
        }

        void OpenProjectFromLinkSharing(AccessToken accessToken)
        {
            Debug.Log($"accessToken.SyncServiceAccessToken = {accessToken.SyncServiceAccessToken}");
            m_RequestedProject = new Project(accessToken.UnityProject);

            if (m_ProjectSettingStateData.activeProject != Project.Empty)
            {
                // Ask to close only if not the same project
                if (m_ProjectSettingStateData.activeProject.serverProjectId != m_RequestedProject.serverProjectId)
                {
                    // close project popup.
                    var data = m_PopUpManager.GetModalPopUpData();
                    data.title = k_CloseProjectTitle;
                    data.text = k_CloseProjectText;
                    data.negativeText = "Cancel";
                    data.positiveCallback = delegate
                    {
                        StarOpenProjectFromModal(accessToken);
                    };
                    data.negativeCallback = delegate
                    {
                        m_RequestedProject = Project.Empty;
                    };
                    m_PopUpManager.DisplayModalPopUp(data);
                }
                else
                {
                    // Same project. Handle query args, if any.
                    if (m_QueryArgs.Count > 0)
                    {
                        QueryArgHandler.InvokeQueryArgMethods(m_QueryArgs);
                        m_QueryArgs.Clear();
                    }
                }
            }
            else
            {
                OpenProject(accessToken);
            }
        }

        void OnCreateAccessToken(AccessToken accessToken)
        {
            Debug.Log("OnCreateAccessToken");
        }

        void OnRefreshAccessToken(AccessToken accessToken)
        {
            Debug.Log("OnRefreshAccessToken");
        }

        void OnAccessTokenCreatedWithLinkToken(AccessToken accessToken)
        {
            Debug.Log("OnAccessTokenCreatedWithLinkToken");
        }

        void SetAccessTokenUser(UnityUser user)
        {
            // If Viewer has not been logged in yet. (Anonymous User)
            if (m_UISessionStateData.user == null || m_UISessionStateData.user.UserId != user.UserId)
            {
                ConnectMultiplayerEvents();

                PlayerClientBridge.MatchmakerManager.Connect(user.AccessToken,
                    m_MultiplayerController.connectToLocalServer);

                m_UIStateData.colorPalette = PlayerClientBridge.MatchmakerManager.Palette.Select(c =>
                    new Color(c.R / (float)255, c.G / (float)255, c.B / (float)255)
                ).ToArray();

                m_UISessionStateData.user = user;
                m_UISessionStateData.userIdentity =
                    new UserIdentity(null, -1, user?.DisplayName, DateTime.MinValue, null);

                m_UIStateContextTarget.UpdateWith(ref m_UIStateData);
                m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);

                // if anonymous user, Hide Appbar buttons
                if (string.IsNullOrWhiteSpace(m_UISessionStateData.user.AccessToken))
                {
                    m_AppBarStateData.buttonInteractable = new ButtonInteractable { type = (int)ButtonType.ProjectList, interactable = false };
                    m_AppBarContextTarget.UpdateWith(ref m_AppBarStateData);
                    m_AppBarStateData.buttonInteractable = new ButtonInteractable { type = (int)ButtonType.LinkSharing, interactable = false };
                    m_AppBarContextTarget.UpdateWith(ref m_AppBarStateData);
                }
            }
        }

        void OpenProject(AccessToken accessToken)
        {
            // Validate Requested project match returned project in AccessToken
            if (m_RequestedProject != Project.Empty && m_RequestedProject.UnityProject.ProjectId.Equals(accessToken.UnityProject.ProjectId))
            {
                OpenProject(m_RequestedProject, accessToken);
            }
            else
            {
                Debug.LogWarning($"AccessToken Project mismatch: {m_RequestedProject.UnityProject.Name}, {m_RequestedProject.UnityProject.ProjectId}/{accessToken.UnityProject.ProjectId}");
            }
        }

        void OpenProject(Project project, AccessToken accessToken = null, bool isRestarting = false)
        {
            m_RequestedProject = Project.Empty;
            CloseProject(isRestarting);

            CloseAllDialogs();

            project.host.SyncServerAccessToken = accessToken?.SyncServiceAccessToken;
            m_CurrentOpenProject = m_ProjectSettingStateData.activeProject = project;
            m_ProjectSettingStateData.accessToken = accessToken;

            if (accessToken != null)
            {
                 //m_UISessionStateData.sessionState.dnaLicenseInfo.floatingSeat = accessToken.FloatingSeatDuration; FixMe
                 //m_UISessionStateData.sessionState.dnaLicenseInfo.entitlements = accessToken.GetEntitlements(); FixMe
            }

            EnableMARSSession(true);

            var projectIndex = Array.FindIndex(m_UISessionStateData.rooms, (room) => ((ProjectRoom)room).project.serverProjectId == project.serverProjectId);
            if (projectIndex == -1) // Opening a project not in the project list
            {
                m_UISessionStateData.linkSharedProjectRoom = new ProjectRoom(project);
            }

            if (accessToken != null)
                SetAccessTokenUser(accessToken.UnityUser);

            if (!isRestarting)
                m_MessageManager.SetStatusMessage($"Opening {m_ProjectSettingStateData.activeProject.name}...");

            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);

            m_ProjectSettingStateData.activeProjectThumbnail = ThumbnailController.LoadThumbnailForProject(m_ProjectSettingStateData.activeProject);
            m_ProjectManagementContextTarget.UpdateWith(ref m_ProjectSettingStateData);

            m_Reflect.StreamingStarting += OnStreamingStarting;

            m_Reflect.OpenProject(project, m_UISessionStateData.user, accessToken, isRestarting, bridge =>
            {
                if (!bridge.IsInitialized)
                    return;

                m_Bridge = bridge;
                m_ViewerBridge = m_Reflect.ViewerBridge;

                // Update UI with value in asset only when we change the asset
                // This will preserve the current state when changing project with the same setup
                var reloadSettings = m_Reflect.Asset != m_LasLoadedAsset;
                m_LasLoadedAsset = m_Reflect.Asset;
                if (reloadSettings)
                {
                    m_SceneOptionData.enableLightData = m_Bridge.GetFirstOrEmptySettings<LightActor.Settings>().EnableLights;
                    m_UIStateData.syncEnabled = m_Bridge.GetFirstOrEmptySettings<SyncTreeActor.Settings>().IsLiveSyncEnabled;

                    var boxSettings = m_Bridge.GetFirstOrEmptySettings<BoundingBoxActor.Settings>();
                    boxSettings.DisplayOnlyBoundingBoxes = false;

                    var debugOptionsData = m_UIDebugStateData.debugOptionsData;
                    debugOptionsData.useDebugBoundingBoxMaterials = boxSettings.UseDebugMaterials;

                    var spatialSettings = m_Bridge.GetFirstOrEmptySettings<SpatialActor.Settings>();

                    // ensure depth culling is off by default on devices that don't support AsycGPUReadback
                    spatialSettings.UseDepthCulling &= m_PipelineStateData.deviceCapability.HasFlag(SetVREnableAction.DeviceCapability.SupportsAsyncGPUReadback);

                    debugOptionsData.spatialPriorityWeights = new Vector3(spatialSettings.PriorityWeightAngle,
                        spatialSettings.PriorityWeightDistance, spatialSettings.PriorityWeightSize);

                    debugOptionsData.useCulling = spatialSettings.UseCulling;

                    var syncTreeSettings = m_Bridge.GetFirstOrEmptySettings<SyncTreeActor.Settings>();
                    debugOptionsData.useSpatialManifest = syncTreeSettings.UseSpatialManifest;
                    debugOptionsData.useHlods = syncTreeSettings.UseHlods;
                    debugOptionsData.hlodDelayMode = (int)syncTreeSettings.HlodDelayMode;
                    debugOptionsData.hlodPrioritizer = (int)syncTreeSettings.Prioritizer;
                    debugOptionsData.targetFps = syncTreeSettings.TargetFps;

                    var debugActorSettings = m_Bridge.GetFirstOrEmptySettings<DebugActor.Settings>();
                    debugOptionsData.showActorDebug = debugActorSettings.ShowGui;

                    m_UIDebugStateData.debugOptionsData = debugOptionsData;
                }
                else
                {
                    m_Bridge.GetFirstOrEmptySettings<LightActor.Settings>().EnableLights = m_SceneOptionData.enableLightData;
                    m_Bridge.GetFirstOrEmptySettings<SyncTreeActor.Settings>().IsLiveSyncEnabled = m_UIStateData.syncEnabled;

                    var boxSettings = m_Bridge.GetFirstOrEmptySettings<BoundingBoxActor.Settings>();
                    boxSettings.DisplayOnlyBoundingBoxes = false;
                    boxSettings.UseDebugMaterials = m_UIDebugStateData.debugOptionsData.useDebugBoundingBoxMaterials;

                    var spatialSettings = m_Bridge.GetFirstOrEmptySettings<SpatialActor.Settings>();

                    // ensure depth culling is off by default on devices that don't support AsycGPUReadback
                    spatialSettings.UseDepthCulling &= m_PipelineStateData.deviceCapability.HasFlag(SetVREnableAction.DeviceCapability.SupportsAsyncGPUReadback);
                    spatialSettings.PriorityWeightAngle = m_UIDebugStateData.debugOptionsData.spatialPriorityWeights.x;
                    spatialSettings.PriorityWeightDistance = m_UIDebugStateData.debugOptionsData.spatialPriorityWeights.y;
                    spatialSettings.PriorityWeightSize = m_UIDebugStateData.debugOptionsData.spatialPriorityWeights.z;
                    spatialSettings.UseCulling = m_UIDebugStateData.debugOptionsData.useCulling;

                    var syncTreeSettings = m_Bridge.GetFirstOrEmptySettings<SyncTreeActor.Settings>();
                    syncTreeSettings.UseSpatialManifest = m_UIDebugStateData.debugOptionsData.useSpatialManifest;
                    syncTreeSettings.UseHlods = m_UIDebugStateData.debugOptionsData.useHlods;
                    syncTreeSettings.HlodDelayMode = (HlodMode)m_UIDebugStateData.debugOptionsData.hlodDelayMode;
                    syncTreeSettings.Prioritizer = (SyncTreeActor.Prioritizer)m_UIDebugStateData.debugOptionsData.hlodPrioritizer;
                    syncTreeSettings.TargetFps = m_UIDebugStateData.debugOptionsData.targetFps;

                    var debugActorSettings = m_Bridge.GetFirstOrEmptySettings<DebugActor.Settings>();
                    debugActorSettings.ShowGui = m_UIDebugStateData.debugOptionsData.showActorDebug;
                }
            });

            // set enable texture and light
            Shader.SetGlobalFloat(SetEnableTextureAction.k_UseTexture, m_SceneOptionData.enableTexture ? 1 : 0);

            m_ARStateData.placementStateData.boundingBoxRootNode.gameObject.SetActive(true);

            var spatialSettings = m_Reflect.Bridge.GetFirstMatchingSettings<SpatialActor.Settings>();
            if (spatialSettings != null)
            {
                m_TeleportSelector = new SpatialSelector
                {
                    SpatialPicker = m_Reflect.ViewerBridge,
                    SpatialPickerAsync = m_Reflect.ViewerBridge,
                    WorldRoot = m_PipelineStateData.rootNode
                };
                m_UIProjectStateData.teleportPicker = m_TeleportSelector;

                m_ProjectStateContextTarget.UpdateWith(ref m_UIProjectStateData);
            }

            // reset the toolbars
            ResetToolBars();
            m_UIStateData.navigationStateData.EnableAllNavigation(true);

            m_UIStateData.settingsToolStateData = new SettingsToolStateData
            {
                bimFilterEnabled = true,
                sceneSettingsEnabled = true,
                sunStudyEnabled = true,
                markerSettingsEnabled = true
            };

            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);
            m_NavigationContextTarget.UpdateWith(ref m_UIStateData.navigationStateData, UpdateNotification.ForceNotify);
            m_SettingsToolContextTarget.UpdateWith(ref m_UIStateData.settingsToolStateData);
            m_ToolStateContextTarget.UpdateWith(ref m_UIStateData.toolState);
            m_DebugOptionContextTarget.UpdateWith(ref m_UIDebugStateData.debugOptionsData);
            m_StateInfoContextTarget.UpdateWith(ref m_UIDebugStateData.statsInfoData);
            m_SceneOptionContextTarget.UpdateWith(ref m_SceneOptionData);

            if (accessToken != null)
                PlayerClientBridge.MatchmakerManager.JoinRoom(m_ProjectSettingStateData.activeProject.serverProjectId, () => m_ProjectSettingStateData.accessToken.CloudServicesAccessToken);

            QueryArgHandler.InvokeQueryArgMethods(m_QueryArgs);
            m_QueryArgs.Clear();
        }

        void ConfigurePopupExceptionMessage(Exception exception, ref ModalPopup.ModalPopupData data)
        {
            switch (exception)
            {
                case NoSeatEntitlementException _:
                    data.title = k_NoSeats;
                    if (m_UISessionStateData.loggedState.Equals(LoginState.LoggedIn))
                    {
                        data.text = k_MaxSeatsLoggedIn;
                    }
                    else
                    {
                        data.text = k_MaxSeatsLoggedOut;
                    }
                    break;
                case ForbiddenException _:
                    data.title = k_AccessDeniedTitle;
                    data.text = k_AccessDeniedText;
                    break;
                default:
                    data.title = k_Error;
                    data.text = exception.Message;
                    break;
            }
        }

        void OnGeneralException(Exception exception)
        {
            var data = m_PopUpManager.GetModalPopUpData();
            ConfigurePopupExceptionMessage(exception, ref data);

            data.positiveText = k_Close;
            m_PopUpManager.DisplayModalPopUp(data);
        }

        void OnCreateAccessTokenException(Exception exception)
        {
            Debug.LogException(exception);
            if (m_RequestedProject != Project.Empty && m_RequestedProject.IsLocal)
            {
                OpenProject(m_RequestedProject);
            }
            else
            {
                CloseAllDialogs();

                Action additionalAction = null;
                var data = m_PopUpManager.GetModalPopUpData();
                data.positiveCallback = () =>
                {
                    //to cancel failed project request if any
                    m_RequestedProject = Project.Empty;
                    m_ProjectSettingStateData.activeProject = m_CurrentOpenProject;
                    m_ProjectManagementContextTarget.UpdateWith(ref m_ProjectSettingStateData);

                    additionalAction?.Invoke();
                };

                if (m_CurrentOpenProject == Project.Empty)
                {
                    if (m_UISessionStateData.user == null)
                    {
                        data.positiveText = k_Login;
                        additionalAction = () =>
                        {
                            m_UIStateData.activeDialog = OpenDialogAction.DialogType.LoginScreen;
                            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);
                        };
                    }
                    else
                    {
                        data.positiveText = k_OpenProjectList;
                        additionalAction = () =>
                        {
                            RefreshProjectList();
                            m_UIStateData.activeDialog = OpenDialogAction.DialogType.LandingScreen;
                            m_UIStateContextTarget.UpdateWith(ref m_UIStateData);
                        };
                    }
                }

                ConfigurePopupExceptionMessage(exception, ref data);

                m_PopUpManager.DisplayModalPopUp(data);
            }
        }

        void OnAccessTokenCreateWithLinkTokenException(Exception exception)
        {
            Debug.LogException(exception);
        }

        void OnRefreshAccessTokenException(Exception exception)
        {
            // when refresh Token is failed. what we gonna do?
            Debug.LogException(exception);
        }

        void OnProjectsRefreshException(ProjectListRefreshException exception)
        {
            var data = m_PopUpManager.GetModalPopUpData();
            data.positiveText = k_Close;
            data.title = k_Error;
            data.text = exception.Message + "\n Previously Downloaded Projects might still be available";
            m_PopUpManager.DisplayModalPopUp(data);
        }
    }
}
