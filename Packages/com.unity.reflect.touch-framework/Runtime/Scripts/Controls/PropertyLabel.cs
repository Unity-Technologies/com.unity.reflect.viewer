using System;
using TMPro;
using UnityEngine;

namespace Unity.TouchFramework
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class PropertyLabel : MonoBehaviour, IPropertyLabel
    {
        TextMeshProUGUI m_Text;

        void Awake()
        {
            m_Text = GetComponent<TextMeshProUGUI>();
        }

        public string label
        {
            get => m_Text.text;
            set => m_Text.text = value;
        }
    }
}
