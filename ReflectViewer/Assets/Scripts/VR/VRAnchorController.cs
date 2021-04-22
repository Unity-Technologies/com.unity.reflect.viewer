using System.Collections.Generic;
using Unity.Reflect.Viewer.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace UnityEngine.Reflect.Viewer
{
    public class VRAnchorController : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] float m_UiCanvasDistance = 1f;
        [SerializeField] float m_UiCanvasSize = 0.0008f;
        [SerializeField] int m_UiCanvasWidthOverride = 850;
        [SerializeField] XRRig m_XrRig;
        #pragma warning restore 0649

        Transform m_ReflectTransform;
        Canvas m_RootCanvas;
        Transform m_OriginalRootCanvasParent;
        List<TrackedDeviceGraphicRaycaster> m_TrackedDeviceGraphicRaycasters;
        List<VRAnchor.DeviceAlignmentAnchor> m_Anchors;
        List<VRAnchor> m_VrAnchors;

        bool m_WasPressed;
        CameraTransformInfo m_CameraTransformInfo;

        void Awake()
        {
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;

            if (m_XrRig == null)
                m_XrRig = FindObjectOfType<XRRig>();

            m_RootCanvas = FindObjectOfType<UIStateManager>().GetComponent<Canvas>();
            m_OriginalRootCanvasParent = m_RootCanvas.transform.parent;
            m_TrackedDeviceGraphicRaycasters = new List<TrackedDeviceGraphicRaycaster>();
            m_VrAnchors = new List<VRAnchor>();
            m_Anchors = new List<VRAnchor.DeviceAlignmentAnchor>();
        }

        public void Load()
        {
            // add tracked device graphic raycasters
            var canvases = m_RootCanvas.GetComponentsInChildren<Canvas>(true);
            foreach (var canvas in canvases)
                m_TrackedDeviceGraphicRaycasters.Add(canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>());

            // reparent main UI canvas under VR camera
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError($"[{typeof(VRAnchorController)}] main camera not found!");
                return;
            }

            m_RootCanvas.renderMode = RenderMode.WorldSpace;
            m_RootCanvas.worldCamera = cam;

            if (!(m_RootCanvas.transform is RectTransform rootTransform))
                return;

            rootTransform.SetParent(cam.transform);
            rootTransform.localPosition = new Vector3(0f, 0f, m_UiCanvasDistance);
            rootTransform.localRotation = Quaternion.identity;
            rootTransform.localScale = Vector3.one * m_UiCanvasSize;
            rootTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_UiCanvasWidthOverride);

            // reparent anchors
            m_Anchors.Clear();
            var controllers = m_XrRig.GetComponentsInChildren<VRControllerWidget>(true);
            foreach (var controller in controllers)
            {
                m_Anchors.AddRange(controller.Anchors);
            }
            m_RootCanvas.GetComponentsInChildren(true, m_VrAnchors);
            foreach (var anchor in m_VrAnchors)
                anchor.Attach(m_Anchors);
        }

        public void Unload()
        {
            // restore anchors backward to avoid misalignment
            for (int i = m_VrAnchors.Count - 1; i >= 0; i--)
            {
                m_VrAnchors[i].Restore();
            }

            // restore original canvas parent
            m_RootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // set parent to current scene root object first in case original parent is null
            // this ensures the canvas won't stay in the VR scene (and therefore destroyed)
            if (m_OriginalRootCanvasParent == null)
                m_RootCanvas.transform.SetParent(SceneManager.GetActiveScene().GetRootGameObjects()[0].transform, true);
            m_RootCanvas.transform.SetParent(m_OriginalRootCanvasParent, false);

            // remove tracked device graphic raycasters
            foreach (var trackedDeviceGraphicRaycaster in m_TrackedDeviceGraphicRaycasters)
                Destroy(trackedDeviceGraphicRaycaster);
            m_TrackedDeviceGraphicRaycasters.Clear();
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            if (m_XrRig == null || m_CameraTransformInfo == data.cameraTransformInfo)
                return;

            m_XrRig.MoveCameraToWorldLocation(data.cameraTransformInfo.position);
            var rotation = data.cameraTransformInfo.rotation.y - m_XrRig.transform.eulerAngles.y;
            m_XrRig.RotateAroundCameraUsingRigUp(rotation);

            m_CameraTransformInfo = data.cameraTransformInfo;
        }
    }
}
