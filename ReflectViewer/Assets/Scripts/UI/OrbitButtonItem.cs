using System;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class OrbitButtonItem : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ButtonControl m_ButtonControl;
        [SerializeField]
        OrbitType m_OrbitType;
#pragma warning restore CS0649

        public ButtonControl buttonControl => m_ButtonControl;

        public OrbitType orbitType => m_OrbitType;

        public event Action<OrbitType> orbitButtonClicked;

        void Awake()
        {
            m_ButtonControl.onControlTap.AddListener(OnButtonTapped);
        }

        void OnButtonTapped(BaseEventData eventData)
        {
            orbitButtonClicked?.Invoke(m_OrbitType);
        }
    }
}
