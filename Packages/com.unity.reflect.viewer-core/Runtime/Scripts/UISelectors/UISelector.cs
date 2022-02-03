using System;
using System.Collections;

namespace UnityEngine.Reflect.Viewer.Core
{
    sealed class UISelector<TValue>: IUISelector<TValue>
    {
        bool m_Initialized;
        TValue m_CachedSelectorValue;
        Func<TValue> m_getSelector;
        Action<TValue> m_updateFunc;
        IUIContext m_Context;
        public bool isDisposed
        {
            get;
            private set;
        }

        public event Action OnDisposed;

        public UISelector(Func<TValue> getSelector, IUIContext context, Action<TValue> updateFunc)
        {
            m_getSelector = getSelector;
            m_updateFunc = updateFunc;
            m_Context = context;

            if (m_Context != null)
                m_Context.stateChanged += OnStateDataChanged;
            OnStateDataChanged();
            m_Initialized = true;
        }

        void OnStateDataChanged()
        {
            TValue tempValue = m_getSelector();
            if (tempValue is ICollection collection)
            {
                m_updateFunc?.Invoke(tempValue);
            }
            else
            {
                if ((m_CachedSelectorValue == null || !m_CachedSelectorValue.Equals(tempValue)) || !m_Initialized)
                {
                    if ((m_CachedSelectorValue == null && tempValue == null) && m_Initialized)
                        return;

                    m_updateFunc?.Invoke(tempValue);
                    m_CachedSelectorValue = tempValue;
                }
            }
        }

        public TValue GetValue()
        {
            return m_getSelector != null ? m_getSelector.Invoke() : default;
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            if (m_Context != null)
                m_Context.stateChanged -= OnStateDataChanged;
            OnDisposed?.Invoke();
        }

        object IUISelector.GetValue()
        {
            return GetValue();
        }
    }
}
