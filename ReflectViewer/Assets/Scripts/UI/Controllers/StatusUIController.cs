using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class StatusUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Image m_Icon;
        [SerializeField]
        TextMeshProUGUI m_MessageText;
#pragma warning restore CS0649

        public string message
        {
            get => m_MessageText.text;
            set
            {
                m_MessageText.text = value;
            }
        }
    }
}
