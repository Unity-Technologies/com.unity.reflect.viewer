using System;
using Unity.SpatialFramework.Handles;
using Unity.SpatialFramework.UI.Layout;
using UnityEngine;

namespace UnityEngine.Reflect.MeasureTool
{
    public class VRMeasureToolController : MonoBehaviour
    {
        [SerializeField]
        GameObject m_VRCursor;
        [SerializeField]
        ZoneScale m_ZoneScale;

        UIMeasureToolController m_UIMeasureToolController;
        BaseHandle m_BaseHandleCursorA;
        BaseHandle m_BaseHandleCursorB;
        GameObject m_VRCursorA;
        GameObject m_VRCursorB;
        bool m_IsDragging;

        void Awake()
        {
            m_UIMeasureToolController = GetComponent<UIMeasureToolController>();
        }

        public void InitVR()
        {
            if (m_VRCursorA != null || m_VRCursorB != null)
                return;

            m_VRCursorA = Instantiate(m_VRCursor);
            m_VRCursorB = Instantiate(m_VRCursor);

            m_BaseHandleCursorA = m_VRCursorA.GetComponent<BaseHandle>();
            m_BaseHandleCursorB = m_VRCursorB.GetComponent<BaseHandle>();

            InitHandle(ref m_BaseHandleCursorA);
            InitHandle(ref m_BaseHandleCursorB);

            m_ZoneScale.enabled = true;
            m_UIMeasureToolController.SetCurrentCursor(ref m_VRCursorA, ref m_VRCursorB);
        }

        void InitHandle(ref BaseHandle handle)
        {
            if (handle == null)
                return;

            handle.dragging += OnPanelHandleDragging;
            handle.dragEnded += OnEndDragging;
            handle.hoverStarted += OnHoverHandleStart;
            handle.hoverEnded += OnHoverHandleEnded;
        }

        void ReleaseHandle(ref BaseHandle handle)
        {
            if (handle == null)
                return;

            handle.dragging -= OnPanelHandleDragging;
            handle.dragEnded -= OnEndDragging;
            handle.hoverStarted -= OnHoverHandleStart;
            handle.hoverEnded -= OnHoverHandleEnded;
        }

        void OnEndDragging(BaseHandle handle, HandleEventData eventData)
        {
            m_IsDragging = false;
        }

        void OnHoverHandleEnded(BaseHandle handle, HandleEventData eventData)
        {
            if (!m_IsDragging)
                m_UIMeasureToolController.UnselectVRCursor();
        }

        void OnHoverHandleStart(BaseHandle handle, HandleEventData eventData)
        {
            if (!m_IsDragging)
                m_UIMeasureToolController.SelectVRCursor(handle.gameObject);
        }

        void OnPanelHandleDragging(BaseHandle handle, HandleEventData eventData)
        {
            m_UIMeasureToolController.OnDrag(eventData.rayOrigin.position);
            m_IsDragging = true;
        }

        public void OnReset()
        {
            Destroy(m_VRCursorA);
            Destroy(m_VRCursorB);
            m_ZoneScale.enabled = false;
        }

        void OnDestroy()
        {
            ReleaseHandle(ref m_BaseHandleCursorA);
            ReleaseHandle(ref m_BaseHandleCursorB);
        }
    }
}
