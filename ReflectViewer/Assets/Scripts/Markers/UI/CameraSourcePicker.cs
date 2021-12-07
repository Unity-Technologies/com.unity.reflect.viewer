using System;
using Unity.Reflect.Markers.Camera;
using Unity.Reflect.Markers.Domain.Controller;
using UnityEngine;

namespace Unity.Reflect.Viewer
{
    /// <summary>
    /// Determines the camera source to use for marker barcode scanning.
    /// </summary>
    public class CameraSourcePicker : MonoBehaviour
    {
        [SerializeField]
        MarkerController m_MarkerController;

        [SerializeField]
        WebCameraSource m_WebCameraSource;

        XRCameraSource m_XRCameraSource;

        public XRCameraSource XRCameraSource
        {
            get => m_XRCameraSource;
            set
            {
                m_XRCameraSource = value;
                if (value == null)
                {
                    m_MarkerController.CameraSource = m_WebCameraSource;
                }
                else
                {
                    m_MarkerController.CameraSource = m_XRCameraSource;
                }
            }
        }

        void Start()
        {
            XRCameraSource = null;
        }
    }
}
