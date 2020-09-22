using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.XR.Management;

namespace UnityEngine.Reflect.Viewer
{
    public class VRMode : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] VRAnchorController m_VrAnchorController;
        [SerializeField] bool m_SkipVrInit;
        #pragma warning restore 0649

        XRManagerSettings m_Manager;
        Camera m_ScreenModeCamera;

        void Start()
        {
            m_Manager = XRGeneralSettings.Instance.Manager;
            m_ScreenModeCamera = Camera.main;

            var standaloneInputModule = FindObjectOfType<StandaloneInputModule>();
            if (standaloneInputModule != null)
                Destroy(standaloneInputModule);

            StartCoroutine(Load());
        }

        void OnDestroy()
        {
            Unload();
        }

        IEnumerator Load()
        {
            // swap cameras
            m_ScreenModeCamera.gameObject.SetActive(false);

            if (!m_SkipVrInit)
            {
                // initialize VR
                yield return m_Manager.InitializeLoader();

                if (m_Manager.activeLoader == null)
                {
                    Debug.LogError("VR initialization failed!");
                    yield break;
                }

                // start VR subsystems
                m_Manager.StartSubsystems();
            }

            m_VrAnchorController.Load();
        }

        void Unload()
        {
            m_VrAnchorController.Unload();

            if (!m_SkipVrInit)
            {
                // stop VR subsystems
                m_Manager.StopSubsystems();

                // deinitialize VR
                m_Manager.DeinitializeLoader();
            }

            // swap cameras
            m_ScreenModeCamera.gameObject.SetActive(true);
        }
    }
}
