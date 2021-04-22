using System;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public class UnsafeAreaFiller : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] RectTransform m_Background;
        [SerializeField] Direction m_Directions;
#pragma warning restore 649

        bool m_UpdateRequired;
        Rect m_SafeArea;
        Vector2 m_OriginalOffsetMin;
        Vector2 m_OriginalOffsetMax;

        void OnEnable()
        {
            m_OriginalOffsetMin = m_Background.offsetMin;
            m_OriginalOffsetMax = m_Background.offsetMax;
            m_UpdateRequired = true;
        }

        void OnDisable()
        {
            m_Background.offsetMin = m_OriginalOffsetMin;
            m_Background.offsetMax = m_OriginalOffsetMax;
        }

        void Update()
        {
            var safeArea = Screen.safeArea;
            if (m_UpdateRequired || safeArea != m_SafeArea)
            {
                m_UpdateRequired = false;
                m_SafeArea = safeArea;
                AdjustBackground();
            }
        }

        void AdjustBackground()
        {
            var screen = new Rect(0, 0, Screen.width, Screen.height);
            var leftOffset = m_Directions.HasFlag(Direction.Left) ? -m_SafeArea.xMin : m_OriginalOffsetMin.x;
            var rightOffset = m_Directions.HasFlag(Direction.Right) ? screen.xMax - m_SafeArea.xMax : m_OriginalOffsetMax.x;
            var bottomOffset = m_Directions.HasFlag(Direction.Bottom) ? -m_SafeArea.yMin : m_OriginalOffsetMin.y;
            var topOffset = m_Directions.HasFlag(Direction.Top) ? screen.yMax - m_SafeArea.yMax : m_OriginalOffsetMax.y;

            var offsetMin = new Vector2(leftOffset, bottomOffset);
            var offsetMax = new Vector2(rightOffset, topOffset);

            m_Background.offsetMin = offsetMin;
            m_Background.offsetMax = offsetMax;
        }

        [Flags]
        enum Direction
        {
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8
        }
    }
}
