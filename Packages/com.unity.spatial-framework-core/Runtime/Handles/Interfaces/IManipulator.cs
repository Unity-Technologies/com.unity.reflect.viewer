using System;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Implementors can translate, rotate, and scale the transform properties of target transforms via drag interactions
    /// </summary>
    public interface IManipulator
    {
        /// <summary>
        /// The root gameObject of the manipulator
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// Delegate that processes the translation, using the vector3 passed in
        /// </summary>
        event Action<Vector3> translate;

        /// <summary>
        /// Delegate that processes the rotation, using the quaternion passed in
        /// </summary>
        event Action<Quaternion> rotate;

        /// <summary>
        /// Delegate that processes the scale, using the vector3 passed in
        /// </summary>
        event Action<Vector3> scale;

        /// <summary>
        /// Delegate that is called once after every drag starts
        /// </summary>
        event Action dragStarted;

        /// <summary>
        /// Delegate that is called once after every drag ends
        /// </summary>
        event Action dragEnded;

        /// <summary>
        /// Bool denoting the whether the manipulator is currently being dragged
        /// </summary>
        bool dragging { get; }

        /// <summary>
        /// Set the transforms that the manipulator is acting upon, and the active or "key" transform
        /// </summary>
        void SetTargetTransforms(Transform[] targetTransforms, Transform activeTargetTransform);

        /// <summary>
        /// Sets the position and rotation of the manipulator
        /// </summary>
        /// <param name="position">The position in world space</param>
        /// <param name="rotation">The rotation in world space</param>
        void SetPose(Vector3 position, Quaternion rotation);
    }
}
