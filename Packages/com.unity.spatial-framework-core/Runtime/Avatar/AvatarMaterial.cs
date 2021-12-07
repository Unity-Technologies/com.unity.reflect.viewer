using Unity.SpatialFramework.Rendering;
using UnityEngine;

namespace Unity.SpatialFramework.Avatar
{
    /// <summary>
    /// Controls the avatar's materials. The avatar will activate the material highlights when the viewer enters the trigger and return to normal when it exits.
    /// The material highlights should be configured to make the avatar transparent so that they do not obstruct the viewer.
    /// </summary>
    public class AvatarMaterial : MonoBehaviour
    {
        [SerializeField, Tooltip("The material highlight components that will be activated and deactivated when the viewer enters and exits the trigger.")]
        BaseHighlight[] m_Highlights;

        [SerializeField, Tooltip("If a GameObject with this tag enters the avatar's trigger volume, the avatar materials changes will activate.")]
        string m_ViewerTag = "VRPlayer";

        /// <summary>
        /// If a GameObject with this tag enters the avatar's trigger volume, the avatar materials changes will activate.
        /// </summary>
        public string viewerTag
        {
            get => m_ViewerTag;
            set => m_ViewerTag = value;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(m_ViewerTag))
            {
                ActivateTransparent();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(m_ViewerTag))
            {
                DeactivateTransparent();
            }
        }

        /// <summary>
        /// Activates the avatar's transparent material state.
        /// This is normally triggered when an object with the specified tag enters the trigger, but it can also be controlled manually by disabling the trigger and calling this method.
        /// </summary>
        public void ActivateTransparent()
        {
            if (m_Highlights != null)
            {
                foreach (var baseHighlight in m_Highlights)
                {
                    baseHighlight.Activate();
                }
            }
        }

        /// <summary>
        /// Deactivates the avatar's transparent material state.
        /// This is normally triggered when an object with the specified tag exits the trigger, but it can also be controlled manually by disabling the trigger and calling this method.
        /// </summary>
        void DeactivateTransparent()
        {
            if (m_Highlights != null)
            {
                foreach (var baseHighlight in m_Highlights)
                {
                    baseHighlight.Deactivate();
                }
            }
        }

        [ContextMenu("Get All Child Highlights")]
        void GetAllChildHighlights()
        {
            m_Highlights = GetComponentsInChildren<BaseHighlight>();
        }
    }
}
