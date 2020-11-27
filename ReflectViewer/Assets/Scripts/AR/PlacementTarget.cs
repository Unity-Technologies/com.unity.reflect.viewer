using UnityEngine;


namespace Unity.Reflect.Viewer
{
    /// <summary>
    /// Tags objects as being able to have content placed on them dynamically.
    /// Also has hovering-highlight behavior built-in
    /// </summary>
    public class PlacementTarget : MonoBehaviour
    {
        Renderer m_Renderer;

        void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
            m_Renderer.enabled = false;
        }

        public void HoverBegin()
        {
            m_Renderer.enabled = true;
        }

        public void HoverEnd()
        {
            m_Renderer.enabled = false;
        }
    }
}
