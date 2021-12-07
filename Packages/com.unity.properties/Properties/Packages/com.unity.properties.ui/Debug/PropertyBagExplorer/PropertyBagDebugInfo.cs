using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties.Editor;
using Unity.Properties.Reflection.Internal;
using Unity.Properties.UI;
using Unity.Properties.UI.Internal;
using UnityEditor;
using UnityEngine;

namespace Unity.Properties.Debug
{
    class PropertyBagDebugInfo
    {
        readonly IPropertyBag m_PropertyBag;
        
        [CreateProperty, TypeHeader]
        public Type Type { get; }

        [CreateProperty, UsedImplicitly, HideInInspector] public string Namespace { get; }
        [CreateProperty, UsedImplicitly, HideInInspector] public string Assembly { get; }
        [CreateProperty, UsedImplicitly, HideInInspector] public string Name { get; }
        [CreateProperty, UsedImplicitly, HideInInspector] public string FullName { get; }
        [CreateProperty, UsedImplicitly, HideInInspector] public PropertyBagType PropertyBagType { get; }
        [CreateProperty, UsedImplicitly, HideInInspector] public TypeTraits TypeTraits { get; }
        [CreateProperty, UsedImplicitly, HideInInspector] public ExtensionType Extensions { get; }
        [CreateProperty, UsedImplicitly, HideInInspector] public bool CanBeConstructed { get; }
        [CreateProperty, UsedImplicitly, HideInInspector] public int PropertyCount => Properties.Count;
        [CreateProperty, UsedImplicitly, HideInInspector] public List<string> PropertyNames { get; }
        [CreateProperty, UsedImplicitly, HideInInspector] public List<string> PropertyTypes { get; }

        [CreateProperty]
        [InlineList]
        public List<IAttributeDescriptor> TypeInfo { get; }

        [CreateProperty]
        [InlineList(MessageWhenEmpty = "No properties")]
        public List<IAttributeDescriptor> Properties { get; }

        [CreateProperty]
        [InlineList(MessageWhenEmpty = "No serialization adapters or migration")]
        public List<IAttributeDescriptor> Serialization { get; }

        [CreateProperty]
        [InlineList(MessageWhenEmpty = "No inspectors")]
        public List<IAttributeDescriptor> UI { get; }

        public PropertyBagDebugInfo(Type type, IPropertyBag propertyBag)
        {
            Type = type;
            Namespace = type.Namespace;
            Assembly = type.Assembly.GetName().Name;
            Name = TypeUtility.GetTypeDisplayName(type);
            FullName = $"{Namespace}.{Name}";
            m_PropertyBag = propertyBag;

            var bagType = m_PropertyBag.GetType();
            if (null != bagType.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>())
                PropertyBagType = PropertyBagType.CodeGen;
            else if (null != bagType.GetCustomAttribute<ReflectedPropertyBagAttribute>())
                PropertyBagType = PropertyBagType.Reflection;
            else
                PropertyBagType = PropertyBagType.Manual;

            TypeTraits = type.IsValueType
                ? TypeTraits.Struct
                : TypeTraits.Class;
            if (UnsafeUtility.IsUnmanaged(type))
                TypeTraits |= TypeTraits.Unmanaged;
            if (UnsafeUtility.IsBlittable(type))
                TypeTraits |= TypeTraits.Blittable;

            if (type.IsGenericType)
                TypeTraits |= TypeTraits.Generic;

            TypeInfo = CacheTypeInfo();
            Properties = CacheProperties();
            PropertyNames = new List<string>();
            PropertyTypes = new List<string>();
            foreach (var property in Properties)
            {
                if (!(property is PropertyTypeDescriptor typed)) 
                    continue;
                PropertyNames.Add(typed.Descriptor.Name);
                PropertyTypes.Add(TypeUtility.GetTypeDisplayName(typed.Value));
            }
            Serialization = CacheSerializationInfo();
            UI = CacheInspectorInfo();
            CanBeConstructed = TypeConstruction.CanBeConstructed(type);

            if (Properties.Count > 0)
                Extensions |= ExtensionType.Properties;
            
            if (Serialization.Count > 0)
                Extensions |= ExtensionType.Serialization;

            if (UI.Count > 0)
                Extensions |= ExtensionType.UI;
        }

        List<IAttributeDescriptor> CacheTypeInfo()
        {
            var info = new List<IAttributeDescriptor>();
            info.Add(AttributeDescriptor.Make("Namespace", Namespace));
            info.Add(AttributeDescriptor.Make("Assembly", Assembly));
            info.Add(AttributeDescriptor.Make("Property Bag Type", PropertyBagType.ToString()));
            info.Add(AttributeDescriptor.Make("Traits", string.Join(" | ", Editor.EnumUtility.EnumerateFlags(TypeTraits))));
            info.Add(AttributeDescriptor.Make("Constructable", CanBeConstructed.ToString()));
            return info;
        }

        List<IAttributeDescriptor> CacheProperties()
        {
            var info = new List<IAttributeDescriptor>();

            // ReSharper disable once PossibleNullReferenceException
            var typedMethod = typeof(PropertyBagDebugInfo)
                .GetMethod(nameof(VisitProperties), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(Type);
            info.AddRange(
                (IEnumerable<PropertyTypeDescriptor>) typedMethod.Invoke(null,
                    new object[] {m_PropertyBag}));
            return info;
        }

        static IEnumerable<PropertyTypeDescriptor> VisitProperties<TContainer>(
            IPropertyBag<TContainer> properties)
        {
            foreach (var property in properties.GetProperties())
            {
                yield return new PropertyTypeDescriptor(property, property.DeclaredValueType());
            }
        }

        List<IAttributeDescriptor> CacheSerializationInfo()
        {
            var adapters = new List<IAttributeDescriptor>();
            var pooled = ListPool<AttributeDescriptor<Type, string>>.Get();
            try
            {
                // Adapters
                pooled.AddRange(GetTypeDescriptor(
                    typeof(Serialization.Json.Adapters.IJsonAdapter<>).MakeGenericType(Type), "Json | Adapter"));
                pooled.AddRange(GetTypeDescriptor(
                    typeof(Serialization.Json.Adapters.Contravariant.IJsonAdapter<>).MakeGenericType(Type),
                    "Json | Contravariant Adapter"));
                pooled.AddRange(GetTypeDescriptor(
                    typeof(Serialization.Binary.Adapters.IBinaryAdapter<>).MakeGenericType(Type), "Binary | Adapter"));
                pooled.AddRange(GetTypeDescriptor(
                    typeof(Serialization.Binary.Adapters.Contravariant.IBinaryAdapter<>).MakeGenericType(Type),
                    "Binary | Contravariant Adapter"));
                pooled.Sort((lhs, rhs) => string.Compare(TypeUtility.GetTypeDisplayName(lhs.Descriptor),
                    TypeUtility.GetTypeDisplayName(rhs.Descriptor), StringComparison.OrdinalIgnoreCase));
                adapters.AddRange(pooled);
                pooled.Clear();
                
                // Migration                
                pooled.AddRange(GetTypeDescriptor(
                    typeof(Serialization.Json.Adapters.IJsonMigration<>).MakeGenericType(Type), "Json | Migration"));
                pooled.AddRange(GetTypeDescriptor(
                    typeof(Serialization.Json.Adapters.Contravariant.IJsonMigration<>).MakeGenericType(Type),
                    "Json | Contravariant Migration"));
                pooled.Sort((lhs, rhs) => string.Compare(TypeUtility.GetTypeDisplayName(lhs.Descriptor),
                    TypeUtility.GetTypeDisplayName(rhs.Descriptor), StringComparison.OrdinalIgnoreCase));
                adapters.AddRange(pooled);
            }
            finally
            {
                ListPool<AttributeDescriptor<Type, string>>.Release(pooled);
            }

            return adapters;
        }
        

        static IEnumerable<AttributeDescriptor<Type, string>> GetTypeDescriptor(Type targetType, string value)
        {
            foreach (var t in TypeCache.GetTypesDerivedFrom(targetType))
            {
                if (t.IsAbstract || t.IsInterface) continue;
                yield return AttributeDescriptor.Make(t, value);
            }
        }

        List<IAttributeDescriptor> CacheInspectorInfo()
        {
            var inspectors = new List<IAttributeDescriptor>();
            var pooled = ListPool<AttributeDescriptor<Type, string>>.Get();
            try
            {
                pooled.AddRange(GetInspectorDescriptor(typeof(IInspector<>).MakeGenericType(Type)));
                pooled.Sort((lhs, rhs) => string.Compare(TypeUtility.GetTypeDisplayName(lhs.Descriptor),
                TypeUtility.GetTypeDisplayName(rhs.Descriptor), StringComparison.OrdinalIgnoreCase));
                inspectors.AddRange(pooled);
            }
            finally
            {
                ListPool<AttributeDescriptor<Type, string>>.Release(pooled);
            }
            return inspectors;
        }

        static IEnumerable<AttributeDescriptor<Type, string>> GetInspectorDescriptor(Type targetType)
        {
            foreach (var t in TypeCache.GetTypesDerivedFrom(targetType))
            {
                if (t.IsAbstract || t.IsInterface) continue;
                var value = "";
                if (typeof(IRootInspector).IsAssignableFrom(t))
                    value = "Inspector";
                else if (typeof(IPropertyDrawer).IsAssignableFrom(t))
                    value = "Property Inspector";
                else
                    value = "Attribute Inspector";

                yield return AttributeDescriptor.Make(t, value);
            }
        }
    }
}
