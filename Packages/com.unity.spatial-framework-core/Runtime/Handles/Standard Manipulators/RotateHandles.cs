using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    public class RotateHandles : StandardTransformHandleGroup
    {
        Quaternion m_TotalRotationAmount;

        protected override string LabelText()
        {
            m_TotalRotationAmount.ToAngleAxis(out var angle, out var axis);
            var axisAligned = transform.InverseTransformDirection(axis).IsAxisAligned();
            if (axisAligned)
            {
                var axisName = Mathf.Abs(axis.x) > Mathf.Abs(axis.y) ? Mathf.Abs(axis.x) > Mathf.Abs(axis.z) ? "x" : "z" :
                    Mathf.Abs(axis.y) > Mathf.Abs(axis.z) ? "y" : "z";

                return $"{axisName}: {angle:N0}\u00B0";
            }

            return $"{angle:N0}\u00B0";
        }

        protected override void OnHandleDragStarted(BaseHandle handle, HandleEventData eventData)
        {
            base.OnHandleDragStarted(handle, eventData);
            m_TotalRotationAmount = Quaternion.identity;
            if (m_TemporaryUIModule != null)
            {
                m_TemporaryUIModule.AddAngle(handle, () => m_HandleStartPosition, () => transform.position, () => handle.transform.position);
                m_TemporaryUIModule.AddGuideCircle(handle, () => transform.position, () => m_HandleStartPosition, () => handle.transform.up);
            }
        }

        protected override void OnHandleDragEnded(BaseHandle handle, HandleEventData eventData)
        {
            base.OnHandleDragEnded(handle, eventData);
            if (m_TemporaryUIModule != null)
            {
                m_TemporaryUIModule.RemoveAngle(handle);
                m_TemporaryUIModule.RemoveGuideCircle(handle);
            }
        }

        protected override void OnHandleDragging(BaseHandle handle, HandleEventData eventData)
        {
            base.OnHandleDragging(handle, eventData);

            DoRotate(eventData.deltaRotation);

            m_TotalRotationAmount *= eventData.deltaRotation;
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
                    var lookDirection = handleTransform.position - handleTransform.parent.position;
                    if (lookDirection != Vector3.zero)
                        handleTransform.rotation = Quaternion.LookRotation(lookDirection, handleTransform.up);
                }
            }
        }
    }
}
