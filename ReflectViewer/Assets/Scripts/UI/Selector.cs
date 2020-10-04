using System;

namespace Unity.Reflect.Viewer.UI
{
    public class Selector<TValue, TState>
        where TValue : struct
        where TState : struct
    {
        private TValue? m_CachedSelectorValue;
        private Func<TState, TValue> m_getSelector;
        private Action<TValue> m_updateFunc;

        public Selector(Func<TState, TValue> getSelector, Action<TValue> updateFunc, Action<Action<TState>> attach)
        {
            m_getSelector = getSelector;
            m_updateFunc = updateFunc;
            attach(OnStateDataChanged);
        }

        private void OnStateDataChanged(TState state)
        {
            TValue tempValue = m_getSelector(state);
            if (!m_CachedSelectorValue.Equals(tempValue))
            {
                m_updateFunc(tempValue);
                m_CachedSelectorValue = tempValue;
            }
        }
    }

    public static class Selector
    {
        public static void useSelector(string name, Action<bool> updateFunc)
        {
            var toolbarEnabled = new Selector<bool, UIStateData>(
                (state) => state.getValueByName(name),
                updateFunc,
                handler=> UIStateManager.stateChanged += handler);
        }
    }
}
