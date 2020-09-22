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
    /// <summary>
    /// Select new active project, download projects, manage projects
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class StatusUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        public TextMeshProUGUI m_MessageText;
#pragma warning restore CS0649

        string m_CurrentMessage = String.Empty;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (data.statusMessage != String.Empty && !data.statusMessage.Equals(m_CurrentMessage))
            {
                m_MessageText.text = data.statusMessage;
                m_CurrentMessage = data.statusMessage;
            }
        }
    }
}
