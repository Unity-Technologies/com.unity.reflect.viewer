using Unity.SpatialFramework.Interaction;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    public class Scale1dHandles : StandardTransformHandleGroup
    {
        float m_TotalScalingAmount;

        protected override void OnHandleDragStarted(BaseHandle handle, HandleEventData eventData)
        {
            base.OnHandleDragStarted(handle, eventData);
            if (m_TemporaryUIModule != null)
            {
                m_TotalScalingAmount = 1f;
                var extent = m_HandleStartPosition - m_StartPosition;
                m_TemporaryUIModule.AddLineSegment(handle, () => m_StartPosition, () => m_StartPosition + extent * m_TotalScalingAmount);
                m_TemporaryUIModule.AddGuideLine(handle, () => handle.transform.position, () => handle.transform.forward);
            }
        }

        protected override void OnHandleDragEnded(BaseHandle handle, HandleEventData eventData)
        {
            base.OnHandleDragEnded(handle, eventData);
            if (m_TemporaryUIModule != null)
            {
                m_TemporaryUIModule.RemoveLineSegment(handle);
                m_TemporaryUIModule.RemoveGuideLine(handle);
            }
        }

        protected override void OnHandleDragging(BaseHandle handle, HandleEventData eventData)
        {
            base.OnHandleDragging(handle, eventData);
            var handleTransform = handle.transform;
            var inverseRotation = Quaternion.Inverse(handleTransform.rotation);

            var thisTransform = transform;
            var localStartDragPosition = inverseRotation
                * (m_HandleStartPosition - thisTransform.position);
            var delta = (inverseRotation * eventData.deltaPosition).z / localStartDragPosition.z;

            m_TotalScalingAmount += delta;

            DoScale(Quaternion.Inverse(thisTransform.rotation) * handleTransform.forward * delta);
        }

        protected override string LabelText()
        {
            return $"{m_TotalScalingAmount:N}x";
        }

        protected override Vector3 LabelPosition()
        {
            var localPosition = m_HandleStartPosition - m_StartPosition;
            var thisTransform = transform;
            var offset = m_LabelOffset * thisTransform.localScale.y;
            return thisTransform.position + localPosition + offset;
        }

        void Update()
        {
            if (!dragging && m_Camera != null)
            {
                var viewerPosition = m_Camera.transform.position;
                foreach (var handle in m_Handles)
                {
                    // Position the handles
                    var handleTransform = handle.transform;
                    var localPos = handleTransform.localPosition;
                    FlipPositionTowardsViewer(transform, viewerPosition, ref localPos);
                    handleTransform.localPosition = localPos;

                    // Hide handles at a bad angle
                    var viewerToHandle = handleTransform.position - viewerPosition;
                    if (Mathf.Abs(Vector3.Dot(handleTransform.forward, viewerToHandle.normalized)) > HandleSettings.instance.ViewParallelDotThreshold)
                        handle.selectionFlags = SelectionFlags.Direct;
                    else
                        handle.selectionFlags = SelectionFlags.Direct | SelectionFlags.Ray;
                }
            }
        }
    }
}
