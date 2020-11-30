using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Viewer.UI;
using UnityEngine;

namespace Unity.Reflect.Viewer
{
    public class ActiveOnlyInDebug : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        GameObject[] m_DebugGameObjects;
#pragma warning restore CS0649


        bool? m_CachedAxisTrackingEnabled;

        void Start()
        {
            UIStateManager.debugStateChanged += OnDebugStateChanged;

            foreach (var go in m_DebugGameObjects)
            {
                go.SetActive(UIStateManager.current.debugStateData.debugOptionsData.ARAxisTrackingEnabled);
            }
        }

        void OnDebugStateChanged(UIDebugStateData data)
        {
            if (m_CachedAxisTrackingEnabled == data.debugOptionsData.ARAxisTrackingEnabled)
                return;


            foreach (var go in m_DebugGameObjects)
            {
                go.SetActive(data.debugOptionsData.ARAxisTrackingEnabled);
            }

            m_CachedAxisTrackingEnabled = data.debugOptionsData.ARAxisTrackingEnabled;
        }

        void OnDestroy()
        {
            UIStateManager.debugStateChanged -= OnDebugStateChanged;
        }
    }
}
