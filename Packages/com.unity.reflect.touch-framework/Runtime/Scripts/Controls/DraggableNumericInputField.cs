using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Unity.Reflect.Viewer.UI.Utils
{
    public class DraggableNumericInputField : TMP_InputField, IPropertyValue<float>
    {
        [Serializable]
        struct MinMaxFloat
        {
            public float Min;
            public float Max;

            public MinMaxFloat(float min, float max)
            {
                Min = min;
                Max = max;
            }

            public float Clamp(float value)
            {
                return Mathf.Clamp(value, Min, Max);
            }
        }

        [Serializable]
        public class FloatChangedEvent : UnityEvent<float> {}

        const string Delta = @"Δ ";

        [SerializeField]
        float m_Increment = 1f;

        [SerializeField]
        MinMaxFloat m_Limits = new MinMaxFloat(float.MinValue, float.MaxValue);

        [SerializeField]
        string m_ValueTextFormat = "F3";

        [SerializeField]
        GameObject m_Tooltip;

        [SerializeField]
        TMP_Text m_TooltipText;


        [SerializeField]
        string m_TooltipTextFormat = "+#;-#;0";

        public FloatChangedEvent OnFloatValueChanged;

        bool m_IsDragging;
        Vector2 m_LastPointerPosition;
        RectTransform m_Rect;


        float m_FloatValue;
        float m_DragValue;
        List<Action> m_Handlers = new List<Action>();
        bool m_DisplayTooltip;
        const float k_TimeToDismissTooltip = 0.5f;

        protected override void Awake()
        {
            base.Awake();
            m_Rect = GetComponent<RectTransform>();
            onValueChanged.AddListener((str)=>UpdateFloatValue(str, false));
            onDeselect.AddListener((str)=>UpdateFloatValue(str, true));
        }

        void UpdateFloatValue(string newText, bool formatText)
        {
            float.TryParse(newText, out m_FloatValue);
            m_FloatValue = m_Limits.Clamp(m_FloatValue);

            if (formatText)
            {
                SetTextWithoutNotify(m_FloatValue.ToString(m_ValueTextFormat));
            }

            foreach (var handler in m_Handlers)
            {
                handler?.Invoke();
            }
            OnFloatValueChanged?.Invoke(m_FloatValue);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            shouldHideSoftKeyboard = true;
            base.OnPointerDown(eventData);
            m_DisplayTooltip = true;
            m_DragValue = 0;
            m_TooltipText.SetText(m_DragValue.ToString(m_TooltipTextFormat));
            m_Tooltip.SetActive(true);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (m_DragValue == 0 && IsMouseOver(eventData.position))
            {
                shouldHideSoftKeyboard = false;
                base.OnPointerDown(eventData);
            }
            base.OnPointerUp(eventData);
            m_DisplayTooltip = false;
            StartCoroutine(DismissTooltip());
        }

        bool IsMouseOver(Vector2 mousePosition)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), mousePosition);
        }

        IEnumerator DismissTooltip()
        {
            yield return new WaitForSeconds(k_TimeToDismissTooltip);
            m_Tooltip.SetActive(m_DisplayTooltip);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            m_LastPointerPosition = eventData.LocalPosition(m_Rect);
            if (float.TryParse(m_Text, out m_FloatValue))
            {
                m_IsDragging = true;
                m_DragValue = 0;
                UpdateValue();
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (m_IsDragging)
            {
                m_IsDragging = false;
                UpdateValue();
                m_DragValue = 0;
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (m_IsDragging)
            {
                var pointerPos = eventData.LocalPosition(m_Rect);
                var delta = pointerPos - m_LastPointerPosition;
                if (!float.IsInfinity(m_DragValue + delta.x * m_Increment) && !float.IsInfinity(m_FloatValue + delta.x * m_Increment))
                {
                    m_DragValue += delta.x * m_Increment;
                    m_FloatValue += delta.x * m_Increment;
                }
                m_LastPointerPosition = pointerPos;
                UpdateValue();
            }
        }

        void UpdateValue()
        {
            m_FloatValue = m_Limits.Clamp(m_FloatValue);
            foreach (var handler in m_Handlers)
            {
                handler?.Invoke();
            }
            OnFloatValueChanged?.Invoke(m_FloatValue);

            SetTextWithoutNotify(m_FloatValue.ToString(m_ValueTextFormat));
            m_TooltipText.SetText(Delta + m_DragValue.ToString(m_TooltipTextFormat));
        }

        float IPropertyValue<float>.value => m_FloatValue;
        Type IPropertyValue.type => typeof(float);

        object IPropertyValue.objectValue
        {
            get => m_FloatValue;

            set
            {
                var newValue = m_Limits.Clamp((float)value);
                if (!isFocused || Math.Abs(newValue - m_FloatValue) > 0.01)
                {
                    m_FloatValue = newValue;
                    SetTextWithoutNotify(m_FloatValue.ToString(m_ValueTextFormat));
                }
            }
        }

        void IPropertyValue.AddListener(Action eventFunc)
        {
            m_Handlers.Add(eventFunc);
        }

        void IPropertyValue.RemoveListener(Action eventFunc)
        {
            m_Handlers.Remove(eventFunc);
        }

        public void SetIncrement(float increment)
        {
            m_Increment = increment;
        }

        public void SetLimits(float min, float max)
        {
            m_Limits.Min = min;
            m_Limits.Max = max;
        }

        public void DisableLimits()
        {
            m_Limits.Min = float.MinValue;
            m_Limits.Max = float.MaxValue;
        }

        public void SetToolTipTextFormat(string format)
        {
            m_TooltipTextFormat = format;
        }
    }
}
