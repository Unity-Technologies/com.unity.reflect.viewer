using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.TouchFramework
{
    /// <summary>
    /// Simple TextInput field using TMP_InputField control
    /// </summary>
    public class TextInputPropertyValue : MonoBehaviour, IPropertyValue<string>
    {
        [SerializeField]
        string m_Value = "";

        [SerializeField]
        TMP_InputField m_TextInputField = null;

        public Type type => typeof(string);

        public object objectValue
        {
            get => value;
            set => this.value = (string)value;
        }

        public string value
        {
            get => m_Value;
            set
            {
                if (value == m_Value)
                    return;
                m_Value = value;
                UpdateInputFields();
                m_OnValueChanged?.Invoke(m_Value);
            }
        }

        UnityEvent<string> m_OnValueChanged = new UnityEvent<string>();

        Dictionary<Action, UnityAction<string>> m_Handlers = new Dictionary<Action, UnityAction<string>>();

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
            m_OnValueChanged.RemoveListener(m_Handlers[eventFunc]);
            m_Handlers.Remove(eventFunc);
        }

        void Start()
        {
            m_TextInputField.onEndEdit.AddListener(HandleEndEdit);
        }

        void OnDestroy()
        {
            if (m_TextInputField)
                m_TextInputField.onEndEdit.RemoveListener(HandleEndEdit);
        }

        void HandleEndEdit(string input)
        {
            value = input;
        }

        void UpdateInputFields()
        {
            if (m_TextInputField.text != value)
                m_TextInputField.text = value;
        }
    }
}
