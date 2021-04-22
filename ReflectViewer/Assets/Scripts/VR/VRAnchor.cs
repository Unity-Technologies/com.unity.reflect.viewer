using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class VRAnchor : MonoBehaviour
    {
        public enum Device { Head, LeftController, RightController }
        public enum Alignment { Up, Left, Down, Right }

        [Serializable]
        public class DeviceAlignmentAnchor
        {
            public Device device;
            public Alignment alignment;
            public Transform transform;
        }

        static readonly Dictionary<Alignment, Vector2> k_AnchorMinMaxPivots = new Dictionary<Alignment, Vector2>
        {
            { Alignment.Up, new Vector2(0.5f, 0f) },
            { Alignment.Left, new Vector2(1f, 0.5f) },
            { Alignment.Down, new Vector2(0.5f, 1f) },
            { Alignment.Right, new Vector2(0f, 0.5f) },
        };

#pragma warning disable 0649
        [SerializeField] bool m_HideInVr;
        [SerializeField] Device m_Device;
        [SerializeField] Alignment m_Alignment;
        [SerializeField] Vector3 m_PositionOffset;
#pragma warning restore 0649

        bool m_WasActive;

        RectTransform m_RectTransform;
        Transform m_InitialParent;
        int m_SiblingIndex;

        Vector2 m_AnchorMin;
        Vector2 m_AnchorMax;
        Vector2 m_Pivot;

        Vector3 m_Position;
        Vector3 m_Rotation;
        Vector3 m_Scale;

        UnsafeAreaFiller m_Filler;
        bool m_WasFillerEnabled;

        void Start()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_Filler = GetComponent<UnsafeAreaFiller>();
        }

        public void Attach(List<DeviceAlignmentAnchor> anchors)
        {
            if (m_Filler != null)
            {
                m_WasFillerEnabled = m_Filler.enabled;
                m_Filler.enabled = false;
            }

            if (m_HideInVr)
            {
                m_WasActive = gameObject.activeSelf;
                gameObject.SetActive(false);
                return;
            }

            var anchor = anchors.Find(x => x.device == m_Device && x.alignment == m_Alignment);
            if (anchor == null)
            {
                Debug.LogError($"[{nameof(VRAnchor)}] anchor for {m_Device} device with {m_Alignment} alignment not found!");
                return;
            }

            m_InitialParent = m_RectTransform.parent;
            m_SiblingIndex = m_RectTransform.GetSiblingIndex();

            m_AnchorMin = m_RectTransform.anchorMin;
            m_AnchorMax = m_RectTransform.anchorMax;
            m_Pivot = m_RectTransform.pivot;

            m_Position = m_RectTransform.localPosition;
            m_Rotation = m_RectTransform.localEulerAngles;
            m_Scale = m_RectTransform.localScale;

            m_RectTransform.SetParent(anchor.transform);

            // only change if anchors are equal and therefore not stretched to fit
            if (m_RectTransform.anchorMin == m_RectTransform.anchorMax)
                m_RectTransform.anchorMin = m_RectTransform.anchorMax = k_AnchorMinMaxPivots[m_Alignment];
            m_RectTransform.pivot = k_AnchorMinMaxPivots[m_Alignment];

            m_RectTransform.localPosition = m_PositionOffset;
            m_RectTransform.localEulerAngles = Vector3.zero;
            m_RectTransform.localScale = Vector3.one;
        }

        public void Restore()
        {
            if (m_Filler != null)
                m_Filler.enabled = m_WasFillerEnabled;

            if (m_HideInVr)
            {
                gameObject.SetActive(m_WasActive);
                return;
            }

            m_RectTransform.SetParent(m_InitialParent);
            m_RectTransform.SetSiblingIndex(m_SiblingIndex);

            m_RectTransform.anchorMin = m_AnchorMin;
            m_RectTransform.anchorMax = m_AnchorMax;
            m_RectTransform.pivot = m_Pivot;

            m_RectTransform.localPosition = m_Position;
            m_RectTransform.localEulerAngles = m_Rotation;
            m_RectTransform.localScale = m_Scale;
        }
    }
}
