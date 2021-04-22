using System;
using Unity.Reflect.Viewer.UI;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UIStateData))]
public class UIStateDataDrawer : PropertyDrawer
{
    static int buttonHeight = 18;
    static int buttonMargin = buttonHeight / 2;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label, true);

        using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
        {
            if (GUI.Button(new Rect(position.x + position.width / 3, position.y + position.height - buttonHeight - buttonMargin, position.width / 3, buttonHeight), "Update State Change"))
            {
                UIStateManager.current.ForceSendStateChangedEvent();
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property) + buttonHeight + 2 * buttonMargin;
    }
}
