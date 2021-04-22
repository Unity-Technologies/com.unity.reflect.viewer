using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    public static class Utilities
    {
        public static Vector2 LocalPosition(this PointerEventData pointerEventData, RectTransform rectTransform)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pointerEventData.position,
                pointerEventData.pressEventCamera, out var localPosition);
            return localPosition;
        }

        public static float Smoothstep(float a, float b, float x)
        {
            float t = Mathf.Clamp01((x - a) / (b - a));
            return t * t * (3.0f - (2.0f * t));
        }
    }
}
