using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleGraphicSwitch : MonoBehaviour
    {
        [SerializeField]
        Image m_OnGraphic;
        [SerializeField]
        Image m_OffGraphic;
        Toggle m_Toggle;

        void Start()
        {
            m_Toggle = GetComponent<Toggle>();
            m_Toggle.onValueChanged.AddListener(HandleToggleInput);
            HandleToggleInput(m_Toggle.isOn);
        }

        void OnDestroy()
        {
            if (m_Toggle)
                m_Toggle.onValueChanged.RemoveListener(HandleToggleInput);
        }

        void HandleToggleInput(bool value)
        {
            if (m_OnGraphic)
                m_OnGraphic.enabled = value;
            if (m_OffGraphic)
                m_OffGraphic.enabled = !value;
        }
    }
}
