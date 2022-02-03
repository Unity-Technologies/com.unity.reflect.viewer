using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    public class ButtonActionPropertyValue : MonoBehaviour, IPropertyValue<Action>
    {
        [SerializeField]
        ButtonControl m_ButtonControl = null;

        Type m_Type;
        Action m_Value;

        public Type type => typeof(Action);

        public Action value => m_Value;
        public object objectValue
        {
            get => m_Value;
            set => m_Value = (Action)value;
        }
        UnityEvent<Action> m_OnValueChanged = new UnityEvent<Action>();
        Dictionary<Action, UnityAction<Action>> m_Handlers = new Dictionary<Action, UnityAction<Action>>();

        public void AddListener(Action eventFunc)
        {
            m_Handlers[eventFunc] = (newValue) =>
            {
                eventFunc();
            };
            m_OnValueChanged.AddListener(m_Handlers[eventFunc]);
        }

        public void RemoveListener(Action eventFunc)
        {
            if (!m_Handlers.ContainsKey(eventFunc))
                return;
            m_OnValueChanged.RemoveListener(m_Handlers[eventFunc]);
            m_Handlers.Remove(eventFunc);
        }

        void HandleButtonClick(BaseEventData eventData)
        {
            value?.Invoke();
        }

        void Start()
        {
            m_ButtonControl.onControlTap.AddListener(HandleButtonClick);
        }

        void OnDestroy()
        {
            m_ButtonControl.onControlTap.RemoveListener(HandleButtonClick);
        }
    }
}
