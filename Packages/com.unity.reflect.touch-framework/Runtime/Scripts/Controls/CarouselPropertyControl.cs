using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    /// <summary>
    /// Control that lets the user select a value from a list of options.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CarouselPropertyControl : MonoBehaviour
    {
        const float k_SegmentSwitchDragDistance = 110f;
        
        [Serializable]
        public class SegmentSelectEvent : UnityEvent<int> {}
#pragma warning disable CS0649
        [Tooltip("The currently active index")]
        [SerializeField]
        int m_ActivePropertyIndex = 0;
        [Tooltip("The selectable components representing each segment")]
        [SerializeField]
        List<Selectable> m_Segments = new List<Selectable>();
        [Tooltip("The image that will be used as a cursor")]
        [SerializeField]
        Image m_Underline;
#pragma warning restore CS0649

        bool m_CanClick;
        bool m_CanDragSelect;
        Image m_Image;
        Selectable m_InteractingSegment;
        Vector2 m_DragStartPointerPosition;
        ColorTween m_ColorTween;
        TweenRunner<ColorTween> m_ColorTweenRunner;
        PositionTween m_CursorPositionTween;
        TweenRunner<PositionTween> m_CursorPositionTweenRunner;
        Dictionary<Selectable, TextMeshProUGUI> m_SegmentTexts = new Dictionary<Selectable, TextMeshProUGUI>();
        SegmentSelectEvent m_OnValueChanged = new SegmentSelectEvent();

        public SegmentSelectEvent onValueChanged => m_OnValueChanged;

        /// <summary>
        /// The active property index
        /// </summary>
        public int activePropertyIndex
        {
            get => m_ActivePropertyIndex;
            set
            {
                if (m_ActivePropertyIndex == value)
                    return;
                
                m_ActivePropertyIndex = value;
                UpdateVisualState();
            }
        }

        void UpdateVisualState()
        {
            var i = 0;
            foreach (var segment in m_Segments)
            {
                m_SegmentTexts[segment].color = i == m_ActivePropertyIndex ? UIConfig.propertyTextBaseColor : UIConfig.propertyTextInactiveColor;
                i++;
            }
        }

        void SetCursorPosition(Vector3 newPosition)
        {
            Vector2 newPos = new Vector2(newPosition.x, newPosition.y);
            m_Underline.rectTransform.position = newPos;
        }
        void TweenCursorPosition()
        {
            m_CursorPositionTween.startPosition = m_Underline.rectTransform.position;
            var activeSegment = m_Segments[m_ActivePropertyIndex];
            var activeSegmentText = m_SegmentTexts[activeSegment];
            Vector2 newPosition = new Vector2();
            newPosition.x = m_Underline.rectTransform.position.x; // can we generalize using the pivots?
            newPosition.y = activeSegmentText.rectTransform.position.y;
            m_CursorPositionTween.targetPosition = newPosition;
            m_CursorPositionTweenRunner.StartTween(m_CursorPositionTween, EaseType.EaseInCubic);
        }
        
        void Awake()
        {
            m_Image = GetComponent<Image>();
            RegisterTextToButtons();
            UpdateVisualState();
        }

        void RegisterTextToButtons()
        {
            foreach (var segment in m_Segments)
            {
                if (!m_SegmentTexts.ContainsKey(segment))
                {
                    var text = segment.GetComponentInChildren<TextMeshProUGUI>();
                    m_SegmentTexts.Add(segment, text);
                }
            }
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                foreach (var segment in m_Segments)
                {
                    EventTriggerUtility.CreateEventTrigger(segment.gameObject, OnControlPointerDown,
                        EventTriggerType.PointerDown);
                    EventTriggerUtility.CreateEventTrigger(segment.gameObject, OnControlPan,
                        EventTriggerType.Drag);
                    EventTriggerUtility.CreateEventTrigger(segment.gameObject, OnControlPointerUp,
                        EventTriggerType.PointerUp);
                }

                m_ColorTween = new ColorTween()
                {
                    duration = UIConfig.propertyColorTransitionTime,
                    ignoreTimeScale = true,
                    tweenMode = ColorTween.ColorTweenMode.RGB
                };
                m_ColorTween.AddOnChangedCallback(SetImageColor);
                m_ColorTweenRunner = new TweenRunner<ColorTween>();
                m_ColorTweenRunner.Init(this);
                
                m_CursorPositionTween = new PositionTween()
                {
                    duration = UIConfig.propertyColorTransitionTime,
                    ignoreTimeScale = true,
                    
                };
                m_CursorPositionTween.AddOnChangedCallback(SetCursorPosition);
                m_CursorPositionTweenRunner = new TweenRunner<PositionTween>();
                m_CursorPositionTweenRunner.Init(this);
            }
        }

        void SetImageColor(Color color)
        {
            m_Image.color = color;
        }

        void OnControlPointerDown(BaseEventData eventData)
        {
            m_ColorTween.startColor = m_Image.color;
            m_ColorTween.targetColor = UIConfig.propertyPressedColor;
            m_ColorTweenRunner.StartTween(m_ColorTween, EaseType.EaseInCubic);
            
            var pointerPosition = ((PointerEventData)eventData).position;
            m_DragStartPointerPosition = pointerPosition;

            m_CanClick = true;
            m_CanDragSelect = true;
            if (eventData.selectedObject != null)
            {
                m_InteractingSegment = eventData.selectedObject.GetComponent<Selectable>();
            }
        }

        void OnControlPan(BaseEventData eventData)
        {
            if (!m_CanDragSelect)
                return;
            
            var pointerPosition = ((PointerEventData)eventData).position;
            var dragXDistance = pointerPosition.x - m_DragStartPointerPosition.x;
            if (Mathf.Abs(dragXDistance) > k_SegmentSwitchDragDistance)
            {
                var index = m_Segments.IndexOf(m_InteractingSegment);
                if (dragXDistance > 0 && index < m_Segments.Count - 1)
                {
                    OnSegmentSelected(index + 1);
                    m_CanDragSelect = false;
                    m_CanClick = false;
                }
                else if (dragXDistance < 0 && index > 0)
                {
                    OnSegmentSelected(index - 1);
                    m_CanDragSelect = false;
                    m_CanClick = false;
                }
            }
        }
        
        void OnControlPointerUp(BaseEventData eventData)
        {
            m_ColorTween.startColor = m_Image.color;
            m_ColorTween.targetColor = UIConfig.propertyBaseColor;
            m_ColorTweenRunner.StartTween(m_ColorTween, EaseType.EaseInCubic);
            
            OnControlPan(eventData);

            m_CanDragSelect = false;
            
            if (m_CanClick)
            {
                var i = 0;
                foreach (var button in m_Segments)
                {
                    if (eventData.selectedObject == button.gameObject)
                        OnSegmentSelected(i);
                    i++;
                }
            }
            
            m_CanClick = false;
        }

        void OnSegmentSelected(int index)
        {
            if (activePropertyIndex == index)
                return;
            
            activePropertyIndex = index;
            TweenCursorPosition();
            
            if (onValueChanged != null)
                onValueChanged.Invoke(activePropertyIndex);
        }

        void Reset()
        {
            GetComponentsInChildren(m_Segments);
        }
    }
}
