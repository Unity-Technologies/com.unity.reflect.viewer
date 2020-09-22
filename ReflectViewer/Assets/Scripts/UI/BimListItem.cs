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

        string m_Group;
        string m_Category;
        string m_Value;

        public string Group => m_Group;

        public void InitItem(string group, string category, string value)
        {
            m_Group = group;
            m_CategoryText.text = m_Category = category;
            m_ValueText.text = m_Value = value;
        }
    }
}
