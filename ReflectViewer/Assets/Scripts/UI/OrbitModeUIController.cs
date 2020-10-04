using System;
using System.Collections;
using SharpFlux;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using TouchPhase = UnityEngine.TouchPhase;

namespace Unity.Reflect.Viewer.UI
{
    public class OrbitModeUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField] FreeFlyCamera m_Camera;
        [SerializeField] UITeleportController m_TeleportController;
        [SerializeField] InputActionAsset m_InputActionAsset;
        [SerializeField] UINavigationControllerSettings m_UINavigationControllerSettings;
        [SerializeField] ToolButton m_ResetButton;
#pragma warning restore CS0649

        ToolType? m_CachedToolType;
        ToolType? m_PreviousToolType;
        OrbitType? m_PreviousOrbitType;
        NavigationMode? m_CachedNavigationMode;

        static readonly float k_ToolDebounceTime = 0.2f;

        bool m_ZoomPressed;
        bool m_PanPressed;

        Bounds m_InitialCameraBounds;

        DialogType m_CurrentActiveDialog;

        Vector3 m_LastMovingAction;

        DeltaCalculator m_ZoomDelta;
        DeltaCalculator m_PanDelta;
        DeltaCalculator m_WorldOrbitDelta;

        InputAction m_MovingAction;

        uint m_InputSkipper = 0;

        bool m_IsBlocked;

        public void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

            m_InputActionAsset["Pan Mode Action"].performed += OnPanMode;
            m_InputActionAsset["Zoom Mode Action"].performed += OnZoomMode;
            m_InputActionAsset["Zoom Action"].performed += OnZoom;
            m_InputActionAsset["Quick Zoom Action"].performed += OnQuickZoom;
            m_InputActionAsset["Pan Action"].performed += OnPan;
            m_InputActionAsset["Orbit Action"].performed += OnOrbit;
            m_InputActionAsset["Quick Pan Action"].performed += OnQuickPan;
            m_InputActionAsset["Quick WorldOrbit Action"].performed += OnQuickWorldOrbit;
            m_InputActionAsset["Teleport Action"].performed += OnTeleport;
            m_MovingAction = m_InputActionAsset["Moving Action"];

            m_ResetButton.buttonClicked += OnResetButtonClicked;

            Selector.useSelector("toolbarsEnabled", (toolbarEnabled) =>
            {
                if (toolbarEnabled)
                {
                    if (m_CachedNavigationMode == NavigationMode.Orbit)
                    {
                        m_Camera.enabled = true;
                        m_InputActionAsset.Enable();
                    }
                    else
                    {
                        m_Camera.enabled = false;
                        m_InputActionAsset.Disable();
                    }
                }
                else
                {
                    m_Camera.enabled = false;
                    m_InputActionAsset.Disable();
                }
                m_ResetButton.button.interactable = toolbarEnabled;
            });
        }

        void Update()
        {
            IsBlockedByUI();
            m_ZoomDelta.Reset();
            m_PanDelta.Reset();
            m_WorldOrbitDelta.Reset();
            m_InputSkipper++;

            if (Time.unscaledDeltaTime <= m_UINavigationControllerSettings.inputLagCutoffThreshold)
            {
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
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (data.rootBounds != m_InitialCameraBounds)
            {
                m_InitialCameraBounds = data.rootBounds;
                OnResetButtonClicked(); // use dispatch to ensure XR cameras also properly change
            }
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_CachedNavigationMode != stateData.navigationState.navigationMode)
            {
                if (stateData.navigationState.navigationMode == NavigationMode.Orbit)
                {
                    m_Camera.enabled = true;
                    m_InputActionAsset.Enable();
                    StartCoroutine(ResetHomeView());
                    m_ResetButton.button.interactable = stateData.toolbarsEnabled; // only disable home button on AR mode
                }
                else if (stateData.navigationState.navigationMode == NavigationMode.AR)
                {
                    m_Camera.enabled = false;
                    m_InputActionAsset.Disable();
                    m_ResetButton.button.interactable = false;
                }
                else
                {
                    m_Camera.enabled = false;
                    m_InputActionAsset.Disable();
                    m_ResetButton.button.interactable = stateData.toolbarsEnabled;
                }

                m_CachedNavigationMode = stateData.navigationState.navigationMode;
            }

            if (m_CachedToolType == null || m_CachedToolType != stateData.toolState.activeTool)
            {
                m_CachedToolType = stateData.toolState.activeTool;
            }

            if (m_PreviousToolType == null && !IsTemporaryTool(stateData.toolState.activeTool))
            {
                m_PreviousToolType = stateData.toolState.activeTool;
            }
            else if (stateData.toolState.activeTool == ToolType.None)
            {
                m_PreviousToolType = ToolType.OrbitTool;
            }
        }

        private IEnumerator ResetHomeView()
        {
            yield return new WaitForSeconds(0);
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ResetHomeView, null));
            if (!Application.isEditor)
            {
                m_Camera.camera.clearFlags = CameraClearFlags.Skybox;
            }
        }

        void OnZoomMode(InputAction.CallbackContext context)
        {
            if (CheckTreatInput(context))
            {

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

                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            }
        }

        bool IsTemporaryTool(ToolType activeTool)
        {
            switch (activeTool)
            {
                case ToolType.ClippingTool:
                case ToolType.OrbitTool:
                case ToolType.SelectTool:
                case ToolType.MeasureTool:
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
            if (CheckTreatInput(context))
            {

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
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            }
        }

        void Zoom(Vector2 delta)
        {
            m_Camera.MoveOnLookAtAxis(delta.y);
        }

        void Pan(Vector2 delta)
        {
            m_Camera.Pan(delta);
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
        }

        void OnOrbit(InputAction.CallbackContext context)
        {
            if (m_IsBlocked)
                return;

            if (CheckTreatInput(context))
            {
                if (m_CachedToolType != ToolType.OrbitTool)
                    return;

                m_WorldOrbitDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
                var delta = m_WorldOrbitDelta.delta;
                var worldVector = new Vector2(delta.x, -delta.y);
                Orbit(worldVector);
            }
        }

        void OnPan(InputAction.CallbackContext context)
        {
            if (m_IsBlocked)
                return;

            if (CheckTreatInput(context))
            {
                if (m_CachedToolType != ToolType.PanTool)
                    return;

                m_PanDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
                var delta = m_PanDelta.delta * -Vector2.one;
                Pan(delta);
            }
        }

        void OnZoom(InputAction.CallbackContext context)
        {
            if (m_IsBlocked)
                return;

            if (CheckTreatInput(context))
            {
                if (m_CachedToolType != ToolType.ZoomTool)
                    return;

                m_ZoomDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
                Zoom(m_ZoomDelta.delta);
            }
        }

        struct QuickToolData
        {
            public ToolType toolType;
            public OrbitType orbitType;
        }

        void OnQuickToolEnd()
        {
            var toolState = UIStateManager.current.stateData.toolState;

            toolState.activeTool = m_PanPressed ? (m_ZoomPressed ? ToolType.ZoomTool:ToolType.PanTool):m_PreviousToolType ?? ToolType.None;

            if (m_PreviousOrbitType != null)
            {
                toolState.orbitType = m_PreviousOrbitType ?? toolState.orbitType;
                m_PreviousOrbitType = null;
            }

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
        }

        IEnumerator DelayEndQuickTool(QuickToolData toolData)
        {
            var toolState = UIStateManager.current.stateData.toolState;

            if (toolState.activeTool != toolData.toolType ||  (toolData.orbitType != OrbitType.None && toolState.orbitType != toolData.orbitType))
            {
                if (!IsTemporaryTool(toolState.activeTool))
                {
                    m_PreviousToolType = toolState.activeTool;
                }

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
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));

            yield return new WaitForSeconds(k_ToolDebounceTime);

            OnQuickToolEnd();
        }

        void OnQuickZoom(InputAction.CallbackContext context)
        {
            if (m_IsBlocked)
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
            if (m_IsBlocked)
                return;

            if (CheckTreatInput(context))
            {
                StopCoroutine(nameof(DelayEndQuickTool));
                StartCoroutine(nameof(DelayEndQuickTool), new QuickToolData() { toolType = ToolType.PanTool, orbitType = OrbitType.None});

                m_PanDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
                var delta = m_PanDelta.delta * -Vector2.one;

                Pan(delta);
            }
        }

        void OnQuickWorldOrbit(InputAction.CallbackContext context)
        {
            if (m_IsBlocked)
                return;

            if (CheckTreatInput(context))
            {
                StopCoroutine(nameof(DelayEndQuickTool) );
                StartCoroutine(nameof(DelayEndQuickTool), new QuickToolData(){ toolType = ToolType.OrbitTool, orbitType = OrbitType.WorldOrbit} );

                m_WorldOrbitDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
                var delta = m_WorldOrbitDelta.delta;
                var worldVector = new Vector2(delta.x, -delta.y);

                Orbit(worldVector);
            }
        }

        void OnTeleport(InputAction.CallbackContext context)
        {
            if (m_IsBlocked || m_TeleportController == null)
                return;

            if (CheckTreatInput(context))
                m_TeleportController.TriggerTeleport(Pointer.current.position.ReadValue());
        }

        bool IsBlockedByUI()
        {
            var id = -1;
            var pressed = false;
            var scrolled = false;

            for (var i = 0; i < Input.touchCount; ++i)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    id = Input.GetTouch(i).fingerId;
                    pressed = true;
                    break;
                }
            }

            if (!pressed)
            {
                pressed = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
            }

            if (!pressed)
            {
                scrolled = Input.mouseScrollDelta.y > 0.0f || Input.mouseScrollDelta.y < 0.0f;
            }

            if (pressed || scrolled)
            {
                m_IsBlocked = EventSystem.current.IsPointerOverGameObject(id);
            }

            return m_IsBlocked;
        }


        void OnResetButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ResetHomeView, null));
        }

        public CameraTransformInfo ResetCamera()
        {
            if (!Application.isEditor)
            {
                m_Camera.camera.clearFlags = CameraClearFlags.Skybox;
            }
            m_Camera.SetupInitialCameraPosition(m_InitialCameraBounds, 20.0f, 0.75f);
            var camTransform = m_Camera.camera.transform;
            return new CameraTransformInfo {position = camTransform.position, rotation = camTransform.eulerAngles};
        }
    }
}
