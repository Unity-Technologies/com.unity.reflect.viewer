using System;
using Unity.Reflect;
using Unity.Reflect.Viewer.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.UIElements;

// ProjectDrawerUIE
[CustomPropertyDrawer(typeof(Project))]
public class ProjectDrawerUIE : PropertyDrawer
{
    bool m_valid;
    [Serializable]
    internal class ProjectDummy 
    {
        public string Name;
        public string ProjectId;
        public UnityProjectHost Host;
        public UnityProject.SourceOption Source;
    }

    internal class ProjectSO : ScriptableObject
    {
        public ProjectDummy project;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // The 6 comes from extra spacing between the fields (2px each)
        if (m_valid)
        {
            var propertyDrawer = (PropertyDrawer) Activator.CreateInstance(typeof(HostDrawerUIE));
            var hostHeight = propertyDrawer.GetPropertyHeight(property, label);
            return (EditorGUIUtility.singleLineHeight * 5) + hostHeight;
        }
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
        var project = ScriptableObject.CreateInstance<ProjectSO>();

        project.project = new ProjectDummy();
        var realProject = property.serializedObject.targetObject as UIStateManager;
        if (realProject && label.text.Equals("Active Project"))
        {
            try
            {
                UnityProject unityProject = realProject.projectStateData.activeProject;
                project.project.Name = unityProject.Name;
                project.project.ProjectId = unityProject.ProjectId;
                project.project.Host = unityProject.Host;
                project.project.Source = unityProject.Source;
                m_valid = true;
            }
            catch (Exception)
            {
                m_valid = false;
            }
        }
        else
        {
            if (realProject && label.text.StartsWith("Element"))
            {
                try
                {
                    var index = int.Parse(label.text.Split(' ')[1]);
                    UnityProject unityProject = realProject.sessionStateData.sessionState.rooms[index].project;
                    project.project.Name = unityProject.Name;
                    project.project.ProjectId = unityProject.ProjectId;
                    project.project.Host = unityProject.Host;
                    project.project.Source = unityProject.Source;
                    m_valid = true;  
                }
                catch (Exception)
                {
                    m_valid = false;
                }
            }
        }
        
        SerializedObject serializedObject = new UnityEditor.SerializedObject(project);

        SerializedProperty serializedPropertyProject = serializedObject.FindProperty("project");
        
        EditorGUI.BeginProperty( position, label, property );

        EditorGUI.LabelField( new Rect( position.x, position.y, position.width, 16 ), label );

        if (m_valid)
        {
            var nameRect = new Rect( position.x, position.y + 18, position.width, 16 );
            var projecIdRect = new Rect( position.x, position.y + 36, position.width, 16 );
            var hostRect = new Rect(position.x, position.y + 54, position.width, 16);

            EditorGUI.indentLevel++;

            EditorGUI.PropertyField( nameRect, serializedPropertyProject.FindPropertyRelative( "Name" ) );
            EditorGUI.PropertyField( projecIdRect, serializedPropertyProject.FindPropertyRelative( "ProjectId" ) );
            var hostProperty = serializedPropertyProject.FindPropertyRelative("Host");
            if (hostProperty != null)
            {
                EditorGUI.PropertyField( hostRect, serializedPropertyProject.FindPropertyRelative( "Host" ) );
            }

            EditorGUI.indentLevel--;            
        }
        EditorGUI.EndProperty();
        
        ScriptableObject.DestroyImmediate(project);
    }
}
