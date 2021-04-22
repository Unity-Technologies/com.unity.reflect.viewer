using System;
using System.Collections.Generic;
using Unity.SpatialFramework.UI;
using Unity.SpatialFramework.UI.Layout;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    public class StandardTransformHandleGroup : MonoBehaviour, IManipulator
    {
#pragma warning disable 649
        [SerializeField, Tooltip("References to all the handles in this group.")]
        protected List<BaseHandle> m_Handles;
#pragma warning restore 649

        protected Camera m_Camera;
        protected Vector3 m_StartPosition;
        protected Vector3 m_HandleStartPosition;
        protected Vector3 m_LabelOffset = Vector3.up * 0.1f;
        protected BaseHandle m_ActiveHandle;

        protected TemporaryUIModule m_TemporaryUIModule;
        protected ZoneScale m_ZoneScale;

        public event Action<Vector3> translate;
        public event Action<Quaternion> rotate;
        public event Action<Vector3> scale;

        public bool dragging { get; set; }

        public event Action dragStarted;
        public event Action dragEnded;


        protected virtual void Awake()
        {
            m_Camera = Camera.main;

            foreach (var handle in m_Handles)
            {
                handle.dragStarted += OnHandleDragStarted;
                handle.dragging += OnHandleDragging;
                handle.dragEnded += OnHandleDragEnded;
            }

            m_TemporaryUIModule = ModuleLoaderCore.instance.GetModule<TemporaryUIModule>();
            if (m_TemporaryUIModule == null)
                Debug.LogWarning($"No temporary UI module loaded, the transform handles {gameObject.name} will not create temporary ui such as labels and guide lines.");

            m_ZoneScale = GetComponent<ZoneScale>();
        }

        void IManipulator.SetTargetTransforms(Transform[] targetTransforms, Transform activeTargetTransform)
        {
            m_ZoneScale.Snap();
        }

        void IManipulator.SetPose(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        protected virtual void OnHandleDragging(BaseHandle handle, HandleEventData eventData)
        {
        }

        protected virtual void OnHandleDragStarted(BaseHandle handle, HandleEventData eventData)
        {
            m_HandleStartPosition = handle.transform.position;
            m_StartPosition = transform.position;
            m_ActiveHandle = handle;

            dragging = true;

            if (dragStarted != null)
                dragStarted();

            if (m_TemporaryUIModule != null)
                m_TemporaryUIModule.AddLabel(handle, LabelText, LabelPosition);

            m_ZoneScale.enabled = false;
        }

        protected virtual void OnHandleDragEnded(BaseHandle handle, HandleEventData eventData)
        {
            dragging = false;
            dragEnded?.Invoke();

            if (m_TemporaryUIModule != null)
                m_TemporaryUIModule.RemoveLabel(handle);

            m_ZoneScale.enabled = true;
        }

        protected virtual Vector3 LabelPosition()
        {
            var localPosition = m_HandleStartPosition - m_StartPosition;
            var groupTransform = transform;
            var offset = m_LabelOffset * groupTransform.localScale.y;
            return groupTransform.position + localPosition + offset;
        }

        protected virtual string LabelText()
        {
            var totalTranslationAmount = Vector3.Distance(transform.position, m_StartPosition);
            return $"{totalTranslationAmount:N2}m";
        }

        protected static void FlipPositionTowardsViewer(Transform t, Vector3 viewerPosition, ref Vector3 localPos,
            bool negativeX = false, bool negativeY = false, bool negativeZ = false)
        {
            // Get the offset vector from the pivot to the viewer
            var centerToViewer = viewerPosition - t.position;

            // For each axis, check if the viewer is on the positive side (dot product greater than zero)
            // If not, negate the referenced local position.
            // Negate it again if intentionally targeting the negative side of the axis.
            var xVisible = Vector3.Dot(centerToViewer, t.right) > 0f;
            localPos.x = Mathf.Abs(localPos.x) * (xVisible ? 1 : -1);
            localPos.x *= negativeX ? -1 : 1;

            var yVisible = Vector3.Dot(centerToViewer, t.up) > 0f;
            localPos.y = Mathf.Abs(localPos.y) * (yVisible ? 1 : -1);
            localPos.y *= negativeY ? -1 : 1;

            var zVisible = Vector3.Dot(centerToViewer, t.forward) > 0f;
            localPos.z = Mathf.Abs(localPos.z) * (zVisible ? 1 : -1);
            localPos.z *= negativeZ ? -1 : 1;
        }

        protected void DoTranslate(Vector3 value) => translate?.Invoke(value);
        protected void DoRotate(Quaternion value) => rotate?.Invoke(value);
        protected void DoScale(Vector3 value) => scale?.Invoke(value);
    }
}
