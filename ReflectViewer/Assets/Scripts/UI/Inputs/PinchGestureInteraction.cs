using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Reflect.Viewer.Input;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    ///     Pinch Gesture interaction.
    /// </summary>

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    public class PinchGestureInteraction : GestureInteraction<PinchGesture>
    {
        [Preserve]
        static PinchGestureInteraction()
        {
            InputSystem.RegisterInteraction<PinchGestureInteraction>();
        }

        protected override GestureRecognizer<PinchGesture> CreateRecognizer(TouchControl touch1, TouchControl touch2)
        {
            return new PinchGestureRecognizer(touch1, touch2);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [Preserve]
        private static void Init()
        {
        }
    }
}
