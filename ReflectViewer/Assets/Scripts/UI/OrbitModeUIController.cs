using System;
using System.Collections;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Input;

namespace Unity.Reflect.Viewer.UI
{
    public class OrbitModeUIController : MonoBehaviour
    {
#pragma warning disable CS0649
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
        Button m_OrbitButton;
        [SerializeField]
        Button m_PanButton;
        [SerializeField]
        Button m_ZoomButton;
        [SerializeField]
        ToolButton m_DebugButton;
        [SerializeField]
        float m_ZoomGestureSensibility = 30.0f;

        [SerializeField]
        GameObject m_PointMesh;
#pragma warning restore CS0649

        ToolType? m_CachedToolType;
        ToolType? m_PreviousToolType;
        ToolType? m_PreviousOrbitToolType = ToolType.OrbitTool;
        OrbitType? m_PreviousOrbitType;
        NavigationMode? m_CachedNavigationMode;
        NavigationState? m_CachedNavigationState;
        CameraOptionData? m_CachedCameraOptionData;
        DebugOptionsData? m_CachedDebugOptions;
        InfoType? m_CachedInfoType;
        bool m_ArEnabled;

        static readonly float k_ToolDebounceTime = 0.2f;

        bool m_ZoomPressed;
        bool m_PanPressed;
        bool isTeleportInit = false;

        Bounds m_InitialCameraBounds;

        DialogType m_CurrentActiveDialog;

        Vector3 m_LastMovingAction;

        DeltaCalculator m_ZoomDelta;
        DeltaCalculator m_PanDelta;
        DeltaCalculator m_WorldOrbitDelta;

        InputAction m_MovingAction;

        uint m_InputSkipper = 0;

        bool m_ToolbarsEnabled;
        bool? m_GizmoEnabled;

        bool m_ZoomGestureInProgress;
        bool m_PanGestureInProgress;
        Color m_DefaultColor;
        Coroutine m_ZoomGestureCoroutine;
        Coroutine m_PanGestureCoroutine;

        float m_GestureCameraStartPosition;

        public void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;
            UIStateManager.debugStateChanged += OnDebugStateDataChanged;

            m_InputActionAsset["Pan Mode Action"].performed += OnPanMode;
            m_InputActionAsset["Zoom Mode Action"].performed += OnZoomMode;
            m_InputActionAsset["Zoom Gesture Action"].started += OnZoomGestureStarted;
            m_InputActionAsset["Zoom Gesture Action"].performed += OnZoomGesture;
            m_InputActionAsset["Zoom Action"].performed += OnZoom;
            m_InputActionAsset["Quick Zoom Action"].performed += OnQuickZoom;
            m_InputActionAsset["Pan Gesture Action"].started += OnPanGestureStarted;
            m_InputActionAsset["Pan Gesture Action"].performed += OnPanGesture;
            m_InputActionAsset["Pan Action"].performed += OnPan;
            m_InputActionAsset["Orbit Action"].performed += OnOrbit;
            m_InputActionAsset["Quick Pan Action"].performed += OnQuickPan;
            m_InputActionAsset["Quick WorldOrbit Action"].performed += OnQuickWorldOrbit;
            m_InputActionAsset["Teleport Action"].performed += OnTeleport;
            m_InputActionAsset["TouchTeleport Action"].performed += OnTeleport;
            m_InputActionAsset["Focus Action"].performed += OnFocusButtonPressed;
            m_MovingAction = m_InputActionAsset["Moving Action"];

            m_ResetButton.buttonClicked += OnResetButtonClicked;
            m_FocusButton.onClick.AddListener(OnFocusButtonClicked);
            m_OrbitButton.onClick.AddListener(OnOrbitButtonClicked);
            m_PanButton.onClick.AddListener(OnPanButtonClicked);
            m_ZoomButton.onClick.AddListener(OnZoomButtonClicked);

            m_DefaultColor = m_DebugButton.buttonRound.color;

            m_PointMesh = Instantiate(m_PointMesh);
            m_PointMesh.SetActive(false);
        }

        void OnZoomButtonClicked()
        {
            m_PreviousOrbitToolType = ToolType.ZoomTool;
        }

        void OnPanButtonClicked()
        {
            m_PreviousOrbitToolType = ToolType.PanTool;
        }

        void OnOrbitButtonClicked()
        {
            m_PreviousOrbitToolType = ToolType.OrbitTool;
        }

        void OnFocusButtonClicked()
        {
            var selectedObj = UIStateManager.current.projectStateData.objectSelectionInfo.CurrentSelectedObject();
            if (selectedObj != null)
            {
                Vector3 focusPoint = selectedObj.transform.position;
                var childBounds = selectedObj.CalculateBoundsInChildren();
                if (childBounds.HasValue)
                    focusPoint = childBounds.Value.center;
                m_Camera.FocusOnPoint(focusPoint);
            }
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
                if (UIStateManager.current.stateData.navigationState.moveEnabled)
                {
                    m_MovingAction.Enable();
                }
                else
                {
                    m_MovingAction.Disable();
                    return;
                }

                var val = m_MovingAction.ReadValue<Vector3>();
                if (val != m_LastMovingAction)
                {
                    m_LastMovingAction = val;
                    m_Camera.MoveInLocalDirection(val, LookAtConstraint.Follow);
                }

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
            }
            else
            {
                m_MovingAction.Disable();
                m_Camera.ForceStop();
                m_LastMovingAction = Vector3.zero;
                m_Camera.MoveInLocalDirection(Vector3.zero, LookAtConstraint.Follow);
            }

            if (m_CachedInfoType != null && m_CachedInfoType == InfoType.Debug &&
                m_CachedDebugOptions != null && m_CachedDebugOptions.Value.gesturesTrackingEnabled)
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

        void OnDebugStateDataChanged(UIDebugStateData data)
        {
            if (m_CachedDebugOptions != data.debugOptionsData)
                m_CachedDebugOptions = data.debugOptionsData;
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (data.rootBounds != m_InitialCameraBounds)
            {
                m_InitialCameraBounds = data.rootBounds;
                OnResetButtonClicked(); // use dispatch to ensure XR cameras also properly change
            }

            m_FocusButton.interactable = UIStateManager.current.projectStateData.objectSelectionInfo.CurrentSelectedObject() != null;
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_ToolbarsEnabled != stateData.toolbarsEnabled)
            {
                m_ResetButton.button.interactable = stateData.toolbarsEnabled;
                m_ToolbarsEnabled = stateData.toolbarsEnabled;
            }

            if (m_CachedNavigationState != stateData.navigationState)
            {
                m_Camera.enabled = stateData.navigationState.freeFlyCameraEnabled;
                m_FocusButton.enabled = stateData.navigationState.freeFlyCameraEnabled;

                if (stateData.navigationState.orbitEnabled)
                {
                    m_InputActionAsset["Orbit Action"].Enable();
                }
                else
                {
                    m_InputActionAsset["Orbit Action"].Disable();
                }

                if (stateData.navigationState.panEnabled)
                {
                    m_InputActionAsset["Pan Mode Action"].Enable();
                    m_InputActionAsset["Pan Gesture Action"].Enable();
                    m_InputActionAsset["Pan Gesture Action"].Enable();
                    m_InputActionAsset["Pan Action"].Enable();
                    m_InputActionAsset["Quick Pan Action"].Enable();
                }
                else
                {
                    m_InputActionAsset["Pan Mode Action"].Disable();
                    m_InputActionAsset["Pan Gesture Action"].Disable();
                    m_InputActionAsset["Pan Gesture Action"].Disable();
                    m_InputActionAsset["Pan Action"].Disable();
                    m_InputActionAsset["Quick Pan Action"].Disable();
                }

                if (stateData.navigationState.zoomEnabled)
                {
                    m_InputActionAsset["Zoom Mode Action"].Enable();
                    m_InputActionAsset["Zoom Gesture Action"].Enable();
                    m_InputActionAsset["Zoom Gesture Action"].Enable();
                    m_InputActionAsset["Zoom Action"].Enable();
                    m_InputActionAsset["Quick Zoom Action"].Enable();
                }
                else
                {
                    m_InputActionAsset["Zoom Mode Action"].Disable();
                    m_InputActionAsset["Zoom Gesture Action"].Disable();
                    m_InputActionAsset["Zoom Gesture Action"].Disable();
                    m_InputActionAsset["Zoom Action"].Disable();
                    m_InputActionAsset["Quick Zoom Action"].Disable();
                }

                if (stateData.navigationState.moveEnabled)
                {
                    m_InputActionAsset["Moving Action"].Enable();
                }
                else
                {
                    m_InputActionAsset["Moving Action"].Disable();
                }

                if (stateData.navigationState.worldOrbitEnabled)
                {
                    m_InputActionAsset["Quick WorldOrbit Action"].Enable();
                }
                else
                {
                    m_InputActionAsset["Quick WorldOrbit Action"].Disable();
                }

                if (stateData.navigationState.teleportEnabled)
                {
                    m_InputActionAsset["Teleport Action"].Enable();
                    m_InputActionAsset["TouchTeleport Action"].Enable();
                }
                else
                {
                    m_InputActionAsset["Teleport Action"].Disable();
                    m_InputActionAsset["TouchTeleport Action"].Disable();
                }

                if (m_CachedNavigationMode != stateData.navigationState.navigationMode)
                {
                    if (stateData.navigationState.navigationMode == NavigationMode.Orbit && m_CachedNavigationMode != NavigationMode.Walk)
                    {
                        StartCoroutine(ResetHomeView());
                    }

                    m_CachedNavigationMode = stateData.navigationState.navigationMode;
                }

                m_GizmoCube.SetActive(stateData.navigationState.gizmoEnabled);

                /*
                 TODO: Temporary fix till next MARS Update. This code will make sure that the cube stay hidden in AR mode.
                 */
                GizmoController gizmoController = GetComponent<GizmoController>();
                if (!stateData.navigationState.gizmoEnabled)
                {
                    gizmoController.HideGizmo();
                }
                else
                {
                    gizmoController.ShowGizmo();
                }

                m_CachedNavigationState = stateData.navigationState;
            }

            if (m_CachedToolType == null || m_CachedToolType != stateData.toolState.activeTool)
            {
                m_CachedToolType = stateData.toolState.activeTool;
            }

            if (m_CachedCameraOptionData != stateData.cameraOptionData)
            {
                switch (stateData.cameraOptionData.cameraViewType)
                {
                    case CameraViewType.Top:
                        OnTopView();
                        break;
                    case CameraViewType.Left:
                        OnLeftView();
                        break;
                    case CameraViewType.Right:
                        OnRightView();
                        break;
                }

                m_CachedCameraOptionData = stateData.cameraOptionData;
            }

            if (m_CachedInfoType != stateData.toolState.infoType)
            {
                m_CachedInfoType = stateData.toolState.infoType;
            }
        }

        void LeaveFollowUserMode()
        {
            if (!string.IsNullOrEmpty(UIStateManager.current.stateData.toolState.followUserTool.userId))
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.FollowUser, null));
            }
        }

        IEnumerator ResetHomeView()
        {
            yield return new WaitForSeconds(0);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ResetHomeView, null));
            m_Camera.camera.clearFlags = CameraClearFlags.Skybox;
        }

        void OnZoomMode(InputAction.CallbackContext context)
        {
            if (!CheckTreatInput(context))
                return;

            var toolState = UIStateManager.current.stateData.toolState;

            if (context.control.IsPressed())
            {
                if (toolState.activeTool == ToolType.PanTool)
                {
                    toolState.activeTool = ToolType.ZoomTool;
                }
                else
                {
                    m_ZoomPressed = true;
                    if (!IsTemporaryTool(toolState.activeTool))
                    {
                        m_PreviousToolType = toolState.activeTool;
                    }
                }
            }
            else
            {
                toolState.activeTool = m_PanPressed ? ToolType.PanTool : m_PreviousToolType ?? ToolType.None;
                m_ZoomPressed = false;
            }

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
        }

        bool IsTemporaryTool(ToolType activeTool)
        {
            switch (activeTool)
            {
                case ToolType.ClippingTool:
                case ToolType.OrbitTool:
                case ToolType.SelectTool:
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

        void OnPanMode(InputAction.CallbackContext context)
        {
            if (!CheckTreatInput(context))
                return;

            var toolState = UIStateManager.current.stateData.toolState;

            if (context.control.IsPressed())
            {
                m_PanPressed = true;
                if (!IsTemporaryTool(toolState.activeTool))
                {
                    m_PreviousToolType = toolState.activeTool;
                }

                toolState.activeTool = m_ZoomPressed ? ToolType.ZoomTool : ToolType.PanTool;
            }
            else
            {
                toolState.activeTool = m_PreviousToolType ?? ToolType.None;
                m_PanPressed = false;
            }

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
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

        void Orbit(Vector2 delta)
        {
            var data = UIStateManager.current.stateData;
            switch (data.toolState.orbitType)
            {
                case OrbitType.WorldOrbit:
                    m_Camera.Rotate(new Vector2(delta.y, delta.x));
                    break;
                case OrbitType.OrbitAtPoint:
                    m_Camera.OrbitAroundLookAt(new Vector2(delta.y, delta.x));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (m_CachedCameraOptionData?.cameraViewType != CameraViewType.Default)
            {
                data.cameraOptionData.cameraViewType = CameraViewType.Default;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetCameraOption, data.cameraOptionData));
            }
            OnInteraction();
        }

        void OnOrbit(InputAction.CallbackContext context)
        {
            if (m_CachedToolType != ToolType.OrbitTool || OrphanUIController.isTouchBlockedByUI ||
                m_ZoomGestureInProgress || m_PanGestureInProgress || !CheckTreatInput(context))
                return;

            var readValue = context.ReadValue<Vector2>();

            m_WorldOrbitDelta.SetNewFrameDelta(readValue);
            var delta = m_WorldOrbitDelta.delta;
            var worldVector = new Vector2(delta.x, -delta.y);

            Orbit(worldVector);
        }

        void OnPan(InputAction.CallbackContext context)
        {
            if (m_CachedToolType != ToolType.PanTool || OrphanUIController.isTouchBlockedByUI ||
                m_ZoomGestureInProgress || !CheckTreatInput(context))
                return;

            m_PanDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
            var delta = m_PanDelta.delta * -Vector2.one;
            Pan(delta);
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
                float ratio = 0.01f;
#if UNITY_ANDROID
                ratio = 0.1f;
#endif
                var delta = m_PanDelta.delta * (-Vector2.one * ratio);
                Pan(delta);
            }
        }

        void OnZoom(InputAction.CallbackContext context)
        {
            if (m_CachedToolType != ToolType.ZoomTool || OrphanUIController.isTouchBlockedByUI ||
                m_PanGestureInProgress || !CheckTreatInput(context))
                return;

            m_ZoomDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
            Zoom(m_ZoomDelta.delta);
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
#if UNITY_ANDROID
                // TODO: This should be the general algorithm, but we will need to test it on iOS first.
                var distance = m_GestureCameraStartPosition - (m_GestureCameraStartPosition) * ((pinchGesture.gap - pinchGesture.startGap) / Screen.height);
                if (distance > m_GestureCameraStartPosition)
                {
                    // Double zoom out ratio
                    distance += distance - m_GestureCameraStartPosition;
                }

                m_Camera.SetDistanceFromLookAt(distance);
#else
                m_ZoomDelta.SetNewFrameDelta(new Vector2(0.0f, pinchGesture.gapDelta / pinchGesture.gap) * m_ZoomGestureSensibility);
                Zoom(m_ZoomDelta.delta);
#endif
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

        struct QuickToolData
        {
            public ToolType toolType;
            public OrbitType orbitType;
        }

        void OnQuickToolEnd()
        {
            var toolState = UIStateManager.current.stateData.toolState;
            var navigationToolState = toolState;

            toolState.activeTool = m_PanPressed ? (m_ZoomPressed ? ToolType.ZoomTool : ToolType.PanTool) : m_PreviousToolType ?? ToolType.None;
            m_PreviousToolType = null;
            if (m_PreviousOrbitType != null)
            {
                toolState.orbitType = m_PreviousOrbitType ?? toolState.orbitType;
                m_PreviousOrbitType = null;
            }

            // Reset Orbit tool type first
            navigationToolState.activeTool = m_PreviousOrbitToolType ?? ToolType.None;
            navigationToolState.orbitType = OrbitType.OrbitAtPoint;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, navigationToolState));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
        }

        IEnumerator DelayEndQuickTool(QuickToolData toolData)
        {
            var toolState = UIStateManager.current.stateData.toolState;

            if (m_PreviousToolType == null)
            {
                m_PreviousToolType = toolState.activeTool;
            }

            if (toolState.activeTool != toolData.toolType || (toolData.orbitType != OrbitType.None && toolState.orbitType != toolData.orbitType))
            {
                if (toolState.orbitType != toolData.orbitType)
                {
                    m_PreviousOrbitType = toolState.orbitType;
                }

                toolState.activeTool = toolData.toolType;

                if (toolData.toolType == ToolType.OrbitTool)
                {
                    toolState.orbitType = toolData.orbitType;
                }
            }

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));

            yield return new WaitForSeconds(k_ToolDebounceTime);

            OnQuickToolEnd();
        }

        void OnQuickZoom(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isPointBlockedByUI)
                return;

            if (CheckTreatInput(context))
            {
                StopCoroutine(nameof(DelayEndQuickTool));
                StartCoroutine(nameof(DelayEndQuickTool), new QuickToolData() { toolType = ToolType.ZoomTool, orbitType = OrbitType.None });

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
                StopCoroutine(nameof(DelayEndQuickTool));
                StartCoroutine(nameof(DelayEndQuickTool), new QuickToolData() { toolType = ToolType.PanTool, orbitType = OrbitType.None });

                m_PanDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
                var delta = m_PanDelta.delta * -Vector2.one;

                Pan(delta);
            }
        }

        void OnQuickWorldOrbit(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isTouchBlockedByUI || !CheckTreatInput(context))
                return;

            StopCoroutine(nameof(DelayEndQuickTool));
            StartCoroutine(nameof(DelayEndQuickTool),
                new QuickToolData() { toolType = ToolType.OrbitTool, orbitType = OrbitType.WorldOrbit });

            m_WorldOrbitDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
            var delta = m_WorldOrbitDelta.delta;
            var worldVector = new Vector2(delta.x, -delta.y);

            Orbit(worldVector);
        }

        void OnTeleport(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isTouchBlockedByUI || m_TeleportController == null || !CheckTreatInput(context))
                return;

            m_TeleportController.TriggerTeleport(Pointer.current.position.ReadValue());
        }

        public bool OnGetTeleportTarget(bool placementMeshActive, bool teleport = false)
        {
            Vector2 pointerPos = Pointer.current.position.ReadValue();
            bool validTarget = MeshTargetPlacement(pointerPos);
            m_PointMesh.SetActive(placementMeshActive);
            if (validTarget && teleport)
            {
                m_TeleportController.TriggerTeleport(pointerPos);
                isTeleportInit = false;
            }

            return validTarget;
        }

        public Vector3 GetWalkReticlePosition()
        {
            return m_PointMesh.transform.position;
        }

        bool MeshTargetPlacement(Vector2 pointerPos)
        {
            Vector3 target = m_TeleportController.GetTeleportTarget(pointerPos);
            bool validTarget = (target != Vector3.zero);

            if (!isTeleportInit)
            {
                m_PointMesh.SetActive(true);
                isTeleportInit = true;
            }

#if ENABLE_VR
            Vector3 originalpos = new Vector3(pointerPos.x, pointerPos.y, GetComponent<Canvas>().planeDistance);
            m_PointMesh.GetComponent<BezierCurve>().StartPosition = m_Camera.camera.ScreenToWorldPoint(originalpos);
#endif
            m_PointMesh.transform.position = target;
            float size = (m_Camera.transform.position - target).magnitude;
            m_PointMesh.transform.localScale = Vector3.one * size;

            return validTarget;
        }

        void OnResetButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.HomeReset)) return;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ResetHomeView, null));
        }

        public CameraTransformInfo ResetCamera()
        {
            m_Camera.camera.clearFlags = CameraClearFlags.Skybox;
            m_Camera.SetupInitialCameraPosition(m_InitialCameraBounds, 20.0f, 0.75f);
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
    }
}
