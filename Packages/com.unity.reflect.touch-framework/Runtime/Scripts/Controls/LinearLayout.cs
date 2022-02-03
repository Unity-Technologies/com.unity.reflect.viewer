using System;
using Unity.Reflect.Viewer.UI.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    [Serializable]
    public class LinearLayout
    {
        [Serializable]
        public enum LayoutOrientation
        {
            Vertical,
            Horizontal
        }

        public LayoutOrientation Orientation;

        ScrollRect m_ScrollRect;

        public LinearLayout(ScrollRect scrollRect, LayoutOrientation orientation)
        {
            m_ScrollRect = scrollRect;
            Orientation = orientation;
        }

        public void UpdateChildPosition(RecyclerViewHolder child, Rect childRect, int dataIndex)
        {
            Vector2 pivot = child.RectTransform.pivot;
            float xPosition = Orientation == LayoutOrientation.Horizontal ? childRect.width * dataIndex + pivot.x * childRect.width: 0f;
            float yPosition = Orientation == LayoutOrientation.Vertical ? childRect.height * dataIndex + pivot.y * childRect.height : 0f;
            child.RectTransform.anchoredPosition = new Vector2(xPosition, -yPosition);
        }

        public void UpdateLayout(RecyclerAdapter.State state)
        {
            if (Orientation == LayoutOrientation.Horizontal)
            {
                m_ScrollRect.content.sizeDelta = new Vector2(state.childSize.width * state.childCount, state.childSize.height);
            }
            else
            {
                m_ScrollRect.content.sizeDelta = new Vector2(state.childSize.width, state.childSize.height * state.childCount);
            }
        }

        public int GetViewHoldersCapacity(RecyclerAdapter.State state)
        {
            if (state.childCount == 0)
            {
                return 0;
            }

            if (Orientation == LayoutOrientation.Vertical)
            {
                if (state.childSize.height == 0)
                {
                    return 0;
                }
                return Mathf.RoundToInt(0.5f + m_ScrollRect.viewport.rect.height / state.childSize.height); //TODO support multiple types
            }

            if (state.childSize.width == 0)
            {
                return 0;
            }
            return Mathf.RoundToInt(0.5f + m_ScrollRect.viewport.rect.width / state.childSize.width); //TODO support multiple types
        }

        public int GetFirstVisibleIndex(RecyclerAdapter recyclerAdapter)
        {
            if (Orientation == LayoutOrientation.Vertical)
            {
                return (int)(recyclerAdapter.scrollRect.content.localPosition.y / recyclerAdapter.state.childSize.height); //TODO support multiple types
            }

            return (int)(-recyclerAdapter.scrollRect.content.localPosition.x / recyclerAdapter.state.childSize.width); //TODO support multiple types
        }
    }
}
