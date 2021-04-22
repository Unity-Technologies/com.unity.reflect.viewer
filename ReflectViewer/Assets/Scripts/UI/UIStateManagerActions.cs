using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using SharpFlux.Stores;
using Unity.MARS;
using Unity.MARS.Providers;
using Unity.Reflect.Multiplayer;
using Unity.Reflect.Streaming;
using Unity.TouchFramework;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Pipeline;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Component that hold the state of the UI.
    /// Partial Class with Actions and Dispatcher
    /// </summary>
    public partial class UIStateManager : MonoBehaviour,
        IStore<UIStateData>, IStore<UISessionStateData>, IStore<UIProjectStateData>, IStore<UIARStateData>,
        IStore<UIDebugStateData>, IStore<ApplicationStateData>, IStore<RoomConnectionStateData>,IStore<ExternalToolStateData>, IStore<DragStateData>,
        IUsesSessionControl, IUsesPointCloud, IUsesPlaneFinding
    {
        //Returns whether the store has changed during the most recent dispatch
        bool hasChanged;

        IDispatcher dispatcher;
        public string DispatchToken { get; private set; }

        [SerializeField, Tooltip("State of the UI")]
        UIStateData m_UIStateData;
        [SerializeField, Tooltip("State of the Session")]
        UISessionStateData m_UISessionStateData;
        [SerializeField, Tooltip("State of the Project")]
        UIProjectStateData m_UIProjectStateData;
        [SerializeField, Tooltip("State of the AR Simulation")]
        UIARStateData m_ARStateData;
        [SerializeField, Tooltip("State of the walk Simulation")]
        UIWalkStateData m_WalkStateData;
        [SerializeField, Tooltip("State of the Debug Data")]
        UIDebugStateData m_UIDebugStateData;
        [SerializeField, Tooltip("State of the Application Data")]
        ApplicationStateData m_ApplicationStateData;
        [SerializeField, Tooltip("State of the Application Data")]
        RoomConnectionStateData m_RoomConnectionStateData;
        [SerializeField, Tooltip("State of the Tool")]
        ExternalToolStateData m_ExternalToolStateData;
        [SerializeField, Tooltip("State of Drag")]
        DragStateData m_DragStateData;

        public PopUpManager popUpManager => m_PopUpManager;

        /// <summary>
        /// State of the UI
        /// </summary>
        public UIStateData stateData
        {
            get => m_UIStateData;
        }

        /// <summary>
        /// State of the Session
        /// </summary>
        public UISessionStateData sessionStateData
        {
            get => m_UISessionStateData;
        }

        /// <summary>
        /// State of the Project
        /// </summary>
        public UIProjectStateData projectStateData
        {
            get => m_UIProjectStateData;
        }

        /// <summary>
        /// State of the Project
        /// </summary>
        public UIARStateData arStateData
        {
            get => m_ARStateData;
        }

        public UIDebugStateData debugStateData
        {
            get => m_UIDebugStateData;
        }

        public UIWalkStateData walkStateData
        {
            get => m_WalkStateData;
        }

        /// <summary>
        /// State of the Application
        /// </summary>
        public ApplicationStateData applicationStateData { get => m_ApplicationStateData; }

        /// <summary>
        /// State of the Application
        /// </summary>
        public RoomConnectionStateData roomConnectionStateData { get => m_RoomConnectionStateData; }

        /// <summary>
        /// State of the Tool
        /// </summary>
        public ExternalToolStateData externalToolStateData { get => m_ExternalToolStateData; }

        /// <summary>
        /// State of Drag
        /// </summary>
        public DragStateData dragStateData { get => m_DragStateData; }

        UIStateData IStore<UIStateData>.Data => m_UIStateData;

        UISessionStateData IStore<UISessionStateData>.Data => m_UISessionStateData;

        UIProjectStateData IStore<UIProjectStateData>.Data => m_UIProjectStateData;

        UIARStateData IStore<UIARStateData>.Data => m_ARStateData;

        UIDebugStateData IStore<UIDebugStateData>.Data => m_UIDebugStateData;

        ApplicationStateData IStore<ApplicationStateData>.Data => m_ApplicationStateData;

        RoomConnectionStateData IStore<RoomConnectionStateData>.Data => m_RoomConnectionStateData;

        ExternalToolStateData IStore<ExternalToolStateData>.Data => m_ExternalToolStateData;

        UIWalkStateData IStore<UIWalkStateData>.Data => m_WalkStateData;

        DragStateData IStore<DragStateData>.Data => m_DragStateData;

        /// <summary>
        /// Event signaled when the state of the UI has changed
        /// </summary>
        public static event Action<UIStateData> stateChanged = delegate { };

        /// <summary>
        /// Event signaled when the state of the session has changed
        /// </summary>
        public static event Action<UISessionStateData> sessionStateChanged = delegate { };

        /// <summary>
        /// Event signaled when the state of the Project has changed
        /// </summary>
        public static event Action<UIProjectStateData> projectStateChanged = delegate { };

        /// <summary>
        /// Event signaled when the state of the AR Simulation has changed
        /// </summary>
        public static event Action<UIARStateData> arStateChanged = delegate { };

        /// <summary>
        /// Event signaled when the state of the walk Simulation has changed
        /// </summary>
        public static event Action<UIWalkStateData> walkStateChanged = delegate { };

        /// <summary>
        /// Event signaled when the state of the debug Simulation has changed
        /// </summary>
        public static event Action<UIDebugStateData> debugStateChanged = delegate { };

        /// <summary>
        /// Event signaled when the state of the Application has changed
        /// </summary>
        public static event Action<ApplicationStateData> applicationStateChanged = delegate { };

        /// <summary>
        /// Event signaled when the state of the Connection has changed
        /// </summary>
        public static event Action<RoomConnectionStateData> roomConnectionStateChanged = delegate { };

        /// <summary>
        /// Event signaled when the state of a Tool has changed
        /// </summary>
        public static event Action<ExternalToolStateData> externalToolChanged = delegate {};

        /// <summary>
        /// Event signaled when the Login Setting has changed
        /// </summary>
        ///
        public static event Action loginSettingChanged = delegate {};

        /// <summary>
        /// Event signaled when the state of Drag has changed
        /// </summary>
        public static event Action<DragStateData> dragStateChanged = delegate {};

        void AwakeActions()
        {
            dispatcher = new Dispatcher();
            Dispatcher.RegisterDefaultDispatcher(dispatcher);
            // set Project.Empty for the NonSerialized value
            m_UIStateData.selectedProjectOption = Project.Empty;
            m_UIProjectStateData.activeProject = Project.Empty;
            m_UIProjectStateData.activeProjectThumbnail = null;
            m_RoomConnectionStateData.localUser = NetworkUserData.defaultData;
            m_ApplicationStateData.qualityStateData = QualityState.defaultData;

            DispatchToken = Subscribe();
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
        protected string Subscribe()
        {
            return dispatcher.Register<Payload<ActionTypes>>(InvokeOnDispatch);
        }

        protected void Unsubscribe(string dispatchToken)
        {
            dispatcher.Unregister(dispatchToken);
        }

        protected void WaitFor(IEnumerable<string> dispatchTokens)
        {
            dispatcher.WaitFor<ActionTypes>(dispatchTokens);
        }

        IEnumerator TakeDelayedThumbnail()
        {
            yield return new WaitForSeconds(1.0f);
            m_UIProjectStateData.activeProjectThumbnail = m_ThumbnailController.CaptureActiveProjectThumbnail(current.projectStateData);
            projectStateChanged?.Invoke(m_UIProjectStateData);
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
                    if(m_WalkStateData.walkEnabled)
                        m_WalkStateData.instruction.Cancel();

                    m_UISessionStateData.sessionState.loggedState = LoginState.LoggingOut;
#if UNITY_EDITOR
                    m_LoginManager.userLoggedOut?.Invoke();
#else
                    m_LoginManager.Logout();
#endif
                    sessionStateChanged?.Invoke(sessionStateData);
                    break;
                }
                case ActionTypes.OpenURL:
                {
                    Application.OpenURL((string)payload.Data);
                    break;
                }
                case ActionTypes.SetToolState:
                {
                    var toolState = (ToolState)payload.Data;
                    m_UIStateData.toolState = toolState;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.OpenDialog:
                {
                    var dialogType = (DialogType)payload.Data;
                    m_UIStateData.activeDialog = dialogType;
                    m_UIStateData.activeSubDialog = DialogType.None;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.OpenSubDialog:
                {
                    var dialogType = (DialogType)payload.Data;
                    m_UIStateData.activeSubDialog = dialogType;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetProgressIndicator:
                {
                    var progressIndicatorData = (ProgressData)payload.Data;
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
                    var optionDialogType = (OptionDialogType)payload.Data;
                    m_UIStateData.activeOptionDialog = optionDialogType;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetProjectSortMethod:
                {
                    var sortData = (ProjectListSortData)payload.Data;
                    m_UIProjectStateData.projectSortData = sortData;
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }
                case ActionTypes.SetOptionProject:
                {
                    var project = (Project)payload.Data;
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
                case ActionTypes.ResetToolbars:
                {
                    ResetToolBars();
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
                case ActionTypes.SetStatusMessage:
                {
                    var message = (string)payload.Data;
                    m_MessageManager.SetStatusMessage(message);
                    break;
                }
                case ActionTypes.SetStatusWithType:
                {
                    var messageData = (StatusMessageData)payload.Data;
                    m_MessageManager.SetStatusMessage(messageData.text, messageData.type);
                    break;
                }
                case ActionTypes.SetStatusInstructionMode:
                {
                    var mode = (bool)payload.Data;
                    m_MessageManager.SetInstructionMode(mode);
                    break;
                }
                case ActionTypes.ClearStatus:
                {
                    m_MessageManager.ClearAllMessage();
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
                    var project = (Project)payload.Data;
                    ReflectPipelineFactory.DownloadProject(project);

                    m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingIndeterminate;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.RemoveProject:
                {
                    var project = (Project)payload.Data;
                    ReflectPipelineFactory.DeleteProjectLocally(project);

                    m_UIStateData.progressData.progressState = ProgressData.ProgressState.PendingIndeterminate;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetVisibleFilter:
                {
                    var filterItemInfo = (FilterItemInfo)payload.Data;
                    m_MetadataFilter?.processor.SetVisibility(filterItemInfo.groupKey, filterItemInfo.filterKey, filterItemInfo.visible);

                    if (m_UseExperimentalActorSystem)
                    {
                        m_Bridge.SetHighlightVisibility(filterItemInfo.groupKey, filterItemInfo.filterKey, filterItemInfo.visible);
                    }

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
                    var highlightFilterInfo = (HighlightFilterInfo)payload.Data;

                    if (!m_UseExperimentalActorSystem)
                    {
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
                    }
                    else
                    {
                        GetFilterItemInfos(m_UIStateData.filterGroup, filters =>
                        {
                            var filter = filters.FirstOrDefault(x => x.groupKey == highlightFilterInfo.groupKey && x.filterKey == highlightFilterInfo.filterKey);
                            if (filter.groupKey == string.Empty && filter.filterKey == string.Empty ||
                                filter.highlight)
                            {
                                m_UIProjectStateData.highlightFilter.groupKey = string.Empty;
                                m_UIProjectStateData.highlightFilter.filterKey = string.Empty;
                            }
                            else
                                m_UIProjectStateData.highlightFilter = highlightFilterInfo;

                            m_Bridge.SetHighlightFilter(highlightFilterInfo.groupKey, highlightFilterInfo.filterKey);
                            projectStateChanged?.Invoke(m_UIProjectStateData);
                        });
                    }

                    break;
                }
                case ActionTypes.SetFilterGroup:
                {
                    m_UIStateData.filterGroup = (string)payload.Data;
                    stateChanged?.Invoke(m_UIStateData);

                    GetFilterItemInfos(m_UIStateData.filterGroup, filters =>
                    {
                        m_UIProjectStateData.filterItemInfos = filters;
                        projectStateChanged?.Invoke(m_UIProjectStateData);
                    });
                    break;
                }
                case ActionTypes.SetFilterSearch:
                {
                    var searchString = (string)payload.Data;

                    m_UIProjectStateData.filterSearchString = searchString;
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }
                case ActionTypes.SetViewOption:
                {
                    var sceneOptionData = (SceneOptionData)payload.Data;

                    if (sceneOptionData.enableTexture != m_UIStateData.sceneOptionData.enableTexture)
                    {
                        // set enable texture
                        if (sceneOptionData.enableTexture)
                            Shader.SetGlobalFloat(k_UseTexture, 1);
                        else
                            Shader.SetGlobalFloat(k_UseTexture, 0);
                    }

                    if (!m_UseExperimentalActorSystem)
                    {
                        if (m_LightFilterNode != null && sceneOptionData.enableLightData != m_UIStateData.sceneOptionData.enableLightData)
                        {
                            m_LightFilterNode.settings.enableLights = sceneOptionData.enableLightData;
                            m_LightFilterNode.processor.RefreshLights();
                        }
                    }
                    else if (sceneOptionData.enableLightData != m_UIStateData.sceneOptionData.enableLightData)
                    {
                        var settings = m_Bridge.GetFirstMatchingSettings<LightActor.Settings>();
                        if (settings != null)
                            m_Bridge.UpdateSetting<LightActor.Settings>(settings.Id, nameof(LightActor.Settings.EnableLights), sceneOptionData.enableLightData);
                    }

                    m_UIStateData.sceneOptionData = sceneOptionData;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetSkybox:
                {
                    var sceneOptionData = (SceneOptionData)payload.Data;

                    // set skybox option
                    // sceneOptionData.skyboxData

                    m_UIStateData.sceneOptionData = sceneOptionData;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetClimateOption:
                {
                    var sceneOptionData = (SceneOptionData)payload.Data;

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

                    var sunStatusMessage = SunStudyData.GetSunStatusMessage(m_UIStateData.activeToolbar, m_UIStateData.sunStudyData);
                    if(!string.IsNullOrEmpty(sunStatusMessage))
                        m_MessageManager.SetStatusMessage(sunStatusMessage);

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
                    else if (navigationState.navigationMode != NavigationMode.AR)
                    {
                        // TODO: Move this as part of switching ot Orbit mode.
                        // Reset scale for most mode except AR
                        m_UIStateData.modelScale = ArchitectureScale.OneToOne;

                        m_BoundingBoxRootNode.SetActive(true);
                        if (!m_UseExperimentalActorSystem)
                        {
                            if (m_ReflectPipeline.TryGetNode<BoundingBoxControllerNode>(out var boundingBoxControllerNode))
                            {
                                boundingBoxControllerNode.settings.displayOnlyBoundingBoxes = false;
                            }
                        }
                        else if (m_Bridge.IsInitialized)
                        {
                            var settings = m_Bridge.GetFirstMatchingSettings<BoundingBoxActor.Settings>();
                            m_Bridge.UpdateSetting<BoundingBoxActor.Settings>(settings.Id, nameof(BoundingBoxActor.Settings.DisplayOnlyBoundingBoxes), false);
                        }
                    }

                    m_UIStateData.navigationState = navigationState;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SelectObjects:
                {
                    var selectionInfo = (ObjectSelectionInfo)payload.Data;
                    m_UIProjectStateData.objectSelectionInfo = selectionInfo;
                    SetNetworkSelected(selectionInfo);
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }
                case ActionTypes.SetBimGroup:
                {
                    m_UIStateData.bimGroup = (string)payload.Data;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetBimSearch:
                {
                    var searchString = (string)payload.Data;

                    m_UIProjectStateData.bimSearchString = searchString;
                    projectStateChanged?.Invoke(m_UIProjectStateData);
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
                    m_ARStateData.instructionUIState = (InstructionUIState)payload.Data;
                    arStateChanged?.Invoke(m_ARStateData);

                    if (m_ARStateData.instructionUIState == InstructionUIState.Completed && m_UIStateData.progressData.progressState == ProgressData.ProgressState.NoPendingRequest)
                    {
                        // This has to be delayed because the renderers are
                        // not activated instantly after the instruction is complete
                        StartCoroutine(TakeDelayedThumbnail());
                    }

                    break;
                }
                case ActionTypes.SetInstructionUI:
                {
                    m_ARStateData.currentInstructionUI = (IInstructionUI)payload.Data;
                    arStateChanged?.Invoke(m_ARStateData);
                    break;
                }
                case ActionTypes.ResetHomeView:
                {
                    m_UIProjectStateData.cameraTransformInfo = m_OrbitModeUIController.ResetCamera();
                    m_UIStateData.cameraOptionData.cameraViewType = CameraViewType.Default;

                    stateChanged?.Invoke(m_UIStateData);
                    projectStateChanged?.Invoke(m_UIProjectStateData);
                    break;
                }
                case ActionTypes.Teleport:
                {
                    m_UIProjectStateData.teleportTarget = (Vector3)payload.Data;
                    projectStateChanged?.Invoke(m_UIProjectStateData);

                    if (m_UIStateData.navigationState.navigationMode == NavigationMode.VR)
                    {
                        var teleportationProviders = Resources.FindObjectsOfTypeAll<TeleportationProvider>();
                        if (teleportationProviders.Length > 0)
                        {
                            var teleportationProvider = teleportationProviders[0];
                            if (!ReferenceEquals(teleportationProvider, null) && m_UIProjectStateData.teleportTarget != null)
                            {
                                teleportationProvider.QueueTeleportRequest(new TeleportRequest()
                                {
                                    destinationPosition = m_UIProjectStateData.teleportTarget.Value
                                });
                            }
                        }
                    }

                    break;
                }
                case ActionTypes.FinishTeleport:
                {
                    m_WalkStateData.instructionUIState = InstructionUIState.Started;
                    walkStateChanged?.Invoke(m_WalkStateData);
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
                        if (!m_UseExperimentalActorSystem)
                        {
                            if (m_ReflectPipeline.TryGetNode<BoundingBoxControllerNode>(out var boundingBoxControllerNode))
                            {
                                boundingBoxControllerNode.settings.displayOnlyBoundingBoxes = false;
                            }
                        }
                        else
                        {
                            var settings = m_Bridge.GetFirstMatchingSettings<BoundingBoxActor.Settings>();
                            m_Bridge.UpdateSetting<BoundingBoxActor.Settings>(settings.Id, nameof(BoundingBoxActor.Settings.DisplayOnlyBoundingBoxes), false);
                        }
                    }

                    break;
                }
                case ActionTypes.ShowBoundingBoxModel:
                {
                    var active = (bool)payload.Data;
                    m_BoundingBoxRootNode.SetActive(active);

                    if (!m_UseExperimentalActorSystem)
                    {
                        if (m_ReflectPipeline.TryGetNode<BoundingBoxControllerNode>(out var boundingBoxControllerNode))
                        {
                            boundingBoxControllerNode.settings.displayOnlyBoundingBoxes = active;
                        }
                    }
                    else
                    {
                        var settings = m_Bridge.GetFirstMatchingSettings<BoundingBoxActor.Settings>();
                        m_Bridge.UpdateSetting<BoundingBoxActor.Settings>(settings.Id, nameof(BoundingBoxActor.Settings.DisplayOnlyBoundingBoxes), active);
                    }

                    break;
                }
                case ActionTypes.SetModelScale:
                {
                    var scale = (ArchitectureScale)payload.Data;
                    m_UIStateData.modelScale = scale;

                    // TODO: Use Mars World Scale to scale objects
                    float scalef = (float)m_UIStateData.modelScale;

                    m_PlacementRoot.gameObject.transform.localScale = new Vector3(1.0f / scalef, 1.0f / scalef, 1.0f / scalef);
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetModelRotation:
                {
                    var rotation = (Vector3)payload.Data;
                    m_PlacementRoot.transform.Rotate(rotation);
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
                    var value = (bool)payload.Data;
                    m_UIStateData.operationCancelled = value;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetTheme:
                {
                    var value = (string)payload.Data;
                    m_UIStateData.themeName = value;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetDisplay:
                {
                    var value = (DisplayData)payload.Data;
                    UpdateDisplayData(value);
                    break;
                }
                case ActionTypes.EnableAR:
                {
                    var value = (bool)payload.Data;

                    if (value == false)
                    {
                        // Reset scale and position
                        m_UIStateData.modelScale = ArchitectureScale.OneToOne;
                        m_PlacementRoot.transform.localScale = Vector3.one;
                        m_PlacementRoot.transform.position = Vector3.zero;
                        m_PlacementRoot.transform.rotation = Quaternion.identity;

                        m_RootNode.transform.localPosition = Vector3.zero;
                        m_BoundingBoxRootNode.transform.localPosition = Vector3.zero;

                        stateChanged?.Invoke(m_UIStateData);
                    }

                    m_ARStateData.arEnabled = value;
                    arStateChanged?.Invoke(m_ARStateData);
                    break;
                }
                case ActionTypes.EnableWalk:
                {
                    m_WalkStateData.walkEnabled = (bool)payload.Data;
                    m_WalkStateData.instructionUIState = InstructionUIState.Init;
                    m_WalkStateData.instruction ??= new WalkModeInstruction();
                    walkStateChanged?.Invoke(m_WalkStateData);
                    break;
                }
                case ActionTypes.EnableVR:
                {
                    var value = (bool)payload.Data;
                    m_UIStateData.VREnable = value;
                    stateChanged?.Invoke(m_UIStateData);
                    // disable depth culling in VR due to depth shader incompatibility with SinglePassInstanced
                    // ignore this if the device doesn't support AsyncGPUReadback
                    if (m_UIStateData.deviceCapability.HasFlag(DeviceCapability.SupportsAsyncGPUReadback))
                    {
                        if (!m_UseExperimentalActorSystem)
                        {
                            if (m_ReflectPipeline.TryGetNode<SpatialFilterNode>(out var spatialFilterNode))
                                spatialFilterNode.settings.cullingSettings.useDepthCulling = !value;
                        }
                        else
                        {
                            var settings = m_Bridge.GetFirstMatchingSettings<SpatialActor.Settings>();
                            m_Bridge.UpdateSetting<SpatialActor.Settings>(settings.Id, nameof(SpatialActor.Settings.UseDepthCulling), !value);
                        }
                    }
                    break;
                }
                case ActionTypes.SetARToolState:
                {
                    var value = (ARToolStateData)payload.Data;
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
                case ActionTypes.SetPrivateMode:
                {
                    m_UISessionStateData.sessionState.isInPrivateMode = (bool)payload.Data;
                    if(m_UISessionStateData.sessionState.isInPrivateMode)
                    {
                        PlayerClientBridge.MatchmakerManager.LeaveRoom();
                        // We dont disconnect from the room so the player identity is kept on the matchmaker when we reconnect
                        PlayerClientBridge.MatchmakerManager.Disconnect();
                    }
                    else
                    {
                        PlayerClientBridge.MatchmakerManager.Connect( m_UISessionStateData.sessionState.user.AccessToken, m_MultiplayerController.connectToLocalServer);
                        PlayerClientBridge.MatchmakerManager.MonitorRooms(sessionStateData.sessionState.rooms.Select(r => r.project.serverProjectId));
                        if(m_UIProjectStateData.activeProject != Project.Empty)
                        {
                            PlayerClientBridge.MatchmakerManager.JoinRoom(m_UIProjectStateData.activeProject.serverProjectId);
                        }
                    }
                    break;
                }
                case ActionTypes.FollowUser:
                {
                    NetworkUserData? user = (NetworkUserData?)payload.Data;
                    var shouldFollowThisUser = (user != null && m_UIStateData.toolState.followUserTool.userId != user.Value.matchmakerId);
                    m_UIStateData.toolState.followUserTool.userId = shouldFollowThisUser ? user.Value.matchmakerId: null;
                    m_UIStateData.toolState.followUserTool.userObject = shouldFollowThisUser ? user.Value.visualRepresentation.gameObject : null;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.SetUserInfo:
                {
                    UserInfoDialogData user = (UserInfoDialogData)payload.Data;
                    m_UIStateData.SelectedUserData = user;
                    stateChanged?.Invoke(m_UIStateData);
                    break;
                }
                case ActionTypes.ToggleUserMicrophone:
                {
                    var user = (string)payload.Data;
                    if(m_VivoxManager.IsConnected)
                    {
                        ToggleUserMicrophone(user);
                    }
                    else
                    {
                        m_RoomConnectionStateData.localUser.voiceStateData.isServerMuted = !m_RoomConnectionStateData.localUser.voiceStateData.isServerMuted;
                        roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                    }
                    break;
                }
                case ActionTypes.SetSpatialPriorityWeights:
                {
                    var value = (Vector3)payload.Data;
                    if (!m_UseExperimentalActorSystem)
                    {
                        if (m_ReflectPipeline.TryGetNode<SpatialFilterNode>(out var spatialFilterNode))
                        {
                            spatialFilterNode.settings.priorityWeightAngle = value.x;
                            spatialFilterNode.settings.priorityWeightDistance = value.y;
                            spatialFilterNode.settings.priorityWeightSize = value.z;
                        }
                    }
                    else
                    {
                        var settings = m_Bridge.GetFirstMatchingSettings<SpatialActor.Settings>();
                        m_Bridge.UpdateSetting<SpatialActor.Settings>(settings.Id, nameof(SpatialActor.Settings.PriorityWeightAngle), value.x);
                        m_Bridge.UpdateSetting<SpatialActor.Settings>(settings.Id, nameof(SpatialActor.Settings.PriorityWeightDistance), value.y);
                        m_Bridge.UpdateSetting<SpatialActor.Settings>(settings.Id, nameof(SpatialActor.Settings.PriorityWeightSize), value.z);
                    }
                    break;
                }
                case ActionTypes.SetDebugBoundingBoxMaterials:
                {
                    var value = (bool)payload.Data;
                    if (!m_UseExperimentalActorSystem)
                    {
                        if (m_ReflectPipeline.TryGetNode<BoundingBoxControllerNode>(out var boundingBoxControllerNode))
                            boundingBoxControllerNode.settings.useDebugMaterials = value;
                    }
                    else
                    {
                        var settings = m_Bridge.GetFirstMatchingSettings<BoundingBoxActor.Settings>();
                        m_Bridge.UpdateSetting<BoundingBoxActor.Settings>(settings.Id, nameof(BoundingBoxActor.Settings.UseDebugMaterials), value);
                    }
                    break;
                }
                case ActionTypes.SetCulling:
                {
                    var value = (bool)payload.Data;
                    if (!m_UseExperimentalActorSystem)
                    {
                        if (m_ReflectPipeline.TryGetNode<SpatialFilterNode>(out var spatialFilterNode))
                            spatialFilterNode.settings.useCulling = value;
                    }
                    else
                    {
                        var settings = m_Bridge.GetFirstMatchingSettings<SpatialActor.Settings>();
                        m_Bridge.UpdateSetting<SpatialActor.Settings>(settings.Id, nameof(SpatialActor.Settings.UseCulling), value);
                    }
                    break;
                }
                case ActionTypes.SetStaticBatching:
                {
                    var value = (bool)payload.Data;
                    if (!m_UseExperimentalActorSystem)
                    {
                        if (m_ReflectPipeline.TryGetNode<BoundingBoxControllerNode>(out var boundingBoxControllerNode))
                            boundingBoxControllerNode.settings.useStaticBatching = value;
                    }
                    else
                    {
                        var settings = m_Bridge.GetFirstMatchingSettings<BoundingBoxActor.Settings>();
                        m_Bridge.UpdateSetting<BoundingBoxActor.Settings>(settings.Id, nameof(BoundingBoxActor.Settings.UseStaticBatching), value);
                    }
                    break;
                }
                case ActionTypes.SetMeasureToolOptions:
                {
                    var value = (MeasureToolStateData)payload.Data;
                    m_ExternalToolStateData.measureToolStateData = value;
                    externalToolChanged?.Invoke(m_ExternalToolStateData);
                    break;
                }
                case ActionTypes.SetLoginSetting:
                {
                    var value = (EnvironmentInfo)payload.Data;
                    LocaleUtils.SaveEnvironmentInfo(value);
                    UnityEngine.Reflect.ProjectServer.Cleanup();
                    UnityEngine.Reflect.ProjectServer.Init();
                    loginSettingChanged?.Invoke();
                    break;
                }
                case ActionTypes.DeleteCloudEnvironmentSetting:
                {
                    LocaleUtils.DeleteCloudEnvironmentSetting();
                    UnityEngine.Reflect.ProjectServer.Cleanup();
                    UnityEngine.Reflect.ProjectServer.Init();
                    loginSettingChanged?.Invoke();
                    break;
                }
                case ActionTypes.BeginDrag:
                {
                    var value = (DragStateData)payload.Data;
                    m_DragStateData = value;
                    dragStateChanged?.Invoke(m_DragStateData);
                    break;
                }
                case ActionTypes.OnDrag:
                {
                    var value = (DragStateData)payload.Data;
                    m_DragStateData = value;
                    dragStateChanged?.Invoke(m_DragStateData);
                    break;
                }
                case ActionTypes.EndDrag:
                {
                    var value = (DragStateData)payload.Data;
                    m_DragStateData = value;
                    dragStateChanged?.Invoke(m_DragStateData);
                    break;
                }
                case ActionTypes.ResetExternalTools:
                {
                    ResetExternalTools();
                    break;
                }
                case ActionTypes.SetLinkSharePermission:
                {
                    var value = (LinkPermission)payload.Data;
                    m_UISessionStateData.sessionState.linkSharePermission = value;
                    sessionStateChanged?.Invoke(sessionStateData);
                    break;
                }
            }
        }
    }
}
