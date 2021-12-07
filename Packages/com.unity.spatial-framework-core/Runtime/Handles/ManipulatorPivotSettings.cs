using System;
using Unity.XRTools.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Settings for how a manipulator pivot interacts with its target selection
    /// </summary>
    [Serializable]
    public class ManipulatorPivotSettings
    {
        [SerializeField, Tooltip("If set to Group, all objects will rotate and scale around a shared point. If set to Individual, each object is rotated and scaled around their individual pivot or center point.")]
        PivotGrouping m_PivotGrouping = PivotGrouping.Group;

        [SerializeField, Tooltip("If set to Local, the manipulator will align with the active object. If set to Global, the manipulator will align to the world.")]
        PivotRotation m_PivotRotation = PivotRotation.Local;

        [SerializeField, Tooltip("If set to Pivot, the manipulator will be positioned at the active object's pivot. If set to Center, it will be located at the center of the bounding volume.")]
        PivotMode m_PivotMode = PivotMode.Pivot;

        /// <summary>
        /// If set to Group, all objects will rotate and scale around a shared point. If set to Individual, each object is rotated and scaled around their individual pivot or center point.
        /// </summary>
        public PivotGrouping pivotGrouping
        {
            get => m_PivotGrouping;
            set => m_PivotGrouping = value;
        }

        /// <summary>
        /// If set to Local, the manipulator will align with the active object. If set to Global, the manipulator will align to the world.
        /// </summary>
        public PivotRotation pivotRotation
        {
            get => m_PivotRotation;
            set => m_PivotRotation = value;
        }

        /// <summary>
        /// If set to Pivot, the manipulator will be positioned at the active object's pivot. If set to Center, it will be located at the center of all selected objects.
        /// </summary>
        public PivotMode pivotMode
        {
            get => m_PivotMode;
            set => m_PivotMode = value;
        }

        /// <summary>
        /// Calculates the pivot position based on the current pivot settings for a selection group
        /// </summary>
        /// <param name="activeTransform">The active transform</param>
        /// <param name="selectionTransforms">A list of all selected transforms</param>
        /// <returns>The pivot position that should be used for the given selection</returns>
        public Vector3 GetPivotPosition(Transform activeTransform, Transform[] selectionTransforms)
        {
            if (activeTransform == null || selectionTransforms == null || selectionTransforms.Length == 0)
                return Vector3.zero;
            
            Vector3 pivotPosition;
            if (pivotMode == PivotMode.Pivot)
            {
                pivotPosition = activeTransform.position;
            }
            else // PivotMode.Center
            {
                Vector3 center;
                if (pivotGrouping == PivotGrouping.Group)
                {
                    var selectionBounds = selectionTransforms != null ? BoundsUtils.GetBounds(selectionTransforms) : new Bounds();
                    center = selectionBounds.center;
                }
                else
                {
                    var activeTransformBounds = BoundsUtils.GetBounds(activeTransform);
                    center = activeTransformBounds.center;
                }

                pivotPosition = center;
            }

            return pivotPosition;
        }
    }
}
