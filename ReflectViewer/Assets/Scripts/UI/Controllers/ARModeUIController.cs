using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.MARS;
#if UNITY_EDITOR
using UnityEditor.MARS.Simulation;
using Unity.MARS.Simulation;
using Unity.MARS.Providers.Synthetic;
#endif
using Unity.MARS.Providers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
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
    public class ARModeUIController : MonoBehaviour, IExposedPropertyTable, IARModeUIController
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
        MARS.MARSCamera m_MARSCamera;
#if URP_AVAILABLE
        UniversalAdditionalCameraData m_CameraData;
#endif
        float m_InitialScaleSize;
        float m_InitialModelScale;
        bool m_ScaleActionInProgress;
        Array m_ScaleValues;
        IARInstructionUI m_CurrentIARInstructionUI;

        IUISelector<ARPlacementStateData> m_ARPlacementStateDataSelector;
        IUISelector<SetModelScaleAction.ArchitectureScale> m_ModelScaleSelector;
        IUISelector<Transform> m_RootSelector;
        IUISelector<bool> m_ARScaleEnabledSelector;
        IUISelector<bool> m_ARRotateEnabledSelector;
        MARSSession m_MarsSession = null;
        IUISelector<bool> m_ToolBarEnabledSelector;
        SetARModeAction.ARMode m_ARmode = SetARModeAction.ARMode.None;

        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void Awake()
        {
            EnableMARSSession(true);
            foreach (var obj in InstructionUIList)
            {
                var instructionUI = obj as IARInstructionUI;
                instructionUI.Initialize(this);
            }

            m_DisposeOnDestroy.Add(m_ARScaleEnabledSelector = UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.scaleEnabled)));
            m_DisposeOnDestroy.Add(m_ARRotateEnabledSelector = UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.rotateEnabled)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(ARContext.current, nameof(IARModeDataProvider.arEnabled), OnAREnabledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetInstructionUIStateAction.InstructionUIState>(ARContext.current, nameof(IARModeDataProvider.instructionUIState), OnInstructionUIStateChanged));

            var arModeSelector = UISelectorFactory.createSelector<SetARModeAction.ARMode>(ARContext.current, nameof(IARModeDataProvider.arMode), OnARModeChanged);
            m_DisposeOnDestroy.Add(arModeSelector);
            m_DisposeOnDestroy.Add(m_ARPlacementStateDataSelector = UISelectorFactory.createSelector<ARPlacementStateData>(ARContext.current, nameof(IARPlacement<ARPlacementStateData>.placementStateData)));
            m_DisposeOnDestroy.Add(m_ModelScaleSelector = UISelectorFactory.createSelector<SetModelScaleAction.ArchitectureScale>(ARPlacementContext.current, nameof(IARPlacementDataProvider.modelScale)));
            m_DisposeOnDestroy.Add(m_RootSelector = UISelectorFactory.createSelector<Transform>(PipelineContext.current, nameof(IPipelineDataProvider.rootNode)));
            m_DisposeOnDestroy.Add(m_ToolBarEnabledSelector = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled), null));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IUIStateDataProvider.operationCancelled), OnOperationCancelledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged));

            m_MARSCamera = Camera.main.GetComponent<MARS.MARSCamera>();
#if URP_AVAILABLE
            m_CameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
#endif

            m_InputActionAsset["Placement Rotate Action"].performed += OnPlacementRotateAction;
            m_InputActionAsset["Placement Scale Action"].started += OnPlacementScaleActionStarted;
            m_InputActionAsset["Placement Scale Action"].performed += OnPlacementScaleAction;

            m_ScaleValues = Enum.GetValues(typeof(SetModelScaleAction.ArchitectureScale));

            // Force ARMode because the awake can be called on the next frame than UISelector
            OnARModeChanged(arModeSelector.GetValue());
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType newData)
        {
            m_InstructionUI.SetActive(newData == OpenDialogAction.DialogType.None);
        }

        void OnOperationCancelledChanged(bool newData)
        {
            if (m_ToolBarEnabledSelector.GetValue() && newData)
            {
                m_CurrentIARInstructionUI.Cancel();
            }
        }

        void OnPlacementScaleActionStarted(InputAction.CallbackContext context)
        {
            if (!m_ARScaleEnabledSelector.GetValue())
                return;

            PinchGestureInteraction interaction = context.interaction as PinchGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                PinchGesture pinchGesture = interaction?.currentGesture as PinchGesture;
                m_InitialScaleSize = pinchGesture.gap;
                m_InitialModelScale = (float) m_ModelScaleSelector.GetValue();
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
            if (OrphanUIController.isTouchBlockedByUI || m_ARRotateEnabledSelector.GetValue() != true || m_ScaleActionInProgress)
                return;

            var delta = context.ReadValue<Vector2>();

            var forward = m_MARSCamera.transform.forward;
            var worldToVerticalOrientedDevice = Quaternion.Inverse(Quaternion.LookRotation(forward, Vector3.up));
            var deviceToWorld = m_MARSCamera.transform.rotation;
            var rotatedDelta = worldToVerticalOrientedDevice * deviceToWorld * delta;

            var rotationAmount = -1.0f * (rotatedDelta.x / Screen.dpi) * m_RotationRateDegreesDrag;

            Dispatcher.Dispatch(SetModelRotationAction.From(new Vector3(0.0f, rotationAmount, 0.0f)));
        }

        void OnPlacementScaleAction(InputAction.CallbackContext context)
        {
            if (!m_ARScaleEnabledSelector.GetValue())
                return;

            PinchGestureInteraction interaction = context.interaction as PinchGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                PinchGesture pinchGesture = interaction?.currentGesture as PinchGesture;
                var ratio = m_InitialScaleSize / pinchGesture.gap; // inverted because scale size is 1/N
                var newScale = GetNearestEnumValue(m_InitialModelScale * ratio);
                Dispatcher.Dispatch(SetModelScaleAction.From(newScale));
            }
        }

        SetModelScaleAction.ArchitectureScale GetNearestEnumValue(float value)
        {
            SetModelScaleAction.ArchitectureScale returnValue = SetModelScaleAction.ArchitectureScale.OneToOne;
            float distance = float.MaxValue;
            foreach (var enumValue in m_ScaleValues)
            {
                var newValue = Math.Abs(value - Convert.ToSingle(enumValue));
                if (newValue < distance)
                {
                    returnValue = (SetModelScaleAction.ArchitectureScale)enumValue;
                    distance = newValue;
                }
            }

            return returnValue;
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

        void OnAREnabledChanged(bool newData)
        {
            if (m_MARSCamera != null)
            {
                if (newData)
                {
                    EnableARMode();
                }
                else
                {
                    DisableARMode();
                }
            }
        }

        void OnInstructionUIStateChanged(SetInstructionUIStateAction.InstructionUIState newData)
        {
            if (newData == SetInstructionUIStateAction.InstructionUIState.Init)
            {
                StartCoroutine(StartInstructionAR());
            }
        }

        void OnARModeChanged(SetARModeAction.ARMode newData)
        {
            if (m_ARmode == newData)
                return;

            m_ARmode = newData;
            if (newData == SetARModeAction.ARMode.None)
                return;

            m_CurrentIARInstructionUI = null;
            foreach (var obj in InstructionUIList)
            {
                var instructionUI = obj as IARInstructionUI;
                if (instructionUI.arMode == newData)
                {
                    m_CurrentIARInstructionUI = instructionUI;
                    break;
                }
            }

            if (m_CurrentIARInstructionUI == null)
            {
                Debug.LogError("AR Instruction UI is null");
                return;
            }

            m_CurrentIARInstructionUI.Restart();
        }

        void EnableMARSSession(bool activate)
        {
            Debug.Log($"Enable MARSSession: {activate}");

            if (m_MarsSession == null)
            {
                m_MarsSession = FindObjectOfType<MARSSession>();
                if (m_MarsSession == null)
                {
                    return;
                }
            }

            m_MarsSession.enabled = activate;
        }

        void EnableARMode()
        {
            var m_actionMap = m_InputActionAsset.FindActionMap("AR", true);
            m_actionMap.Enable();

            // enable SimulationView
            EnableSimulationViewInEditor(true);

            // disable RootNode
            m_RootSelector.GetValue().gameObject.SetActive(false);
            m_ARPlacementStateDataSelector.GetValue().boundingBoxRootNode.gameObject.SetActive(false);
#if URP_AVAILABLE

            // Set AR Renderer
            m_CameraData.SetRenderer((int)UniversalRenderer.ARRenderer);
#endif
            m_MARSCamera.enabled = true;

            // un-Pause MARSSession
            UIStateManager.current.StartDetectingPlanes();
            UIStateManager.current.StartDetectingPoints();
            UIStateManager.current.ResumeSession();

            // Enable all the culling mask layer to allow Mars to be able to render the simulation view
            m_MARSCamera.GetComponent<Camera>().cullingMask = -1;

            InitSimulationCameraPosition();
        }

        void DisableARMode()
        {
            Camera.main.clearFlags = CameraClearFlags.Skybox;
            var m_actionMap = m_InputActionAsset.FindActionMap("AR", true);
            m_actionMap.Disable();

            // disable SimulationView
            EnableSimulationViewInEditor(false);

            m_MARSCamera.enabled = false;
#if URP_AVAILABLE

            // return to default Renderer
            m_CameraData.SetRenderer((int)UniversalRenderer.DefaultForwardRenderer);
#endif

            // enable RootNode
            m_RootSelector.GetValue().gameObject.SetActive(true);

            if (UIStateManager.current.SessionReady())
            {
                // Pause MARSSession
                UIStateManager.current.StopDetectingPlanes();
                UIStateManager.current.StopDetectingPoints();
                UIStateManager.current.PauseSession();
            }

            // Disable the gizmo layer when we are not in AR.
            m_MARSCamera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("Gizmo"));

            // Hack: internal camera state is not refreshing the farPlane update.
            // Toggling this field seems to reset the internal state and update appropriately
            m_MARSCamera.GetComponent<Camera>().orthographic = true;
            m_MARSCamera.GetComponent<Camera>().orthographic = false;

            m_CurrentIARInstructionUI?.Reset();
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

        IEnumerator StartInstructionAR()
        {
            yield return new WaitForSeconds(0);

            Dispatcher.Dispatch(SetInstructionUIStateAction.From(SetInstructionUIStateAction.InstructionUIState.Started));
        }

        void OnDestroy()
        {
            Dispatcher.Dispatch(SetARModeAction.From(SetARModeAction.ARMode.None));

            StopAllCoroutines();

            if (m_MARSCamera != null)
                m_MARSCamera.enabled = false;

            if (UIStateManager.current != null && m_ARPlacementStateDataSelector.GetValue().placementRulesGameObject != null)
            {
                DestroyImmediate(m_ARPlacementStateDataSelector.GetValue().placementRulesGameObject);
            }

            UIStateManager.current.ResetSession();
            UIStateManager.current.PauseSession();

            // remove input system hooks
            m_InputActionAsset["Placement Rotate Action"].performed -= OnPlacementRotateAction;
            m_InputActionAsset["Placement Scale Action"].started -= OnPlacementScaleActionStarted;
            m_InputActionAsset["Placement Scale Action"].performed -= OnPlacementScaleAction;

            m_ARPlacementStateDataSelector = null;
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
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

        public void ActivePlacementRules(SetModelFloorAction.PlacementRule rule)
        {
            if (m_ARPlacementStateDataSelector.GetValue().placementRulesGameObject != null && rule == m_ARPlacementStateDataSelector.GetValue().placementRule)
            {
                if (rule != SetModelFloorAction.PlacementRule.None)
                {
                    m_ARPlacementStateDataSelector.GetValue().placementRulesGameObject.SetActive(true);
                }
            }
        }

        // TODO: add OnValidate to cleanup stale ExposedReferences
        // TODO: user serialized dictionary
    }
}
