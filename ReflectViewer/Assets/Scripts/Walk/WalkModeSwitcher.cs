using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.UI;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer
{
    public class WalkModeSwitcher: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        GameObject m_FPSController;
        [SerializeField]
        Vector3 m_Offset;
        [SerializeField]
        Vector3 m_RotationOffset;
        [SerializeField]
        WalkModeTeleportController m_WalkModeTeleportController;
        [SerializeField]
        FreeFlyCamera m_FreeFlyCamera;
        [SerializeField]
        Camera m_MainCamera;
        [SerializeField]
        GameObject m_LeftJoystick;
        [SerializeField]
        InputActionAsset m_InputActionAsset;
        [SerializeField]
        Vector3 m_Destination;
#pragma warning restore CS0649
        InputActionMap m_InputActionMap;
        CharacterController m_CharacterController;
        bool m_IsPlacementMode;
        bool m_IsInit;
        bool m_IsResettingPosition;
        bool m_CancelWalkMode;

        FirstPersonController m_FirstPersonController;

        bool m_IsActivated;
        OpenDialogAction.DialogType m_CachedSubDialog, m_CachedDialog;
        SetDialogModeAction.DialogMode m_CachedDialogMode;
        SetActiveToolAction.ToolType m_CachedActiveTool;
        IUISelector<bool> m_MeasureToolStateSelector;
        IUISelector<bool> m_WalkModeEnableSelector;
        IUISelector<SetInstructionUIStateAction.InstructionUIState> m_WalkInstructionStateSelector;
        IUISelector<IWalkInstructionUI> m_WalkInstructionSelector;
        bool m_RotateTarget;
        Image[] m_JoystickImages;
        Coroutine m_StatusDialogCloseCoroutine;
        IUISelector<float> m_ScaleFactorSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }


        void Awake()
        {
#if UNITY_ANDROID || UNITY_IOS
            m_InputActionAsset["Place Joystick Action"].performed += OnPlaceJoystick;
            OrphanUIController.onBeginDrag += OnBeginDrag;
            OrphanUIController.onDrag += OnDrag;
            OrphanUIController.onEndDrag += OnCancel;
#else
            m_InputActionAsset["Walk Mode Action"].started += OnBeginDrag;
            m_InputActionAsset["Walk Mode Action"].performed += OnDrag;
            m_InputActionAsset["Walk Mode Action"].canceled += OnCancel;
#endif

            m_InputActionAsset["Walk Mode Action Tap"].canceled += OnCancel;
            m_InputActionMap = m_InputActionAsset.FindActionMap("Walk");
            m_InputActionMap.Disable();

            m_DisposeOnDestroy.Add(m_MeasureToolStateSelector = UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState), OnMeasureToolToolChanged));
            m_DisposeOnDestroy.Add(m_WalkModeEnableSelector = UISelectorFactory.createSelector<bool>(WalkModeContext.current, nameof(IWalkModeDataProvider.walkEnabled), OnWalkEnable));
            m_DisposeOnDestroy.Add(m_WalkInstructionStateSelector = UISelectorFactory.createSelector<SetInstructionUIStateAction.InstructionUIState>(WalkModeContext.current, nameof(IWalkModeDataProvider.instructionUIState)));
            m_DisposeOnDestroy.Add(m_WalkInstructionSelector = UISelectorFactory.createSelector<IWalkInstructionUI>(WalkModeContext.current, nameof(IWalkModeDataProvider.instruction)));
            m_DisposeOnDestroy.Add(m_ScaleFactorSelector = UISelectorFactory.createSelector<float>(UIStateContext.current, nameof(IUIStateDisplayProvider<DisplayData>.display) + "." + nameof(IDisplayDataProvider.scaleFactor)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog),
                type =>
                {
                    m_CachedDialog = type;
                    CheckJoystickAvailable();
                }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog),
                type =>
                {
                    m_CachedSubDialog = type;
                    CheckJoystickAvailable();
                }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetDialogModeAction.DialogMode>(UIStateContext.current, nameof(IDialogDataProvider.dialogMode),
                type =>
                {
                    m_CachedDialogMode = type;
                    CheckJoystickAvailable();
                }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetActiveToolAction.ToolType>(ToolStateContext.current, nameof(IToolStateDataProvider.activeTool),
                type =>
                {
                    m_CachedActiveTool = type;
                    CheckJoystickAvailable();
                }));

            m_JoystickImages = m_LeftJoystick.GetComponentsInChildren<Image>();
        }

        void OnWalkEnable(bool newData)
        {
            if (m_WalkModeEnableSelector != null)
            {
                if (!newData)
                    m_CancelWalkMode = true;
                else
                    ((WalkModeInstruction)m_WalkInstructionSelector.GetValue()).Initialize(this);
            }
        }

        void OnBeginDrag<T>(T evt)
        {
            BeginDragRotation();
        }

        void OnCancel<T>(T evt)
        {
            OnFinishTeleport();
        }

        void OnDrag(BaseEventData evt)
        {
            RotateTarget();
        }

        void OnDrag(InputAction.CallbackContext context)
        {
            RotateTarget();
        }

        void OnFinishTeleport()
        {
            if (!m_RotateTarget && !m_IsInit)
                return;

            m_WalkModeTeleportController.IsTargetPositionValid(result =>
            {
                if (result || m_RotateTarget)
                {
                    m_WalkModeTeleportController.EnableTarget(false);
                    WalkStartPosition();
                }
            });
        }

        void RotateTarget()
        {
            if (m_RotateTarget)
            {
                m_WalkModeTeleportController.StartDistanceAnimation();
                m_WalkModeTeleportController.SetRotation(m_RotationOffset);
                m_RotationOffset.y = m_WalkModeTeleportController.GetRotation();
            }
        }

        void BeginDragRotation()
        {
            Dispatcher.Dispatch(SetWalkStateAction.From(SetInstructionUIStateAction.InstructionUIState.Started));
            if (!m_IsInit)
                return;

            m_WalkModeTeleportController.SetTeleportDestination(Pointer.current.position.ReadValue());
            m_WalkModeTeleportController.IsTargetPositionValid(result =>
            {
                if (!result)
                    return;

                if (m_WalkModeEnableSelector.GetValue() && m_IsPlacementMode)
                {
                    m_RotateTarget = true;
                    m_WalkModeTeleportController.EnableTarget(true);
                    m_IsPlacementMode = false;
                    m_InputActionAsset["Camera Control Action"].Enable();
                    m_InputActionAsset["Orbit Action"].Disable();
                }
            });
        }

        void OnMeasureToolToolChanged(bool data)
        {
            CheckJoystickAvailable();
        }

        void CheckJoystickAvailable()
        {
            if (m_IsActivated)
            {
                if (m_CachedDialog != OpenDialogAction.DialogType.None
                    || (m_CachedSubDialog != OpenDialogAction.DialogType.None && m_CachedDialog != OpenDialogAction.DialogType.GizmoMode)
                    || m_CachedDialogMode == SetDialogModeAction.DialogMode.Help
                    || m_MeasureToolStateSelector.GetValue()
                    || m_CachedActiveTool == SetActiveToolAction.ToolType.SunstudyTool)
                {
                    m_InputActionAsset["Place Joystick Action"].Disable();
                }
                else
                {
                    m_InputActionAsset["Place Joystick Action"].Enable();
                }
            }
        }

        void EnableJoystick(bool enable)
        {
            foreach (var image in m_JoystickImages)
            {
                image.enabled = enable;
            }
        }

        void OnPlaceJoystick(InputAction.CallbackContext context)
        {
            if (!m_WalkModeEnableSelector.GetValue() ||  m_FirstPersonController.GetJoystickTouch() != null)
                return;

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                foreach (var touch in touchscreen.touches)
                {
                    var pos = touch.position.ReadValue();
                    if (touch.isInProgress && (IsLeftSide(pos) || m_FirstPersonController.GetJoystickTouch() == touch)
                        && m_FirstPersonController.IsTouchControlDifferent())
                    {
                        m_LeftJoystick.transform.position = pos;
                        m_FirstPersonController.SetJoystickTouch(touch);

                        EnableJoystick(true);
                    }
                }
            }
        }

        bool IsLeftSide(Vector2 coordinate)
        {
            bool retval = false;
            if (!IsInsideBound(coordinate))
                return retval;

            var boundRect = Screen.safeArea;
            boundRect.xMax -= boundRect.width / 2f;

            if (boundRect.Contains(coordinate))
            {
                retval = true;
            }

            return retval;
        }

        void WalkStartPosition()
        {
            if (m_WalkModeEnableSelector.GetValue() && m_IsInit)
            {
                m_InputActionAsset["Orbit Action"].Enable();

#if UNITY_ANDROID || UNITY_IOS
                if (!m_RotateTarget)
                {
                    m_WalkModeTeleportController.GetAsyncTargetPosition(result =>
                    {
                        m_Destination = result;
                        OnActivateTeleport();
                    });
                }
                else
                {
                    m_Destination = m_WalkModeTeleportController.GetWalkReticlePosition();
                    OnActivateTeleport();
                }
#else
                m_Destination = m_WalkModeTeleportController.GetWalkReticlePosition();
                OnActivateTeleport();
#endif
                m_RotateTarget = false;
            }
        }

        void OnActivateTeleport()
        {
            m_WalkModeTeleportController.OnGetTeleportTarget(false, true, result =>
            {
                if (result)
                {
                    m_IsPlacementMode = false;
                }
            });
        }

        public void OnQuitWalkMode()
        {
            m_LeftJoystick.SetActive(false);
            m_FirstPersonController.SetJoystickTouch(null);

            m_IsPlacementMode = false;
            ActivateFlyMode(!m_IsInit);
            m_IsActivated = false;

            if (m_StatusDialogCloseCoroutine != null)
            {
                StopCoroutine(m_StatusDialogCloseCoroutine);
                m_StatusDialogCloseCoroutine = null;
            }
            Dispatcher.Dispatch(SetWalkStateAction.From(SetInstructionUIStateAction.InstructionUIState.Init));
        }



        public void OnWalkStart()
        {
            ActivateWalkMode();
            m_IsActivated = true;

            if (m_StatusDialogCloseCoroutine != null)
            {
                StopCoroutine(m_StatusDialogCloseCoroutine);
                m_StatusDialogCloseCoroutine = null;
            }

            m_StatusDialogCloseCoroutine = StartCoroutine(((WalkModeInstruction)m_WalkInstructionSelector.GetValue()).WaitCloseStatusDialog());
        }

        bool IsInsideBound(Vector2 coordinate, float range = 90)
        {
            range *= m_ScaleFactorSelector.GetValue();
            var boundRect = Screen.safeArea;
            boundRect.xMin += range;
            boundRect.xMax -= range;
            boundRect.yMin += range;
            boundRect.yMax -= range;

            return boundRect.Contains(coordinate);
        }

        public void Init()
        {
            m_InputActionMap.Enable();
            if (m_WalkModeEnableSelector.GetValue())
            {
                m_IsPlacementMode = true;
            }
            else
            {
                m_IsPlacementMode = false;
                ActivateFlyMode();
            }

            m_IsInit = true;
            m_WalkModeTeleportController.SetRotation(Vector3.zero);
            m_InputActionAsset["Place Joystick Action"].Disable();
        }

        void Start()
        {
            m_FPSController = Instantiate(m_FPSController);
            m_FirstPersonController = m_FPSController.GetComponent<FirstPersonController>();
            m_FirstPersonController.Init(m_LeftJoystick.GetComponentInChildren<JoystickControl>(true));
            m_FreeFlyCamera = m_MainCamera.GetComponent<FreeFlyCamera>();
        }

        void Update()
        {
            if (m_CancelWalkMode)
            {
                m_CancelWalkMode = false;
                m_WalkInstructionSelector.GetValue().Cancel();
            }

            if (m_IsPlacementMode && !m_WalkModeTeleportController.IsTeleporting())
            {
                m_WalkModeTeleportController.OnGetTeleportTarget(m_IsPlacementMode, false, null);
            }
#if UNITY_ANDROID || UNITY_IOS
            if (m_FirstPersonController.GetJoystickTouch() == null)
            {
                m_FirstPersonController.SetJoystickTouch(null);
                EnableJoystick(false);
            }
#endif
        }

        public void ResetCamPos(Vector3 offset)
        {
            if (m_IsResettingPosition)
            {
                return;
            }

            m_IsResettingPosition = true;
            m_Offset = offset;
            ActivateFlyMode(false);
            m_FreeFlyCamera.ResetDesiredPosition();
            ActivateWalkMode();
            m_InputActionMap.Enable();
        }

        void ActivateFlyMode(bool moveCamera = true)
        {
            m_FPSController.SetActive(false);
            m_MainCamera.gameObject.SetActive(true);
            m_MainCamera.clearFlags = CameraClearFlags.Skybox;

#if UNITY_IOS
            // Camera clearFlags is changed to CameraClearFlags.Nothing after one frame on iOS so need to set to Skybox in Coroutine
            StartCoroutine(SetCameraSkybox());
#endif

            if (moveCamera)
            {
                m_FreeFlyCamera.TransformTo(m_FPSController.transform);
            }

            m_InputActionMap.Disable();
            m_InputActionAsset["Orbit Action"].Enable();
            m_IsInit = false;
        }

#if UNITY_IOS
        IEnumerator SetCameraSkybox()
        {
            yield return null;
            m_MainCamera.clearFlags = CameraClearFlags.Skybox;
        }
#endif

        void ActivateWalkMode()
        {
            m_FPSController.SetActive(true);
            StartCoroutine(ChangePos());
            m_MainCamera.gameObject.SetActive(false);

            m_IsInit = false;
            m_InputActionAsset["Place Joystick Action"].Disable();
        }

        void SetCameraRotation(Quaternion rot)
        {
            m_CharacterController.transform.localRotation = rot;
            m_CharacterController.GetComponentInChildren<Camera>().transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        IEnumerator ChangePos()
        {
            m_CharacterController = m_FPSController.GetComponent<CharacterController>();
            m_CharacterController.enabled = false;

            if (!m_IsResettingPosition)
            {
                // When joystick is hidden, first touch and drag is not working. So this code is needed.
                foreach (var image in m_JoystickImages)
                {
                    image.enabled = false;
                }

                m_LeftJoystick.SetActive(true);
            }

            m_CharacterController.transform.position = m_Destination + m_Offset;
            SetCameraRotation(Quaternion.Euler(m_RotationOffset));
            yield return null;
            m_CharacterController.enabled = true;

            m_InputActionAsset["Place Joystick Action"].Enable();
            m_WalkModeTeleportController.TeleportFinish();
            m_RotationOffset = Vector3.zero;
            m_IsResettingPosition = false;

            yield return new WaitForEndOfFrame();
            Dispatcher.Dispatch(SetWalkStateAction.From(SetInstructionUIStateAction.InstructionUIState.Completed));
        }
    }
}
