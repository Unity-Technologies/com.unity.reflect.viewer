using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.TouchFramework
{
    /// <summary>
    /// Place this component on RectTransform to generate an input axis based on the position relative to its parent
    /// RectTransform
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class JoystickControl : MonoBehaviour, IPointerUpHandler, IDragHandler, IPointerDownHandler
    {
        const float k_ResetJoystickTransitionTime = 0.1f;

        [SerializeField]
        [Tooltip("Higher values result in more movement of the joystick")]
        float m_Sensitivity = 0.4f;
#pragma warning disable 649
        [SerializeField]
        bool m_DisableXMovement;
        [SerializeField]
        bool m_IsPersistentJoystick = false;
#pragma warning restore 649
        RectTransform m_JoystickContainerRect;
        RectTransform m_JoystickRect;
        Vector2 m_InputAxis;
        Vector2 m_JoystickPositionOnPointerUp;
        public float distanceFromCenter;
        Coroutine m_MoveJoystickCoroutine;
        const float k_MovementRange = 50;

        /// <summary>
        /// Values of x and y between 0 and 1 that represent joystick movement
        /// </summary>
        public Vector2 inputAxis
        {
            get { return m_InputAxis; }
        }

        /// <summary>
        /// Higher values result in more movement of the joystick
        /// </summary>
        public float sensitivity
        {
            get => m_Sensitivity;
            set => m_Sensitivity = value;
        }

        void Awake()
        {
            m_JoystickRect = GetComponent<RectTransform>();

            if (transform.parent == null)
                throw new NullReferenceException("Parent is null. The JoystickControl component can't be on " +
                    "root game object.");

            m_JoystickContainerRect = transform.parent.GetComponent<RectTransform>();

            if (m_JoystickContainerRect == null)
                throw new NullReferenceException("JoystickControl component must be on a game object that is" +
                    " the child of a RectTransform");
        }

        IEnumerator ResetJoystickPosition()
        {
            var elapsedTime = 0f;
            var alpha = 0f;
            while (alpha <= 1f)
            {
                alpha = elapsedTime / k_ResetJoystickTransitionTime;
                alpha -= 1f;
                alpha = alpha * alpha * alpha + 1f;

                m_JoystickRect.anchoredPosition = Vector2.Lerp(m_JoystickPositionOnPointerUp, Vector2.zero, alpha);

                elapsedTime += Time.unscaledDeltaTime;

                yield return null;
            }

            if (m_IsPersistentJoystick)
                transform.parent.gameObject.SetActive(false);

            m_MoveJoystickCoroutine = null;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData pointerEventData)
        {
            if (m_MoveJoystickCoroutine != null)
            {
                StopCoroutine(m_MoveJoystickCoroutine);
                m_MoveJoystickCoroutine = null;
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData pointerEventData)
        {
            m_InputAxis = Vector2.zero;
            m_JoystickPositionOnPointerUp = m_JoystickRect.anchoredPosition;

            if (m_MoveJoystickCoroutine != null)
            {
                StopCoroutine(m_MoveJoystickCoroutine);
                m_MoveJoystickCoroutine = null;
            }

            m_MoveJoystickCoroutine = StartCoroutine(ResetJoystickPosition());
        }

        void IDragHandler.OnDrag(PointerEventData pointerEventData)
        {
            var pointerPosition = pointerEventData.position;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_JoystickContainerRect,
                pointerPosition, pointerEventData.pressEventCamera, out var position);

            position.x /= m_JoystickContainerRect.sizeDelta.x;
            position.y /= m_JoystickContainerRect.sizeDelta.y;

            var x = 0f;
            if (!m_DisableXMovement)
                x = (Math.Abs(m_JoystickContainerRect.pivot.x - 1f) < Mathf.Epsilon) ? position.x * 2f + 1f : position.x * 2f - 1f;

            var y = (Math.Abs(m_JoystickContainerRect.pivot.y - 1f) < Mathf.Epsilon) ? position.y * 2f + 1f : position.y * 2f - 1f;

            m_InputAxis.Set(x, y);
            m_InputAxis = (m_InputAxis.magnitude > 1) ? m_InputAxis.normalized : m_InputAxis;

            distanceFromCenter = m_JoystickRect.localPosition.magnitude / k_MovementRange;
            m_JoystickRect.anchoredPosition = new Vector2(m_InputAxis.x * m_JoystickContainerRect.sizeDelta.x *
                sensitivity, m_InputAxis.y * m_JoystickContainerRect.sizeDelta.y * sensitivity);
        }
    }
}
