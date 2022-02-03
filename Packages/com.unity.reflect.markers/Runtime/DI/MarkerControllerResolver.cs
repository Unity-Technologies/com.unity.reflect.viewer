using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.ImageTracker;
using UnityEngine;

namespace Unity.Reflect.Markers.DI
{
    public class MarkerControllerResolver : MonoBehaviour
    {
        IMarkerController m_MarkerController;
        [SerializeField]
        ImageTrackerBase m_ImageTrackerBase;

        public IMarkerController MarkerController
        {
            get
            {
                if (m_MarkerController == null)
                    m_MarkerController = FindObjectOfType<MarkerController>();
                return m_MarkerController;
            }
        }

        void Start()
        {
            m_ImageTrackerBase.markerController = FindObjectOfType<MarkerController>();
        }
    }
}
