using System;
using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Module that creates manipulators to translate, rotate, and scale objects
    /// </summary>
    [ScriptableSettingsPath("Assets/SpatialFramework/Settings")]
    public class ManipulatorModule : ScriptableSettings<ManipulatorModule>, IUsesFunctionalityInjection, IModuleBehaviorCallbacks,
        IProvidesTransformManipulators
    {
        [Serializable]
        class ManipulatorGroup
        {
            [Tooltip("The name for the group of manipulators")]
            public string name;

            [Tooltip("The indices in the manipulator prefabs list that should be active when this group is active.")]
            public List<int> manipulatorIndices = new List<int>();
        }

#pragma warning disable 649
        [SerializeField, Tooltip("All the prefabs for manipulators that are available to the module.")]
        List<GameObject> m_ManipulatorPrefabs = new List<GameObject>();

        [SerializeField, Tooltip("Defines named groups of manipulators that the module can switch between.")]
        List<ManipulatorGroup> m_ManipulatorGroups = new List<ManipulatorGroup>();

        [SerializeField, Tooltip("Pivot position and rotation settings")]
        ManipulatorPivotSettings m_ManipulatorPivotSettings;

        [SerializeField, Tooltip("The speed that the manipulator will move when being dragged")]
        float m_TranslateEaseSpeed = 10f;

        [SerializeField, Tooltip("The speed that the manipulator will rotate when being dragged")]
        float m_RotateEaseSpeed = 8f;
#pragma warning restore 649

        List<IManipulator> m_AllManipulators = new List<IManipulator>();
        List<IManipulator> m_ActiveGroupManipulators = new List<IManipulator>();
        int m_CurrentManipulatorGroupIndex;

        readonly HashSet<IUsesTransformManipulators> m_ManipulatorVisibleRequests = new HashSet<IUsesTransformManipulators>();
        readonly Dictionary<IManipulator, ManipulatorSelectionState> m_ManipulatorStates = new Dictionary<IManipulator, ManipulatorSelectionState>();
        Transform m_ActiveTransform;
        Transform[] m_SelectionTransforms;

        bool m_ManipulatorVisible;

        /// <summary>
        /// The current pivot mode
        /// </summary>
        public PivotMode PivotMode => m_ManipulatorPivotSettings.pivotMode;

        /// <summary>
        /// The current pivot rotation mode
        /// </summary>
        public PivotRotation PivotRotation => m_ManipulatorPivotSettings.pivotRotation;

        IProvidesFunctionalityInjection IFunctionalitySubscriber<IProvidesFunctionalityInjection>.provider { get; set; }

        /// <summary>
        /// Switch the pivot mode between center and pivot modes
        /// </summary>
        public void TogglePivotMode()
        {
            m_ManipulatorPivotSettings.pivotMode = m_ManipulatorPivotSettings.pivotMode == PivotMode.Pivot ? PivotMode.Center : PivotMode.Pivot;
            UpdateAllManipulators();
        }

        /// <summary>
        /// Switch the pivot rotation between global and local modes
        /// </summary>
        public void TogglePivotRotation()
        {
            m_ManipulatorPivotSettings.pivotRotation = m_ManipulatorPivotSettings.pivotRotation == PivotRotation.Global ? PivotRotation.Local : PivotRotation.Global;
            UpdateAllManipulators();
        }

        void IModule.LoadModule()
        {
            m_SelectionTransforms = null;
        }

        void IProvidesTransformManipulators.SetManipulatorSelection(Transform[] selectionTransforms, Transform activeTransform)
        {
            m_SelectionTransforms = selectionTransforms;
            m_ActiveTransform = activeTransform;
            if (m_ActiveTransform == null && !SelectionIsEmpty())
                m_ActiveTransform = m_SelectionTransforms[0];

            UpdateAllManipulators(true);
        }

        bool SelectionIsEmpty() => m_SelectionTransforms == null || m_SelectionTransforms.Length == 0;

        [ContextMenu("Next manipulator group.")]
        void IProvidesTransformManipulators.NextManipulatorGroup()
        {
            var count = m_ManipulatorGroups.Count;
            if (count == 0)
                return;

            var index = (m_CurrentManipulatorGroupIndex + 1) % count;
            SetActiveGroupIndex(index);
        }

        void IProvidesTransformManipulators.SetManipulatorGroup(string groupName)
        {
            var groupIndex = m_ManipulatorGroups.FindIndex(x => x.name.Equals(groupName, StringComparison.OrdinalIgnoreCase));
            SetActiveGroupIndex(groupIndex);
        }

        void SetActiveGroupIndex(int groupIndex)
        {
            foreach (var manipulator in m_AllManipulators)
            {
                manipulator.gameObject.SetActive(false);
            }

            m_ActiveGroupManipulators.Clear();

            // If manipulators are not visible yet, the list of all manipulators will be empty
            // Still keep track of which group should be active when the manipulators are created.
            m_CurrentManipulatorGroupIndex = groupIndex;
            if (groupIndex < 0 || m_AllManipulators.Count == 0 || m_ManipulatorGroups.Count == 0)
                return;

            var group = m_ManipulatorGroups[m_CurrentManipulatorGroupIndex];
            foreach (var manipulatorIndex in group.manipulatorIndices)
            {
                m_ActiveGroupManipulators.Add(m_AllManipulators[manipulatorIndex]);
            }
        }

        void IModule.UnloadModule()
        {
            DestroyManipulators();
        }

        void IProvidesTransformManipulators.SetManipulatorsVisible(IUsesTransformManipulators setter, bool visible)
        {
            if (visible)
                m_ManipulatorVisibleRequests.Add(setter);
            else
                m_ManipulatorVisibleRequests.Remove(setter);

            var manipulatorsNeeded = m_ManipulatorVisibleRequests.Count > 0;

            if (!m_ManipulatorVisible && manipulatorsNeeded)
            {
                CreateManipulators();
            }
            else if (m_ManipulatorVisible && !manipulatorsNeeded)
            {
                DestroyManipulators();
            }

            UpdateAllManipulators();
        }

        /// <summary>
        /// Checks whether any manipulator in the currently active group is being dragged
        /// </summary>
        /// <returns>True if any active manipulator is currently being dragged</returns>
        public bool GetManipulatorDragState()
        {
            foreach (var manipulator in m_ActiveGroupManipulators)
                if (manipulator.dragging)
                    return true;

            return false;
        }

        void UpdateAllManipulators(bool forceReset = false)
        {
            var deltaTime = Time.unscaledDeltaTime;
            var translateEase = 1f - Mathf.Exp(-m_TranslateEaseSpeed * deltaTime);
            var rotateEase = 1f - Mathf.Exp(-m_RotateEaseSpeed * deltaTime);
            var interactionInProgress = GetManipulatorDragState();

            foreach (var manipulator in m_ActiveGroupManipulators)
            {
                var manipulatorGameObject = manipulator.gameObject;
                if (!m_ManipulatorVisible || SelectionIsEmpty() ||
                    (interactionInProgress && !manipulator.dragging)) // Check if another manipulator is being interacted with, hide this one
                {
                    manipulatorGameObject.SetActive(false);
                    continue;
                }

                manipulatorGameObject.SetActive(true);
                var manipulatorState = m_ManipulatorStates[manipulator];
                if (!manipulator.dragging || forceReset)
                {
                    manipulatorState.Reset(m_ManipulatorPivotSettings, m_SelectionTransforms, m_ActiveTransform); // Then cache the local offsets of the selection from the pivot
                }

                manipulatorState.UpdateManipulatorTransform(m_ManipulatorPivotSettings, translateEase, rotateEase);
                manipulatorState.UpdateSelection(m_ManipulatorPivotSettings, m_SelectionTransforms);
            }
        }

        void CreateManipulators()
        {
            foreach (var prefab in m_ManipulatorPrefabs)
            {
                CreateManipulator(prefab);
            }

            m_ManipulatorVisible = true;
            SetActiveGroupIndex(m_CurrentManipulatorGroupIndex);
        }

        void DestroyManipulators()
        {
            foreach (var manipulator in m_AllManipulators)
            {
                if (manipulator.gameObject)
                    UnityObjectUtils.Destroy(manipulator.gameObject);
            }

            m_ManipulatorVisible = false;
            m_AllManipulators.Clear();
            m_ManipulatorStates.Clear();
            m_ActiveGroupManipulators.Clear();
        }

        void CreateManipulator(GameObject prefab)
        {
            prefab.SetActive(false);
            var parent = ModuleLoaderCore.instance.GetModuleParent().transform;
            var go = Instantiate(prefab, parent);
            foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>())
            {
                this.InjectFunctionalitySingle(mb);
            }

            var manipulator = go.GetComponent<IManipulator>();
            var state = new ManipulatorSelectionState(manipulator);

            manipulator.dragStarted += OnDragStarted;

            m_ManipulatorStates.Add(manipulator, state);
            m_AllManipulators.Add(manipulator);
        }

        static void OnDragStarted()
        {
#if UNITY_EDITOR
            Undo.IncrementCurrentGroup();
#endif
        }

        void IModuleBehaviorCallbacks.OnBehaviorAwake() { }

        void IModuleBehaviorCallbacks.OnBehaviorEnable() { }

        void IModuleBehaviorCallbacks.OnBehaviorStart() { }

        void IModuleBehaviorCallbacks.OnBehaviorUpdate()
        {
            UpdateAllManipulators();
        }

        void IModuleBehaviorCallbacks.OnBehaviorDisable() { }

        void IModuleBehaviorCallbacks.OnBehaviorDestroy() { }

        void IFunctionalityProvider.LoadProvider() { }

        void IFunctionalityProvider.ConnectSubscriber(object obj)
        {
            if (obj is IFunctionalitySubscriber<IProvidesTransformManipulators> manipulatorVisibilitySubscriber)
                manipulatorVisibilitySubscriber.provider = this;
        }

        void IFunctionalityProvider.UnloadProvider() { }
    }
}
