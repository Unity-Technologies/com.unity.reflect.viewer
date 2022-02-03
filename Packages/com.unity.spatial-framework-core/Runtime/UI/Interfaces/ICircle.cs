using System;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Interface for pooled circle UI
    /// </summary>
    interface ICircle : IPooledUI
    {
        /// <summary>
        /// Callback to get the center of the circle in 3d space
        /// </summary>
        Func<Vector3> getCenter { get; set; }

        /// <summary>
        /// Callback to get the normal direction of the circle
        /// </summary>
        Func<Vector3> getNormal { get; set; }

        /// <summary>
        /// Callback to get the radius of the circle
        /// </summary>
        Func<Vector3> getRadius { get; set; }
    }
}
