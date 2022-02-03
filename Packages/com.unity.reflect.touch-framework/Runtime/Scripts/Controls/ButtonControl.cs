using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class ButtonControl : MonoBehaviour
    {
        const string k_NullText = "";

        public enum TransitionType
        {
            ChangeColor,
            SwapSprite,
        }

        [Serializable]
        public class ButtonTapEvent : UnityEvent<BaseEventData> {}

        [SerializeField]
        bool m_IsToggle = true;
        [SerializeField]
        TransitionType m_TransitionType = TransitionType.ChangeColor;
        [SerializeField]
        Image m_TargetImage;
        [SerializeField]
        Graphic m_DescriptorGraphic;
        [SerializeField]
        Sprite m_ToggleOnSprite;
        [SerializeField]
        Sprite m_ToggleOffSprite;
        [SerializeField]
        TextMeshProUGUI m_Text;
        [SerializeField]
        Image m_IconImage;
        [SerializeField]
        bool m_Interactable = true;
        [SerializeField]
        bool m_AddSelectableComponent;

        [SerializeField]
        Color m_OnColor = UIConfig.propertySelectedColor;
        [SerializeField]
        Color m_OffColor = UIConfig.propertyBaseColor;

        Selectable m_Selectable;
        bool m_On;
        ButtonTapEvent m_OnControlDown = new ButtonTapEvent();
        ButtonTapEvent m_OnControlTap = new ButtonTapEvent();
        ButtonTapEvent m_OnControlUp = new ButtonTapEvent();
        ColorTween m_ColorTween;
        TweenRunner<ColorTween> m_TweenRunner;
        ScrollRect m_ScrollView;

        public bool on
        {
            get => m_On;
            set
            {
                m_On = value;
                UpdateVisualState();
            }
        }

        public bool interactable
        {
            get => m_Interactable;
            set
            {
                m_Interactable = value;
                if (m_Selectable != null)
                    m_Selectable.interactable = value;
            }
        }

        public UnityEvent<BaseEventData> onControlTap => m_OnControlTap;
        public UnityEvent<BaseEventData> onControlDown => m_OnControlDown;
        public UnityEvent<BaseEventData> onControlUp => m_OnControlUp;

        public bool isToggle
        {
            get => m_IsToggle;
            set => m_IsToggle = value;
        }

        public Image targetImage
        {
            get => m_TargetImage;
            set => m_TargetImage = value;
        }

        public Graphic descriptorGraphic
        {
            get => m_DescriptorGraphic;
            set => m_DescriptorGraphic = value;
        }

        /// <summary>
        /// Gets and sets the
        /// </summary>
        public string text
        {
            get => m_Text == null ? k_NullText : m_Text.text;
            set
            {
                if (m_Text == null)
                    return;

                m_Text.text = value;
            }
        }

        public Sprite toggleOnSprite
        {
            get => m_ToggleOnSprite;
            set => m_ToggleOnSprite = value;
        }

        public Sprite toggleOffSprite
        {
            get => m_ToggleOffSprite;
            set => m_ToggleOffSprite = value;
        }

        public Image iconImage
        {
            get => m_IconImage;
            set => m_IconImage = value;
        }

        public bool addSelectableComponent
        {
            get => m_AddSelectableComponent;
            set => m_AddSelectableComponent = value;
        }

        void UpdateVisualState()
        {
            if (isToggle)
            {
                switch (m_TransitionType)
                {
                    case TransitionType.ChangeColor when targetImage != null:
                        targetImage.color = @on ? m_OnColor : m_OffColor;
                        break;
                    case TransitionType.SwapSprite when m_TargetImage != null:
                    {
                        if (@on)
                            m_TargetImage.sprite = m_ToggleOnSprite != null ? m_ToggleOnSprite : null;
                        else
                            m_TargetImage.sprite = m_ToggleOffSprite != null ? m_ToggleOffSprite : null;
                        break;
                    }
                }
            }

            if (descriptorGraphic != null)
                descriptorGraphic.color = on ? UIConfig.propertyTextSelectedColor : UIConfig.propertyTextBaseColor;
        }

        void Awake()
        {
            if (m_Text == null)
                m_Text = GetComponentInChildren<TextMeshProUGUI>();

            // Some UI elements may need a selectable component to be queried in the event system
            m_Selectable = GetComponent<Selectable>();
            if (addSelectableComponent && m_Selectable == null)
            {
                m_Selectable = gameObject.AddComponent<Selectable>();
                m_Selectable.transition = Selectable.Transition.None;
            }

            m_ScrollView = GetComponentInParent<ScrollRect>();

            if (Application.isPlaying)
            {
                m_ColorTween = new ColorTween()
                {
                    duration = UIConfig.propertyColorTransitionTime,
                    ignoreTimeScale = true,
                    tweenMode = ColorTween.ColorTweenMode.RGB
                };
                m_ColorTween.AddOnChangedCallback(SetTargetImageColor);
                m_TweenRunner = new TweenRunner<ColorTween>();
                m_TweenRunner.Init(this);
            }
        }

        void Start()
        {
            EventTriggerUtility.CreateEventTrigger(gameObject, OnPointerDown, EventTriggerType.PointerDown);
            EventTriggerUtility.CreateEventTrigger(gameObject, OnPointerUp, EventTriggerType.PointerUp);
            EventTriggerUtility.CreateEventTrigger(gameObject, OnPointerClick, EventTriggerType.PointerClick);

            //If inside a scroll view, bubble up the drag events
            if (m_ScrollView != null)
            {
                EventTriggerUtility.CreateEventTrigger(gameObject, OnBeginDrag, EventTriggerType.BeginDrag);
                EventTriggerUtility.CreateEventTrigger(gameObject, OnDrag, EventTriggerType.Drag);
                EventTriggerUtility.CreateEventTrigger(gameObject, OnEndDrag, EventTriggerType.EndDrag);
                EventTriggerUtility.CreateEventTrigger(gameObject, OnInitializePotentialDrag, EventTriggerType.InitializePotentialDrag);
                EventTriggerUtility.CreateEventTrigger(gameObject, OnScroll, EventTriggerType.Scroll);
            }

            UpdateVisualState();
        }

        void SetTargetImageColor(Color color)
        {
            targetImage.color = color;
        }

        void OnPointerDown(BaseEventData eventData)
        {
            if (!interactable)
                return;

            if (!isToggle && m_TransitionType == TransitionType.ChangeColor && targetImage != null)
            {
                m_ColorTween.startColor = targetImage.color;
                m_ColorTween.targetColor = UIConfig.propertyPressedColor;
                m_TweenRunner.StartTween(m_ColorTween, EaseType.EaseInCubic);
            }
            onControlDown.Invoke(eventData);
        }

        void OnPointerUp(BaseEventData eventData)
        {
            if (!interactable)
                return;

            if (!isToggle && m_TransitionType == TransitionType.ChangeColor && targetImage != null)
            {
                m_ColorTween.startColor = targetImage.color;
                m_ColorTween.targetColor = UIConfig.propertyBaseColor;
                m_TweenRunner.StartTween(m_ColorTween, EaseType.EaseInCubic);
            }
            onControlUp.Invoke(eventData);
        }

        void OnPointerClick(BaseEventData eventData)
        {
            if (!interactable)
                return;

            onControlTap.Invoke(eventData);
        }

        void OnBeginDrag(BaseEventData eventData)
        {
            m_ScrollView.OnBeginDrag((PointerEventData)eventData);
        }

        void OnDrag(BaseEventData eventData)
        {
            m_ScrollView.OnDrag((PointerEventData)eventData);
        }

        void OnEndDrag(BaseEventData eventData)
        {
            m_ScrollView.OnEndDrag((PointerEventData)eventData);
        }

        void OnInitializePotentialDrag(BaseEventData eventData)
        {
            m_ScrollView.OnInitializePotentialDrag((PointerEventData)eventData);
        }

        void OnScroll(BaseEventData eventData)
        {
            m_ScrollView.OnScroll((PointerEventData)eventData);
        }
    }
}
