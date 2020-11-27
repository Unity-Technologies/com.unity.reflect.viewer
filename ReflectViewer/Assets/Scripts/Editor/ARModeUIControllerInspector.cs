using System.Collections.Generic;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ARModeUIController))]
public class ARModeUIControllerInspector : Editor
{
    SerializedProperty m_InstructionUIs;
    SerializedProperty m_InstructionUI;
    SerializedProperty m_Raycaster;

    SerializedObject m_AssetSO;
    List<SerializedObject> m_AssetSOList;

    void OnEnable()
    {
        m_InstructionUIs = serializedObject.FindProperty("InstructionUIList");
        SetAssetSO();
    }

    void SetAssetSO()
    {
        if (m_InstructionUIs.arraySize != 0)
        {
            m_AssetSOList = new List<SerializedObject>(m_InstructionUIs.arraySize);
            for (var i = 0; i < m_InstructionUIs.arraySize; i++)
            {
                var currAss = m_InstructionUIs.GetArrayElementAtIndex(i);
                if (currAss.objectReferenceValue != null)
                {
                    m_AssetSOList.Add(new SerializedObject( currAss.objectReferenceValue, serializedObject.targetObject ));
                }
                else
                {
                    m_AssetSOList.Add( null);
                }
            }
        }
        else
        {
            m_AssetSOList = null;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        SerializedProperty iterator = serializedObject.GetIterator();
        for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
        {
            if ("InstructionUIList" == iterator.propertyPath)
            {
                iterator.isExpanded = EditorGUILayout.Foldout(iterator.isExpanded, ObjectNames.NicifyVariableName("InstructionUIList"));
                if (iterator.isExpanded) {
                    EditorGUI.BeginChangeCheck();
                    iterator.arraySize = EditorGUILayout.IntField("Size", iterator.arraySize);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetAssetSO();
                    }

                    if (m_AssetSOList != null)
                    {
                        for (int i = 0; i < iterator.arraySize; ++i) {
                            SerializedProperty transformProp = iterator.GetArrayElementAtIndex(i);
                            EditorGUILayout.PropertyField(transformProp, new GUIContent("Element " + i));
                            var so = m_AssetSOList[i];
                            if (so != null)
                            {
                                SerializedProperty sp = so.GetIterator();
                                EditorGUI.indentLevel++;
                                bool enterChild = true;
                                while (sp.NextVisible(enterChild))
                                {
                                    if ("m_Script" == sp.propertyPath)
                                        continue;
                                    enterChild = false;
                                    EditorGUILayout.PropertyField(sp, true);
                                }
                                EditorGUI.indentLevel--;
                                so.ApplyModifiedProperties();
                            }
                        }
                    }

                }
                continue;
            }
            using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                EditorGUILayout.PropertyField(iterator, true);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
