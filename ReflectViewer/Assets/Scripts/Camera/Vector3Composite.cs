using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem.Editor;
#endif
namespace UnityEngine.Reflect
{
    /// <summary>
    ///     This implementation is a copy/paste/upgrade of <see cref="Vector2Composite"/>.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad] // Automatically register in editor.
#endif
    [Preserve]
    [DisplayStringFormat("{up}/{left}/{down}/{right}/{forward}/{backward}")]
    public class Vector3Composite : InputBindingComposite<Vector3>
    {
        [InputControl(layout = "Button")] public int up = 0;
        [InputControl(layout = "Button")] public int down = 0;

        [InputControl(layout = "Button")] public int left = 0;
        [InputControl(layout = "Button")] public int right = 0;

        [InputControl(layout = "Button")] public int forward = 0;
        [InputControl(layout = "Button")] public int backward = 0;

        [Obsolete("Use Mode.DigitalNormalized with 'mode' instead")]
        public bool normalize = true;
        public Mode mode;

        static Vector3Composite()
        {
            InputSystem.InputSystem.RegisterBindingComposite<Vector3Composite>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init() {} // Trigger static constructor.

        public override Vector3 ReadValue(ref InputBindingCompositeContext context)
        {
            var mode = this.mode;

            if (mode == Mode.Analog)
            {
                var upValue = context.ReadValue<float>(up);
                var downValue = context.ReadValue<float>(down);
                var leftValue = context.ReadValue<float>(left);
                var rightValue = context.ReadValue<float>(right);
                var forwardValue = context.ReadValue<float>(forward);
                var backwardValue = context.ReadValue<float>(backward);

                return new Vector3(-leftValue + rightValue, upValue - downValue, forwardValue - backwardValue);
            }

            var upIsPressed = context.ReadValueAsButton(up);
            var downIsPressed = context.ReadValueAsButton(down);
            var leftIsPressed = context.ReadValueAsButton(left);
            var rightIsPressed = context.ReadValueAsButton(right);
            var forwardIsPressed = context.ReadValueAsButton(forward);
            var backwardIsPressed = context.ReadValueAsButton(backward);

            // Legacy. We need to reference the obsolete member here so temporarily
            // turn of the warning.
            #pragma warning disable CS0618
            if (!normalize) // Was on by default.
                mode = Mode.Digital;
            #pragma warning restore CS0618

            return MakeDpadVector3(upIsPressed, downIsPressed, leftIsPressed, rightIsPressed, forwardIsPressed, backwardIsPressed, mode == Mode.DigitalNormalized);
        }

        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            var value = ReadValue(ref context);
            return value.magnitude;
        }

        public enum Mode
        {
            Analog = 2,
            DigitalNormalized = 0,
            Digital = 1
        }

        static Vector3 MakeDpadVector3(bool up, bool down, bool left, bool right, bool forward, bool backward, bool normalize = true)
        {
            var upValue = up ? 1.0f : 0.0f;
            var downValue = down ? -1.0f : 0.0f;
            var leftValue = left ? -1.0f : 0.0f;
            var rightValue = right ? 1.0f : 0.0f;
            var forwardValue = forward ? 1.0f : 0.0f;
            var backwardValue = backward ? -1.0f : 0.0f;

            var result = new Vector3(leftValue + rightValue, upValue + downValue, forwardValue + backwardValue);

            if (normalize)
            {
                var nbDirections = (result.x != 0.0f ? 1 : 0) + (result.y != 0.0f ? 1 : 0) + (result.z != 0 ? 1 : 0);
                if (nbDirections > 1)
                {
                    result.Normalize();
                }
            }

            return result;
        }
    }

    #if UNITY_EDITOR
    internal class Vector3CompositeEditor : InputParameterEditor<Vector3Composite>
    {
        private GUIContent m_ModeLabel = new GUIContent("Mode",
            "How to create synthesize a Vector3 from the inputs. Digital "
            + "treats part bindings as buttons (on/off) whereas Analog preserves "
            + "floating-point magnitudes as read from controls.");

        public override void OnGUI()
        {
            target.mode = (Vector3Composite.Mode)EditorGUILayout.EnumPopup(m_ModeLabel, target.mode);
        }
    }
    #endif
}
