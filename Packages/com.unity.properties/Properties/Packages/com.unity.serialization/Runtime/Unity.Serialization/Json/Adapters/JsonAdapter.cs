#if !NET_DOTS
namespace Unity.Serialization.Json.Adapters
{
    /// <summary>
    /// Base interface for json adapters.
    /// </summary>
    public interface IJsonAdapter
    {
        
    }

    /// <summary>
    /// Implement this interface to override serialization and deserialization behaviour for a given type.
    /// </summary>
    /// <typeparam name="TValue">The type to override serialization for.</typeparam>
    public interface IJsonAdapter<TValue> : IJsonAdapter
    {
        /// <summary>
        /// Invoked during serialization to handle writing out the specified <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="context">The current serialization context.</param>
        /// <param name="value">The value to write.</param>
        void Serialize(JsonSerializationContext<TValue> context, TValue value);
        
        /// <summary>
        /// Invoked during deserialization to handle reading the specified <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="context">The current de-serialization context.</param>
        /// <returns>The deserialized value.</returns>
        TValue Deserialize(JsonDeserializationContext<TValue> context);
    }

    namespace Contravariant
    {
        /// <summary>
        /// Implement this interface to override serialization and deserialization behaviour for a given type.
        /// </summary>
        /// <typeparam name="TValue">The type to override serialization for.</typeparam>
        public interface IJsonAdapter<in TValue> : IJsonAdapter
        {
            /// <summary>
            /// Invoked during serialization to handle writing out the specified <typeparamref name="TValue"/>.
            /// </summary>
            /// <param name="context">The current serialization context.</param>
            /// <param name="value">The value to write.</param>
            void Serialize(IJsonSerializationContext context, TValue value);
            
            /// <summary>
            /// Invoked during deserialization to handle reading the specified <typeparamref name="TValue"/>.
            /// </summary>
            /// <param name="context">The current de-serialization context.</param>
            /// <returns>The deserialized value.</returns>
            object Deserialize(IJsonDeserializationContext context);
        }
    }
}
#endif