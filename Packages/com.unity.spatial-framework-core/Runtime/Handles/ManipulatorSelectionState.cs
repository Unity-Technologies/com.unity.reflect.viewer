using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Stores the current state of a manipulator and the selection transforms that are being manipulated by it.
    /// </summary>
    public class ManipulatorSelectionState
    {
        Vector3 m_TargetPosition;
        Quaternion m_TargetRotation;
        Vector3 m_TargetScale;
        Quaternion m_StartRotation;
        Quaternion m_PositionOffsetRotation;
        Vector3 m_PositionOffsetScale;

        readonly Dictionary<Transform, Vector3> m_PositionOffsets = new Dictionary<Transform, Vector3>();
        readonly Dictionary<Transform, Quaternion> m_RotationOffsets = new Dictionary<Transform, Quaternion>();
        readonly Dictionary<Transform, Vector3> m_ScaleOffsets = new Dictionary<Transform, Vector3>();
        IManipulator m_Manipulator;
        Quaternion m_EasedRotation;
        Vector3 m_EasedPosition;

        /// <summary>
        /// Create new manipulator state data
        /// </summary>
        /// <param name="manipulator">The manipulator this state is referencing</param>
        public ManipulatorSelectionState(IManipulator manipulator)
        {
            m_Manipulator = manipulator;
            manipulator.translate += Translate;
            manipulator.rotate += Rotate;
            manipulator.scale += Scale;
        }

        /// <summary>
        /// Reset the manipulator state for a new set of selection transforms
        /// </summary>
        /// <param name="pivotSettings">The manipulator pivot settings</param>
        /// <param name="selectionTransforms">The array of selected transforms</param>
        /// <param name="activeTransform">The active selected transform</param>
        public void Reset(ManipulatorPivotSettings pivotSettings, Transform[] selectionTransforms, Transform activeTransform)
        {
            var pivotPosition = pivotSettings.GetPivotPosition(activeTransform, selectionTransforms);
            var pivotRotation = pivotSettings.pivotRotation == PivotRotation.Global || activeTransform == null ? Quaternion.identity : activeTransform.rotation;
            m_Manipulator.SetPose(pivotPosition, pivotRotation);
            m_TargetPosition = pivotPosition;
            m_TargetRotation = pivotRotation;
            m_EasedPosition = pivotPosition;
            m_EasedRotation = pivotRotation;
            m_StartRotation = m_TargetRotation;
            m_TargetScale = Vector3.one;
            m_PositionOffsetScale = Vector3.one;
            m_PositionOffsetRotation = Quaternion.identity;

            // Save the initial position, rotation, and scale relative to the manipulator
            m_PositionOffsets.Clear();
            m_RotationOffsets.Clear();
            m_ScaleOffsets.Clear();

            m_Manipulator.SetTargetTransforms(selectionTransforms, activeTransform);

            foreach (var transform in selectionTransforms)
            {
                m_PositionOffsets.Add(transform, transform.position - pivotPosition);
                m_ScaleOffsets.Add(transform, transform.localScale);
                m_RotationOffsets.Add(transform, Quaternion.Inverse(pivotRotation) * transform.rotation);
            }
        }

        /// <summary>
        /// Update the manipulator transform with a new target pose
        /// </summary>
        /// <param name="pivotSettings">The manipulator pivot settings</param>
        /// <param name="translateEase">How much the manipulator should move towards its target, where 0 is no movement and 1 is directly to the target </param>
        /// <param name="rotateEase">How much the manipulator should rotate towards its target, where 0 is no rotation and 1 is directly to the target</param>
        public void UpdateManipulatorTransform(ManipulatorPivotSettings pivotSettings, float translateEase = 1f, float rotateEase = 1f)
        {
            m_EasedPosition = Vector3.Lerp(m_EasedPosition, m_TargetPosition, translateEase);
            m_EasedRotation = Quaternion.Slerp(m_EasedRotation, m_TargetRotation, rotateEase);

            // Manipulator does not rotate when in global mode
            var newRotation = pivotSettings.pivotRotation == PivotRotation.Global ?
                Quaternion.identity
                : m_EasedRotation;

            m_Manipulator.SetPose(m_EasedPosition , newRotation);
        }

        /// <summary>
        /// Updates the selection transforms based on the current manipulator's state
        /// </summary>
        /// <param name="pivotSettings">The current pivot settings to use.</param>
        /// <param name="targetTransforms">The transforms to affect</param>
        public void UpdateSelection(ManipulatorPivotSettings pivotSettings, Transform[] targetTransforms)
        {
            if (targetTransforms.Length <= 0)
                return;

#if UNITY_EDITOR
            Undo.RecordObjects(targetTransforms, "Move");
#endif

            foreach (var transform in targetTransforms)
            {
                if (transform == null)
                {
                    Debug.LogWarning($"Manipulator {m_Manipulator.gameObject} selection cannot update a null transform in the list.");
                    continue;
                }

                if (!m_RotationOffsets.ContainsKey(transform) || !m_PositionOffsets.ContainsKey(transform) || !m_ScaleOffsets.ContainsKey(transform))
                {
                    Debug.LogWarning($"Manipulator {m_Manipulator.gameObject} cannot update transform {transform} because it was not listed when the state was last reset.", transform);
                    continue;
                }

                var targetRotation = m_EasedRotation * m_RotationOffsets[transform];
                if (transform.rotation != targetRotation)
                    transform.rotation = targetRotation;

                if (pivotSettings.pivotGrouping == PivotGrouping.Group) // Rotate and scale the transform's offset from the group pivot
                {
                    m_PositionOffsetRotation = m_EasedRotation * Quaternion.Inverse(m_StartRotation);
                    m_PositionOffsetScale = m_TargetScale;
                }

                var targetPosition = m_EasedPosition + Vector3.Scale(m_PositionOffsetScale, m_PositionOffsetRotation * m_PositionOffsets[transform]);

                if (transform.position != targetPosition)
                    transform.position = targetPosition;

                var targetScale = Vector3.Scale(m_TargetScale, m_ScaleOffsets[transform]);
                if (transform.localScale != targetScale)
                    transform.localScale = targetScale;
            }
        }

        void Translate(Vector3 delta)
        {
            m_TargetPosition += delta;
        }

        void Rotate(Quaternion delta)
        {
            m_TargetRotation = delta * m_TargetRotation;
        }

        void Scale(Vector3 delta)
        {
            m_TargetScale += delta;
        }
    }
}
