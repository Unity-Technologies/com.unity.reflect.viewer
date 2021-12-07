using System;
using Unity.XRTools.ModuleLoader;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Provide access to user handedness functionality
    /// </summary>
    public interface IProvidesDeviceHandedness : IFunctionalityProvider
    {
        /// <summary>
        /// The callback invoked whenever handedness changes.
        /// </summary>
        event Action<XRControllerHandedness> handednessChanged;

        /// <summary>
        /// The primary user's current handedness.  Implementors will call <see cref="handednessChanged"/> when the value changes.
        /// </summary>
        XRControllerHandedness handedness { get; set; }
    }
}
