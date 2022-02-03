using Unity.Properties.UI.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI
{
    sealed class InspectorContent : ScriptableObject
    {
        public static InspectorContent Show(ContentProvider provider, InspectorContentParameters parameters)
        {
            var dynamicContent = CreateInstance<InspectorContent>();
            dynamicContent.SetContent(new SerializableContent {Provider = provider}, parameters);
            Selection.activeObject = dynamicContent;
            return dynamicContent;
        }
        
        [SerializeField] SerializableContent m_Content;
        [SerializeField] InspectorContentParameters m_Parameters;
        public SerializableContent Content => m_Content;
        public BindableElement Root { get; private set; }

        void SetContent(SerializableContent content, InspectorContentParameters parameters)
        {
            m_Parameters = parameters;
            m_Content = content;
            m_Content.InspectionContext.ApplyInspectorStyling = m_Parameters.ApplyInspectorStyling;
            m_Content.InspectionContext.UseDefaultMargins = m_Parameters.UseDefaultMargins;
            m_Content.Root = Root;
            m_Content.Initialize();
            m_Content.Load();
        }
        
        // Invoked by the Unity update loop
        void OnEnable() 
        {
            Root = new BindableElement();
            if (null != m_Content)
                SetContent(m_Content, m_Parameters);
        }
    }
}
