using System;
using UnityEngine;

namespace Unity.SpatialFramework.Avatar
{
    /// <summary>
    /// Reads mic input and determines the current input level
    /// </summary>
    public class MicInput : MonoBehaviour
    {
        const int k_SampleWindowSize = 128;

        [SerializeField, Tooltip("The microphone index to use.")]
        int m_MicDeviceIndex;

        /// <summary>
        /// The current mic level
        /// </summary>
        public float MicInputLevel { get; set; }

        public Action<float> OnMicLevelChanged;

        string m_DeviceName;
        AudioClip m_ClipRecord;
        float m_RecentMaxLevel = 0.5f;

        void InitMic()
        {
            if (m_MicDeviceIndex >= Microphone.devices.Length)
                return;

            if (m_DeviceName == null)
                m_DeviceName = Microphone.devices[m_MicDeviceIndex];

            m_ClipRecord = Microphone.Start(m_DeviceName, true, 999, 44100);
        }

        void StopMic()
        {
            MicInputLevel = 0f;
            if (string.IsNullOrEmpty(m_DeviceName))
                return;

            Microphone.End(m_DeviceName);
            OnMicLevelChanged?.Invoke(MicInputLevel);
        }

        float CalculateCurrentLevel()
        {
            // Get data from microphone into audio clip
            var currentInput = 0f;
            var waveData = new float[k_SampleWindowSize];
            var micPosition = Microphone.GetPosition(m_DeviceName) - (k_SampleWindowSize + 1);
            if (micPosition < 0)
                return 0;

            m_ClipRecord.GetData(waveData, micPosition);

            // Find peak in the last 128 samples
            for (var i = 0; i < k_SampleWindowSize; i++)
            {
                var wavePeak = Mathf.Abs(waveData[i]);
                if (wavePeak > currentInput)
                    currentInput = wavePeak;
            }

            // Simple compressor to normalize based on average level
            const float decayFactor = 0.9999f; // Slowly reduced the "max" value to normalize against
            m_RecentMaxLevel = currentInput > m_RecentMaxLevel ? currentInput : m_RecentMaxLevel * decayFactor; // If input is higher than max, adjust max. Otherwise decay max.
            var output = m_RecentMaxLevel > 0 ? currentInput / m_RecentMaxLevel : 0; // Normalize current against max, avoiding divide by zero

            return output;
        }


        void Update()
        {
            if (m_ClipRecord != null)
            {
                var level = CalculateCurrentLevel();
                if (level != MicInputLevel)
                {
                    MicInputLevel = level;
                    OnMicLevelChanged?.Invoke(MicInputLevel);
                }
            }
        }

        void OnEnable()
        {
            InitMic();
        }

        void OnDisable()
        {
            StopMic();
        }
    }
}
