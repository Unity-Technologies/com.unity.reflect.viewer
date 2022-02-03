using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ToggleExpand : MonoBehaviour
    {
        [SerializeField]
        Toggle m_Toggle;
        [SerializeField]
        RectTransform m_ExpandableRectTransform;

        [SerializeField]
        bool m_Lerp = true;
        [SerializeField]
        float m_LerpTime = .5f;

        [SerializeField]
        float m_CollapsedSize = 40f;
        [SerializeField]
        float m_ExpandedSize = 180f;
        [SerializeField]
        RectTransform.Axis m_Axis = RectTransform.Axis.Vertical;

        LayoutGroup m_LayoutGroup;
        LayoutElement m_LayoutElement;
        IEnumerator m_Coroutine;

        // Lerp state
        float m_LerpPosition = 0f;
        bool m_Expanding = true;
        float m_Size = 0f;

        void Start()
        {
            m_LayoutGroup = GetComponentInParent<LayoutGroup>();
            m_LayoutElement = GetComponent<LayoutElement>();

            m_Toggle.onValueChanged.AddListener(HandleToggleInput);
            InstantEffect(m_Toggle.isOn);
        }

        void HandleToggleInput(bool value)
        {
            if (m_Lerp)
                LerpEffect(value);
            else
                InstantEffect(value);
        }

        /// <summary>
        /// Instantly expand/contract element
        /// </summary>
        /// <param name="value"></param>
        void InstantEffect(bool value)
        {
            if (value)
                m_Size = m_ExpandedSize;
            else
                m_Size = m_CollapsedSize;
            m_ExpandableRectTransform.SetSizeWithCurrentAnchors(m_Axis, m_Size);
            UpdateLayout();
        }

        /// <summary>
        /// Animate the expanding/contracting of the element
        /// </summary>
        /// <param name="value"></param>
        void LerpEffect(bool value)
        {
            m_Expanding = value;
            if (m_Coroutine == null)
            {
                m_Coroutine = LerpSize();
                StartCoroutine(m_Coroutine);
            }
        }

        /// <summary>
        /// Animate the expanding/contracting of the element.
        /// </summary>
        /// <returns></returns>
        IEnumerator LerpSize()
        {
            SyncLerpSize();
            while ((m_Expanding && m_LerpPosition < 1f) ||
                (!m_Expanding && m_LerpPosition > 0f))
            {
                if (m_Expanding)
                    m_LerpPosition += (1/m_LerpTime) * Time.deltaTime;
                else
                    m_LerpPosition -= (1/m_LerpTime) * Time.deltaTime;

                m_Size = Mathf.Lerp(m_CollapsedSize, m_ExpandedSize, m_LerpPosition);
                
                m_ExpandableRectTransform.SetSizeWithCurrentAnchors(m_Axis, m_Size);
                UpdateLayout();
                yield return null;
            }

            m_Coroutine = null;
        }

        /// <summary>
        /// Update lerpPosition value with actual element state
        /// </summary>
        void SyncLerpSize()
        {
            float travel = m_ExpandedSize - m_CollapsedSize;
            float rawState = 0f;
            switch (m_Axis)
            {
                case RectTransform.Axis.Vertical:
                    rawState = m_ExpandableRectTransform.rect.size.y;
                    break;
                case RectTransform.Axis.Horizontal:
                    rawState = m_ExpandableRectTransform.rect.size.x;
                    break;
            }

            float state = rawState - m_CollapsedSize;
            m_LerpPosition = state / travel;
        }

        /// <summary>
        /// Set layout elements with updated size, force rebuilding the layout
        /// </summary>
        void UpdateLayout()
        {
            switch (m_Axis)
            {
                case RectTransform.Axis.Vertical:
                    if (m_LayoutElement)
                        m_LayoutElement.preferredHeight = m_Size;
                    break;
                case RectTransform.Axis.Horizontal:
                    if (m_LayoutElement)
                        m_LayoutElement.preferredWidth = m_Size;
                    break;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)m_LayoutGroup.transform);
        }
    }
}
