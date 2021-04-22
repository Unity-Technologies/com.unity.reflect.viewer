using System;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class BimListItem : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        TextMeshProUGUI m_CategoryText;

        [SerializeField]
        TextMeshProUGUI m_ValueText;
#pragma warning restore CS0649

        string m_GroupKey;
        string m_Category;
        string m_Value;

        public string groupKey => m_GroupKey;

        public string category => m_Category;

        public string value => m_Value;

        public void InitItem(string _group, string _category, string _value)
        {
            m_GroupKey = _group;
            m_CategoryText.text = m_Category = _category;
            m_ValueText.text = m_Value = _value;
        }
    }
}
