using System;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Used to specify a combination of 3D axes
    /// </summary>
    [Flags]
    public enum AxisFlags
    {
        /// <summary>
        /// The X axis
        /// </summary>
        X = 1 << 0,

        /// <summary>
        /// The Y axis
        /// </summary>
        Y = 1 << 1,

        /// <summary>
        /// The Z axis
        /// </summary>
        Z = 1 << 2

    }

    /// <summary>
    /// Extension methods for AxisFlag
    /// </summary>
    public static class AxisFlagsExtensions
    {
        /// <summary>
        /// Get a Vector3 corresponding to the axis described by this AxisFlags
        /// </summary>
        /// <param name="this">The AxisFlags</param>
        /// <returns>The axis</returns>
        public static Vector3 GetAxis(this AxisFlags @this)
        {
            return new Vector3(
                (@this & AxisFlags.X) != 0 ? 1 : 0,
                (@this & AxisFlags.Y) != 0 ? 1 : 0,
                (@this & AxisFlags.Z) != 0 ? 1 : 0
            );
        }
    }
}
