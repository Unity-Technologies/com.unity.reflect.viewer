using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class UISelectorCondition : MonoBehaviour, IPropertyValue<bool>
    {
        [Serializable]
        public class UIConditionChangedEvent : UnityEvent {}


        [SerializeField]
        UIConditionChangedEvent m_OnTrue;
        [SerializeField]
        UIConditionChangedEvent m_OnFalse;

        bool m_Value;
        List<Action> m_Handlers = new List<Action>();

        public bool value
        {
            get => m_Value;
        }

        public Type type => typeof(bool);

        public object objectValue
        {
            get => m_Value;

            set
            {
                m_Value = (bool)value;
                if (m_Value)
                {
                    m_OnTrue?.Invoke();
                }
                else
                {
                    m_OnFalse?.Invoke();
                }
            }
        }

        public void AddListener(Action eventFunc)
        {
            m_Handlers.Add(eventFunc);
        }

        public void RemoveListener(Action eventFunc)
        {
            m_Handlers.Remove(eventFunc);
        }

        [UsedImplicitly]
        public void SetValue(bool newValue)
        {
            m_Value = newValue;
            foreach (var handler in m_Handlers)
            {
                handler?.Invoke();
            }
        }

        [UsedImplicitly]
        public void Toggle()
        {
            m_Value = !m_Value;
            foreach (var handler in m_Handlers)
            {
                handler?.Invoke();
            }
        }
    }
}
