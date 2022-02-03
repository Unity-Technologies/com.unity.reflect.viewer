#if !NET_DOTS
using System;
using System.Collections.Generic;
using Unity.Properties;
using Unity.Properties.Internal;
using Unity.Serialization.Json.Adapters;

namespace Unity.Serialization.Json
{
    class DictionaryElementProperty<TDictionary, TKey, TValue> : Property<TDictionary, TValue>
        where TDictionary : IDictionary<TKey, TValue>
    {
        public override string Name => Key.ToString();
        public override bool IsReadOnly => false;

        public override TValue GetValue(ref TDictionary container)
        {
            return container[Key];
        }

        public override void SetValue(ref TDictionary container, TValue value)
        {
            container[Key] = value;
        }

        public TKey Key { get; internal set; }
    }
    
    /// <summary>
    /// A visitor that traverses a property container and outputs a JSON string.
    /// </summary>
    class JsonPropertyWriter : JsonPropertyVisitor,
        IPropertyBagVisitor,
        ICollectionPropertyBagVisitor,
        IListPropertyBagVisitor,
        IDictionaryPropertyBagVisitor,
        IPropertyVisitor
    {
        struct SerializedId
        {
            public int Id;
        }
        
        struct SerializedType
        {
            public Type Type;
        }
        
        struct SerializedVersion
        {
            public int Version;
        }
        
        class SerializedIdProperty : Property<SerializedId, int>
        {
            public override string Name => k_SerializedId;
            public override bool IsReadOnly => true;
            public override int GetValue(ref SerializedId container) => container.Id;
            public override void SetValue(ref SerializedId container, int value) => throw new InvalidOperationException("Property is ReadOnly.");
        }

        class SerializedTypeProperty : Property<SerializedType, string>
        {
            public override string Name => k_SerializedTypeKey;
            public override bool IsReadOnly => true;
            public override string GetValue(ref SerializedType container) => $"{container.Type}, {container.Type.Assembly.GetName().Name}";
            public override void SetValue(ref SerializedType container, string value) => throw new InvalidOperationException("Property is ReadOnly.");
        }

        class SerializedVersionProperty : Property<SerializedVersion, int>
        {
            public override string Name => k_SerializedVersionKey;
            public override bool IsReadOnly => true;
            public override int GetValue(ref SerializedVersion container) => container.Version;
            public override void SetValue(ref SerializedVersion container, int value) => throw new InvalidOperationException("Property is ReadOnly.");
        }

        struct SerializedContainerMetadata
        {
            public bool IsSerializedReference;
            
            public bool HasSerializedId;
            public bool HasSerializedType;
            public bool HasSerializedVersion;
            
            /// <summary>
            /// Returns true if there is any metadata to write out.
            /// </summary>
            public bool Exists => HasSerializedId || HasSerializedType || HasSerializedVersion;
            
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public int SerializedId;
            
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public int SerializedVersion;
        }

        /// <summary>
        /// Shared property used to write the serialized type metadata.
        /// </summary>
        static readonly SerializedIdProperty s_SerializedIdProperty = new SerializedIdProperty();

        /// <summary>
        /// Shared property used to write the serialized type metadata.
        /// </summary>
        static readonly SerializedTypeProperty s_SerializedTypeProperty = new SerializedTypeProperty();
        
        /// <summary>
        /// Shared property used to write the serialized version metadata.
        /// </summary>
        static readonly SerializedVersionProperty s_SerializedVersionProperty = new SerializedVersionProperty();

        JsonWriter m_Writer;
        Type m_SerializedType;
        bool m_DisableRootAdapters;
        JsonAdapterCollection m_Adapters;
        JsonMigrationCollection m_Migrations;
        SerializedReferences m_SerializedReferences;

        public JsonWriter Writer => m_Writer;
        
        public void SetWriter(JsonWriter writer)
            => m_Writer = writer;

        public void SetSerializedType(Type type) 
            => m_SerializedType = type;
        
        public void SetDisableRootAdapters(bool disableRootAdapters) 
            => m_DisableRootAdapters = disableRootAdapters;
        
        public void SetGlobalAdapters(List<IJsonAdapter> adapters) 
            => m_Adapters.Global = adapters;
        
        public void SetUserDefinedAdapters(List<IJsonAdapter> adapters) 
            => m_Adapters.UserDefined = adapters;
        
        public void SetGlobalMigrations(List<IJsonMigration> migrations) 
            => m_Migrations.Global = migrations;
        
        public void SetUserDefinedMigration(List<IJsonMigration> migrations) 
            => m_Migrations.UserDefined = migrations;

        public void SetSerializedReferences(SerializedReferences serializedReferences)
            => m_SerializedReferences = serializedReferences;
        
        public JsonPropertyWriter()
        {
            m_Adapters.InternalAdapter = new JsonAdapter();
        }

        SerializedContainerMetadata GetSerializedContainerMetadata<TContainer>(ref TContainer container)
        {
            var type = typeof(TContainer);
            
            // Never write metadata for special json types.
            if (type == typeof(JsonObject) || type == typeof(JsonArray)) return default;
            
            var metadata = default(SerializedContainerMetadata);

            if (!(RuntimeTypeInfoCache<TContainer>.IsValueType || container.GetType().IsValueType))
            {
                var reference = container as object;
                
                if (m_SerializedReferences != null && m_SerializedReferences.TryGetSerializedReference(reference, out var id))
                {
                    if (!m_SerializedReferences.SetSerialized(reference))
                    {
                        return new SerializedContainerMetadata
                        {
                            IsSerializedReference = true,
                            SerializedId = id
                        };
                    }
                
                    metadata.HasSerializedId = true;
                    metadata.SerializedId = id;
                }
            }
            else
            {
                metadata.SerializedId = -1;
            }
            
            var isRootAndTypeWasGiven = Property is IPropertyWrapper && null != m_SerializedType;
            var declaredValueType = Property.DeclaredValueType();
            
            // We need to write out the serialize type name in any of the following cases are FALSE:
            // 1) The type is the same as the declared property type. This means deserialization can infer the property type.
            // 2) The root type was explicitly provided. This means the user is expected to provide a type upon deserialization.
            // 3) This is a nullable type. This means deserialization can infer the underlying property type.
            metadata.HasSerializedType = type != declaredValueType && !isRootAndTypeWasGiven && Nullable.GetUnderlyingType(declaredValueType) != type;
            metadata.HasSerializedVersion = m_Migrations.TryGetSerializedVersion<TContainer>(out var serializedVersion);
            metadata.SerializedVersion = serializedVersion;

            return metadata;
        }

        void WriteSerializedContainerMetadata<TContainer>(ref TContainer container, SerializedContainerMetadata metadata, ref int count)
        {
            if (metadata.HasSerializedId)
            {
                using (CreatePropertyScope(s_SerializedIdProperty))
                {
                    var serializedId = new SerializedId {Id = metadata.SerializedId};
                    ((IPropertyAccept<SerializedId>) s_SerializedIdProperty).Accept(this, ref serializedId);
                }
            }
            
            if (metadata.HasSerializedType)
            {
                using (CreatePropertyScope(s_SerializedTypeProperty))
                {
                    var typeInfo = new SerializedType {Type = container.GetType()};
                    ((IPropertyAccept<SerializedType>) s_SerializedTypeProperty).Accept(this, ref typeInfo);
                }
            }

            if (metadata.HasSerializedVersion)
            {
                using (CreatePropertyScope(s_SerializedVersionProperty))
                {
                    var serializedVersion = new SerializedVersion {Version = metadata.SerializedVersion};
                    ((IPropertyAccept<SerializedVersion>) s_SerializedVersionProperty).Accept(this, ref serializedVersion);
                }
            }
        }
        
        void WriteSerializedReference(int id)
        {
            using (m_Writer.WriteObjectScope())
            {
                m_Writer.WriteKey(k_SerializedReferenceKey);
                m_Writer.WriteValue(id);
            }
        }
        
        void IPropertyBagVisitor.Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
        {
            var isRootContainer = properties is IPropertyWrapper;
            
            var count = 0;

            if (!isRootContainer)
            {
                var metadata = GetSerializedContainerMetadata(ref container);

                if (metadata.IsSerializedReference)
                {
                    WriteSerializedReference(metadata.SerializedId);
                    return;
                }

                m_Writer.WriteBeginObject();
                WriteSerializedContainerMetadata(ref container, metadata, ref count);
            }

            foreach (var property in properties.GetProperties(ref container))
            {
                if (PropertyChecks.IsPropertyExcludedFromSerialization(property))
                    continue;
                
                using (CreatePropertyScope(property)) 
                    property.Accept(this, ref container);
            }

            if (!isRootContainer)
            {
                m_Writer.WriteEndObject();
            }
        }
        
        void ICollectionPropertyBagVisitor.Visit<TCollection, TElement>(ICollectionPropertyBag<TCollection, TElement> properties, ref TCollection container)
        {
            var metadata = GetSerializedContainerMetadata(ref container);

            if (metadata.IsSerializedReference)
            {
                WriteSerializedReference(metadata.SerializedId);
                return;
            }
            
            var metadataCount = 0;

            if (metadata.Exists)
            {
                m_Writer.WriteBeginObject();
                WriteSerializedContainerMetadata(ref container, metadata, ref metadataCount);
                m_Writer.WriteKey(k_SerializedElementsKey);
            }

            using (m_Writer.WriteArrayScope())
            {
                foreach (var property in properties.GetProperties(ref container))
                {
                    using (CreatePropertyScope(property))
                        property.Accept(this, ref container);
                }
            }

            if (metadata.Exists)
            {
                m_Writer.WriteEndObject();
            }
        }

        void IListPropertyBagVisitor.Visit<TList, TElement>(IListPropertyBag<TList, TElement> properties, ref TList container)
        {
            var metadata = GetSerializedContainerMetadata(ref container);

            if (metadata.IsSerializedReference)
            {
                WriteSerializedReference(metadata.SerializedId);
                return;
            }
            
            var metadataCount = 0;
            
            if (metadata.Exists)
            {
                m_Writer.WriteBeginObject();
                WriteSerializedContainerMetadata(ref container, metadata, ref metadataCount);
                m_Writer.WriteKey(k_SerializedElementsKey);
            }
            
            using (m_Writer.WriteArrayScope())
            {
                foreach (var property in properties.GetProperties(ref container))
                {
                    using (CreatePropertyScope(property))
                        property.Accept(this, ref container);
                }
            }
            
            if (metadata.Exists)
            {
                m_Writer.WriteEndObject();
            }
        }

        void IDictionaryPropertyBagVisitor.Visit<TDictionary, TKey, TValue>(IDictionaryPropertyBag<TDictionary, TKey, TValue> properties, ref TDictionary container)
        {
            if (typeof(TKey) != typeof(string))
            {
                ((ICollectionPropertyBagVisitor) this).Visit(properties, ref container);
            }
            else
            {
                var metadata = GetSerializedContainerMetadata(ref container);

                if (metadata.IsSerializedReference)
                {
                    WriteSerializedReference(metadata.SerializedId);
                    return;
                }

                var metadataCount = 0;
            
                if (metadata.Exists)
                {
                    m_Writer.WriteBeginObject();
                    WriteSerializedContainerMetadata(ref container, metadata, ref metadataCount);
                    m_Writer.WriteKey(k_SerializedElementsKey);
                }

                using (m_Writer.WriteObjectScope())
                {
                    // @FIXME allocations
                    var property = new DictionaryElementProperty<TDictionary, TKey, TValue>();

                    foreach (var kvp in container)
                    {
                        property.Key = kvp.Key;
                        ((IPropertyAccept<TDictionary>) property).Accept(this, ref container);
                    }
                }

                if (metadata.Exists)
                {
                    m_Writer.WriteEndObject();
                }
            }
        }

        void IPropertyVisitor.Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
        {
            var isRootProperty = property is IPropertyWrapper;

            if (!isRootProperty && !(property is ICollectionElementProperty))
            {
                m_Writer.WriteKey(property.Name);
            }

            var value = property.GetValue(ref container);
            WriteValue(ref value, isRootProperty);
        }
        
        internal void WriteValue<TValue>(ref TValue value, bool isRoot = false)
        {
            // Special handling of primitive types.
            if (RuntimeTypeInfoCache<TValue>.IsPrimitiveOrString)
            {
                // This would be nice to optimize and avoid the conversion all together.
                (m_Adapters.InternalAdapter as IJsonAdapter<TValue>).Serialize(new JsonSerializationContext<TValue>(this, default, value, isRoot), value);
                return;
            }
            
            if (!(isRoot && m_DisableRootAdapters))
                WriteValueWithAdapters(value, m_Adapters.GetEnumerator(), isRoot);
            else
                WriteValueWithoutAdapters(value, isRoot);
        }

        internal void WriteValueWithAdapters<TValue>(TValue value, JsonAdapterCollection.Enumerator adapters, bool isRoot)
        {
            while (adapters.MoveNext())
            {
                switch (adapters.Current)
                {
                    case IJsonAdapter<TValue> typed:
                        typed.Serialize(new JsonSerializationContext<TValue>(this, adapters, value, isRoot), value);
                        return;
                    case Adapters.Contravariant.IJsonAdapter<TValue> typedContravariant:
                        // NOTE: Boxing
                        typedContravariant.Serialize((IJsonSerializationContext) new JsonSerializationContext<TValue>(this, adapters, value, isRoot), value);
                        return;
                }
            }
            
            // Do the default thing.
            WriteValueWithoutAdapters(value, isRoot);
        }

        internal void WriteValueWithoutAdapters<TValue>(TValue value, bool isRoot)
        {
#if !UNITY_DOTSPLAYER
            if (!RuntimeTypeInfoCache<TValue>.IsValueType)
            {            
                if (!(isRoot && m_DisableRootAdapters))
                {
                    if (value is UnityEngine.Object)
                    {
                        throw new NotSupportedException("JsonSerialization does not support polymorphic unity object references.");
                    }
                }
            }
#endif
            
#if UNITY_EDITOR
            if (RuntimeTypeInfoCache<TValue>.IsLazyLoadReference)
            {
                var instanceID = PropertyContainer.GetValue<TValue, int>(ref value, "m_InstanceID");
                Writer.WriteValue(UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(instanceID).ToString());
                return;
            }
#endif
            
            if (RuntimeTypeInfoCache<TValue>.IsEnum)
            {
                WritePrimitiveBoxed(m_Writer, value, Enum.GetUnderlyingType(typeof(TValue)));
                return;
            }
            
            if (RuntimeTypeInfoCache<TValue>.CanBeNull && EqualityComparer<TValue>.Default.Equals(value, default))
            {
                m_Writer.WriteNull();
                return;
            }

            if (RuntimeTypeInfoCache<TValue>.IsMultidimensionalArray)
            {
                // No support for multidimensional arrays yet. This can be done using adapters for now.
                m_Writer.WriteNull();
                return;
            }
            
            if (RuntimeTypeInfoCache<TValue>.IsNullable)
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(TValue));

                if (RuntimeTypeInfoCache.IsContainerType(underlyingType))
                {
                    // Unpack Nullable<T> as T
                    var underlyingValue = System.Convert.ChangeType(value, underlyingType);
                    
                    if (!PropertyContainer.TryAccept(this, ref underlyingValue, out var errorCode))
                    {
                        switch (errorCode)
                        {
                            case VisitErrorCode.NullContainer:
                                throw new ArgumentNullException(nameof(value));
                            case VisitErrorCode.InvalidContainerType:
                                throw new InvalidContainerTypeException(value.GetType());
                            case VisitErrorCode.MissingPropertyBag:
                                throw new MissingPropertyBagException(value.GetType());
                            default:
                                throw new Exception($"Unexpected {nameof(VisitErrorCode)}=[{errorCode}]");
                        }
                    }
                }
                else
                {
                    WritePrimitiveBoxed(m_Writer, value, underlyingType);
                }
                
                return;
            }
            
            if (RuntimeTypeInfoCache<TValue>.IsObjectType && !RuntimeTypeInfoCache.IsContainerType(value.GetType()))
            {
                WritePrimitiveBoxed(m_Writer, value, value.GetType());
                return;
            }
            
            if (RuntimeTypeInfoCache<TValue>.IsContainerType)
            {
                if (!PropertyContainer.TryAccept(this, ref value, out var errorCode))
                {
                    switch (errorCode)
                    {
                        case VisitErrorCode.NullContainer:
                            throw new ArgumentNullException(nameof(value));
                        case VisitErrorCode.InvalidContainerType:
                            throw new InvalidContainerTypeException(value.GetType());
                        case VisitErrorCode.MissingPropertyBag:
                            throw new MissingPropertyBagException(value.GetType());
                        default:
                            throw new Exception($"Unexpected {nameof(VisitErrorCode)}=[{errorCode}]");
                    }
                }
                return;
            }
            
            throw new Exception($"Unsupported Type {value.GetType()}.");
        }
        
        internal static void WritePrimitiveBoxed(JsonWriter writer, object value, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                    writer.WriteValue((sbyte) value);
                    return;
                case TypeCode.Int16:
                    writer.WriteValue((short) value);
                    return;
                case TypeCode.Int32:
                    writer.WriteValue((int) value);
                    return;
                case TypeCode.Int64:
                    writer.WriteValue((long) value);
                    return;
                case TypeCode.Byte:
                    writer.WriteValue((byte) value);
                    return;
                case TypeCode.UInt16:
                    writer.WriteValue((ushort) value);
                    return;
                case TypeCode.UInt32:
                    writer.WriteValue((uint) value);
                    return;
                case TypeCode.UInt64:
                    writer.WriteValue((ulong) value);
                    return;
                case TypeCode.Single:
                    writer.WriteValue((float) value);
                    return;
                case TypeCode.Double:
                    writer.WriteValue((double) value);
                    return;
                case TypeCode.Boolean:
                    writer.WriteValueLiteral(((bool) value) ? "true" : "false");
                    return;
                case TypeCode.Char:
                    writer.WriteValue((char) value);
                    return;
                case TypeCode.String:
                    writer.WriteValue(value as string);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
#endif