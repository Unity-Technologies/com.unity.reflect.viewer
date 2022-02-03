using System;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer
{
    public class ActiveOnlyInDebug : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        GameObject[] m_DebugGameObjects;
#pragma warning restore CS0649

        IUISelector<bool> m_axisTrackingEnabledSelector;

        void Awake()
        {
            m_axisTrackingEnabledSelector = UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.ARAxisTrackingEnabled),
                (active) =>
                {
                    foreach (var go in m_DebugGameObjects)
                    {
                        go.SetActive(active);
                    }
                });

            var trackingEnabled = m_axisTrackingEnabledSelector.GetValue();
            foreach (var go in m_DebugGameObjects)
            {
                go.SetActive(trackingEnabled);
            }

        }

        void OnDestroy()
        {
            m_axisTrackingEnabledSelector?.Dispose();
        }
    }
}
