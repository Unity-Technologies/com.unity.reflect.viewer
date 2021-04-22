using System;
using System.Collections;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.UI;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer
{
    public class WalkModeSwitcher : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        GameObject m_FPSController;
        [SerializeField]
        Vector3 m_Offset;
        [SerializeField]
        Vector3 m_RotationOffset;
        [SerializeField]
        OrbitModeUIController m_OrbitModeUIController;
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
        WalkModeInstruction m_WalkModeInstruction;
        CharacterController m_CharacterController;
        bool m_IsPlacementMode = false;
        bool m_HasFinishPlacement = true;
        bool m_IsInit = false;
        bool m_IsResetingPosition = false;
        OnScreenStick m_OnScreenStick;
        FirstPersonController m_FirstPersonController;

        UIWalkStateData? m_CachedWalkStateData;
        bool m_IsActivated;
        DialogType m_CachedSubDialog, m_CachedDialog;
        DialogMode m_CachedDialogMode;
        MeasureToolStateData? m_CachedMeasureToolStateData;

        Image[] m_JoystickImages;
        void Awake()
        {
            m_InputActionAsset["Walk Mode Action"].performed += OnWalkStartPositionValidate;
            m_InputActionAsset["Switch Mode Action"].performed += OnSwitchMode;
#if UNITY_ANDROID || UNITY_IOS
            m_InputActionAsset["Place Joystick Action"].performed += OnPlaceJoystick;
#endif
            m_InputActionMap = m_InputActionAsset.FindActionMap("Walk");
            m_InputActionMap.Disable();
            m_OnScreenStick = m_LeftJoystick.GetComponentInChildren<OnScreenStick>(true);
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.externalToolChanged += OnExternalToolChanged;
            m_JoystickImages = m_LeftJoystick.GetComponentsInChildren<Image>();
        }

        void OnExternalToolChanged(ExternalToolStateData data)
        {
            if (m_CachedMeasureToolStateData != data.measureToolStateData)
            {
                m_CachedMeasureToolStateData = data.measureToolStateData;
                CheckJoystickAvailable();
            }
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_CachedDialog != stateData.activeDialog || m_CachedSubDialog != stateData.activeSubDialog
            || m_CachedDialogMode != stateData.dialogMode)
            {
                m_CachedDialog = stateData.activeDialog;
                m_CachedSubDialog = stateData.activeSubDialog;
                m_CachedDialogMode = stateData.dialogMode;
                CheckJoystickAvailable();
            }
        }

        void CheckJoystickAvailable()
        {
            if (m_IsActivated)
            {
                if ( (m_CachedDialog != DialogType.None && m_CachedDialog != DialogType.GizmoMode)
                    || m_CachedSubDialog != DialogType.None
                    || m_CachedDialogMode == DialogMode.Help
                    || m_CachedMeasureToolStateData?.toolState == true)
                {
                    m_InputActionAsset["Place Joystick Action"].Disable();
                }
                else
                {
                    m_InputActionAsset["Place Joystick Action"].Enable();
                }
            }
        }

        public void EnableJoystick(bool enable)
        {
            m_OnScreenStick.enabled = enable;
            m_LeftJoystick.SetActive(enable);
        }

        void OnPlaceJoystick(InputAction.CallbackContext obj)
        {
            if (m_CachedWalkStateData == null)
                return;

            Vector3 pos = Pointer.current.position.ReadValue();

            if (IsInsideBound(pos))
            {
                m_LeftJoystick.transform.position = pos;
                EnableJoystick(true);
            }
        }

        void OnWalkStartPositionValidate(InputAction.CallbackContext obj)
        {
            if (m_CachedWalkStateData.HasValue && m_CachedWalkStateData.Value.walkEnabled)
            {
                m_Destination = m_OrbitModeUIController.GetWalkReticlePosition();
                if (m_OrbitModeUIController.OnGetTeleportTarget(false, true))
                {
                    m_IsPlacementMode = false;
                }
            }
        }

        void OnWalkStateDataChanged(UIWalkStateData walkData)
        {
            if (m_WalkModeInstruction == null && walkData.walkEnabled)
            {
                m_WalkModeInstruction = walkData.instruction as WalkModeInstruction;
                m_WalkModeInstruction.Initialize(this);
            }

            if (walkData != m_CachedWalkStateData)
            {
                m_CachedWalkStateData = walkData;
            }
        }

        public void OnQuitWalkMode()
        {
            EnableJoystick(false);

            m_IsPlacementMode = false;
            m_OrbitModeUIController.OnGetTeleportTarget(m_IsPlacementMode);
            ActivateFlyMode(!m_IsInit);
            m_IsActivated = false;
        }

        public void OnWalkStart()
        {
            ActivateWalkMode();
            m_IsActivated = true;
        }

        bool IsInsideBound(Vector2 coordinate, float range = 90)
        {
            range *= UIStateManager.current.stateData.display.scaleFactor;
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
            if (m_CachedWalkStateData != null && m_CachedWalkStateData.Value.walkEnabled)
            {
                m_IsPlacementMode = true;
            }
            else
            {
                m_IsPlacementMode = false;
                ActivateFlyMode();
            }

            m_IsInit = true;
            m_InputActionAsset["Place Joystick Action"].Disable();
        }

        void Start()
        {
            m_FPSController = Instantiate(m_FPSController);
            m_FirstPersonController = m_FPSController.GetComponent<FirstPersonController>();
            m_FirstPersonController.Init(m_LeftJoystick.GetComponentInChildren<JoystickControl>(true));
            UIStateManager.walkStateChanged += OnWalkStateDataChanged;
            m_FreeFlyCamera = m_MainCamera.GetComponent<FreeFlyCamera>();
        }

        void OnSwitchMode(InputAction.CallbackContext callbackContext)
        {
            if (m_CachedWalkStateData != null)
            {
                var navigationState = UIStateManager.current.stateData.navigationState;
                navigationState.navigationMode = !m_CachedWalkStateData.Value.walkEnabled ? NavigationMode.Walk : NavigationMode.Fly;

                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableWalk,
                    !m_CachedWalkStateData.Value.walkEnabled));
            }
        }

        void Update()
        {
            if (m_IsPlacementMode)
            {
                m_OrbitModeUIController.OnGetTeleportTarget(m_IsPlacementMode);
            }
#if UNITY_ANDROID || UNITY_IOS
            if (!m_LeftJoystick.activeSelf)
            {
                m_OnScreenStick.enabled = false;
            }
#endif
        }

        public void ResetCamPos(Vector3 offset)
        {
            if (m_IsResetingPosition)
            {
                return;
            }

            m_IsResetingPosition = true;
            m_Offset = offset;
            ActivateFlyMode(false);
            m_FreeFlyCamera.ResetDesiredPosition();
            ActivateWalkMode(true);
            m_InputActionMap.Enable();
        }

        void ActivateFlyMode(bool moveCamera = true)
        {
            m_FPSController.SetActive(false);
            m_MainCamera.gameObject.SetActive(true);
            m_MainCamera.clearFlags = CameraClearFlags.Skybox;

            if (moveCamera)
            {
                m_FreeFlyCamera.SetMovePosition(m_FPSController.transform.position, m_FPSController.transform.rotation);
            }

            m_InputActionMap.Disable();
            m_IsInit = false;
        }

        void ActivateWalkMode(bool isReset = false)
        {
            if (m_HasFinishPlacement || isReset)
            {
                m_FPSController.SetActive(true);
                m_FreeFlyCamera.SetMovePosition(m_MainCamera.transform.position, m_MainCamera.transform.rotation);

                m_HasFinishPlacement = false;
                StartCoroutine(ChangePos());
                m_MainCamera.gameObject.SetActive(false);
            }

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

            if (!m_IsResetingPosition)
            {
                // When joystick is hidden, first touch and drag is not working. So this code is needed.
                foreach (var image in m_JoystickImages)
                {
                    image.enabled = false;
                }
                m_LeftJoystick.SetActive(true);
                yield return null;
                m_LeftJoystick.SetActive(false);
                foreach (var image in m_JoystickImages)
                {
                    image.enabled = true;
                }
            }

            m_CharacterController.transform.position = m_Destination + m_Offset;
            SetCameraRotation(Quaternion.Euler(m_RotationOffset));
            yield return null;
            m_CharacterController.enabled = true;
            m_HasFinishPlacement = true;
            m_IsResetingPosition = false;
            m_InputActionAsset["Place Joystick Action"].Enable();
            m_FirstPersonController.SetInitialHeight();
        }
    }
}
