using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [ExecuteAlways]
    public class LandscapeSafeArea : MonoBehaviour
    {
        RectTransform m_RectTransform;
        Rect m_CachedSafeArea = Rect.zero;

        void Awake() { m_RectTransform = GetComponent<RectTransform>(); }

        void Update() { Refresh(); }

        void Refresh()
        {
            var safeArea = Screen.safeArea;
            if (safeArea != m_CachedSafeArea)
            {
                m_CachedSafeArea = safeArea;
                ApplySafeArea(safeArea);
            }
        }

        void ApplySafeArea(Rect safeArea)
        {
            var screenRect = new Rect(0, 0, Screen.width, Screen.height);
            var offsetMin = new Vector2(safeArea.x, 0);
            var offsetMax = new Vector2(safeArea.max.x - screenRect.max.x, 0);
            m_RectTransform.offsetMin = offsetMin;
            m_RectTransform.offsetMax = offsetMax;
        }
    }
}
