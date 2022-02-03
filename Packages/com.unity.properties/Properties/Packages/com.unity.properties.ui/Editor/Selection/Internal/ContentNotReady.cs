using System;
using JetBrains.Annotations;
using Unity.Properties.Editor;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    struct ContentNotReady
    {
        readonly ContentProvider m_Provider;
        string m_PreviousName;
        string m_CachedText;

        public ContentNotReady(ContentProvider provider)
        {
            m_Provider = provider;
            m_PreviousName = string.Empty;
            m_CachedText = string.Empty;
            CacheDisplayText();
        }

        [CreateProperty, UsedImplicitly]
        public string Text
        {
            get
            {
                CacheDisplayText();
                return m_CachedText;
            }
        }
        
        void CacheDisplayText()
        {
            var name = m_Provider.Name;
            if (m_PreviousName == name) 
                return;
            m_PreviousName = name;
            m_CachedText = $"{(string.IsNullOrEmpty(name) ? TypeUtility.GetTypeDisplayName(m_Provider.GetType()) : name)} is not ready for display";
        }
    }

    [UsedImplicitly]
    class ContentNotReadyInspector : Inspector<ContentNotReady>
    {
        const string k_Prefix = "content-not-ready__spinner-";
        
        VisualElement m_Spinner;
        int m_Index;
        long m_TimePerImage = Convert.ToInt64(1000.0f / 12.0f);
        
        public override VisualElement Build()
        {
            var root = Resources.Templates.ContentNotReady.Clone();
            m_Spinner = root.Q(className: "content-not-ready__spinner");
            m_Spinner.schedule.Execute(TimerUpdateEvent).Every(m_TimePerImage);
            return root;
        }

        void TimerUpdateEvent(TimerState obj)
        {
            m_Spinner.RemoveFromClassList($"{k_Prefix}{m_Index}"); 
            m_Index = (m_Index + 1) % 12;
            m_Spinner.AddToClassList($"{k_Prefix}{m_Index}");
        }
    }
}
