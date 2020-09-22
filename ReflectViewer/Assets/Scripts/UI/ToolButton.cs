using System;
using System.Collections;
using System.Configuration;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ToolButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_Button;

        [SerializeField]
        Image m_ButtonRound;

        [SerializeField]
        Image m_ButtonIcon;
#pragma warning restore CS0649

        bool m_Selected;

        public Button button => m_Button;

        public Image buttonRound => m_ButtonRound;

        public Image buttonIcon => m_ButtonIcon;

        public event Action buttonClicked;
        public event Action buttonLongPressed;

        bool m_LongPressed;

        public bool selected
        {
            get
            {
                return m_Selected;

            }
            set
            {
                if (m_Selected == value)
                    return;
                m_ButtonRound.enabled = value;
                m_Selected = value;
            }
        }

        void Start()
        {
            m_Button.onClick.AddListener(OnButtonClicked);
        }

        public void SetIcon(Sprite icon)
        {
            m_ButtonIcon.sprite = icon;
        }

        void OnButtonClicked()
        {
            if (!m_LongPressed)
            {
                buttonClicked?.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            StartCoroutine("DelayPress", UIConfig.buttonLongPressTime);
            m_LongPressed = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StopCoroutine("DelayPress");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopCoroutine("DelayPress");
            m_LongPressed = false;
        }

        private IEnumerator DelayPress(float delay)
        {
            yield return new WaitForSeconds(delay);
            OnLongPress();
        }

        void OnLongPress()
        {
            if (buttonLongPressed != null)
            {
                buttonLongPressed.Invoke();
                m_LongPressed = true;
            }
        }
    }
}
