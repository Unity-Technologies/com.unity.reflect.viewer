using Unity.Reflect.ActorFramework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Reflect.Viewer
{
    [CustomEditor(typeof(ViewerReflectBootstrapper))]
    public class ViewerReflectBootstrapperEditor : Editor
    {
        VisualElement m_Container;
        Editor m_SubEditor;

        public override VisualElement CreateInspectorGUI()
        {
            m_Container = new VisualElement();
            BuildDefaultSection(m_Container);
            return m_Container;
        }

        void BuildDefaultSection(VisualElement container)
        {
            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    var propertyField = new PropertyField(iterator.Copy()) { name = "PropertyField:" + iterator.propertyPath };
                    propertyField.BindProperty(iterator.Copy());

                    if (iterator.propertyPath == nameof(ViewerReflectBootstrapper.Asset))
                        propertyField.RegisterValueChangeCallback(OnAssignedAssetChanged);

                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                        propertyField.SetEnabled(false);
 
                    container.Add(propertyField);
                }
                while (iterator.NextVisible(false));
            }
        }

        void OnAssignedAssetChanged(SerializedPropertyChangeEvent evt)
        {
            if (m_SubEditor == null)
            {
                var assetProperty = serializedObject.FindProperty(nameof(ViewerReflectBootstrapper.Asset));
                m_SubEditor = CreateEditorWithContext(new Object[]{ assetProperty.objectReferenceValue }, target, typeof(ActorSystemSetupEditor));
                if (m_SubEditor != null)
                    m_Container.Add(m_SubEditor.CreateInspectorGUI());
            }
        }
    }
}
