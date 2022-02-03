using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Unity.Reflect.Markers.Domain.Controller
{
    public class MarkerAnchorer : MonoBehaviour, IMarkerAnchorer
    {
        ARSession m_ARSession;
        ARSessionOrigin m_SessionOrigin;
        ARAnchorManager m_AnchorManager;
        ARAnchor m_Anchor;
        MarkerController m_MarkerController;

        void Awake()
        {
            AttachRefrences();
        }

        void AttachRefrences()
        {
            if (m_ARSession == null)
            {
                m_ARSession = FindObjectOfType<ARSession>();
            }

            if (m_SessionOrigin == null)
            {
                m_SessionOrigin = FindObjectOfType<ARSessionOrigin>();
            }

            if (m_AnchorManager == null)
            {
                m_AnchorManager = FindObjectOfType<ARAnchorManager>();
                if (m_AnchorManager == null && m_SessionOrigin != null)
                {
                    m_AnchorManager = m_SessionOrigin.gameObject.AddComponent<ARAnchorManager>();
                }
            }

            if (m_MarkerController == null)
            {
                m_MarkerController = FindObjectOfType<MarkerController>();
                m_MarkerController.MarkerAnchorer = this;
            }
        }

        void OnDestroy()
        {
            if (m_Anchor)
                Destroy(m_Anchor.gameObject);
        }

        public GameObject Anchor(Pose location)
        {
            if (!m_Anchor)
            {
                var anchorObj = new GameObject("MarkerAnchor");
                m_Anchor = anchorObj.AddComponent<ARAnchor>();
            }
            m_Anchor.transform.position = location.position;
            m_Anchor.transform.rotation = location.rotation;
            return m_Anchor.gameObject;
        }
    }
}
