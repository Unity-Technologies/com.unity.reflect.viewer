using Unity.SpatialFramework.Interaction;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    public class Translate1dHandles : StandardTransformHandleGroup
    {
        protected override void OnHandleDragStarted(BaseHandle handle, HandleEventData eventData)
        {
            base.OnHandleDragStarted(handle, eventData);
            if (m_TemporaryUIModule != null)
            {
                m_TemporaryUIModule.AddLineSegment(handle, () => m_StartPosition, () => transform.position);
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
            DoTranslate(eventData.deltaPosition);
        }

        protected override string LabelText()
        {
            var totalTranslationAmount = Vector3.Distance(transform.position, m_StartPosition);
            return $"{totalTranslationAmount:N2}m";
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

                    // Rotate the handles
                    Vector3 forward;
                    if (handle.hasDragSource)
                    {
                        forward = handleTransform.position - m_HandleStartPosition;
                        if (forward == Vector3.zero)
                            continue;
                    }
                    else
                    {
                        forward = handleTransform.localPosition;
                    }

                    handleTransform.localRotation = Quaternion.LookRotation(forward, Vector3.up);

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
