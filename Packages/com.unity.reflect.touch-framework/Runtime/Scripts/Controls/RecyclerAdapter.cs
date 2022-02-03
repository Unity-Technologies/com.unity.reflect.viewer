using System;
using Unity.Reflect.Viewer.UI.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    [RequireComponent(typeof(ScrollRect))]
    public abstract class RecyclerAdapter : UIBehaviour
    {
        protected ScrollRect m_ScrollRect;
        protected RecyclerViewHolder[] m_ViewHolders;

        protected int m_CircularViewHolderStartIndex = 0;
        protected int m_DataSourceStartIndex;

        protected bool m_IgnoreScrollChange = false;
        protected const int k_ViewHolderBufferSize = 1;
        protected LinearLayout m_Layout;
        State m_State = new State();

        public ScrollRect scrollRect
        {
            set { m_ScrollRect = value; }
            get { return m_ScrollRect; }
        }

        public State state
        {
            set { m_State = value; }
            get { return m_State; }
        }

        protected virtual void Awake()
        {
            base.Awake();
            m_ScrollRect = GetComponent<ScrollRect>();
            m_Layout = new LinearLayout(m_ScrollRect, LinearLayout.LayoutOrientation.Vertical);
        }

        protected virtual void OnEnable()
        {
            base.OnEnable();
            Refresh();
            m_ScrollRect.onValueChanged.AddListener(OnScrollChanged);
            m_IgnoreScrollChange = false;
        }

        protected virtual void OnDisable()
        {
            base.OnDisable();
            m_ScrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }

        void OnScrollChanged(Vector2 normalisedPos)
        {
            if (!m_IgnoreScrollChange)
            {
                UpdateViews(false);
            }
        }

        public void Refresh()
        {
            var size = GetViewSize(0);
            var count = GetItemCount();
            if (count != m_State.childCount || size != m_State.childSize)
            {
                m_State.childCount = count;
                m_State.childSize = size;
                m_IgnoreScrollChange = true;
                m_Layout.UpdateLayout(m_State);
                m_IgnoreScrollChange = false;
            }
            UpdateViews(true);
        }

        bool BuildViewHolders()
        {
            var viewHoldersCapacity = m_Layout.GetViewHoldersCapacity(m_State);
            viewHoldersCapacity += k_ViewHolderBufferSize * 2;
            bool rebuild = m_ViewHolders == null || viewHoldersCapacity > m_ViewHolders.Length;
            if (rebuild)
            {
                if (m_ViewHolders == null)
                {
                    m_ViewHolders = new RecyclerViewHolder[viewHoldersCapacity];
                }
                else
                {
                    Array.Resize(ref m_ViewHolders, viewHoldersCapacity);
                }

                for (int i = 0; i < m_ViewHolders.Length; ++i)
                {
                    if (m_ViewHolders[i] == null)
                    {
                        m_ViewHolders[i] = new RecyclerViewHolder();
                        m_ViewHolders[i].ViewType = GetViewType(i);
                        m_ViewHolders[i].GameObject = OnCreateViewHolder(m_ScrollRect.content, m_ViewHolders[i].ViewType);
                        m_ViewHolders[i].RectTransform = m_ViewHolders[i].GameObject.GetComponent<RectTransform>();
                    }
                    m_ViewHolders[i].GameObject.SetActive(false);
                }
            }

            return rebuild;
        }

        void UpdateViews(bool dataChanged)
        {
            bool childrenChanged = BuildViewHolders();
            bool populateAll = childrenChanged || dataChanged;

            var firstVisibleIndex = m_Layout.GetFirstVisibleIndex(this);
            int newStartIndex = firstVisibleIndex - k_ViewHolderBufferSize;
            int rowsMovement = newStartIndex - m_DataSourceStartIndex;

            if (populateAll || Mathf.Abs(rowsMovement) >= m_ViewHolders.Length)
            {
                m_DataSourceStartIndex = newStartIndex;
                m_CircularViewHolderStartIndex = 0;
                int rowIdx = newStartIndex;
                foreach (var item in m_ViewHolders)
                {
                    UpdateChild(item, rowIdx++);
                }
            }
            else if (rowsMovement != 0)
            {
                int newViewHolderStartIndex = (m_CircularViewHolderStartIndex + rowsMovement) % m_ViewHolders.Length;

                if (rowsMovement < 0)
                {
                    for (int i = 1; i <= -rowsMovement; ++i)
                    {
                        int temp = WrapViewHolderIndex(m_CircularViewHolderStartIndex - i);
                        int rowIdx = m_DataSourceStartIndex - i;
                        UpdateChild(m_ViewHolders[temp], rowIdx);
                    }
                }
                else
                {
                    int previousViewHolderEndIndex = m_CircularViewHolderStartIndex + m_ViewHolders.Length - 1;
                    int previousDataEndIndex = m_DataSourceStartIndex + m_ViewHolders.Length - 1;
                    for (int i = 1; i <= rowsMovement; ++i)
                    {
                        int temp = WrapViewHolderIndex(previousViewHolderEndIndex + i);
                        int dataIndex = previousDataEndIndex + i;
                        UpdateChild(m_ViewHolders[temp], dataIndex);
                    }
                }

                m_DataSourceStartIndex = newStartIndex;
                m_CircularViewHolderStartIndex = newViewHolderStartIndex;
            }
        }

        int WrapViewHolderIndex(int idx)
        {
            while (idx < 0)
            {
                idx += m_ViewHolders.Length;
            }
            return idx % m_ViewHolders.Length;
        }

        void UpdateChild(RecyclerViewHolder child, int dataIndex)
        {
            if (dataIndex >= 0 && dataIndex < m_State.childCount)
            {
                m_Layout.UpdateChildPosition(child, m_State.childSize, dataIndex);
                OnBindViewHolder(child.GameObject, dataIndex);
                child.GameObject.SetActive(true);
            }
            else
            {
                child.GameObject.SetActive(false);
            }
        }

        protected virtual int GetViewType(int ItemIndex)
        {
            return 0;
        }

        protected abstract int GetItemCount();
        public abstract Rect GetViewSize(int typeId);
        protected abstract void OnBindViewHolder(GameObject viewHolder, int ItemIndex);
        protected abstract GameObject OnCreateViewHolder(Transform viewGroup, int viewType);

        public class State
        {
            public int childCount;
            public Rect childSize;
        }
    }
}

namespace Unity.Reflect.Viewer.UI.Utils
{
    public class RecyclerViewHolder
    {
        public GameObject GameObject { get; set; }
        public RectTransform RectTransform { get; set; }

        public int ViewType;
    }
}
