using System;
using Unity.Reflect.Viewer.UI;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ExternalToolStateData))]
public class ToolStateDataDrawer : PropertyDrawer
{
    static int buttonHeight = 18;
    static int buttonMargin = buttonHeight / 2;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label, true);

        using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
        {
            if (GUI.Button(new Rect(position.x + position.width / 3, position.y + position.height - buttonHeight - buttonMargin, position.width / 3, buttonHeight), "Update Tools Change"))
            {
                UIStateManager.current.ForceExternalToolChangedEvent();
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property) + buttonHeight + 2 * buttonMargin;
    }
}
