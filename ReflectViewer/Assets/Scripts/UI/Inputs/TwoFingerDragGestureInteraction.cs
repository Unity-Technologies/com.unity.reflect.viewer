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
    ///     Two Finger Drag Gesture interaction.
    /// </summary>

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    public class TwoFingerDragGestureInteraction : GestureInteraction<TwoFingerDragGesture>
    {
        [Preserve]
        static TwoFingerDragGestureInteraction()
        {
            InputSystem.RegisterInteraction<TwoFingerDragGestureInteraction>();
        }

        protected override GestureRecognizer<TwoFingerDragGesture> CreateRecognizer(TouchControl touch1, TouchControl touch2)
        {
            return new TwoFingerDragGestureRecognizer(touch1, touch2);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [Preserve]
        private static void Init()
        {
        }
    }
}
