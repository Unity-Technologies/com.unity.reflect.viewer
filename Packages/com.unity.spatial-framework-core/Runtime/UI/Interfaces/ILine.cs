using System;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Interface for pooled line UI
    /// </summary>
    interface ILine : IPooledUI
    {
        /// <summary>
        /// Callback to get the current number of vertices
        /// </summary>
        int vertexCount { get; set; }

        /// <summary>
        /// Callback to get an array of vertex positions
        /// </summary>
        Func<Vector3[]> getLinePositions { get; set; }
    }
}
