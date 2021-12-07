using System;
using System.Collections;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class ModalPopup : BasePopup
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_NegativeButton;
        [SerializeField]
        Button m_PositiveButton;
#pragma warning restore CS0649

        TextMeshProUGUI m_Title;
        TextMeshProUGUI m_Body;
        TextMeshProUGUI m_PositiveText;
        TextMeshProUGUI m_NegativeText;
        Image m_BackgroundImage;
        float m_FadeDuration;

        public struct ModalPopupData
        {
            public string title;
            public string text;
            public string positiveText;
            public string negativeText;
            public Action positiveCallback;
            public Action negativeCallback;
            public float fadeDuration;
        }
        public ModalPopupData DefaultData()
        {
            return new ModalPopupData
            {
                title = "Alert",
                text = string.Empty,
                positiveText = "Ok",
                negativeText = String.Empty,
                positiveCallback = delegate {},
                negativeCallback = delegate {},
                fadeDuration = m_DefaultFadeDuration,
            };
        }

        public Action positiveAction { get; set; }
        public Action negativeAction { get; set; }

        void Awake()
        {
            Initialize();
            var textFields = m_PopUpRect.GetComponentsInChildren<TextMeshProUGUI>();
            m_Title = textFields[0];
            m_Body = textFields[1];
            m_NegativeText = m_NegativeButton.GetComponentInChildren<TextMeshProUGUI>();
            m_PositiveText = m_PositiveButton.GetComponentInChildren<TextMeshProUGUI>();
            m_BackgroundImage = GetComponent<Image>();
            m_NegativeButton.onClick.AddListener(OnNegativeClick);
            m_PositiveButton.onClick.AddListener(OnPositiveClick);
        }

        public void Display(ModalPopupData data)
        {
            m_Title.text = data.title;
            m_Body.text = data.text;
            m_PositiveText.text = data.positiveText;
            m_NegativeText.text = data.negativeText;
            m_FadeDuration = data.fadeDuration;
            positiveAction = data.positiveCallback;
            negativeAction = data.negativeCallback;

            m_NegativeButton.gameObject.SetActive(m_NegativeText.text != string.Empty);

            m_BackgroundImage.enabled = true;
            StartAnimation(AnimationIn(data.fadeDuration));
        }

        void OnNegativeClick()
        {
            negativeAction();
            m_BackgroundImage.enabled = false;
            StartAnimation(AnimationOut(m_FadeDuration));
        }

        void OnPositiveClick()
        {
            positiveAction();
            m_BackgroundImage.enabled = false;
            StartAnimation(AnimationOut(m_FadeDuration));
        }
    }
}
