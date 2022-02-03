using System;
using System.Collections.Concurrent;
using Unity.Properties.Internal;
using UnityEditor;

namespace Unity.Properties.Reflection.Editor
{
    /// <summary>
    /// Helper class to help generate property bags using reflection.
    /// </summary>
    public static class PropertyBagUtility
    {
        static readonly Pool<PropertyBagRequestVisitor> s_PropertyBagRequestVisitorPool = new Pool<PropertyBagRequestVisitor>(() => new PropertyBagRequestVisitor(), v => v.Reset());
        static readonly EditorApplication.CallbackFunction s_ProcessQueueFunction = ProcessRequestQueue;
        static readonly ConcurrentQueue<TypeOption> s_RequestQueue = new ConcurrentQueue<TypeOption>();
        
        class PropertyBagRequestVisitor : IPropertyVisitor, ITypeVisitor
        {
            public PropertyBagPreparationOptions Options { get; set; }
            
            public void Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
            {
                if (RuntimeTypeInfoCache<TValue>.IsContainerType)
                    RequestPropertyBagGeneration<TValue>(Options);
            }

            public void Reset()
            {
                Options = default;
            }

            public void Visit<TContainer>()
            {
                var propertyBag = PropertyBag.GetPropertyBag<TContainer>();
                if (null == propertyBag || !Options.Recursive)
                    return;
            
                var empty = default(TContainer);
                foreach (var property in propertyBag.GetProperties())
                {
                    property.Accept(this, ref empty);
                }
            }
        }

        /// <summary>
        /// Options detailing how the property bag generation should be handled when explicitly requested.
        /// </summary>
        public struct PropertyBagPreparationOptions
        {
            /// <summary>
            /// Indicates if the generation should be time sliced.
            /// </summary>
            public bool Async;
            
            /// <summary>
            /// Indicates if property bags should be generated recursively.
            /// </summary>
            public bool Recursive;
        }

        struct TypeOption
        {
            public Type Type;
            public PropertyBagPreparationOptions Options;
        }
        
        /// <summary>
        /// Helper method to generate a reflected property bag ahead of time. 
        /// </summary>
        /// <param name="options">Options detailing how the generation should be handled.</param>
        /// <typeparam name="TContainer">The type for which we request a  property bag generation.</typeparam>
        public static void RequestPropertyBagGeneration<TContainer>(PropertyBagPreparationOptions options = default)
        {
            RequestPropertyBagGeneration(typeof(TContainer), options);
        }

        /// <summary>
        /// Helper method to generate a reflected property bag ahead of time.
        /// </summary>
        /// <param name="type">The type for which we request a  property bag generation.</param>
        /// <param name="options">Options detailing how the generation should be handled.</param>
        public static void RequestPropertyBagGeneration(Type type, PropertyBagPreparationOptions options = default)
        {
            // If no provider exists, queue anyway to give a chance to the registration of the reflection provider
            if (options.Async || !PropertyBagStore.HasProvider)
            {
                if (s_RequestQueue.IsEmpty)
                    EditorApplication.update += s_ProcessQueueFunction;
                    
                s_RequestQueue.Enqueue(new TypeOption{ Type = type, Options = options});
                return;
            }
            
            ProcessRequest(type, options);
        }

        static void RequestPropertyBagGeneration(PropertyBagRequestVisitor visitor, Type type)
        {
            var propertyBag = PropertyBag.GetPropertyBag(type);
            if (null == propertyBag || !visitor.Options.Recursive)
                return;
            
            propertyBag.Accept(visitor);
        }
        
        static void ProcessRequestQueue()
        {
            // Wait until a provider is registered. 
            if (!PropertyBagStore.HasProvider)
                return;
            
            while (s_RequestQueue.TryDequeue(out var info))
            {
                if (PropertyBagStore.Exists(info.Type)) 
                     continue;
                
                ProcessRequest(info.Type, info.Options);
                break;
            }
            
            if (s_RequestQueue.IsEmpty)
            {
                EditorApplication.update -= s_ProcessQueueFunction; 
            }
        }

        static void ProcessRequest(Type type, PropertyBagPreparationOptions options)
        {
            var visitor = s_PropertyBagRequestVisitorPool.Get();
            try
            {
                visitor.Options = options;
                RequestPropertyBagGeneration(visitor, type);
            }
            finally
            {
                s_PropertyBagRequestVisitorPool.Release(visitor);
            }
        }
    }
}
