using System;
using Unity.SpatialFramework.Input;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Component fires a scroll input event from an input action such as a joystick.
    /// The scroll is constrained to be either horizontal or vertical, but not both, based on whichever one passes a threshold first
    /// </summary>
    public class ScrollInputController : MonoBehaviour, IUsesInputActions
    {
        const float k_InputSmoothTime = 0.1f;
        const float k_ReleaseThreshold = 0.05f;

        enum StickAxis
        {
            None,
            Horizontal,
            Vertical,
        }

#pragma warning disable 649
        [SerializeField, Tooltip("An action map that contains an action named Scroll")]
        InputActionAsset m_ActionsAsset;
#pragma warning restore 649

        InputAction m_ScrollAction;
        StickAxis m_CurrentDirection = StickAxis.None;
        Vector2 m_InputValue;
        Vector2 m_InputValueVelocity;

        IProvidesInputActions IFunctionalitySubscriber<IProvidesInputActions>.provider { get; set; }

        public InputActionAsset inputActionsAsset { get => m_ActionsAsset; }

        /// <summary>
        /// Action that is called when scrolling occurs
        /// </summary>
        public Action<Vector2> onInput { private get; set; }

        void IUsesInputActions.OnActionsCreated(InputActionAsset input)
        {
            m_ScrollAction = input.FindAction("Scroll");
            m_CurrentDirection = StickAxis.None;
        }

        void IUsesInputActions.ProcessInput(InputActionAsset input)
        {
            var value = m_ScrollAction.ReadValue<Vector2>();
            var x = value.x;
            var y = value.y;
            var absX = Mathf.Abs(x);
            var absY = Mathf.Abs(y);

            // Stick dead zone will already be processed by input system, but this threshold is to release from one axis and switch to the other
            if (m_CurrentDirection == StickAxis.None)
            {
                if (absX > absY)
                {
                    m_CurrentDirection = StickAxis.Horizontal;
                }
                else if (absY > absX)
                {
                    m_CurrentDirection = StickAxis.Vertical;
                }
            }
            else if (absX <= k_ReleaseThreshold && absY <= k_ReleaseThreshold)
            {
                m_CurrentDirection = StickAxis.None;
            }

            if (m_CurrentDirection == StickAxis.Horizontal)
            {
                y = 0f;
            }
            else if (m_CurrentDirection == StickAxis.Vertical)
            {
                x = 0f;
            }

            if (m_CurrentDirection != StickAxis.None)
            {
                m_InputValue = Vector2.SmoothDamp(current: m_InputValue, target: new Vector2(x, y), currentVelocity: ref m_InputValueVelocity, smoothTime: k_InputSmoothTime, maxSpeed: Mathf.Infinity, deltaTime: Time.unscaledDeltaTime);
                this.ConsumeControl(m_ScrollAction.activeControl);
            }
            else
            {
                m_InputValue = Vector2.zero;
            }

            onInput?.Invoke(m_InputValue);
        }
    }
}
