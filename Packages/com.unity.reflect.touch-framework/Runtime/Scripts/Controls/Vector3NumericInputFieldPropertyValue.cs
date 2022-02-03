using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.TouchFramework
{
    public class Vector3NumericInputFieldPropertyValue : MonoBehaviour, IPropertyValue<Vector3>
    {
        [SerializeField]
        NumericInputFieldPropertyControl m_TextX;
        [SerializeField]
        NumericInputFieldPropertyControl m_TextY;
        [SerializeField]
        NumericInputFieldPropertyControl m_TextZ;

        Vector3 m_Value;
        public Type type => typeof(Vector3);

        public object objectValue
        {
            get => value;
            set
            {
                m_Value = (Vector3)value;
                UpdateInputFields();
            }
        }
        UnityEvent<Vector3> m_OnValueChanged = new UnityEvent<Vector3>();
        Dictionary<Action, UnityAction<Vector3>> m_Handlers = new Dictionary<Action, UnityAction<Vector3>>();

        /// <summary>
        /// Trigger event when value changes
        /// </summary>
        /// <param name="eventFunc"></param>
        public void AddListener(Action eventFunc)
        {
            m_Handlers[eventFunc] = (newValue) =>
            {
                eventFunc();
            };
            m_OnValueChanged.AddListener(m_Handlers[eventFunc]);
        }

        /// <summary>
        /// Remove value change event
        /// </summary>
        /// <param name="eventFunc"></param>
        public void RemoveListener(Action eventFunc)
        {
            if (!m_Handlers.ContainsKey(eventFunc))
                return;
            m_OnValueChanged.RemoveListener(m_Handlers[eventFunc]);
            m_Handlers.Remove(eventFunc);
        }

        public Vector3 value
        {
            get => m_Value;
            private set
            {
                if (m_Value == value)
                    return;
                m_Value = value;
                UpdateInputFields();
                m_OnValueChanged?.Invoke(m_Value);
            }
        }

        void Start()
        {
            // Attach listeners
            m_TextX.onFloatValueChanged.AddListener(HandleXUpdate);
            m_TextY.onFloatValueChanged.AddListener(HandleYUpdate);
            m_TextZ.onFloatValueChanged.AddListener(HandleZUpdate);

            m_TextX.SetValue(m_Value.x);
            m_TextY.SetValue(m_Value.y);
            m_TextZ.SetValue(m_Value.z);
        }

        /// <summary>
        /// Disconnect listeners
        /// </summary>
        void OnDestroy()
        {
            if (m_TextX)
                m_TextX.onFloatValueChanged.RemoveListener(HandleXUpdate);
            if (m_TextY)
                m_TextY.onFloatValueChanged.RemoveListener(HandleYUpdate);
            if (m_TextZ)
                m_TextZ.onFloatValueChanged.RemoveListener(HandleZUpdate);
        }

        void HandleXUpdate(float input)
        {
            Vector3 newValue = value;
            newValue.x = input;
            value = newValue;
        }

        void HandleYUpdate(float input)
        {
            Vector3 newValue = value;
            newValue.y = input;
            value = newValue;
        }

        void HandleZUpdate(float input)
        {
            Vector3 newValue = value;
            newValue.z = input;
            value = newValue;
        }

        /// <summary>
        /// Verify changes, and update fields
        /// </summary>
        void UpdateInputFields()
        {
            m_TextX.SetValue(value.x);
            m_TextY.SetValue(value.y);
            m_TextZ.SetValue(value.z);
        }
    }
}
