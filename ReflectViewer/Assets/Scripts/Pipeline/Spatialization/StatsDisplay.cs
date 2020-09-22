using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    public class StatsDisplay : MonoBehaviour
    {
        // use this to avoid GC allocation every frame
        static readonly string[] k_StringDisplayCache = new[]
        {
            "00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
            "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
            "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
            "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
            "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
            "50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
            "60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
            "70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
            "80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
            "90", "91", "92", "93", "94", "95", "96", "97", "98", "99",
        };

        // private const string MbFormat = "{0}mb";
        // private const int BytesToMb = 1024 * 1024;

        public int FrameBufferCount = 30;
        public int TargetFrameRate = 60;

        public Text MaxFrameRateText;
        public Text CurrentFrameRateText;
        public Text MinFrameRateText;

        public Gradient ColorGradient;

        // public Text ReservedMemoryText;
        // public Text AllocatedMemoryText;
        // public Text UnusedMemoryText;

        float[] m_FrameCounts;
        int m_CurrentIndex;
        int m_CurrentValidFrameCount;
        float m_CurrentFrameRate;
        float m_TotalFrameRate;
        float m_MinFrameRate;
        float m_MaxFrameRate;
        float m_FrameRateRatio;

        void Start()
        {
            m_FrameCounts = new float[FrameBufferCount];
            for (int i = 0; i < m_FrameCounts.Length; ++i)
            {
                m_FrameCounts[i] = -1;
            }
        }

        void Update()
        {
            m_FrameCounts[m_CurrentIndex] = 1f / Time.deltaTime;
            ++m_CurrentIndex;
            m_CurrentIndex %= m_FrameCounts.Length;

            Calculate();
            RefreshFrameRateTexts();

            // RefreshMemoryTexts();
        }

        void Calculate()
        {
            m_CurrentValidFrameCount = 0;
            m_TotalFrameRate = 0;
            m_MinFrameRate = float.MaxValue;
            m_MaxFrameRate = float.MinValue;
            for (int i = 0; i < m_FrameCounts.Length; ++i)
            {
                var value = m_FrameCounts[i];
                if (value <= 0)
                    continue;

                ++m_CurrentValidFrameCount;
                m_TotalFrameRate += value;

                if (m_MinFrameRate > value) m_MinFrameRate = value;
                if (m_MaxFrameRate < value) m_MaxFrameRate = value;
            }

            if (m_CurrentValidFrameCount > 0)
                m_CurrentFrameRate = m_TotalFrameRate / m_CurrentValidFrameCount;
        }

        void RefreshFrameRateTexts()
        {
            CurrentFrameRateText.text = k_StringDisplayCache[Mathf.Clamp((int)m_CurrentFrameRate, 0, 99)];
            CurrentFrameRateText.color = ColorGradient.Evaluate(m_CurrentFrameRate / TargetFrameRate);

            MaxFrameRateText.text = k_StringDisplayCache[Mathf.Clamp((int)m_MaxFrameRate, 0, 99)];
            MaxFrameRateText.color = ColorGradient.Evaluate(m_MaxFrameRate / TargetFrameRate);

            MinFrameRateText.text = k_StringDisplayCache[Mathf.Clamp((int)m_MinFrameRate, 0, 99)];
            MinFrameRateText.color = ColorGradient.Evaluate(m_MinFrameRate / TargetFrameRate);
        }

        // void RefreshMemoryTexts()
        // {
        //     ReservedMemoryText.text = string.Format(MbFormat, Profiler.GetTotalReservedMemoryLong() / BytesToMb);
        //     AllocatedMemoryText.text = string.Format(MbFormat, Profiler.GetTotalAllocatedMemoryLong() / BytesToMb);
        //     UnusedMemoryText.text = string.Format(MbFormat, Profiler.GetTotalUnusedReservedMemoryLong() / BytesToMb);
        // }
    }
}
