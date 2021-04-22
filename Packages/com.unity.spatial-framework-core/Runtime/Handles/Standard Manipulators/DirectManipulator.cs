using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// A manipulator that causes a target object to directly follow the movement of the pointer dragging on its handles
    /// </summary>
    public class DirectManipulator : MonoBehaviour, IManipulator
    {
        [SerializeField, Tooltip("The target objects that are directly moved by the handles.")]
        List<Transform> m_DirectTransformTargets;

        [SerializeField, Tooltip("The handles that can be dragged to perform the manipulation")]
        List<BaseHandle> m_AllHandles = new List<BaseHandle>();

        /// <summary>
        /// The target object that is directly manipulated via the handle. This is the first item in the list of all targets
        /// </summary>
        public Transform target
        {
            get
            {
                if (m_DirectTransformTargets == null || m_DirectTransformTargets.Count == 0)
                    return null;

                return m_DirectTransformTargets[0];
            }
            set
            {
                if (m_DirectTransformTargets == null)
                {
                    m_DirectTransformTargets = new List<Transform>();
                }

                if (m_DirectTransformTargets.Count > 0 && m_DirectTransformTargets[0] == value) // Check if value is already first in the list
                    return;

                m_DirectTransformTargets.Remove(value); // Check if it is already in the list but somewhere else, and remove it
                m_DirectTransformTargets.Insert(0, value); // Insert the value at the front of the list

            }
        }

        public List<Transform> targets
        {
            set => m_DirectTransformTargets = value;
            get => m_DirectTransformTargets;
        }

        public event Action<Vector3> translate;
        public event Action<Quaternion> rotate;

        public bool dragging { get; private set; }
        public event Action dragStarted;
        public event Action dragEnded;

        event Action<Vector3> IManipulator.scale
        {
            add { }
            remove { }
        }

        void OnEnable()
        {
            foreach (var h in m_AllHandles)
            {
                h.dragStarted += OnHandleDragStarted;
                h.dragging += OnHandleDragging;
                h.dragEnded += OnHandleDragEnded;
            }
        }

        void OnDisable()
        {
            foreach (var h in m_AllHandles)
            {
                h.dragStarted -= OnHandleDragStarted;
                h.dragging -= OnHandleDragging;
                h.dragEnded -= OnHandleDragEnded;
            }
        }

        void IManipulator.SetTargetTransforms(Transform[] targetTransforms, Transform activeTargetTransform)
        {
            // Target transforms set via this IManipulator method are moved indirectly via the IManipulator events
            // This is not the same as the DirectTransformTargets on this component, which are moved directly instead of via manipulator events
            // If the IManipulator is set to transform an existing target via events, it should be removed from the list of direct targets
            m_DirectTransformTargets.Remove(activeTargetTransform);
            foreach (var targetTransform in targetTransforms)
            {
                m_DirectTransformTargets.Remove(targetTransform);
            }
        }

        void IManipulator.SetPose(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        void OnHandleDragStarted(BaseHandle handle, HandleEventData eventData)
        {
            foreach (var h in m_AllHandles)
            {
                h.gameObject.SetActive(h == handle);
            }

            dragging = true;
            dragStarted?.Invoke();
        }

        void OnHandleDragging(BaseHandle handle, HandleEventData eventData)
        {
            foreach (var targetTransform in m_DirectTransformTargets)
            {
                if (targetTransform != null)
                {
                    targetTransform.position = targetTransform.position + eventData.deltaPosition;
                    targetTransform.rotation = eventData.deltaRotation * targetTransform.rotation;
                }
            }

            translate?.Invoke(eventData.deltaPosition);
            rotate?.Invoke(eventData.deltaRotation);
        }

        void OnHandleDragEnded(BaseHandle handle, HandleEventData eventData)
        {
            if (gameObject.activeSelf)
            {
                foreach (var h in m_AllHandles)
                {
                    h.gameObject.SetActive(true);
                }
            }

            dragging = false;
            dragEnded?.Invoke();
        }
    }
}
