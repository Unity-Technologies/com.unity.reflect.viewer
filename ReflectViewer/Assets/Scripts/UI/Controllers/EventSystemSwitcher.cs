using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class EventSystemSwitcher : MonoBehaviour
    {
        InputSystemUIInputModule m_InputSystemUIInputModule;
        XRUIInputModule m_XruiInputModule;

        UIStateData? m_CachedUIStateData;
        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            m_InputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();
            m_XruiInputModule = GetComponent<XRUIInputModule>();
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (data == m_CachedUIStateData)
            {
                return;
            }

            m_CachedUIStateData = data;
            if (m_CachedUIStateData.Value.VREnable)
            {
                m_InputSystemUIInputModule.enabled = false;
                m_XruiInputModule.enabled = true;
            }
            else
            {
                m_InputSystemUIInputModule.enabled = true;
                m_XruiInputModule.enabled = false;
            }

        }
    }
}
