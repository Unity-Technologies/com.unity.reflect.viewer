using UnityEngine;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Declares a class as being a source for the raycasting input
    /// </summary>
    public interface IRaycastSource
    {
        bool hasObject { get; }
        bool blocked { get; set; }
        Transform rayOrigin { get; }
        float distance { get; }
        GameObject hoveredGameObject { get; }
        GameObject dragGameObject { get; }
    }
}
