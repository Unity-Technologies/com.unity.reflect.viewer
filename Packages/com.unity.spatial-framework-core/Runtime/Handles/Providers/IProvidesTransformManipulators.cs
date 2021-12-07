using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Provide methods for controlling manipulators
    /// </summary>
    public interface IProvidesTransformManipulators : IFunctionalityProvider
    {
        /// <summary>
        /// Show or hide the manipulators
        /// </summary>
        /// <param name="requester">The requesting object that is wanting to set all manipulators visible or hidden</param>
        /// <param name="visibility">Whether the manipulators should be shown or hidden</param>
        void SetManipulatorsVisible(IUsesTransformManipulators requester, bool visibility);

        /// <summary>
        /// Set the current manipulator group by name.
        /// </summary>
        /// <param name="groupName">The name of the manipulator group</param>
        void SetManipulatorGroup(string groupName);

        /// <summary>
        /// Sets the transforms for the manipulators to affect
        /// </summary>
        /// <param name="selectionTransforms">Array of selected transforms</param>
        /// <param name="activeTransform">The active selection transform</param>
        void SetManipulatorSelection(Transform[] selectionTransforms, Transform activeTransform);

        /// <summary>
        /// Cycle to the next group of manipulators available
        /// </summary>
        void NextManipulatorGroup();

        /// <summary>
        /// Returns whether the manipulator is in the dragging state
        /// </summary>
        /// <returns>Whether the manipulator is currently being dragged</returns>
        bool GetManipulatorDragState();
    }
}
