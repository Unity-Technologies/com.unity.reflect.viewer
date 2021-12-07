using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Dispatching;
using Unity.Reflect.Actors;
using Unity.Reflect.Geometry;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.Reflect.Viewer.Input;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.Reflect.Viewer.UI
{
    public class OrbitModeUIController: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ViewerReflectBootstrapper m_Reflect;
        [SerializeField]
        FreeFlyCamera m_Camera;
        [SerializeField]
        UITeleportController m_TeleportController;
        [SerializeField]
        InputActionAsset m_InputActionAsset;
        [SerializeField]
        UINavigationControllerSettings m_UINavigationControllerSettings;
        [SerializeField]
        ToolButton m_ResetButton;
        [SerializeField]
        Button m_FocusButton;
        [SerializeField]
        GameObject m_GizmoCube;
        [SerializeField]
        ToolButton m_DebugButton;
        [SerializeField]
        float m_ZoomGestureSensibility = 30.0f;

        [SerializeField]
        float m_ResetDelaySeconds = 0.4f;

        [SerializeField]
        float m_TeleportAllowDistance = 100.0f;
#pragma warning restore CS0649

        bool m_ArEnabled;
        static readonly float k_ToolDebounceTime = 0.2f;
        Aabb m_InitialCameraBounds;
        Aabb m_InitialZoneBounds;
        bool m_HasBeenPositioned;
        List<SceneZone> m_Zones = new List<SceneZone>();
        List<GameObject> m_ZoneGameObjects = new List<GameObject>();
        OpenDialogAction.DialogType m_CurrentActiveDialog;
        Vector3 m_LastMovingAction;
        DeltaCalculator m_ZoomDelta;
        DeltaCalculator m_PanDelta;
        DeltaCalculator m_WorldOrbitDelta;
        InputAction m_MovingAction;
        uint m_InputSkipper = 0;
        bool? m_GizmoEnabled;
        bool m_ZoomGestureInProgress;
        bool m_PanGestureInProgress;
        Color m_DefaultColor;
        Coroutine m_ZoomGestureCoroutine;
        Coroutine m_PanGestureCoroutine;
        float m_GestureCameraStartPosition;
        Vector2 m_LastTapPoint;
        Coroutine m_ResetCoroutine;
        WaitForSeconds m_DelayWaitForSeconds;
        IUISelector<bool> m_GestureTrackingEnabledSelector;
        IUISelector<bool> m_VREnableSelector;
        IUISelector<SelectObjectAction.IObjectSelectionInfo> m_ObjectSelectionInfoSelector;
        SetOrbitTypeAction.OrbitType m_TouchOrbitType;
        SetVREnableAction.DeviceCapability m_DeviceCapability;
        IUISelector<SetOrbitTypeAction.OrbitType> m_OrbitTypeSelector;
        IUISelector<SetActiveToolAction.ToolType> m_ActiveToolSelector;
        bool m_MoveEnabled;
        SetNavigationModeAction.NavigationMode m_CachedNavigationMode;
        IUISelector<ICameraViewOption> m_CameraViewSelector;
        IUISelector<bool> m_TeleportEnabledSelector;
        IUISelector<SetInfoTypeAction.InfoType> m_InfoTypeSelector;
        IUISelector<string> m_FollowUserIDSelector;
        IUISelector m_TouchOrbitTypeSelector;
        IUISelector m_DeviceCapabilitySelector;
        IUISelector<SetNavigationModeAction.NavigationMode> m_NavigationModeSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnApplicationFocus(bool hasFocus)
        {
            m_MovingAction?.Reset();
        }

        void OnDestroy()
        {
            m_TouchOrbitTypeSelector?.Dispose();
            m_DeviceCapabilitySelector?.Dispose();
            m_NavigationModeSelector?.Dispose();
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        public void Awake()
        {
            m_Reflect.ActorSystemStarting += (bridge, isRestarting) =>
            {
                if (!isRestarting)
                {
                    m_InitialCameraBounds = default;
                    m_HasBeenPositioned = false;
                }
                m_InitialZoneBounds = default;
                m_Zones.Clear();
                m_ZoneGameObjects.ForEach(x => Destroy(x));
                m_ZoneGameObjects.Clear();

                bridge.Subscribe<SceneZonesChanged>(ctx =>
                {
                    if (m_HasBeenPositioned)
                        return;

                    m_Zones = ctx.Data.Zones;
                    m_InitialZoneBounds = m_Zones[0].Bounds;

                    m_HasBeenPositioned = m_Zones.All(x => x.IsReliable);

                    OnResetButtonClicked();
                });
            };

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<Bounds>(ProjectContext.current, nameof(IProjectBound.rootBounds), bounds =>
            {
                var newBound = bounds.ToReflect();
                var isFirstUpdate = m_InitialCameraBounds == default && newBound != default;

                if (newBound != default)
                    m_InitialCameraBounds = newBound;

                if (isFirstUpdate)
                {
                    // Avoid dispatching into a dispatch
                    IEnumerator WaitAFrame()
                    {
                        yield return null;
                        OnResetButtonClicked(); // use dispatch to ensure XR cameras also properly change
                    }
                    StartCoroutine(WaitAFrame());
                }
            }));
            m_DisposeOnDestroy.Add(m_ObjectSelectionInfoSelector = UISelectorFactory.createSelector<SelectObjectAction.IObjectSelectionInfo>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectSelectionInfo),
                info => m_FocusButton.interactable = (info != null) ? ((ObjectSelectionInfo)info).CurrentSelectedObject() != null : false));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled),
                enabled => m_ResetButton.button.interactable = enabled));
            m_DisposeOnDestroy.Add(m_ActiveToolSelector = UISelectorFactory.createSelector<SetActiveToolAction.ToolType>(ToolStateContext.current, nameof(IToolStateDataProvider.activeTool)));
            m_DisposeOnDestroy.Add(m_VREnableSelector = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable)));
            m_DisposeOnDestroy.Add(m_GestureTrackingEnabledSelector = UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.gesturesTrackingEnabled)));
            m_DisposeOnDestroy.Add(m_OrbitTypeSelector = UISelectorFactory.createSelector<SetOrbitTypeAction.OrbitType>(ToolStateContext.current, nameof(IToolStateDataProvider.orbitType)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.gizmoEnabled), OnGizmoEnabledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.moveEnabled), OnMoveEnabledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.freeFlyCameraEnabled), OnFreeFlyCameraEnabledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.orbitEnabled), OnOrbitEnabledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.panEnabled), OnPanEnabledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.zoomEnabled), OnZoomEnabledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.worldOrbitEnabled), OnWorldOrbitEnabledChanged));
            m_DisposeOnDestroy.Add(m_TeleportEnabledSelector = UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.teleportEnabled), OnTeleportEnabledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode), OnNavigationModeChanged));
            m_DisposeOnDestroy.Add(m_CameraViewSelector = UISelectorFactory.createSelector<ICameraViewOption>(CameraOptionsContext.current, nameof(ICameraOptionsDataProvider.cameraViewOption), OnCameraViewTypeChanged));
            m_DisposeOnDestroy.Add(m_InfoTypeSelector = UISelectorFactory.createSelector<SetInfoTypeAction.InfoType>(ToolStateContext.current, nameof(IToolStateDataProvider.infoType)));
            m_DisposeOnDestroy.Add(m_FollowUserIDSelector = UISelectorFactory.createSelector<string>(FollowUserContext.current, nameof(IFollowUserDataProvider.userId)));

            m_InputActionAsset["Orbit Action"].performed += OnOrbit;
            m_InputActionAsset["Quick WorldOrbit Action"].performed += OnQuickWorldOrbit;
            m_InputActionAsset["Zoom Gesture Action"].started += OnZoomGestureStarted;
            m_InputActionAsset["Zoom Gesture Action"].performed += OnZoomGesture;
            m_InputActionAsset["Quick Zoom Action"].performed += OnQuickZoom;
            m_InputActionAsset["Pan Gesture Action"].started += OnPanGestureStarted;
            m_InputActionAsset["Pan Gesture Action"].performed += OnPanGesture;
            m_InputActionAsset["Quick Pan Action"].performed += OnQuickPan;
            m_InputActionAsset["Quick Pan Start"].performed += OnQuickPanStart;
            m_InputActionAsset["Teleport Action"].performed += OnTeleport;
            m_InputActionAsset["TouchTeleport Action"].performed += OnTeleport;
            m_InputActionAsset["Focus Action"].performed += OnFocusButtonPressed;
            m_MovingAction = m_InputActionAsset["Moving Action"];

            m_InputActionAsset["Click"].performed += OnClick;

            m_ResetButton.buttonClicked += OnResetButtonClicked;
            m_FocusButton.onClick.AddListener(OnFocusButtonClicked);

            m_DefaultColor = m_DebugButton.buttonRound.color;
            m_DelayWaitForSeconds = new WaitForSeconds(m_ResetDelaySeconds);
        }

        void OnCameraViewTypeChanged(ICameraViewOption data)
        {
            OnInteraction();
            switch (data?.cameraViewType)
            {
                case SetCameraViewTypeAction.CameraViewType.Top:
                    OnTopView();
                    break;
                case SetCameraViewTypeAction.CameraViewType.Left:
                    OnLeftView();
                    break;
                case SetCameraViewTypeAction.CameraViewType.Right:
                    OnRightView();
                    break;
            }
        }

        void OnGizmoEnabledChanged(bool data)
        {
            m_GizmoCube.SetActive(data);

            /*
            TODO: Temporary fix till next MARS Update. This code will make sure that the cube stay hidden in AR mode.
            */
            GizmoController gizmoController = GetComponent<GizmoController>();
            if (!data)
            {
                gizmoController.HideGizmo();
            }
            else
            {
                gizmoController.ShowGizmo();
            }
        }

        void OnNavigationModeChanged(SetNavigationModeAction.NavigationMode navMode)
        {
            if (navMode == SetNavigationModeAction.NavigationMode.Orbit && m_CachedNavigationMode != SetNavigationModeAction.NavigationMode.Walk)
            {
                StartCoroutine(WaitAndRun(ResetHomeView));
            }

            StartCoroutine(WaitAndRun(SetARFollowVisibility));

            m_CachedNavigationMode = navMode;
        }

        void OnTeleportEnabledChanged(bool data)
        {
            if (data)
            {
                m_InputActionAsset["Teleport Action"].Enable();
                m_InputActionAsset["TouchTeleport Action"].Enable();
            }
            else
            {
                m_InputActionAsset["Teleport Action"].Disable();
                m_InputActionAsset["TouchTeleport Action"].Disable();
            }
        }

        void OnWorldOrbitEnabledChanged(bool data)
        {
            if (data)
            {
                m_InputActionAsset["Quick WorldOrbit Action"].Enable();
            }
            else
            {
                m_InputActionAsset["Quick WorldOrbit Action"].Disable();
            }
        }

        void OnZoomEnabledChanged(bool data)
        {
            if (data)
            {
                m_InputActionAsset["Zoom Gesture Action"].Enable();
                m_InputActionAsset["Zoom Gesture Action"].Enable();
                m_InputActionAsset["Quick Zoom Action"].Enable();
            }
            else
            {
                m_InputActionAsset["Zoom Gesture Action"].Disable();
                m_InputActionAsset["Zoom Gesture Action"].Disable();
                m_InputActionAsset["Quick Zoom Action"].Disable();
            }

        }

        void OnPanEnabledChanged(bool data)
        {
            if (data)
            {
                m_InputActionAsset["Pan Gesture Action"].Enable();
                m_InputActionAsset["Pan Gesture Action"].Enable();
                m_InputActionAsset["Quick Pan Action"].Enable();
                m_InputActionAsset["Quick Pan Start"].Enable();
            }
            else
            {
                m_InputActionAsset["Pan Gesture Action"].Disable();
                m_InputActionAsset["Pan Gesture Action"].Disable();
                m_InputActionAsset["Quick Pan Action"].Disable();
                m_InputActionAsset["Quick Pan Start"].Disable();
            }
        }

        void OnOrbitEnabledChanged(bool data)
        {
            if (data)
            {
                m_InputActionAsset["Orbit Action"].Enable();
            }
            else
            {
                m_InputActionAsset["Orbit Action"].Disable();
            }
        }

        void OnFreeFlyCameraEnabledChanged(bool data)
        {
            m_Camera.enabled = data;
            m_FocusButton.enabled = data;
        }

        void OnMoveEnabledChanged(bool data)
        {
            m_TouchOrbitTypeSelector?.Dispose();
            m_DeviceCapabilitySelector?.Dispose();
            m_MoveEnabled = data;

            if (m_MovingAction != null)
            {
                if (data)
                {
                    m_MovingAction.Enable();
                }
                else
                {
                    m_MovingAction.Disable();
                    m_Camera.ForceStop();
                    m_LastMovingAction = Vector3.zero;
                    m_Camera.MoveInLocalDirection(Vector3.zero, LookAtConstraint.Follow);
                }
                m_TouchOrbitTypeSelector = UISelectorFactory.createSelector<SetOrbitTypeAction.OrbitType>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.touchOrbitType), OnTouchOrbitTypeChanged);
                m_DeviceCapabilitySelector = UISelectorFactory.createSelector<SetVREnableAction.DeviceCapability>(PipelineContext.current, nameof(IPipelineDataProvider.deviceCapability), OnDeviceCapabilityChanged);
            }
            m_NavigationModeSelector = UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode));
        }

        void OnFocusButtonClicked()
        {
            var selectedObj = ((ObjectSelectionInfo)m_ObjectSelectionInfoSelector.GetValue()).CurrentSelectedObject();
            if (selectedObj != null)
            {
                Vector3 focusPoint = selectedObj.transform.position;
                var childBounds = selectedObj.CalculateBoundsInChildren();
                if (childBounds.HasValue)
                    focusPoint = childBounds.Value.center;

                if (!m_VREnableSelector.GetValue())
                {
                    m_Camera.FocusOnPoint(focusPoint);
                }
                else
                {
                    var camTransform = Camera.main.transform;
                    var xrRig = camTransform.GetComponentInParent<XRRig>();

                    SetFocusCameraPosition(xrRig, focusPoint);
                    var direction = (focusPoint - camTransform.position).normalized;
                    xrRig.MatchRigUpRigForward(Vector3.up, direction);
                }
            }
        }

        void SetFocusCameraPosition(XRRig xrRig, Vector3 focusPoint)
        {
            var cameraPlane = new Plane(xrRig.transform.forward, xrRig.transform.position);
            var targetCameraPos = cameraPlane.ClosestPointOnPlane(focusPoint);
            xrRig.MoveCameraToWorldLocation(targetCameraPos);
        }

        void OnFocusButtonPressed(InputAction.CallbackContext context)
        {
            OnFocusButtonClicked();
        }

        void OnZoomGestureStarted(InputAction.CallbackContext context)
        {
            if (m_PanGestureInProgress || m_ZoomGestureInProgress)
                return;

            PinchGestureInteraction interaction = context.interaction as PinchGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                PinchGesture pinchGesture = interaction?.currentGesture as PinchGesture;
                m_ZoomGestureInProgress = true;
                m_GestureCameraStartPosition = m_Camera.GetDistanceFromLookAt();
                pinchGesture.onFinished += OnZoomGestureFinished;
            }
        }

        void OnZoomGestureFinished(PinchGesture pinchGesture)
        {
            m_ZoomGestureInProgress = false;
        }

        void OnPanGestureStarted(InputAction.CallbackContext context)
        {
            if (m_ZoomGestureInProgress || m_PanGestureInProgress)
                return;

            TwoFingerDragGestureInteraction interaction = context.interaction as TwoFingerDragGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                TwoFingerDragGesture dragGesture = interaction?.currentGesture as TwoFingerDragGesture;
                m_PanGestureInProgress = true;
                dragGesture.onFinished += OnPanGestureFinished;

                Vector2 pos = Pointer.current.position.ReadValue();
                m_Camera.PanStart(pos);
            }
        }

        void OnPanGestureFinished(TwoFingerDragGesture dragGesture)
        {
            m_PanGestureInProgress = false;
        }

        void Update()
        {
            m_ZoomDelta.Reset();
            m_PanDelta.Reset();
            m_WorldOrbitDelta.Reset();
            m_InputSkipper++;

            if (Time.unscaledDeltaTime <= m_UINavigationControllerSettings.inputLagCutoffThreshold)
            {
                if (Time.unscaledDeltaTime <= m_UINavigationControllerSettings.inputLagSkipThreshold)
                {
                    if (!m_MovingAction.enabled)
                    {
                        m_MovingAction.Enable();
                    }
                }
                else
                {
                    if (m_MovingAction.enabled)
                    {
                        m_MovingAction.Disable();
                    }
                }

                if (!m_MoveEnabled)
                {
                    return;
                }

                var val = m_MovingAction.ReadValue<Vector3>();
                if (val != m_LastMovingAction)
                {
                    m_LastMovingAction = val;
                    m_Camera.MoveInLocalDirection(val, LookAtConstraint.Follow);
                }
            }
            else
            {
                m_MovingAction.Disable();
                m_Camera.ForceStop();
                m_LastMovingAction = Vector3.zero;
                m_Camera.MoveInLocalDirection(Vector3.zero, LookAtConstraint.Follow);
            }

            if (m_InfoTypeSelector.GetValue() == SetInfoTypeAction.InfoType.Debug &&
                m_GestureTrackingEnabledSelector.GetValue())
            {
                if (m_ZoomGestureInProgress)
                {
                    m_DebugButton.buttonRound.color = Color.red;
                    m_DebugButton.selected = true;
                }
                else
                {
                    if (m_PanGestureInProgress)
                    {
                        m_DebugButton.buttonRound.color = Color.yellow;
                        m_DebugButton.selected = true;
                    }
                    else
                    {
                        m_DebugButton.selected = false;
                        m_DebugButton.buttonRound.color = m_DefaultColor;
                    }
                }
            }
        }

        void LeaveFollowUserMode()
        {
            if (!string.IsNullOrEmpty(m_FollowUserIDSelector?.GetValue()))
            {
                var followUserData = new FollowUserAction.FollowUserData();
                followUserData.matchmakerId = "";
                followUserData.visualRepresentationGameObject = null;
                Dispatcher.Dispatch(FollowUserAction.From(followUserData));
            }
        }

        IEnumerator WaitAndRun(Action action)
        {
            yield return null;
            action.Invoke();
        }

        void SetARFollowVisibility()
        {
            var isARMode = m_NavigationModeSelector.GetValue() == SetNavigationModeAction.NavigationMode.AR;
            if (isARMode)
            {
                LeaveFollowUserMode();
            }
            Dispatcher.Dispatch(SetAppBarButtonVisibilityAction.From(
                new ButtonVisibility { type = (int)ButtonType.Follow, visible = !isARMode }
            ));
        }

        void ResetHomeView()
        {
            Dispatcher.Dispatch(SetCameraViewTypeAction.From(SetCameraViewTypeAction.CameraViewType.Default));
            Dispatcher.Dispatch(SetCameraTransformInfoAction.From(ResetCamera()));
            m_Camera.camera.clearFlags = CameraClearFlags.Skybox;
        }

        bool IsTemporaryTool(SetActiveToolAction.ToolType activeTool)
        {
            switch (activeTool)
            {
                case SetActiveToolAction.ToolType.ClippingTool:
                case SetActiveToolAction.ToolType.SelectTool:
                    return false;
            }

            return true;
        }

        bool CheckTreatInput(double deltaTime)
        {
            return
                deltaTime <= m_UINavigationControllerSettings.inputLagSkipThreshold ||
                ((deltaTime <= m_UINavigationControllerSettings.inputLagCutoffThreshold) && (m_InputSkipper % m_UINavigationControllerSettings.inputLagSkipAmount == 0));
        }

        bool CheckTreatInput(InputAction.CallbackContext context)
        {
            double deltaTime = Time.realtimeSinceStartup - context.time;
            return CheckTreatInput(deltaTime);
        }


        void OnInteraction()
        {
            LeaveFollowUserMode();
        }

        void Zoom(Vector2 delta)
        {
            m_Camera.MoveOnLookAtAxis(delta.y);
            OnInteraction();
        }

        void Pan(Vector2 delta)
        {
            m_Camera.Pan(delta);
            OnInteraction();
        }

        void Orbit(Vector2 delta, SetOrbitTypeAction.OrbitType orbitType)
        {
            switch (orbitType)
            {
                case SetOrbitTypeAction.OrbitType.WorldOrbit:
                    m_Camera.Rotate(new Vector2(delta.y, delta.x));
                    break;
                case SetOrbitTypeAction.OrbitType.OrbitAtPoint:
                    m_Camera.OrbitAroundLookAt(new Vector2(delta.y, delta.x));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (m_CameraViewSelector.GetValue() != null && m_CameraViewSelector.GetValue().cameraViewType != SetCameraViewTypeAction.CameraViewType.Default)
            {
                Dispatcher.Dispatch(SetCameraViewTypeAction.From(SetCameraViewTypeAction.CameraViewType.Default));
            }

            OnInteraction();
        }

        void OnOrbit(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isTouchBlockedByUI ||
                m_ZoomGestureInProgress || m_PanGestureInProgress || !CheckTreatInput(context))
                return;

            var readValue = context.ReadValue<Vector2>();

            m_WorldOrbitDelta.SetNewFrameDelta(readValue);
            var delta = m_WorldOrbitDelta.delta;
            var worldVector = new Vector2(delta.x, -delta.y);

            if (m_DeviceCapability.HasFlag(SetVREnableAction.DeviceCapability.ARCapability))
                Orbit(worldVector, m_TouchOrbitType);
            else
                Orbit(worldVector, SetOrbitTypeAction.OrbitType.OrbitAtPoint);

        }


        void OnPanGesture(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isTouchBlockedByUI || m_ZoomGestureInProgress || !CheckTreatInput(context))
                return;

            var interaction = context.interaction as TwoFingerDragGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                var dragGesture = interaction?.currentGesture as TwoFingerDragGesture;
                if (m_PanGestureCoroutine != null)
                    StopCoroutine(m_PanGestureCoroutine);
                m_PanGestureCoroutine = StartCoroutine(StopPanGesture());
                m_PanDelta.SetNewFrameDelta(dragGesture.Delta);
                var delta = dragGesture.Delta * -Vector2.one;
                Pan(delta);
            }
        }

        void OnZoomGesture(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isTouchBlockedByUI || m_PanGestureInProgress || !CheckTreatInput(context))
                return;

            var interaction = context.interaction as PinchGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                var pinchGesture = interaction?.currentGesture as PinchGesture;
                if (m_ZoomGestureCoroutine != null)
                    StopCoroutine(m_ZoomGestureCoroutine);
                m_ZoomGestureCoroutine = StartCoroutine(StopZoomGesture());

                var distance = m_GestureCameraStartPosition - (m_GestureCameraStartPosition) * ((pinchGesture.gap - pinchGesture.startGap) / Screen.height);
                if (distance > m_GestureCameraStartPosition)
                {
                    // Double zoom out ratio
                    distance += distance - m_GestureCameraStartPosition;
                }

                m_Camera.SetDistanceFromLookAt(distance);
            }
        }

        IEnumerator StopZoomGesture()
        {
            yield return new WaitForSeconds(0.2f);
            OnZoomGestureFinished(null);
        }

        IEnumerator StopPanGesture()
        {
            yield return new WaitForSeconds(0.2f);
            OnPanGestureFinished(null);
        }

        void OnQuickZoom(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isPointBlockedByUI)
                return;

            if (CheckTreatInput(context))
            {
                m_ZoomDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
                Zoom(m_ZoomDelta.delta);
            }
        }

        void OnQuickPan(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isTouchBlockedByUI)
                return;

            if (CheckTreatInput(context))
            {
                m_PanDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
                var delta = m_PanDelta.delta * -Vector2.one;

                Pan(delta);
            }
        }

        void OnQuickPanStart(InputAction.CallbackContext context)
        {
            if (CheckTreatInput(context) && context.control.IsPressed())
            {
                Vector2 pos = Pointer.current.position.ReadValue();
                m_Camera.PanStart(pos);
            }
        }

        void OnQuickWorldOrbit(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isTouchBlockedByUI || !CheckTreatInput(context))
                return;

            m_WorldOrbitDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
            var delta = m_WorldOrbitDelta.delta;
            var worldVector = new Vector2(delta.x, -delta.y);

            Orbit(worldVector, SetOrbitTypeAction.OrbitType.WorldOrbit);
        }

        void OnTeleport(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isTouchBlockedByUI || m_TeleportController == null || !CheckTreatInput(context))
                return;

            Vector2 current = Pointer.current.position.ReadValue();
            var distance = Vector2.Distance(m_LastTapPoint, current);

            if (distance > m_TeleportAllowDistance)
            {
                return;
            }

            m_TeleportController.TriggerTeleport(current);
        }

        public void TriggerTeleport(Vector2 pointerPos)
        {
            m_TeleportController.TriggerTeleport(pointerPos);
        }

        public void AsyncGetTeleportTarget(Vector2 pointerPos, Action<Vector3> callback)
        {
            m_TeleportController.AsyncGetTeleportTarget(pointerPos, callback);
        }

        void OnResetButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.HomeReset))
                return;

            OnInteraction();
            Dispatcher.Dispatch(SetCameraViewTypeAction.From(SetCameraViewTypeAction.CameraViewType.Default));
            Dispatcher.Dispatch(SetCameraTransformInfoAction.From(ResetCamera()));
        }

        public CameraTransformInfo ResetCamera()
        {
            var bounds = m_InitialZoneBounds != default ? m_InitialZoneBounds : m_InitialCameraBounds;
            m_Camera.camera.clearFlags = CameraClearFlags.Skybox;
            m_Camera.SetupInitialCameraPosition(bounds.ToUnity(), 20.0f, 0.90f);
            var camTransform = m_Camera.camera.transform;
            return new CameraTransformInfo { position = camTransform.position, rotation = camTransform.eulerAngles };
        }

        void OnRightView()
        {
            m_Camera.ContinuousOrbitAroundLookAt(new Vector2(0, 90), false);
        }

        void OnLeftView()
        {
            m_Camera.ContinuousOrbitAroundLookAt(new Vector2(0, -90), false);
        }

        void OnTopView()
        {
            m_Camera.ContinuousOrbitAroundLookAt(new Vector2(90, 0), true);
        }

        void OnClick(InputAction.CallbackContext context)
        {
            if (m_TeleportEnabledSelector.GetValue() == false)
                return;

            Vector2 current = Pointer.current.position.ReadValue();
            if (m_LastTapPoint == Vector2.zero)
            {
                m_LastTapPoint = current;
                if (m_ResetCoroutine != null)
                {
                    StopCoroutine(m_ResetCoroutine);
                    m_ResetCoroutine = null;
                }

                m_ResetCoroutine = StartCoroutine(ResetLastTapPoint());
            }
            else
            {
                var distance = Vector2.Distance(m_LastTapPoint, current);
                if (distance > m_TeleportAllowDistance)
                {
                    m_InputActionAsset["TouchTeleport Action"].Reset();
                    m_InputActionAsset["Teleport Action"].Reset();
                    if (m_ResetCoroutine != null)
                    {
                        StopCoroutine(m_ResetCoroutine);
                        m_ResetCoroutine = null;
                    }

                    m_LastTapPoint = Vector2.zero;
                }
            }
        }

        IEnumerator ResetLastTapPoint()
        {
            yield return m_DelayWaitForSeconds;
            m_LastTapPoint = Vector2.zero;
        }


        void OnTouchOrbitTypeChanged(SetOrbitTypeAction.OrbitType orbitType)
        {
            m_TouchOrbitType = orbitType;
        }

        void OnDeviceCapabilityChanged(SetVREnableAction.DeviceCapability deviceCapability)
        {
            m_DeviceCapability = deviceCapability;
        }
    }
}
