using UnityEngine;
using System;
using TMPro;

namespace Unity.SpatialFramework.UI
{
    public class ClockText : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        TMP_Text m_Text;
#pragma warning restore 649

        // Update is called once per frame
        void Update()
        {
            m_Text.text = DateTime.Now.ToString("HH:mm");
        }
    }
}
