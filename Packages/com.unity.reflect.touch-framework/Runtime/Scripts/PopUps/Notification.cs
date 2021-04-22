using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class Notification : BasePopup
    {
        Image m_Image;

        public struct NotificationData
        {
            public float displayDuration;
            public float fadeDuration;
            public string text;
            public Sprite icon;
        }

        public NotificationData DefaultData()
        {
            return new NotificationData
            {
                text = string.Empty,
                icon = null,
                displayDuration = m_DefaultDisplayDuration,
                fadeDuration = m_DefaultFadeDuration,
            };
        }

        void Awake()
        {
            Initialize();
            m_Image = m_PopUpRect.Find("Icon").GetComponent<Image>();
        }

        public void Display(NotificationData data)
        {
            if (data.icon != null)
            {
                m_Image.sprite = data.icon;
                m_Image.gameObject.SetActive(true);
            }
            else
                m_Image.gameObject.SetActive(false);

            m_TextField.text = data.text;

            StartAnimation(AnimationInOut(data.displayDuration, data.fadeDuration));
        }
    }
}
