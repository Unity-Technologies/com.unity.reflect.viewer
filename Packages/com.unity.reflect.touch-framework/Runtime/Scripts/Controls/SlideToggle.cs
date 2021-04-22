using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    /// <summary>
    /// A slide toggle typically found in iOS native apps.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SlideToggle : MonoBehaviour, IPropertyValue<bool>
    {
        const float k_ToggleDragDistance = 20f;

        [Serializable]
        public class ToggleValueChanged : UnityEvent<bool> {}

        [SerializeField]
        bool m_IsOn;

        [SerializeField]
        bool m_IsInteractable;

#pragma warning disable CS0649
        [SerializeField]
        RectTransform m_InputCaptureRect;
        [SerializeField]
        Image m_HandleImage;
        [SerializeField]
        Color m_HandleOffColor = UIConfig.propertyBaseColor;
        [SerializeField]
        Color m_HandleOnColor = UIConfig.propertySelectedColor;
        [SerializeField]
        Image m_BackgroundAreaImage;
        [SerializeField]
        Color m_BackgroundOffColor = UIConfig.propertyTextInactiveColor;
        [SerializeField]
        Color m_BackgroundOnColor = UIConfig.propertyTextSelectedColor;
#pragma warning restore CS0649

        RectTransform m_HandleRectTransform;
        bool m_CanClick;
        bool m_CanDragSelect;
        Vector2 m_DragStartPointerPosition;
        ColorTween m_ColorTween;
        TweenRunner<ColorTween> m_ColorTweenRunner;
        FloatTween m_MoveHandleTween;
        TweenRunner<FloatTween> m_MoveHandleTweenRunner;
        ToggleValueChanged m_OnValueChanged = new ToggleValueChanged();
        float m_OnXPos;
        float m_OffXPos;
        float m_MoveStartXPos;

        // Interactable Colors (same as original colors)
        Color m_InteractableHandleOn;
        Color m_InteractableHandleOff;
        Color m_InteractableBgOn;
        Color m_InteractableBgOff;

        public ToggleValueChanged onValueChanged => m_OnValueChanged;

        /// <summary>
        /// Gets and sets the toggle state.
        /// </summary>
        public bool on
        {
            get => m_IsOn;
            set
            {
                if (m_IsOn == value)
                    return;

                m_IsOn = value;
                UpdateVisualState();
            }
        }

        public bool isInteractable
        {
            get => m_IsInteractable;
            set
            {
                if (m_IsInteractable == value)
                    return;

                m_IsInteractable = value;
                UpdateVisualState();
            }
        }

        void UpdateVisualState()
        {
            m_HandleOnColor.a = isInteractable ? m_InteractableHandleOn.a : m_InteractableHandleOn.a * 0.3f;
            m_HandleOffColor.a = isInteractable ? m_InteractableHandleOff.a : m_InteractableHandleOff.a * 0.3f;
            m_BackgroundOnColor.a = isInteractable ? m_InteractableBgOn.a : m_InteractableBgOn.a * 0.3f;
            m_BackgroundOffColor.a = isInteractable ? m_InteractableBgOff.a : m_InteractableBgOff.a * 0.3f;

            if (Application.isPlaying && m_ColorTweenRunner != null)
            {
                m_ColorTween.startColor = m_HandleImage.color;
                m_ColorTween.targetColor = on ? m_HandleOnColor : m_HandleOffColor;
                m_ColorTweenRunner.StartTween(m_ColorTween, EaseType.EaseInCubic);

                m_MoveStartXPos = m_HandleRectTransform.anchoredPosition.x;
                m_MoveHandleTweenRunner.StartTween(m_MoveHandleTween, EaseType.EaseInCubic);
            }
            else
            {
                m_HandleImage.color = on ? m_HandleOnColor : m_HandleOffColor;
            }

            m_BackgroundAreaImage.color = on ? m_BackgroundOnColor : m_BackgroundOffColor;
        }

        void Awake()
        {
            m_HandleRectTransform = m_HandleImage.GetComponent<RectTransform>();

            if (m_InputCaptureRect == null)
                m_InputCaptureRect = m_BackgroundAreaImage.rectTransform;

            if (Application.isPlaying)
            {
                EventTriggerUtility.CreateEventTrigger(m_InputCaptureRect.gameObject, OnControlPointerDown, EventTriggerType.PointerDown);
                EventTriggerUtility.CreateEventTrigger(m_InputCaptureRect.gameObject, OnControlPan, EventTriggerType.Drag);
                EventTriggerUtility.CreateEventTrigger(m_InputCaptureRect.gameObject, OnControlPointerUp, EventTriggerType.PointerUp);

                m_ColorTween = new ColorTween()
                {
                    duration = UIConfig.propertyColorTransitionTime,
                    ignoreTimeScale = true,
                    tweenMode = ColorTween.ColorTweenMode.RGB
                };
                m_ColorTween.AddOnChangedCallback(SetImageColor);
                m_ColorTweenRunner = new TweenRunner<ColorTween>();
                m_ColorTweenRunner.Init(this);

                m_MoveHandleTween = new FloatTween()
                {
                    duration = UIConfig.propertyColorTransitionTime,
                    ignoreTimeScale = true,
                    startValue = 0f,
                    targetValue = 1f
                };
                m_MoveHandleTween.AddOnChangedCallback(OnMoveHandle);
                m_MoveHandleTweenRunner = new TweenRunner<FloatTween>();
                m_MoveHandleTweenRunner.Init(this);
            }

            m_InteractableHandleOn = m_HandleOnColor;
            m_InteractableHandleOff = m_HandleOffColor;
            m_InteractableBgOn = m_BackgroundOnColor;
            m_InteractableBgOff = m_BackgroundOffColor;
            isInteractable = true;

            UpdateVisualState();
        }

        void Start()
        {
            m_OnXPos = m_BackgroundAreaImage.rectTransform.sizeDelta.x / 4f;
            m_OffXPos = -m_OnXPos;
        }

        void OnMoveHandle(float alpha)
        {
            var targetXPos = on ? m_OnXPos : m_OffXPos;
            var newXPos = Mathf.Lerp(m_MoveStartXPos, targetXPos, alpha);
            m_HandleRectTransform.anchoredPosition = new Vector2(newXPos, m_HandleRectTransform.anchoredPosition.y);
        }

        void SetImageColor(Color color)
        {
            m_HandleImage.color = color;
        }

        void OnControlPointerDown(BaseEventData eventData)
        {
            if (!isInteractable)
                return;

            var pointerPosition = ((PointerEventData)eventData).position;
            m_DragStartPointerPosition = pointerPosition;

            m_CanClick = true;
            m_CanDragSelect = true;
        }

        void OnControlPan(BaseEventData eventData)
        {
            if (!isInteractable)
                return;

            if (!m_CanDragSelect)
                return;

            var pointerPosition = ((PointerEventData)eventData).position;
            var dragXDistance = pointerPosition.x - m_DragStartPointerPosition.x;
            if (Mathf.Abs(dragXDistance) > k_ToggleDragDistance)
            {
                if (dragXDistance > 0 && !on)
                {
                    Toggle(true);
                    m_CanDragSelect = false;
                    m_CanClick = false;
                }
                else if (dragXDistance < 0 && on)
                {
                    Toggle(false);
                    m_CanDragSelect = false;
                    m_CanClick = false;
                }
            }
        }

        void OnControlPointerUp(BaseEventData eventData)
        {
            if (!isInteractable)
                return;

            OnControlPan(eventData);

            m_CanDragSelect = false;

            if (m_CanClick)
            {
                Toggle(!on);
            }

            m_CanClick = false;
        }

        void Toggle(bool toOn)
        {
            if (on == toOn)
                return;

            on = toOn;

            onValueChanged?.Invoke(@on);
        }

        void Reset()
        {
            m_InputCaptureRect = m_BackgroundAreaImage.rectTransform;
        }

        public bool value => on;
        public Type type => typeof(bool);
        public object objectValue
        {
            get => on;
            set => on = (bool)value;
        }

        Dictionary<Action, UnityAction<bool>> m_Handlers = new Dictionary<Action, UnityAction<bool>>();

        public void AddListener(Action eventFunc)
        {
            m_Handlers[eventFunc] = (newValue) =>
            {
                eventFunc();
            };
            onValueChanged.AddListener(m_Handlers[eventFunc]);
        }

        public void RemoveListener(Action eventFunc)
        {
            onValueChanged.RemoveListener(m_Handlers[eventFunc]);
            m_Handlers.Remove(eventFunc);
        }
    }
}
