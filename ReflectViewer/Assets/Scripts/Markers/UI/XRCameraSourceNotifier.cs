using System;
using Unity.Reflect.Markers;
using Unity.Reflect.Markers.Camera;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Unity.Reflect.Viewer
{
    /// <summary>
    /// Notifies the CameraSourcePicker that the XRCamera is available.
    /// </summary>
    public class XRCameraSourceNotifier : MonoBehaviour
    {
        [SerializeField]
        XRCameraSource m_XRCameraSource;
        CameraSourcePicker m_SourcePicker;

        void Start()
        {
            var manager = FindObjectOfType<ARCameraManager>();
            if (!manager)
            {
                Debug.LogError("ARCameraManager not available");
                return;
            }

            m_SourcePicker = FindObjectOfType<CameraSourcePicker>();
            m_SourcePicker.XRCameraSource = m_XRCameraSource;
        }

        void OnDestroy()
        {
            m_SourcePicker.XRCameraSource = null;
        }
    }
}
