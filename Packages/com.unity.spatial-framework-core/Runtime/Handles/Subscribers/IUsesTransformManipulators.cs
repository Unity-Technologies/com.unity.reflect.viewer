using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Gives decorated class access to methods for controlling manipulators
    /// </summary>
    public interface IUsesTransformManipulators : IFunctionalitySubscriber<IProvidesTransformManipulators> { }

    /// <summary>
    /// Extension methods for implementors of IUsesSetManipulatorsVisible
    /// </summary>
    public static class UsesSetManipulatorsVisibleMethods
    {
        /// <summary>
        /// Show or hide the manipulators
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="requester">The requesting object that is wanting to set all manipulators visible or hidden</param>
        /// <param name="visibility">Whether the manipulators should be shown or hidden</param>
        public static void SetManipulatorsVisible(this IUsesTransformManipulators user, IUsesTransformManipulators requester, bool visibility)
        {
            user.provider.SetManipulatorsVisible(requester, visibility);
        }

        /// <summary>
        /// Set the current manipulator group by name.
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="groupName">The name of the manipulator group</param>
        public static void SetManipulatorGroup(this IUsesTransformManipulators user, string groupName)
        {
            user.provider.SetManipulatorGroup(groupName);
        }

        /// <summary>
        /// Sets the transforms for the manipulators to affect
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="selectionTransforms">Array of selected transforms</param>
        /// <param name="activeTransform">The active selection transform</param>
        public static void SetManipulatorSelection(this IUsesTransformManipulators user, Transform[] selectionTransforms, Transform activeTransform)
        {
            user.provider.SetManipulatorSelection(selectionTransforms, activeTransform);
        }

        /// <summary>
        /// Cycle to the next group of manipulators available
        /// </summary>
        /// <param name="user">The functionality user</param>
        public static void NextManipulatorGroup(this IUsesTransformManipulators user)
        {
            user.provider.NextManipulatorGroup();
        }

        /// <summary>
        /// Returns whether the manipulator is in the dragging state
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <returns>Whether the manipulator is currently being dragged</returns>
        public static bool GetManipulatorDragState(this IUsesTransformManipulators user)
        {
            return user.provider.GetManipulatorDragState();
        }
    }
}
