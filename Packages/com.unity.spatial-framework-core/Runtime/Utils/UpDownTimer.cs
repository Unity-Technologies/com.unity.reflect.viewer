using UnityEngine;

namespace Unity.SpatialFramework.Utils
{
    /// <summary>
    /// Helper class that collects a common gain/decay time-counter pattern into a single set of functionality
    /// </summary>
    [System.Serializable]
    public class UpDownTimer : ISerializationCallbackReceiver
    {
        [SerializeField, Tooltip("The time it takes for this timer to reach a value of 1. This is clamped at or above Epsilon.")]
        float m_TimeToMax = 1.0f;

        [SerializeField, Tooltip("The time it takes for this timer to reach a value of 0. This is clamped at or above Epsilon.")]
        float m_TimeToMin = 1.0f;

        // These hold the conversions from time to incremental based deltaTime
        float m_Gain = 1.0f;
        float m_Decay = 1.0f;
        float m_Current;
        float m_Last;

        /// <summary>
        /// The time it takes for this timer to reach a value of 1. This is clamped at or above Epsilon.
        /// </summary>
        public float timeToMax
        {
            get { return m_TimeToMax; }
            set
            {
                m_TimeToMax = Mathf.Max(value, Mathf.Epsilon);
                UpdateGain();
            }
        }

        /// <summary>
        /// The time it takes for this timer to reach a value of 0. This is clamped at or above Epsilon.
        /// </summary>
        public float timeToMin
        {
            get { return m_TimeToMin; }
            set
            {
                m_TimeToMin = Mathf.Max(value, Mathf.Epsilon);
                UpdateDecay();
            }
        }

        /// <summary>
        /// The current value of the timer
        /// </summary>
        public float current
        {
            get { return m_Current; }
            set
            {
                m_Last = m_Current;
                m_Current = value;
            }
        }

        /// <summary>
        /// If the timer changed during the last counting operation
        /// </summary>
        public bool changed
        {
            get { return m_Last != m_Current; }
        }

        /// <summary>
        /// Counts the timer towards the max value of 1
        /// </summary>
        /// <param name="deltaTime">How much time to elapse</param>
        /// <returns>True if the timer has reached max value</returns>
        public bool CountUp(float deltaTime)
        {
            m_Last = m_Current;
            m_Current = Mathf.Clamp01(m_Current + (m_Gain * deltaTime));

            return (m_Current >= 1.0f);
        }


        /// <summary>
        /// Counts the timer towards the minimum value of 0
        /// </summary>
        /// <param name="deltaTime">How much time to elapse</param>
        /// <returns>True if the timer has reached the minimum value</returns>
        public bool CountDown(float deltaTime)
        {
            m_Last = m_Current;
            m_Current = Mathf.Clamp01(m_Current - (m_Decay * deltaTime));

            return (m_Current <= 0.0f);
        }

        /// <summary>
        /// Jumps the timer to the max value of 1
        /// </summary>
        public void SetToMax()
        {
            m_Last = m_Current;
            m_Current = 1.0f;
        }

        /// <summary>
        /// Jumps the timer to the min value of 0
        /// </summary>
        public void SetToMin()
        {
            m_Last = m_Current;
            m_Current = 0.0f;
        }

        public void OnBeforeSerialize()
        {
            m_TimeToMax = Mathf.Max(m_TimeToMax, Mathf.Epsilon);
            m_TimeToMin = Mathf.Max(m_TimeToMin, Mathf.Epsilon);
        }

        /// <summary>
        /// Called after deserialization to initial the incremental counter values
        /// </summary>
        public void OnAfterDeserialize()
        {
            UpdateGain();
            UpdateDecay();
        }

        void UpdateGain()
        {
            m_Gain = 1.0f / m_TimeToMax;
        }

        void UpdateDecay()
        {
            m_Decay = 1.0f / m_TimeToMin;
        }
    }
}
