using System;
using Unity.Reflect;
using UnityEditor;
using UnityEngine;

// ProjectDrawerUIE
[CustomPropertyDrawer(typeof(UnityProjectHost))]
public class HostDrawerUIE : PropertyDrawer
{
    bool m_valid;
    [Serializable]
    class HostDummy
    {
        public string ServerId;
        public string ServerName;
    }

    class HostSO : ScriptableObject
    {
        public HostDummy host;
    }
    
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
        // The 6 comes from extra spacing between the fields (2px each)
        if (m_valid)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
        var host = ScriptableObject.CreateInstance<HostSO>();

        host.host = new HostDummy();
        var realProject = property.serializedObject.targetObject as ProjectDrawerUIE.ProjectSO;
        var flag = label.text.Equals("Host");
        if (realProject && label.text.Equals("Host"))
        {
            try
            {
                UnityProjectHost realHost = realProject.project.Host;
                host.host.ServerId = realHost.ServerId;
                host.host.ServerName = realHost.ServerName;
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
                    UnityProjectHost realHost = realProject.project.Host;
                    host.host.ServerId = realHost.ServerId;
                    host.host.ServerName = realHost.ServerName;
                    m_valid = true;  
                }
                catch (Exception)
                {
                    m_valid = false;
                }
            }
        }
        SerializedObject serializedObject = new UnityEditor.SerializedObject(host);

        SerializedProperty serializedPropertyProject = serializedObject.FindProperty("host");
        
        EditorGUI.BeginProperty( position, label, property );

        EditorGUI.LabelField( new Rect( position.x, position.y, position.width, 16 ), label );

        if (m_valid)
        {
            var serverIdRect = new Rect( position.x, position.y + 18, position.width, 16 );
            var serverNameRect = new Rect( position.x, position.y + 36, position.width, 16 );

            EditorGUI.indentLevel++;

            EditorGUI.PropertyField( serverIdRect, serializedPropertyProject.FindPropertyRelative( "ServerId" ) );
            EditorGUI.PropertyField( serverNameRect, serializedPropertyProject.FindPropertyRelative( "ServerName" ) );

            EditorGUI.indentLevel--;            
        }
        EditorGUI.EndProperty();
        
        ScriptableObject.DestroyImmediate(host);
    }
}