using System;
using TMPro;
using UnityEngine;

namespace Unity.TouchFramework
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class PropertyValueLabel : MonoBehaviour, IPropertyValue<string>
    {
        TextMeshProUGUI m_Text;

        void Awake()
        {
            m_Text = GetComponent<TextMeshProUGUI>();
        }

        public string value => m_Text.text;
        public Type type => typeof(string);
        public object objectValue
        {
            get => m_Text.text;
            set => m_Text.text = (string)value;
        }
        public void AddListener(Action eventFunc)
        {
            // no op, it's a label, it can't be changed by UI
        }

        public void RemoveListener(Action eventFunc)
        {
            // no op, it's a label, it can't be changed by UI
        }
    }
}
