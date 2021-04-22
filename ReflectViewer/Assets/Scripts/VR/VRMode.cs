using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Reflect.Viewer.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Management;

namespace UnityEngine.Reflect.Viewer
{
    [Serializable]
    public enum VRControllerType
    {
        Generic = 0,
        OculusTouch,
        OculusTouchS,
        OculusTouchQuest2,
        ViveWand,
        ViveIndex,
        ViveCosmos
    }

    [Serializable]
    public class VRController
    {
        public VRControllerType Type;
        public Transform LeftPrefab;
        public Transform RightPrefab;
        public Vector3 Rotation;
    }

    public class VRMode : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        InputActionAsset m_InputActionAsset;
        [SerializeField]
        VRAnchorController m_VrAnchorController;
        [SerializeField]
        TransformVariable m_RightController;
        [SerializeField]
        bool m_SkipVrInit;
        [SerializeField]
        XRBaseController m_LeftHandController;
        [SerializeField]
        XRBaseController m_RightHandController;
        [SerializeField]
        List<VRController> m_VRControllers;
        [Space(10)]
        [SerializeField]
        List<string> m_XRManagerNames;
        [SerializeField]
        List<GameObject> m_XRManagerObjectsToInstantiate;
        [Space(10)]
#pragma warning restore 0649
        XRManagerSettings m_Manager;
        Camera m_ScreenModeCamera;
        float m_ScreenModeFieldOfView;
        InputActionMap m_ActionMap;

        void Start()
        {
            m_Manager = XRGeneralSettings.Instance.Manager;
            m_ScreenModeCamera = Camera.main;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;
            m_RightController.Value = m_RightHandController.transform;

            var standaloneInputModule = FindObjectOfType<StandaloneInputModule>();
            if (standaloneInputModule != null)
                Destroy(standaloneInputModule);

            m_ActionMap = m_InputActionAsset.FindActionMap("VR", true);
            m_ActionMap.Enable();

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
            m_ScreenModeFieldOfView = m_ScreenModeCamera.fieldOfView;

            if (!m_SkipVrInit)
            {
                m_Manager.DeinitializeLoader();

                // initialize VR
                yield return m_Manager.InitializeLoader();

                if (m_Manager.activeLoader == null)
                {
                    Debug.LogError("VR initialization failed!");
                    yield break;
                }

                // start VR subsystems
                m_Manager.StartSubsystems();

                var xRManagerIndex = m_XRManagerNames.IndexOf(m_Manager.activeLoader.name);
                if (xRManagerIndex != -1 && m_XRManagerObjectsToInstantiate[xRManagerIndex] != null)
                {
                    Instantiate(m_XRManagerObjectsToInstantiate[xRManagerIndex]);
                }
            }

            // wait to let InputDevice initialise
            for (int i = 120; i >= 0; i--)
            {
                if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand) != null &&
                    !string.IsNullOrEmpty(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).name))
                {
                    break;
                }

                yield return null;
            }

            ChooseVRControllerModels();

            yield return null;

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

            // SteamVR might change the main FoV, so make sure to set it to the default
            m_ScreenModeCamera.fieldOfView = m_ScreenModeFieldOfView;

            // swap cameras
            m_ScreenModeCamera.gameObject.SetActive(true);

            if (m_ActionMap != null)
            {
                m_ActionMap.Disable();
            }

            UIStateManager.projectStateChanged -= OnProjectStateDataChanged;
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (m_ActionMap == null)
            {
                m_ActionMap = m_InputActionAsset.FindActionMap("VR", true);
            }

            m_ActionMap.Enable();
        }

        void ChooseVRControllerModels()
        {
            var type = VRControllerType.Generic;

            var deviceName = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).name;

            Debug.Log($"Device name : {deviceName}");
            if (!string.IsNullOrEmpty(deviceName))
            {
                deviceName = Regex.Replace(deviceName.ToLower(), @"\s+", "");
                if (deviceName.Contains("quest2"))
                {
                    //TODO: Not supported yet
                    type = VRControllerType.Generic;
                    //type = VRControllerType.OculusTouchQuest2;
                }
                else if (deviceName.Contains("rifts") ||
                    deviceName.Contains("touchs") ||
                    deviceName.Contains("quest"))
                {
                    //TODO: Not supported yet
                    type = VRControllerType.Generic;
                    //type = VRControllerType.OculusTouchS;
                }
                else if (deviceName.Contains("oculus") ||
                    deviceName.Contains("rift") ||
                    deviceName.Contains("touch"))
                {
                    type = VRControllerType.OculusTouch;
                }
                else if (deviceName.Contains("index") ||
                    deviceName.Contains("knuckle"))
                {
                    //TODO: Not supported yet
                    type = VRControllerType.ViveIndex;
                }
                else if (deviceName.Contains("cosmos"))
                {
                    //TODO: Not supported yet
                    type = VRControllerType.ViveCosmos;
                }
                else if (deviceName.Contains("vive") ||
                    deviceName.Contains("valve"))
                {
                    type = VRControllerType.ViveWand;
                }
                else
                {
                    type = VRControllerType.Generic;
                }
            }

            Debug.Log(type);

            var vrController = m_VRControllers.FirstOrDefault(c => c.Type == type);
            if (vrController != null)
            {
                var left = Instantiate(vrController.LeftPrefab, m_LeftHandController.transform);
                var right = Instantiate(vrController.RightPrefab, m_RightHandController.transform);

                var leftWidget = left.GetComponent<VRControllerWidget>();
                var rightWidget = right.GetComponent<VRControllerWidget>();

                leftWidget.Rotation = vrController.Rotation;
                rightWidget.Rotation = vrController.Rotation;
            }
        }
    }
}
