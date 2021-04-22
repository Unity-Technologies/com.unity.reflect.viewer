using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
#if UNITY_EDITOR
using UnityEditor.MARS.Simulation;
using Unity.MARS.Simulation;
using Unity.MARS.Providers.Synthetic;
#endif
using Unity.MARS.Providers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Reflect.Viewer.Input;

using Object = UnityEngine.Object;

#if URP_AVAILABLE
    using UnityEngine.Rendering.Universal;
#endif

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible for managing the selection of the type of orbit tool
    /// </summary>
    public class ARModeUIController : MonoBehaviour, IExposedPropertyTable
    {
#pragma warning disable CS0649
        [SerializeField]
        InputActionAsset m_InputActionAsset;
        [SerializeField, Tooltip("Rate at which to rotate object with a drag.")]
        float m_RotationRateDegreesDrag = 100.0f;
        [SerializeField]
        public ScriptableObject[] InstructionUIList;
        [SerializeField]
        List<PropertyName> m_PropertyNameList;
        [SerializeField]
        List<Object> m_ReferenceList;

        [SerializeField]
        GameObject m_InstructionUI;

        [SerializeField]
        float m_DefaultSimulationCameraHeight = 1.6f;
#pragma warning restore CS0649

        bool m_ToolbarsEnabled;
        InstructionUIState? m_CachedInstructionUI;
        bool? m_CachedOperationCancelled;
        bool? m_CachedAREnabled;

        bool m_CachedScaleEnabled;
        bool m_CachedRotateEnabled;

        MARS.MARSCamera m_MARSCamera;
#if URP_AVAILABLE
        UniversalAdditionalCameraData m_CameraData;
#endif
        float m_InitialScaleSize;
        float m_InitialNearClippingPlane;
        float m_InitialFarClippingPlane;
        ArchitectureScale? m_CachedModelScale;
        bool m_ScaleActionInProgress;
        Array m_ScaleValues;
        IInstructionUI m_CurrentInstructionUI;
        ARMode? m_ArMode;
        DialogType m_CurrentActiveDialog = DialogType.None;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.arStateChanged += OnARStateDataChanged;

            m_MARSCamera = Camera.main.GetComponent<MARS.MARSCamera>();
#if URP_AVAILABLE
            m_CameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
#endif

            m_InputActionAsset["Placement Rotate Action"].performed += OnPlacementRotateAction;
            m_InputActionAsset["Placement Scale Action"].started += OnPlacementScaleActionStarted;
            m_InputActionAsset["Placement Scale Action"].performed += OnPlacementScaleAction;

            m_ScaleValues = Enum.GetValues(typeof(ArchitectureScale));

            foreach (var obj in InstructionUIList)
            {
                var instructionUI = obj as IInstructionUI;
                instructionUI.Initialize(this);
            }

            m_InitialNearClippingPlane = m_MARSCamera.GetComponent<Camera>().nearClipPlane;
            m_InitialFarClippingPlane = m_MARSCamera.GetComponent<Camera>().farClipPlane;
        }

        void OnPlacementScaleActionStarted(InputAction.CallbackContext context)
        {
            if (!m_CachedScaleEnabled)
                return;

            PinchGestureInteraction interaction = context.interaction as PinchGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                PinchGesture pinchGesture = interaction?.currentGesture as PinchGesture;
                m_InitialScaleSize = pinchGesture.gap;
                m_CachedModelScale = UIStateManager.current.stateData.modelScale;
                m_ScaleActionInProgress = true;
                pinchGesture.onFinished += OnPlacementScaleActionFinished;
            }
        }

        void OnPlacementScaleActionFinished(PinchGesture context)
        {
            m_ScaleActionInProgress = false;
        }

        void OnPlacementRotateAction(InputAction.CallbackContext context)
        {
            if (OrphanUIController.isTouchBlockedByUI || m_CachedRotateEnabled != true || m_ScaleActionInProgress)
                return;

            var delta = context.ReadValue<Vector2>();

            var forward = m_MARSCamera.transform.forward;
            var worldToVerticalOrientedDevice = Quaternion.Inverse(Quaternion.LookRotation(forward, Vector3.up));
            var deviceToWorld = m_MARSCamera.transform.rotation;
            var rotatedDelta = worldToVerticalOrientedDevice * deviceToWorld * delta;

            var rotationAmount = -1.0f * (rotatedDelta.x / Screen.dpi) * m_RotationRateDegreesDrag;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelRotation, new Vector3(0.0f, rotationAmount, 0.0f)));
        }

        void OnPlacementScaleAction(InputAction.CallbackContext context)
        {
            if (!m_CachedScaleEnabled)
                return;

            PinchGestureInteraction interaction = context.interaction as PinchGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                PinchGesture pinchGesture = interaction?.currentGesture as PinchGesture;
                var ratio = m_InitialScaleSize / pinchGesture.gap; // inverted because scale size is 1/N
                var newScale = GetNearestEnumValue((float)m_CachedModelScale * ratio);
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, newScale));
            }
        }

        ArchitectureScale GetNearestEnumValue(float value)
        {
            ArchitectureScale returnValue = ArchitectureScale.OneToOne;
            float distance = float.MaxValue;
            foreach (var enumValue in m_ScaleValues)
            {
                var newValue =  Math.Abs(value - Convert.ToSingle(enumValue));
                if (newValue < distance)
                {
                    returnValue = (ArchitectureScale)enumValue;
                    distance = newValue;
                }
            }
            return returnValue;
        }

        void Start()
        {
            // since this starts on async scene load, needs to be up to date
            OnStateDataChanged(UIStateManager.current.stateData);
            OnARStateDataChanged(UIStateManager.current.arStateData);
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_ToolbarsEnabled != stateData.toolbarsEnabled)
            {
                m_ToolbarsEnabled = stateData.toolbarsEnabled;
            }

            if (m_CachedOperationCancelled != stateData.operationCancelled && m_ToolbarsEnabled)
            {
                m_CachedOperationCancelled = stateData.operationCancelled;
                if (m_CachedOperationCancelled == true)
                {
                    m_CurrentInstructionUI.Cancel();
                }
            }

            if (m_CurrentActiveDialog != stateData.activeDialog)
            {
                m_CurrentActiveDialog = stateData.activeDialog;
                m_InstructionUI.SetActive(m_CurrentActiveDialog == DialogType.None);
            }
        }

        void EnableSimulationViewInEditor(bool enable)
        {
#if UNITY_EDITOR
            SimulationSettings.instance.ShowSimulatedEnvironment = enable;
            SimulationSettings.instance.ShowSimulatedData = enable;

            if (enable)
            {
                MARSEnvironmentManager.instance.EnvironmentChanged += InitSimulationCameraPosition;
            }
            else
            {
                MARSEnvironmentManager.instance.EnvironmentChanged -= InitSimulationCameraPosition;
            }
#endif
        }

        void OnARStateDataChanged(UIARStateData arStateData)
        {
            if (m_CachedAREnabled != arStateData.arEnabled)
            {
                if (arStateData.arEnabled)
                {
                    EnableARMode();
                }
                else
                {
                    DisableARMode();
                }
                m_CachedAREnabled = arStateData.arEnabled;
            }

            if (m_ArMode != arStateData.arMode)
            {
                m_ArMode = arStateData.arMode;
                if (m_ArMode == null)
                    return;

                m_CurrentInstructionUI = null;
                foreach (var obj in InstructionUIList)
                {
                    var instructionUI = obj as IInstructionUI;
                    if (instructionUI.arMode == arStateData.arMode)
                    {
                        m_CurrentInstructionUI = instructionUI;
                        break;
                    }
                }

                if (m_CurrentInstructionUI == null)
                {
                    Debug.LogError("AR Instruction UI is null");
                    return;
                }
                m_CurrentInstructionUI.Restart();
            }

            if (m_CachedInstructionUI != arStateData.instructionUIState)
            {
                m_CachedInstructionUI = arStateData.instructionUIState;
                if (m_CachedInstructionUI == InstructionUIState.Init)
                {
                    StartCoroutine(StartInstrucitonAR());
                }
            }

            m_CachedScaleEnabled = arStateData.arToolStateData.scaleEnabled;
            m_CachedRotateEnabled = arStateData.arToolStateData.rotateEnabled;
        }

        void EnableARMode()
        {
            var m_actionMap = m_InputActionAsset.FindActionMap("AR", true);
            m_actionMap.Enable();

            // enable SimulationView
            EnableSimulationViewInEditor(true);

            // disable RootNode
            UIStateManager.current.m_RootNode.SetActive(false);
            UIStateManager.current.m_BoundingBoxRootNode.SetActive(false);
#if URP_AVAILABLE
            // Set AR Renderer
            m_CameraData.SetRenderer((int) UniversalRenderer.ARRenderer);
#endif
            m_MARSCamera.enabled = true;

            // un-Pause MARSSession
            UIStateManager.current.StartDetectingPlanes();
            UIStateManager.current.StartDetectingPoints();
            UIStateManager.current.ResumeSession();

            // Enable all the culling mask layer to allow Mars to be able to render the simulation view
            m_MARSCamera.GetComponent<Camera>().cullingMask = -1;

            InitSimulationCameraPosition();

            ChangeClippingPlane(0.01f, m_InitialFarClippingPlane * 10f);
        }

        void ChangeClippingPlane(float near, float far)
        {
            Camera marsCamera = m_MARSCamera.GetComponent<Camera>();
            marsCamera.nearClipPlane = near;
            marsCamera.farClipPlane = far;
        }


        void DisableARMode()
        {
            var m_actionMap = m_InputActionAsset.FindActionMap("AR", true);
            m_actionMap.Disable();

            // disable SimulationView
            EnableSimulationViewInEditor(false);

            // enable RootNode
            m_MARSCamera.enabled = false;
#if URP_AVAILABLE
            // return to default Renderer
            m_CameraData.SetRenderer((int) UniversalRenderer.DefaultForwardRenderer);
#endif

            UIStateManager.current.m_RootNode.SetActive(true);

            if (UIStateManager.current.SessionReady())
            {
                // Pause MARSSession
                UIStateManager.current.StopDetectingPlanes();
                UIStateManager.current.StopDetectingPoints();
                UIStateManager.current.PauseSession();
            }

            // Disable the gizmo layer when we are not in AR.
            m_MARSCamera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("Gizmo"));

            ChangeClippingPlane(m_InitialNearClippingPlane, m_InitialFarClippingPlane);
        }

        void InitSimulationCameraPosition()
        {
#if UNITY_EDITOR
            // Set camera height position
            Vector3 camPos = Camera.main.transform.position;
            camPos.y = m_DefaultSimulationCameraHeight;
            Camera.main.transform.position = camPos;

            // Set environement Prefab to keep the same hight the next time the scene is launch
            MARSEnvironmentSettings marsEnvironmentSettings = SimulationSettings.instance.EnvironmentPrefab.GetComponent<MARSEnvironmentSettings>();
            Pose DefaultCameraWorldPose = MARSEnvironmentManager.instance.DeviceStartingPose;
            DefaultCameraWorldPose.position.y = m_DefaultSimulationCameraHeight;

            // Set MARS provider which drive the initial camera position.
            marsEnvironmentSettings.SetSimulationStartingPose(DefaultCameraWorldPose, true);
            var simCameraPoseProvider = FindObjectOfType<SimulatedCameraPoseProvider>();
            if (simCameraPoseProvider != null)
                simCameraPoseProvider.transform.localPosition = DefaultCameraWorldPose.position;
#endif
        }

        IEnumerator StartInstrucitonAR()
        {
            yield return new WaitForSeconds(0);

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUIState, InstructionUIState.Started));
        }

        void OnDestroy()
        {
            StopAllCoroutines();

            if (m_MARSCamera != null)
                m_MARSCamera.enabled = false;

            if (UIStateManager.current.m_PlacementRules != null)
            {
                DestroyImmediate(UIStateManager.current.m_PlacementRules);
                UIStateManager.current.m_PlacementRules = null;
            }

            UIStateManager.current.ResetSession();
            UIStateManager.current.PauseSession();

            m_CachedInstructionUI = null;
            m_CachedOperationCancelled = null;

            // remove input system hooks
            m_InputActionAsset["Placement Rotate Action"].performed -= OnPlacementRotateAction;
            m_InputActionAsset["Placement Scale Action"].started -= OnPlacementScaleActionStarted;
            m_InputActionAsset["Placement Scale Action"].performed -= OnPlacementScaleAction;
            UIStateManager.stateChanged -= OnStateDataChanged;
            UIStateManager.arStateChanged -= OnARStateDataChanged;
        }

        public void SetReferenceValue(PropertyName id, Object value)
        {
            int index = m_PropertyNameList.IndexOf(id);
            if (index != -1)
            {
                m_ReferenceList[index] = value;
            }
            else
            {
                m_PropertyNameList.Add(id);
                m_ReferenceList.Add(value);
            }
        }

        public Object GetReferenceValue(PropertyName id, out bool idValid)
        {
            int index = m_PropertyNameList.IndexOf(id);
            if (index != -1)
            {
                idValid = true;
                return m_ReferenceList[index];
            }
            idValid = false;
            return null;
        }

        public void ClearReferenceValue(PropertyName id)
        {
            int index = m_PropertyNameList.IndexOf(id);
            if (index != -1)
            {
                m_ReferenceList.RemoveAt(index);
                m_PropertyNameList.RemoveAt(index);
            }
        }

        // TODO: add OnValidate to cleanup stale ExposedReferences
        // TODO: user serialized dictionary
    }
}
