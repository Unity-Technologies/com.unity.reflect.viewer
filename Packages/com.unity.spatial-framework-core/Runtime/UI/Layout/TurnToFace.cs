using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.SpatialFramework.UI.Layout
{
    /// <summary>
    /// Rotates the GameObject's transform to face a target.
    /// </summary>
    public class TurnToFace : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField, Tooltip("Target to face towards. If not set, this will default to the main camera")]
        Transform m_FaceTarget;

        [SerializeField, Tooltip("Speed to turn")]
        float m_TurnToFaceSpeed = 5f;

        [SerializeField, Tooltip("Local rotation offset")]
        Vector3 m_RotationOffset = Vector3.zero;

        [SerializeField, Tooltip("If enabled, ignore the x axis when rotating")]
        bool m_IgnoreX;

        [SerializeField, Tooltip("If enabled, ignore the y axis when rotating")]
        bool m_IgnoreY;

        [SerializeField, Tooltip("If enabled, ignore the z axis when rotating")]
        bool m_IgnoreZ;
#pragma warning restore 649

        static readonly HashSet<TurnToFace> k_EnabledInstances = new HashSet<TurnToFace>();

        void Awake()
        {
            // Default to main camera
            if (m_FaceTarget == null && Camera.main != null)
                m_FaceTarget = Camera.main.transform;

        }

        void OnEnable()
        {
            transform.rotation = GetTargetRotation();
            k_EnabledInstances.Add(this);
        }

        void OnDisable()
        {
            k_EnabledInstances.Remove(this);
        }

        /// <summary>
        /// Causes all turn to face layout components to snap to the correct rotation immediately.
        /// </summary>
        public static void SnapAll()
        {
            foreach (var turnToFace in k_EnabledInstances)
            {
                turnToFace.transform.rotation = turnToFace.GetTargetRotation();
            }
        }

        /// <summary>
        /// Target to face towards. If not set, this will default to the main camera
        /// </summary>
        public Transform faceTarget
        {
            get => m_FaceTarget;
            set => m_FaceTarget = value;
        }

        void Update()
        {
            if (m_FaceTarget != null)
            {
                var facePosition = m_FaceTarget.position;
                var forward = facePosition - transform.position;
                var targetRotation = forward.sqrMagnitude > float.Epsilon ? Quaternion.LookRotation(forward, Vector3.up) : Quaternion.identity;
                targetRotation *= Quaternion.Euler(m_RotationOffset);
                if (m_IgnoreX || m_IgnoreY || m_IgnoreZ)
                {
                    var targetEuler = targetRotation.eulerAngles;
                    var currentEuler = transform.rotation.eulerAngles;
                    targetRotation = Quaternion.Euler
                    (
                        m_IgnoreX ? currentEuler.x : targetEuler.x,
                        m_IgnoreY ? currentEuler.y : targetEuler.y,
                        m_IgnoreZ ? currentEuler.z : targetEuler.z
                    );
                }

                var ease = 1f - Mathf.Exp(-m_TurnToFaceSpeed * Time.unscaledDeltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, ease);
            }
        }

        Quaternion GetTargetRotation()
        {
            var facePosition = m_FaceTarget.position;
            var forward = facePosition - transform.position;
            var targetRotation = forward.sqrMagnitude > float.Epsilon ? Quaternion.LookRotation(forward, Vector3.up) : Quaternion.identity;
            targetRotation *= Quaternion.Euler(m_RotationOffset);
            if (m_IgnoreX || m_IgnoreY || m_IgnoreZ)
            {
                var targetEuler = targetRotation.eulerAngles;
                var currentEuler = transform.rotation.eulerAngles;
                targetRotation = Quaternion.Euler
                (
                    m_IgnoreX ? currentEuler.x : targetEuler.x,
                    m_IgnoreY ? currentEuler.y : targetEuler.y,
                    m_IgnoreZ ? currentEuler.z : targetEuler.z
                );
            }

            return targetRotation;
        }
    }
}
