using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.SpatialFramework.Avatar
{
    /// <summary>
    /// Controls an avatar's eyes to look in various directions and blink
    /// </summary>
    public class AvatarSyntheticEyesLook : MonoBehaviour
    {
        const float k_BlinkDuration = 0.25f;
        static readonly Vector2 k_BlinkIntervalMinMax = new Vector2(3f, 5f);

        [Header("Blink Settings")]
        [SerializeField, Tooltip("If enabled, the eyes will blink occasionally.")]
        bool m_EnableBlinking = true;

        [SerializeField, Tooltip("The left eye. This GameObject will be scaled to perform blinks.")]
        GameObject m_LeftEye;

        [SerializeField, Tooltip("The right eye. This GameObject will be scaled to perform blinks.")]
        GameObject m_RightEye;

        [SerializeField, Tooltip("Animation curve that animates the eye Y scale to simulate blinking.")]
        AnimationCurve m_BlinkTimingCurve;

        [Header("Look Settings")]
        [SerializeField, Tooltip("If enabled, the eyes will look at potential targets that enter its trigger volume.")]
        bool m_EnableEyeLook = true;

        [SerializeField, Tooltip("The socket for the left eye. This GameObject will be rotated based on the look target.")]
        GameObject m_LeftEyeSocket;

        [SerializeField, Tooltip("The socket for the left eye. This GameObject will be rotated based on the look target.")]
        GameObject m_RightEyeSocket;

        [SerializeField, Tooltip("The initial look target, as well as the default look target when there is nothing else to look at.")]
        GameObject m_DefaultLookTarget;

        [SerializeField, Tooltip("Objects with this tag that enter this GameObject's trigger volume will be used as targets to look at.")]
        string m_LookTargetTag = "Dude";

        float m_InitialEyeScaleY;
        bool m_IsBlinking;
        List<GameObject> m_Targets = new List<GameObject>();
        Transform m_TargetTransform;
        int m_LookTargetIndex;
        float m_BlinkInterpolation;
        Coroutine m_BlinkCycle;

        /// <summary>
        /// If enabled, the eyes will blink occasionally.
        /// </summary>
        public bool enableBlinking
        {
            get => m_EnableBlinking;
            set => m_EnableBlinking = value;
        }

        /// <summary>
        /// If enabled, the eyes will look at potential targets that enter its trigger volume.
        /// </summary>
        public bool enableEyeLook
        {
            get => m_EnableEyeLook;
            set => m_EnableEyeLook = value;
        }

        void Start()
        {
            // Set initial values for look target and eye height
            m_InitialEyeScaleY = m_LeftEye.transform.localScale.y;

            // Set blink animation curve to loop and start blink cycle
            m_BlinkTimingCurve.preWrapMode = WrapMode.Loop;
            m_BlinkTimingCurve.postWrapMode = WrapMode.Loop;
        }

        void OnEnable()
        {
            m_TargetTransform = m_DefaultLookTarget.transform;
            m_BlinkCycle = StartCoroutine(BlinkCycle());
        }

        void OnDisable()
        {
            StopCoroutine(m_BlinkCycle);
        }

        IEnumerator BlinkCycle()
        {
            while (enabled)
            {
                // Blink for duration of blink animation curve
                m_IsBlinking = true;

                // Increment index to cycle between look targets while blinking, if there are multiple targets
                m_LookTargetIndex++;
                m_BlinkInterpolation = 0f;
                while (m_BlinkInterpolation < 1f)
                {
                    m_BlinkInterpolation += Time.deltaTime / k_BlinkDuration;
                    yield return null;
                }

                // Wait to blink again after a few seconds
                m_IsBlinking = false;
                yield return new WaitForSeconds(Random.Range(k_BlinkIntervalMinMax[0], k_BlinkIntervalMinMax[1]));
            }

            yield return null;
        }

        void Update()
        {
            UpdateEyeBlink();
            SetLookTarget();
        }

        void UpdateEyeBlink()
        {
            // Scale eyes to blink them
            var rightScale = m_RightEye.transform.localScale;
            var leftScale = m_LeftEye.transform.localScale;

            if (m_EnableBlinking && m_IsBlinking)
            {
                var currentBlinkCurveValue = m_BlinkTimingCurve.Evaluate(k_BlinkDuration * m_BlinkInterpolation);
                var blinkAperture = currentBlinkCurveValue * m_InitialEyeScaleY;
                leftScale = new Vector3(leftScale.x, blinkAperture, leftScale.z);
                rightScale = new Vector3(rightScale.x, blinkAperture, rightScale.z);
            }
            else
            {
                leftScale = new Vector3(leftScale.x, m_InitialEyeScaleY, leftScale.z);
                rightScale = new Vector3(rightScale.x, m_InitialEyeScaleY, rightScale.z);
            }

            m_LeftEye.transform.localScale = leftScale;
            m_RightEye.transform.localScale = rightScale;
        }

        void SetLookTarget()
        {
            // Choose a target transform to look at
            if (m_EnableEyeLook && m_Targets.Count > 0)
            {
                var index = m_LookTargetIndex % m_Targets.Count;
                var newTarget = m_Targets[index];
                if (newTarget == null)
                    m_Targets.Remove(newTarget);
                else
                    m_TargetTransform = newTarget.transform;
            }
            else
            {
                m_TargetTransform = m_DefaultLookTarget.transform;
            }

            // Move eyes to look at targets
            var targetPosition = m_TargetTransform.position;
            var leftTargetRotation = Quaternion.LookRotation(Vector3.Normalize(targetPosition - m_LeftEyeSocket.transform.position), m_LeftEyeSocket.transform.up);
            var rightTargetRotation = Quaternion.LookRotation(Vector3.Normalize(targetPosition - m_RightEyeSocket.transform.position), m_RightEyeSocket.transform.up);

            m_LeftEyeSocket.transform.rotation = Quaternion.Slerp(m_LeftEyeSocket.transform.rotation, leftTargetRotation, m_BlinkInterpolation);
            m_RightEyeSocket.transform.rotation = Quaternion.Slerp(m_RightEyeSocket.transform.rotation, rightTargetRotation, m_BlinkInterpolation);
        }

        void OnTriggerEnter(Collider collision)
        {
            // If a target is in field of view, add as potential target
            if (collision.gameObject.CompareTag(m_LookTargetTag))
            {
                m_Targets.Add(collision.gameObject);
            }
        }

        void OnTriggerExit(Collider collision)
        {
            if (collision.gameObject.CompareTag(m_LookTargetTag))
            {
                m_Targets.Remove(collision.gameObject);
            }
        }
    }
}
