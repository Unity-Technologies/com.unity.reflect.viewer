using System;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Interface for pooled label UI
    /// </summary>
    interface ILabel : IPooledUI
    {
        /// <summary>
        /// Callback to get the target position in 3d space
        /// </summary>
        Func<Vector3> getPosition { get; set; }

        /// <summary>
        /// Callback to get the current text string
        /// </summary>
        Func<string> getText { get; set; }
    }
}
