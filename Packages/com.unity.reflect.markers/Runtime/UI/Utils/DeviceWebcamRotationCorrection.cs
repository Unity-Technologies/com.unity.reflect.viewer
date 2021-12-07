using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Markers.UI
{
    /// <summary>
    /// Attach to a RawImage webcam feed
    /// Fixes mirroring and rotation from the visible webcam output based on device rotation
    /// </summary>
    public class DeviceWebcamRotationCorrection : MonoBehaviour
    {
        [SerializeField]
        RectTransform m_ImageContainer;

        void Start()
        {
            if (m_ImageContainer == null)
                m_ImageContainer = GetComponent<RectTransform>();
        }

        void Update()
        {
            // @@TODO: Add portrait orientations.
#if UNITY_ANDROID
            if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft) // Home button on right
            {
                m_ImageContainer.localScale = new Vector3(1, 1, 1);
            }
            if (Input.deviceOrientation == DeviceOrientation.LandscapeRight) // Home Button on left
            {
                m_ImageContainer.localScale = new Vector3(-1, -1, 1);
            }
#endif
#if UNITY_IOS
            if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft) // Home Button on right
            {
                m_ImageContainer.localScale = new Vector3(1, -1, 1);
            }
            if (Input.deviceOrientation == DeviceOrientation.LandscapeRight) // Home Button on left
            {
                m_ImageContainer.localScale = new Vector3(-1, 1, 1);
            }
#endif
        }
    }
}
