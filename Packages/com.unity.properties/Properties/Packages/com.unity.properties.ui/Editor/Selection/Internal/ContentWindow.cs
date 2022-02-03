using Unity.Properties.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    sealed class ContentWindow : EditorWindow
    {
        public static ContentWindow Show(ContentProvider provider, ContentWindowParameters options)
        {
            var window = CreateInstance<ContentWindow>();
            window.SetContent(new SerializableContent {Provider = provider}, options);
            window.Show();
            return window;
        }

        [SerializeField] SerializableContent m_Content;
        [SerializeField] Vector2 m_ScrollPosition;
        [SerializeField] ContentWindowParameters m_Options;
        
        ScrollView m_ScrollView;

        void SetContent(SerializableContent content, ContentWindowParameters options)
        {
            m_Content = content;
            m_Content.InspectionContext.ApplyInspectorStyling = options.ApplyInspectorStyling;
                
            m_Options = options;
            if (options.AddScrollView)
            {
                m_Content.Root = m_ScrollView;
                rootVisualElement.contentContainer.Add(m_ScrollView);
            }
            else
            {
                m_Content.Root = rootVisualElement;
            }
            
            if (options.ApplyInspectorStyling)
                m_Content.Root.contentContainer.style.paddingLeft = 15;

            m_Content.Initialize();
            m_Content.Load();
            m_Content.Update();
            
            titleContent.text = m_Content.Name ?? nameof(ContentWindow);
            minSize = m_Options.MinSize;
        }

        // Invoked by the Unity update loop
        void OnEnable()
        {
            rootVisualElement.AddToClassList(UssClasses.Unity.Inspector);
            m_ScrollView = new ScrollView {scrollOffset = m_ScrollPosition};
            
            if (null != m_Content)
                SetContent(m_Content, m_Options);
        }

        // Invoked by the Unity update loop
        void Update()
        {
            m_ScrollPosition = m_ScrollView.scrollOffset;
            titleContent.text = !string.IsNullOrEmpty(m_Content.Name)
                ? m_Content.Name
                : TypeUtility.GetTypeDisplayName(m_Content.GetType());
            m_Content.Update();
            if (!m_Content.IsValid)
                Close();
            
            // We are saving here because we want to store the data inside the editor window so that it survives both
            // domain reloads and closing/re-opening Unity.
            m_Content.Save();
        }
    }
}
