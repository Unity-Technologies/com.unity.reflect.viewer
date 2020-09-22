using System;
using Unity.Reflect;
using Unity.Reflect.Viewer.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.UIElements;

// UserDrawerUIE
[CustomPropertyDrawer(typeof(UnityUser))]
public class UserDrawerUIE : PropertyDrawer
{
    bool m_valid;
    [Serializable]
    class UserDummy
    {
        public string AccessToken;
        public string DisplayName;
        public string UserId;
    }

    class UserSO : ScriptableObject
    {
        public UserDummy user;
    }
    
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
        // The 6 comes from extra spacing between the fields (2px each)
        if (m_valid)
        {
            return EditorGUIUtility.singleLineHeight * 4;
        }
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
        var user = ScriptableObject.CreateInstance<UserSO>();

        user.user = new UserDummy();
        var realUser = property.serializedObject.targetObject as UIStateManager;
        if (realUser)
        {
            try
            {
                user.user.AccessToken = realUser.sessionStateData.sessionState.user.AccessToken;
                user.user.DisplayName = realUser.sessionStateData.sessionState.user.DisplayName;
                user.user.UserId = realUser.sessionStateData.sessionState.user.UserId;
                m_valid = true;
            }
            catch (Exception)
            {
                m_valid = false;
            }
        }
    
        SerializedObject serializedObject = new UnityEditor.SerializedObject(user);

        SerializedProperty serializedPropertyUser = serializedObject.FindProperty("user");
        
        EditorGUI.BeginProperty( position, label, property );

        EditorGUI.LabelField( new Rect( position.x, position.y, position.width, 16 ), label );

        if (m_valid)
        {
            var tokenRect = new Rect( position.x, position.y + 18, position.width, 16 );
            var nameRect = new Rect( position.x, position.y + 36, position.width, 16 );
            var userIdRect = new Rect( position.x, position.y + 54, position.width, 16 );

            EditorGUI.indentLevel++;

            EditorGUI.PropertyField( tokenRect, serializedPropertyUser.FindPropertyRelative( "AccessToken" ) );
            EditorGUI.PropertyField( nameRect, serializedPropertyUser.FindPropertyRelative( "DisplayName" ) );
            EditorGUI.PropertyField( userIdRect, serializedPropertyUser.FindPropertyRelative( "UserId" ) );

            EditorGUI.indentLevel--;            
        }
        EditorGUI.EndProperty();
        
        ScriptableObject.DestroyImmediate(user);
    }
}