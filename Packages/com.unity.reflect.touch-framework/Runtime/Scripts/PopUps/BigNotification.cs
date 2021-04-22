using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class BigNotification : BasePopup
    {
        TextMeshProUGUI m_TitleField;
        public struct BigNotificationData
        {
            public string text;
            public string title;
            public float displayDuration;
            public float fadeDuration;
        }

        // Cannot override the base class implementation because return type is different.
        public BigNotificationData DefaultData()
        {
            return new BigNotificationData
            {
                text = string.Empty,
                title = string.Empty,
                displayDuration = m_DefaultDisplayDuration,
                fadeDuration = m_DefaultFadeDuration
            };
        }

        void Awake()
        {
            Initialize();
            var textFields = m_PopUpRect.GetComponentsInChildren<TextMeshProUGUI>();
            m_TitleField = textFields[0];
            m_TextField = textFields[1];
        }

        public void Display(BigNotificationData data)
        {
            m_TitleField.text = data.title;
            m_TextField.text = data.text;
            StartAnimation(AnimationInOut(data.displayDuration, data.fadeDuration));
        }
    }
}
