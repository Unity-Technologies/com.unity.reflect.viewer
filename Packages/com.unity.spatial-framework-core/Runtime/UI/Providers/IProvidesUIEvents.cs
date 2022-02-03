using System;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Provide access to UI events
    /// </summary>
    public interface IProvidesUIEvents : IFunctionalityProvider
    {
        event Action<GameObject, TrackedDeviceEventData> rayEntered;
        event Action<GameObject, TrackedDeviceEventData> rayHovering;
        event Action<GameObject, TrackedDeviceEventData> rayExited;
        event Action<GameObject, TrackedDeviceEventData> dragStarted;
        event Action<GameObject, TrackedDeviceEventData> dragEnded;
    }
}
