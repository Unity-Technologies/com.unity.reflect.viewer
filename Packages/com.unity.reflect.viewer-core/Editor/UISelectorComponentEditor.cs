using System;
using System.Linq;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace UnityEditor.Reflect.Viewer.Core
{

    [CustomEditor(typeof(UISelectorComponent))]
    [CanEditMultipleObjects]
    public class UISelectorComponentEditor : Editor
    {
        SerializedProperty contextTypeName;
        SerializedProperty propertyName;

        void OnEnable()
        {
            contextTypeName = serializedObject.FindProperty("ContextTypeName");
            propertyName = serializedObject.FindProperty("PropertyName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // Contexts
            var contexts = ContextResolver.GetContextTypes();
            var contextNames = contexts.Select(i => i.FullName.Split('.').Last()).ToArray();
            var currentIndex = Array.IndexOf(contextNames, contextTypeName.stringValue);
            var notContextFound = false;
            if (currentIndex < 0)
            {
                // display current value, but with an error box, show prppertyName but disabled
                string[] newValues = new string[contextNames.Length + 1];
                newValues[0] = contextTypeName.stringValue;                                // set the prepended value
                Array.Copy(contextNames, 0, newValues, 1, contextNames.Length);
                contextNames = newValues;
                currentIndex = 0;
                notContextFound = true;
            }
            var index = EditorGUILayout.Popup("Required Context", currentIndex, contextNames);
            if (notContextFound)
                EditorGUILayout.HelpBox("Could not find this named Context, please select a valid one.", MessageType.Error);
            contextTypeName.stringValue = contextNames[index];
            if (notContextFound)
            {
                EditorGUI.BeginDisabledGroup(notContextFound);
                EditorGUILayout.PropertyField(propertyName);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                var propertyNames = ContextResolver.GetContextPropertyNames(contextTypeName.stringValue).ToArray();
                var propertyIndex = Array.IndexOf(propertyNames, propertyName.stringValue);
                var noPropertyFound = false;
                if (propertyIndex < 0)
                {
                    // display current value, but with a warning box
                    string[] newValues = new string[propertyNames.Length + 1];
                    newValues[0] = propertyName.stringValue;                                // set the prepended value
                    Array.Copy(propertyNames, 0, newValues, 1, propertyNames.Length);
                    propertyNames = newValues;
                    propertyIndex = 0;
                    noPropertyFound = true;
                }
                var newIndex = EditorGUILayout.Popup("Property Name", propertyIndex, propertyNames);
                if (noPropertyFound)
                    EditorGUILayout.HelpBox("Could not find this named Property, please select a valid one.", MessageType.Warning);
                else
                {
                    var requiredType = ContextResolver.GetContextPropertyType(contextTypeName.stringValue, propertyName.stringValue);
                    var uiSelectorComponent = (UISelectorComponent)target;
                    var iValue = uiSelectorComponent.GetComponentInChildren<IPropertyValue>(true);
                    if (iValue == null)
                        EditorGUILayout.HelpBox("Missing children widget with iValue implementation.", MessageType.Warning);
                    else
                    {
                        if (!requiredType.IsAssignableFrom(iValue.type))
                            EditorGUILayout.HelpBox("The child iValue found is not assignable to this property.", MessageType.Warning);
                    }
                }

                propertyName.stringValue = propertyNames[newIndex];
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
