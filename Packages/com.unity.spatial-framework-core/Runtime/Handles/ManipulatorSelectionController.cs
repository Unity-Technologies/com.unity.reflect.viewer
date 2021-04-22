using System;
using System.Collections.Generic;
using Unity.XRTools.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Applies the transformations from a manipulator to a fixed list of objects
    /// </summary>
    public class ManipulatorSelectionController : MonoBehaviour
    {
        [SerializeField, Tooltip("Manipulators that affect this selection. They all must implements IManipulator interface")]
        Object[] m_Manipulators = new Object[1];

        [SerializeField, Tooltip("Objects that will be transformed by the manipulator")]
        Transform[] m_SelectionTransforms = new Transform[1];

        [SerializeField, Tooltip("Pivot position and rotation settings")]
        ManipulatorPivotSettings m_ManipulatorPivotSettings;

        [SerializeField, Tooltip("The speed that the manipulator will move when being dragged")]
        float m_TranslateEaseSpeed = 16f;

        [SerializeField, Tooltip("The speed that the manipulator will rotate when being dragged")]
        float m_RotateEaseSpeed = 16f;

        List<Tuple<IManipulator, ManipulatorSelectionState>> m_States = new List<Tuple<IManipulator, ManipulatorSelectionState>>();

        void OnValidate()
        {
            // Validate that the assigned components implement the interfaces that are required
            for (var i = 0; i < m_Manipulators.Length; i++)
            {
                m_Manipulators[i] = UnityObjectUtils.ConvertUnityObjectToType<IManipulator>(m_Manipulators[i]) as Object;
            }
        }

        void Start()
        {
            if (m_Manipulators.Length > 0 && m_SelectionTransforms.Length > 0)
            {
                foreach (var m in m_Manipulators)
                {
                    var manipulator = (IManipulator)m;
                    var state = new ManipulatorSelectionState(manipulator);
                    var activeTransform = m_SelectionTransforms[0];
                    state.Reset(m_ManipulatorPivotSettings, m_SelectionTransforms, activeTransform);
                    m_States.Add(new Tuple<IManipulator, ManipulatorSelectionState>(manipulator, state));
                }
            }
            else
            {
                Debug.LogError("Manipulator Selection Controller is not configured properly. It must have at least one manipulator and one selection transform.", this);
            }
        }

        void Update()
        {
            var deltaTime = Time.unscaledDeltaTime;
            var translateEase = 1f - Mathf.Exp(-m_TranslateEaseSpeed * deltaTime);
            var rotateEase = 1f - Mathf.Exp(-m_RotateEaseSpeed * deltaTime);
            var activeTransform = m_SelectionTransforms[0];

            foreach (var manipulatorState in m_States)
            {
                // Update manipulators with easing
                var state = manipulatorState.Item2;
                state.UpdateManipulatorTransform(m_ManipulatorPivotSettings, translateEase, rotateEase);

                // If manipulator is being dragged, it should apply transformation to the selection objects
                // Other manipulators should just reset their state to follow the selection
                var manipulator = manipulatorState.Item1;
                if (manipulator.dragging)
                    state.UpdateSelection(m_ManipulatorPivotSettings, m_SelectionTransforms);
                else
                    state.Reset(m_ManipulatorPivotSettings, m_SelectionTransforms, activeTransform);
            }
        }
    }
}
