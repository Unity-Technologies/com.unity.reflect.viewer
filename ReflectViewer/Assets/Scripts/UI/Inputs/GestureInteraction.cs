using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Reflect.Viewer.Input;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Reflect.Viewer.UI
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    public abstract class GestureInteraction<T> : IInputInteraction
        where T : Gesture<T>
    {
        private GestureRecognizer<T> m_GestureRecognizer;
        private InputActionPhase m_InternalPhase;
        private bool m_StartedRegistered;
        private Gesture<T> m_CurrentGesture;
        public Gesture<T> currentGesture { get => m_CurrentGesture; }

        public void Reset()
        {
            m_InternalPhase = InputActionPhase.Disabled;
            if (m_GestureRecognizer != null)
            {
                m_GestureRecognizer.Reset();
                m_GestureRecognizer.onGestureStarted -= OnGestureStarted;
                m_StartedRegistered = false;
            }
        }

        public void Process(ref InputInteractionContext context)
        {
            if (m_GestureRecognizer != null &&
                (m_InternalPhase != InputActionPhase.Disabled  ||
                m_InternalPhase != InputActionPhase.Performed ||
                m_InternalPhase != InputActionPhase.Canceled))
            {
                m_GestureRecognizer.Update();
            }

            if (context.timerHasExpired)
            {
                context.Performed();
                return;
            }

            var phase = context.phase;

            switch (phase)
            {
                case InputActionPhase.Disabled:
                    break;
                case InputActionPhase.Waiting:

                    if (context.ControlIsActuated())
                    {
                        if (m_GestureRecognizer == null)
                        {
                            TouchControl touch1 = null, touch2 = null;
                            var controls = context.action.controls;
                            AssignControls(controls,ref touch1, ref touch2);
                            if (touch1 != null && touch2 != null)
                            {
                                m_GestureRecognizer = CreateRecognizer(touch1, touch2);
                                m_InternalPhase = InputActionPhase.Waiting;
                            }
                        }

                        if (m_InternalPhase == InputActionPhase.Started)
                        {
                            context.Started();
                            context.SetTimeout(float.PositiveInfinity);
                        }
                        else
                        {
                            if (m_StartedRegistered == false)
                            {
                                m_GestureRecognizer.onGestureStarted += OnGestureStarted;
                                m_StartedRegistered = true;
                            }
                            context.Waiting();
                        }
                    }
                    break;
                case InputActionPhase.Started:
                    context.PerformedAndStayPerformed();
                    break;
                case InputActionPhase.Performed:
                    if (m_InternalPhase == InputActionPhase.Canceled)
                    {
                        context.Canceled();

                    }
                    else if (m_InternalPhase == InputActionPhase.Performed)
                    {
                        context.Performed();
                    }
                    else
                    {
                        if (context.ControlIsActuated())
                        {
                            context.PerformedAndStayPerformed();
                        }
                    }
                    break;
                case InputActionPhase.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        void AssignControls(ReadOnlyArray<InputControl> controls, ref TouchControl touch1, ref TouchControl touch2)
        {
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i] is TouchControl touch)
                {
                    if (touch.isInProgress)
                    {
                        if (touch1 == null)
                        {
                            touch1 = touch;
                        }
                        else
                        {
                            touch2 = touch;
                            break;
                        }
                    }
                }
            }
        }

        private void OnGestureStarted(Gesture<T> pinchGesture)
        {
            pinchGesture.onFinished += OnGestureFinished;
            m_InternalPhase = InputActionPhase.Started;
            m_CurrentGesture = pinchGesture;
        }

        private void OnGestureFinished(Gesture<T> pinchGesture)
        {
            pinchGesture.onFinished -= OnGestureFinished;

            m_GestureRecognizer.onGestureStarted -= OnGestureStarted;
            m_StartedRegistered = false;
            if (pinchGesture.WasCancelled)
            {
                m_InternalPhase = InputActionPhase.Canceled;
                m_CurrentGesture = null;
            }
            else
            {
                m_InternalPhase = InputActionPhase.Performed;
                m_CurrentGesture = pinchGesture;
            }
            m_GestureRecognizer.Reset();
        }

        protected abstract GestureRecognizer<T> CreateRecognizer(TouchControl touch1, TouchControl touch2);
    }
}
