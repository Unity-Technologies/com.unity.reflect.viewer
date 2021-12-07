#if !NET_DOTS
using Unity.Serialization.Json.Adapters;
using Unity.Serialization.Json.Unsafe;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// The <see cref="IJsonSerializationContext"/> provides an untyped context for contravariant serialization adapters.
    /// </summary>
    public interface IJsonSerializationContext
    {
        /// <summary>
        /// Gets the underlying <see cref="JsonWriter"/> which can be used to output formatted data.
        /// </summary>
        JsonWriter Writer { get; }

        /// <summary>
        /// Continues serialization for the current value. This will run the next adapter in the sequence, or the default behaviour.
        /// </summary>
        void ContinueVisitation();

        /// <summary>
        /// Continues serialization for the current type without running any more adapters. This will perform the default behaviour.
        /// </summary>
        void ContinueVisitationWithoutAdapters();
        
        /// <summary>
        /// Writes the given <paramref name="value"/> to the output using all adapters.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <typeparam name="T">The value type to write.</typeparam>
        void SerializeValue<T>(T value);
    }

    /// <summary>
    /// The <see cref="JsonSerializationContext{T}"/> is available from adapters. It provides access to the current adapter enumerator and allows for control of serialization for a given type. 
    /// </summary>
    public readonly struct JsonSerializationContext<TValue> : IJsonSerializationContext
    {
        readonly JsonPropertyWriter m_Visitor;
        readonly JsonAdapterCollection.Enumerator m_Adapters;
        readonly TValue m_Value;
        readonly bool m_IsRoot;

        /// <summary>
        /// Gets the underlying <see cref="JsonWriter"/> which can be used to output formatted data.
        /// </summary>
        public JsonWriter Writer => m_Visitor.Writer;

        internal JsonSerializationContext(JsonPropertyWriter visitor, JsonAdapterCollection.Enumerator adapters, TValue value, bool isRoot)
        {
            m_Visitor = visitor;
            m_Adapters = adapters;
            m_Value = value;
            m_IsRoot = isRoot;
        }

        /// <summary>
        /// Continues visitation for the current type. This will run the next adapter in the sequence, or the default behaviour.
        /// </summary>
        public void ContinueVisitation()
            => m_Visitor.WriteValueWithAdapters(m_Value, m_Adapters, m_IsRoot);

        /// <summary>
        /// Continues visitation for the current type without running any more adapters. This will perform the default behaviour.
        /// </summary>
        public void ContinueVisitationWithoutAdapters()
            => m_Visitor.WriteValueWithoutAdapters(m_Value, m_IsRoot);
        
        /// <summary>
        /// Writes the given <paramref name="value"/> to the output. This will run all adapters.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <typeparam name="T">The value type to write.</typeparam>
        public void SerializeValue<T>(T value)
            => m_Visitor.WriteValue(ref value);
    }

    /// <summary>
    /// The <see cref="IJsonDeserializationContext"/> provides an untyped context for contravariant serialization adapters.
    /// </summary>
    public interface IJsonDeserializationContext
    {
        /// <summary>
        /// Gets the serialized view for value being deserialized. 
        /// </summary>
        SerializedValueView SerializedValue { get; }

        /// <summary>
        /// Continues de-serialization for the current value. This will run the next adapter in the sequence, or the default behaviour.
        /// </summary>
        /// <returns>The deserialized value.</returns>
        object ContinueVisitation();
        
        /// <summary>
        /// Continues de-serialization for the current type without running any more adapters. This will perform the default behaviour.
        /// </summary>
        /// <returns>The deserialized value.</returns>
        object ContinueVisitationWithoutAdapters();

        /// <summary>
        /// Reads the given value type from the stream.
        /// </summary>
        /// <param name="view">The view containing the serialized data.</param>
        /// <typeparam name="T">The value type to deserialize.</typeparam>
        /// <returns>The deserialized value.</returns>
        T DeserializeValue<T>(SerializedValueView view);
    }

    public readonly struct JsonDeserializationContext<TValue> : IJsonDeserializationContext
    {
        readonly JsonPropertyReader m_Visitor;
        readonly JsonAdapterCollection.Enumerator m_Adapters;
        readonly UnsafeValueView m_View;
        readonly bool m_IsRoot;
        
        /// <summary>
        /// The in-memory representation of the value being deserialized.
        /// </summary>
        public SerializedValueView SerializedValue => m_View.AsSafe();

        internal JsonDeserializationContext(JsonPropertyReader visitor, JsonAdapterCollection.Enumerator adapters, UnsafeValueView view, bool isRoot)
        {
            m_Visitor = visitor;
            m_Adapters = adapters;
            m_View = view;
            m_IsRoot = isRoot;
        }

        /// <summary>
        /// Continues visitation for the current type. This will run the next adapter in the sequence, or the default behaviour and return the deserialized value.
        /// </summary>
        public TValue ContinueVisitation()
        {
            var value = default(TValue);
            m_Visitor.ReadValueWithAdapters(ref value, m_View, m_Adapters, m_IsRoot);
            return value;
        }
        
        /// <summary>
        /// Continues visitation for the current type using the specified <see cref="SerializedValueView"/>. This will run the next adapter in the sequence, or the default behaviour and return the deserialized value.
        /// </summary>
        public TValue ContinueVisitation(SerializedValueView view)
        {
            var value = default(TValue);
            m_Visitor.ReadValueWithAdapters(ref value, view.AsUnsafe(), m_Adapters, m_IsRoot);
            return value;
        }

        /// <summary>
        /// Continues visitation for the current type. This will run the next adapter in the sequence, or the default behaviour and return the deserialized value.
        /// </summary>
        public void ContinueVisitation(ref TValue value)
        {
            m_Visitor.ReadValueWithAdapters(ref value, m_View, m_Adapters, m_IsRoot);
        }
        
        /// <summary>
        /// Continues visitation for the current type using the specified <see cref="SerializedValueView"/>. This will run the next adapter in the sequence, or the default behaviour and return the deserialized value.
        /// </summary>
        public void ContinueVisitation(ref TValue value, SerializedValueView view)
        {
            m_Visitor.ReadValueWithAdapters(ref value, view.AsUnsafe(), m_Adapters, m_IsRoot);
        }
        
        /// <summary>
        /// Continues visitation for the current type. This will run the next adapter in the sequence, or the default behaviour and return the deserialized value.
        /// </summary>
        public TValue ContinueVisitationWithoutAdapters()
        {
            var value = default(TValue);
            m_Visitor.ReadValueWithoutAdapters(ref value, m_View, m_IsRoot);
            return value;
        }
        
        /// <summary>
        /// Continues visitation for the current type using the specified <see cref="SerializedValueView"/>. This will run the next adapter in the sequence, or the default behaviour and return the deserialized value.
        /// </summary>
        public TValue ContinueVisitationWithoutAdapters(SerializedValueView view)
        {
            var value = default(TValue);
            m_Visitor.ReadValueWithoutAdapters(ref value, view.AsUnsafe(), m_IsRoot);
            return value;
        }

        /// <summary>
        /// Continues visitation for the current type. This will invoke the default behaviour and return the deserialized value.
        /// </summary>
        public void ContinueVisitationWithoutAdapters(ref TValue value)
        {
            m_Visitor.ReadValueWithoutAdapters(ref value, m_View, m_IsRoot);
        }
        
        /// <summary>
        /// Continues visitation for the current type using the specified <see cref="SerializedValueView"/>. This will invoke the default behaviour and return the deserialized value..
        /// </summary>
        public void ContinueVisitationWithoutAdapters(ref TValue value, SerializedValueView view)
        {
            m_Visitor.ReadValueWithoutAdapters(ref value, view.AsUnsafe(), m_IsRoot);
        }

        object IJsonDeserializationContext.ContinueVisitation() => ContinueVisitation();
        object IJsonDeserializationContext.ContinueVisitationWithoutAdapters() => ContinueVisitationWithoutAdapters();
        
        /// <summary>
        /// Reads the given <see cref="SerializedValue"/> as <typeparamref name="T"/> and returns it. This will run all adapters.
        /// </summary>
        public T DeserializeValue<T>(SerializedValueView view)
        {
            var value = default(T);
            m_Visitor.ReadValue(ref value, view.AsUnsafe() );
            return value;
        }
    }
}
#endif