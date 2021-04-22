using System;
using System.Collections.Generic;
using Unity.SpatialFramework.Handles;
using Unity.SpatialFramework.Interaction;
using Unity.SpatialFramework.Providers;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.SpatialFramework
{
    /// <summary>
    /// Controls and sets up a basic Spatial Framework scene.
    /// </summary>
    public class SpatialFrameworkSceneController : MonoBehaviour, IUsesFunctionalityInjection, IUsesTransformManipulators, IUsesXRPointers
    {
        [SerializeField]
        Transform m_ActiveSelectionTransform;

        [SerializeField]
        Transform[] m_ManipulatorSelectionTransforms;

        [Serializable]
        internal class PointerSetupConfig
        {
            [Tooltip("XR device usage to find in the input system and create an XR pointer using.")]
            public string usage;

            [Tooltip("(Optional) An existing XR controller to use instead of instantiating a new one. It should have an ActionBasedController and XRRayInteractor")]
            public ActionBasedController xrController;

            [ReadOnly, Tooltip("(Read Only) True if this device has been registered as a UI Pointer.")]
            public bool registered;
        }

        [SerializeField, Tooltip("Setup the XR pointers by defining which device and optionally an existing controller")]
        List<PointerSetupConfig> m_DevicePointers = new List<PointerSetupConfig>();

        FunctionalityInjectionModule m_FIModule;

        readonly HashSet<Type> m_SubscriberTypes = new HashSet<Type>();
        readonly List<object> m_MonoBehaviourObjects = new List<object>();

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<MonoBehaviour> k_MonoBehaviours = new List<MonoBehaviour>();

        IProvidesTransformManipulators IFunctionalitySubscriber<IProvidesTransformManipulators>.provider { get; set; }
        IProvidesFunctionalityInjection IFunctionalitySubscriber<IProvidesFunctionalityInjection>.provider { get; set; }
        IProvidesXRPointers IFunctionalitySubscriber<IProvidesXRPointers>.provider { get; set; }

        void Reset()
        {
            ResetDefaultPointers();
        }

        [ContextMenu("Reset Default Pointers")]
        void ResetDefaultPointers()
        {
            m_DevicePointers = new List<PointerSetupConfig>
            {
                new PointerSetupConfig { usage = "RightHand", xrController = null },
                new PointerSetupConfig { usage = "LeftHand", xrController = null }
            };
        }

        void Awake()
        {
            SetUpViewerScaleProvider();
            CollectAllMonoBehaviors();
            this.EnsureFunctionalityInjected();
            this.InjectFunctionality(m_MonoBehaviourObjects);
        }

        void SetUpViewerScaleProvider()
        {
            var viewerScaleProvider = GetComponent<ViewerScaleProvider>();
            if (viewerScaleProvider == null)
                return;

            var moduleLoaderCore = ModuleLoaderCore.instance;
            var fiModule = moduleLoaderCore.GetModule<FunctionalityInjectionModule>();
            var activeIsland = fiModule.activeIsland;
            activeIsland.AddProvider(typeof(ViewerScaleProvider), viewerScaleProvider);
            moduleLoaderCore.InjectFunctionalityInModules(activeIsland);
        }

        void Start()
        {
            ShowManipulator();
            this.SetManipulatorSelection(m_ManipulatorSelectionTransforms, m_ActiveSelectionTransform);
        }

        void Update()
        {
            foreach (var controller in m_DevicePointers)
            {
                if (!controller.registered)
                {
                    var inputDevice = InputSystem.GetDevice<UnityEngine.InputSystem.XR.XRController>(controller.usage);

                    if (inputDevice != null && inputDevice.added)
                    {
                        var pointer = this.CreateXRPointer(inputDevice, controller.xrController.transform.parent, null, source => true, controller.xrController.gameObject);
                        controller.registered = pointer != null;
                    }
                }
            }
        }

        /// <summary>
        /// Show the active manipulator on the current selection of objects
        /// </summary>
        public void ShowManipulator()
        {
            this.SetManipulatorsVisible(this, true);
        }

        /// <summary>
        /// Hide the manipulator
        /// </summary>
        public void HideManipulator()
        {
            this.SetManipulatorsVisible(this, false);
        }

        /// <summary>
        /// Cycle the active manipulator group
        /// </summary>
        public void CycleManipulatorGroup()
        {
            this.NextManipulatorGroup();
        }

        /// <summary>
        /// Gets all MonoBehaviours in the scene, including ones on inactive objects
        /// </summary>
        void CollectAllMonoBehaviors()
        {
            var activeScene = SceneManager.GetActiveScene();
            foreach (var root in activeScene.GetRootGameObjects())
            {
                GetMonoBehaviorsRecursively(root);
            }
        }

        void GetMonoBehaviorsRecursively(GameObject root)
        {
            root.GetComponents(k_MonoBehaviours);
            foreach (var behaviour in k_MonoBehaviours)
            {
                if (behaviour == null || behaviour.hideFlags != HideFlags.None)
                    continue;

                m_MonoBehaviourObjects.Add(behaviour);
                if (behaviour is IFunctionalitySubscriber)
                    m_SubscriberTypes.Add(behaviour.GetType());
            }

            foreach (Transform child in root.transform)
            {
                GetMonoBehaviorsRecursively(child.gameObject);
            }
        }
    }
}
