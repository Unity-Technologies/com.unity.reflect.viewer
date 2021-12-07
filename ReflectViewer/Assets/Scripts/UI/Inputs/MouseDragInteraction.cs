using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    ///     Mouse drag interaction.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class MouseDragInteraction : IInputInteraction
    {
        static MouseDragInteraction()
        {
            InputSystem.RegisterInteraction<MouseDragInteraction>();
        }

        public void Reset()
        {
        }

        public void Process(ref InputInteractionContext context)
        {
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
                        context.Started();
                        context.SetTimeout(float.PositiveInfinity);
                    }

                    break;
                case InputActionPhase.Started:
                    context.PerformedAndStayPerformed();
                    break;
                case InputActionPhase.Performed:
                    if (context.ControlIsActuated())
                    {
                        context.PerformedAndStayPerformed();
                    }
                    else
                    {
                        ButtonControl buttonControl = context.action.controls[0] as ButtonControl;
                        Pointer pointer = context.action.controls[0] as Pointer;
                        if (buttonControl != null && !buttonControl.isPressed)
                            context.Canceled();
                        if (pointer != null && !pointer.IsPressed())
                            context.Canceled();
                    }

                    break;
                case InputActionPhase.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
        }
    }
}
