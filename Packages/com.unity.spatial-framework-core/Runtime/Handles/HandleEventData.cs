using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Event data for interacting with handles
    /// </summary>
    public class HandleEventData
    {
        /// <summary>
        /// The source transform from where the ray is cast
        /// </summary>
        public Transform rayOrigin;

        /// <summary>
        /// The camera from where the ray is cast if this event came from the screen
        /// </summary>
        public Camera camera { private get; set; }

        /// <summary>
        /// Whether this pointer was within range to be considered "direct"
        /// </summary>
        public bool direct;

        /// <summary>
        /// The screen position of the touch/mouse event if it came from the screen.
        /// </summary>
        public Vector2 screenPosition { private get; set; }

        /// <summary>
        /// The world position where the handle is being dragged
        /// </summary>
        public Vector3 worldPosition;

        /// <summary>
        /// Change in position between last frame and this frame
        /// </summary>
        public Vector3 deltaPosition;

        /// <summary>
        /// Change in rotation between last frame and this frame
        /// </summary>
        public Quaternion deltaRotation;

        /// <summary>
        /// Create a new HandleEventData with a given ray origin
        /// </summary>
        /// <param name="rayOrigin">The ray origin</param>
        /// <param name="direct">Whether this event was a direct manipulation</param>
        public HandleEventData(Transform rayOrigin, bool direct)
        {
            this.rayOrigin = rayOrigin;
            this.direct = direct;
            deltaPosition = Vector3.zero;
            deltaRotation = Quaternion.identity;
        }

        /// <summary>
        /// Get the ray specified by this event
        /// </summary>
        /// <returns>The ray</returns>
        public Ray GetRay()
        {
            return camera == null ?
                new Ray(rayOrigin.position, rayOrigin.forward) :
                camera.ScreenPointToRay(screenPosition);
        }
    }
}
