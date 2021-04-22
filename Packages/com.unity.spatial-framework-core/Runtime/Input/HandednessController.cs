using Unity.SpatialFramework.Interaction;
using Unity.XRTools.ModuleLoader;
using UnityEditor;
using UnityEngine;

namespace Unity.SpatialFramework.Input
{
    /// <summary>
    /// Sample script showing the basic functionality for user's hands.
    /// See the Spatial-Framework/Runtime/Prefabs/XR/DominantHandTracker prefab for an example of using the 'Dominant Hand' binding.
    /// </summary>
    public class HandednessController : MonoBehaviour, IUsesFunctionalityInjection, IUsesDeviceHandedness
    {
#pragma warning disable 649
        [SerializeField]
        XRControllerHandedness m_Handedness;
#pragma warning restore 649

        IProvidesFunctionalityInjection IFunctionalitySubscriber<IProvidesFunctionalityInjection>.provider { get; set; }
        IProvidesDeviceHandedness IFunctionalitySubscriber<IProvidesDeviceHandedness>.provider { get; set; }

        /// <summary>
        /// Sets the dominant hand of the user.  Used to identify a primary controller within the Input System.
        /// </summary>
        public XRControllerHandedness handedness
        {
            get { return m_Handedness; }
            set
            {
                m_Handedness = value;
                if (isActiveAndEnabled)
                    this.SetHandedness(m_Handedness);
            }
        }

        void OnEnable()
        {
            this.EnsureFunctionalityInjected();
            this.SetHandedness(m_Handedness);

            this.SubscribeToHandednessChanged(OnHandednessChanged);
        }

        void OnDisable()
        {
            this.UnsubscribeToHandednessChanged(OnHandednessChanged);
        }

        static void OnHandednessChanged(XRControllerHandedness newHandedness)
        {
            Debug.Log($"Handedness Changed to {newHandedness}");
        }

        void Update()
        {
            m_Handedness = this.GetHandedness();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    this.EnsureFunctionalityInjected();
                    this.SetHandedness(m_Handedness);
                };
            }
        }
#endif
    }
}
