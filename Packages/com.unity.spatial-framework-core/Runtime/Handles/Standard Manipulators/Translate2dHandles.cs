using System.Text;
using Unity.SpatialFramework.Interaction;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    public class Translate2dHandles : StandardTransformHandleGroup
    {
        const float k_LabelMinAmount = 0.01f;

        protected override void OnHandleDragStarted(BaseHandle handle, HandleEventData eventData)
        {
            base.OnHandleDragStarted(handle, eventData);
            if (m_TemporaryUIModule != null)
                m_TemporaryUIModule.AddLineRect(handle, () => m_StartPosition, () => transform.position, () => handle.transform.rotation);
        }

        protected override void OnHandleDragEnded(BaseHandle handle, HandleEventData eventData)
        {
            base.OnHandleDragEnded(handle, eventData);
            if (m_TemporaryUIModule != null)
                m_TemporaryUIModule.RemoveLineRect(handle);
        }

        protected override void OnHandleDragging(BaseHandle handle, HandleEventData eventData)
        {
            base.OnHandleDragging(handle, eventData);
            DoTranslate(eventData.deltaPosition);
        }

        protected override string LabelText()
        {
            var totalTranslationAmount = transform.position - m_StartPosition;
            totalTranslationAmount = transform.InverseTransformVector(totalTranslationAmount);

            var stringBuilder = new StringBuilder();
            if (Mathf.Abs(totalTranslationAmount.x) > k_LabelMinAmount)
                stringBuilder.Append($"x:{totalTranslationAmount.x:N2}m\n");
            if (Mathf.Abs(totalTranslationAmount.y) > k_LabelMinAmount)
                stringBuilder.Append($"y:{totalTranslationAmount.y:N2}m\n");
            if (Mathf.Abs(totalTranslationAmount.z) > k_LabelMinAmount)
                stringBuilder.Append($"z:{totalTranslationAmount.z:N2}m\n");

            if (stringBuilder.Length > 0) // Remove extra new line
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }

            return stringBuilder.ToString();
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
                    if (Mathf.Abs(Vector3.Dot(handleTransform.forward, viewerToHandle.normalized)) < HandleSettings.instance.ViewPerpendicularDotThreshold)
                        handle.selectionFlags = SelectionFlags.Direct;
                    else
                        handle.selectionFlags = SelectionFlags.Direct | SelectionFlags.Ray;

                }
            }
        }
    }
}
