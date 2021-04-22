using System;
using System.Collections.Generic;
using Unity.Reflect.Viewer.UI;
using UnityEngine;

namespace UnityEngine.Reflect.Viewer
{
    public class VRControllerWidget : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        Transform m_Models;

        [SerializeField]
        List<VRAnchor.DeviceAlignmentAnchor> m_Anchors;
#pragma warning restore 0649

        public List<VRAnchor.DeviceAlignmentAnchor> Anchors => m_Anchors;

        public Vector3 Rotation
        {
            set
            {
                m_Models.transform.rotation = Quaternion.Euler(value);
            }
        }

        void Start()
        {
            var mainCam = Camera.main;
            foreach (var anchor in m_Anchors)
            {
                var canvas = anchor.transform.GetComponent<Canvas>();
                canvas.worldCamera = mainCam;
            }
        }
    }
}
