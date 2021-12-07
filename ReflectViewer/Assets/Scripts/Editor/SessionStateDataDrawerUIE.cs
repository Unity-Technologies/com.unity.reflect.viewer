using System;
using System.Collections.Generic;
using Unity.Reflect;
using Unity.Reflect.Viewer.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;

[CustomPropertyDrawer(typeof(UISessionStateData))]
public class SessionStateDataDrawer : PropertyDrawer
{
    static int s_ButtonHeight = 18;
    static int s_ButtonMargin = s_ButtonHeight / 2;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label, true);

        using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
        {
            if (GUI.Button(new Rect(position.x + position.width / 3, position.y + position.height - s_ButtonHeight - s_ButtonMargin, position.width / 3, s_ButtonHeight), "Display Empty list"))
            {
                ReflectProjectsManager.projectsRefreshCompleted.Invoke(new List<Project>());
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property) + 2 * s_ButtonHeight  + 3 * s_ButtonMargin;
    }
}
