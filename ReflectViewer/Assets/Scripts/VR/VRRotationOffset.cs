using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;

namespace UnityEngine.Reflect.Viewer
{
    public class VRRotationOffset : MonoBehaviour
    {
        [SerializeField]
        Vector3 m_Offset;
        [SerializeField]
        bool m_IsLeftHand;

        void Start()
        {
            StartCoroutine(InitController());
        }

        void ResetPosition()
        {
            var hmd = InputDevices.GetDeviceAtXRNode(m_IsLeftHand?XRNode.LeftHand:XRNode.RightHand);
            hmd.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion handRotation);
            transform.rotation = handRotation;

            // Rotation offset added the current rotation
            transform.localRotation *= Quaternion.Euler(m_Offset);
        }

        IEnumerator InitController()
        {
            yield return new WaitForEndOfFrame();
            ResetPosition();
        }
    }
}
