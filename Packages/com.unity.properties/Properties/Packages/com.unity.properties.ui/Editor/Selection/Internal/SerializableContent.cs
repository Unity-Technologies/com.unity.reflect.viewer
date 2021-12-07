using System;
using Unity.Properties.Editor;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    [Serializable]
    partial class SerializableContent
    {
        [SerializeField, HideInInspector] string m_Data;

        [NonSerialized] public ContentProvider Provider;
        [NonSerialized] public VisualElement Root;
        [NonSerialized] public readonly DynamicInspectionContext InspectionContext = new DynamicInspectionContext();

        [NonSerialized] ContentStatus m_PreviousState;
        [NonSerialized] InspectorElement m_ContentRoot;
        [NonSerialized] InspectorElement m_ContentNotReadyRoot;
        [NonSerialized] bool m_RequestQuit;

        public bool IsValid
            => !m_RequestQuit && null != Provider && Provider.IsValid();

        public bool IsReady
            => null != Provider && Provider.IsReady();
        
        public string Name => Provider?.Name;

        public void Update()
        {
            if (!IsValid)
                return;

            if (null != m_ContentRoot && InspectionContext.ApplyInspectorStyling)
                StylingUtility.AlignInspectorLabelWidth(m_ContentRoot);
            
            var state = Provider.MoveNext();
            if (m_PreviousState != state)
            {
                m_ContentRoot?.ClearTarget();
                switch (state)
                {
                    case ContentStatus.ContentUnavailable:
                        return;
                    case ContentStatus.ContentNotReady:
                        SetNotReadyContent();
                        break;
                    case ContentStatus.ContentReady:
                        SetTarget();
                        break;
                    case ContentStatus.ReloadContent:
                        SetNotReadyContent();
                        Load();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            m_PreviousState = state;
        }

        public void Initialize()
        {
            m_ContentRoot = new InspectorElement();
            m_ContentRoot.OnChanged += (element, path) =>
            { 
                Provider.OnContentChanged(new ContentProvider.ChangeContext(element));
            };
            m_ContentRoot.AddContext(InspectionContext);
            if (InspectionContext.ApplyInspectorStyling)
                m_ContentRoot.RegisterCallback<GeometryChangedEvent, VisualElement>((evt, element) => StylingUtility.AlignInspectorLabelWidth(element), m_ContentRoot);
            m_ContentNotReadyRoot = new InspectorElement();
            Root.contentContainer.Add(m_ContentRoot);
            Root.contentContainer.Add(m_ContentNotReadyRoot);
            m_ContentRoot.style.flexGrow = 1;
        }
        
        public void Load()
        {
            if (string.IsNullOrEmpty(m_Data))
                return;

            if (!JsonSerialization.TryFromJsonOverride(m_Data, ref Provider, out var events))
            {
                foreach (var exception in events.Exceptions)
                {
                    Debug.LogException((Exception) exception.Payload);
                }

                foreach (var warnings in events.Warnings)
                {
                    Debug.LogWarning(warnings.Payload);
                }

                foreach (var logs in events.Logs)
                {
                    Debug.Log(logs.Payload);
                }
            }

            var value = Provider?.GetContent();
            if (null == value)
            {
                SetNotReadyContent();
            }
        }

        public void Save()
        {
            m_Data = null != Provider
                ? JsonSerialization.ToJson(Provider)
                : string.Empty;
        }

        void SetTarget()
        {
            try
            {
                var value = Provider.GetContent();
                if (null == value)
                {
                    Debug.LogError($"{TypeUtility.GetTypeDisplayName(Provider.GetType())}: Releasing content named '{Provider.Name}' because it returned null value.");
                    m_RequestQuit = true;
                    return; 
                }

                // Removing from the hierarchy here because Unity will try to bind the elements to a serializedObject and
                // we want to use our own bindings. This will be fixed in UIToolkit directly.
                m_ContentRoot.RemoveFromHierarchy();
                m_ContentNotReadyRoot.RemoveFromHierarchy();

                var visitor = new SetTargetVisitor {Content = this, Inspector = m_ContentRoot};
                PropertyContainer.Accept(visitor, ref value);
                Root.contentContainer.Add(m_ContentRoot);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{TypeUtility.GetTypeDisplayName(Provider.GetType())}: Releasing content named '{Provider.Name}' because it threw an exception.");
                m_RequestQuit = true;
                Debug.LogException(ex);
            }
        }

        void SetNotReadyContent()
        {
            // Removing from the hierarchy here for consistency with the SetTarget() above.
            m_ContentRoot.RemoveFromHierarchy();
            m_ContentNotReadyRoot.RemoveFromHierarchy();
            m_ContentNotReadyRoot.SetTarget(new ContentNotReady(Provider));
            Root.contentContainer.Add(m_ContentNotReadyRoot);
        }
    }
}
