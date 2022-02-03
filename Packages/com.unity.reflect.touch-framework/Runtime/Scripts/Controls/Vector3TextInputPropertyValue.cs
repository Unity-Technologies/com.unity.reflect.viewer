using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.TouchFramework
{
    /// <summary>
    /// Vector3 user input Property Value
    /// Uses 3 TMP_InputFields
    /// </summary>
    public class Vector3TextInputPropertyValue : MonoBehaviour, IPropertyValue<Vector3>
    {
        [SerializeField]
        TMP_InputField m_TextX;
        [SerializeField]
        TMP_InputField m_TextY;
        [SerializeField]
        TMP_InputField m_TextZ;

        Vector3 m_Value;
        public Type type => typeof(Vector3);

        public object objectValue
        {
            get => value;
            set => this.value = (Vector3)value;
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

        /// <summary>
        /// Parse three user input strings into a Vector3, falling back to a default value on parse failures.
        /// </summary>
        /// <param name="xInput"></param>
        /// <param name="yInput"></param>
        /// <param name="zInput"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        Vector3 ParseInput(string xInput, string yInput, string zInput, Vector3 defaultValue)
        {
            float x = ParseFloat(xInput, defaultValue.x);
            float y = ParseFloat(yInput, defaultValue.y);
            float z = ParseFloat(zInput, defaultValue.y);

            Vector3 result = new Vector3(x, y, z);
            return result;
        }

        /// <summary>
        /// Converts the user's input of a number into a proper float, with a fallback if parsing fails.
        /// </summary>
        /// <param name="userInput"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        float ParseFloat(string userInput, float def)
        {
            if (float.TryParse(userInput, out var response))
                return response;
            return def;
        }

        void Start()
        {
            // Configure type
            m_TextX.characterValidation = TMP_InputField.CharacterValidation.Decimal;
            m_TextY.characterValidation = TMP_InputField.CharacterValidation.Decimal;
            m_TextZ.characterValidation = TMP_InputField.CharacterValidation.Decimal;

            // Attach listeners
            m_TextX.onEndEdit.AddListener(HandleXUpdate);
            m_TextY.onEndEdit.AddListener(HandleYUpdate);
            m_TextZ.onEndEdit.AddListener(HandleZUpdate);
        }

        /// <summary>
        /// Disconnect listeners
        /// </summary>
        void OnDestroy()
        {
            if (m_TextX)
                m_TextX.onEndEdit.RemoveListener(HandleXUpdate);
            if (m_TextY)
                m_TextY.onEndEdit.RemoveListener(HandleYUpdate);
            if (m_TextZ)
                m_TextZ.onEndEdit.RemoveListener(HandleZUpdate);
        }

        void HandleXUpdate(string input)
        {
            Vector3 newValue = value;
            newValue.x = ParseFloat(input, value.x);
            value = newValue;
        }

        void HandleYUpdate(string input)
        {
            Vector3 newValue = value;
            newValue.y = ParseFloat(input, value.y);
            value = newValue;
        }

        void HandleZUpdate(string input)
        {
            Vector3 newValue = value;
            newValue.z = ParseFloat(input, value.z);
            value = newValue;
        }

        /// <summary>
        /// Verify changes, and update fields
        /// </summary>
        void UpdateInputFields()
        {
            string x = value.x.ToString();
            string y = value.y.ToString();
            string z = value.z.ToString();

            if (m_TextX.text != x)
                m_TextX.text = x;

            if (m_TextY.text != y)
                m_TextY.text = y;

            if (m_TextZ.text != z)
                m_TextZ.text = z;
        }
    }
}
