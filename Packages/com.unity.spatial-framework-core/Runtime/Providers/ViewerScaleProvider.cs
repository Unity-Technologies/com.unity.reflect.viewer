using Unity.SpatialFramework.Interaction;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.SpatialFramework.Providers
{
    /// <summary>
    /// Attach this to an GameObject to use its Transform to provide the viewer scale
    /// This should usually be attached to the XR rig, or parent of the main camera
    /// </summary>
    public class ViewerScaleProvider : MonoBehaviour, IProvidesViewerScale
    {
        void IFunctionalityProvider.LoadProvider() { }

        void IFunctionalityProvider.ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesViewerScale>(obj);
        }

        void IFunctionalityProvider.UnloadProvider() { }

        /// <summary>
        /// Get the viewer scale
        /// </summary>
        /// <returns>The viewer scale</returns>
        public float GetViewerScale()
        {
            return transform.localScale.x;
        }

        /// <summary>
        /// Set the viewer scale
        /// </summary>
        /// <param name="scale">The viewer scale</param>
        public void SetViewerScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }
    }
}
