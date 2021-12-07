using System;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace UnityEditor.Reflect.Viewer.Core
{
    [CustomPropertyDrawer(typeof(ContextButtonAttribute))]
    public class ContextButtonDrawer : PropertyDrawer
    {
        static int buttonHeight = 18;
        static int buttonMargin = buttonHeight / 2;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // First get the attribute since it contains the range for the slider
            ContextButtonAttribute contextButton = attribute as ContextButtonAttribute;

            EditorGUI.PropertyField(position, property, label, true);

            using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
            {
                if (GUI.Button(new Rect(position.x + position.width / 3, position.y + position.height - buttonHeight - buttonMargin, position.width / 3, buttonHeight), contextButton.ButtonLabel))
                {
                    (property.serializedObject.targetObject as MonoBehaviour).Invoke(contextButton.MethodToInvoke, 0.0f);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property) + buttonHeight + 2 * buttonMargin;
        }
    }
}
