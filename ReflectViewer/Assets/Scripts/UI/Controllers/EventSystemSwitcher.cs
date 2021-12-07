using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class EventSystemSwitcher: MonoBehaviour
    {
        InputSystemUIInputModule m_InputSystemUIInputModule;
        XRUIInputModule m_XRUIInputModule;
        IUISelector m_VREnableSelector;

        void Awake()
        {
            m_InputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();
            m_XRUIInputModule = GetComponent<XRUIInputModule>();
            m_VREnableSelector = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable), OnVREnableChanged);
        }

        void OnDestroy()
        {
            m_VREnableSelector?.Dispose();
        }

        void OnVREnableChanged(bool newData)
        {
            m_InputSystemUIInputModule.enabled = !newData;
            m_XRUIInputModule.enabled = newData;
        }
    }
}
