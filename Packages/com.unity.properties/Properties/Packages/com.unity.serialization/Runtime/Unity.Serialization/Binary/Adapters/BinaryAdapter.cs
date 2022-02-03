#if !NET_DOTS
namespace Unity.Serialization.Binary.Adapters
{
    /// <summary>
    /// Base interface for binary adapters.
    /// </summary>
    public interface IBinaryAdapter
    {
        
    }
    
    /// <summary>
    /// Implement this interface to override serialization and deserialization behaviour for a given type.
    /// </summary>
    /// <typeparam name="TValue">The type to override serialization for.</typeparam>
    public interface IBinaryAdapter<TValue> : IBinaryAdapter
    {
        /// <summary>
        /// Invoked during serialization to handle writing out the specified <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The value to write.</param>
        void Serialize(BinarySerializationContext<TValue> context, TValue value);
        
        /// <summary>
        /// Invoked during deserialization to handle reading the specified <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>The deserialized value.</returns>
        TValue Deserialize(BinaryDeserializationContext<TValue> context);
    }

    namespace Contravariant
    {
        /// <summary>
        /// Implement this interface to override serialization and deserialization behaviour for a given type.
        /// </summary>
        /// <typeparam name="TValue">The type to override serialization for.</typeparam>
        public interface IBinaryAdapter<in TValue> : IBinaryAdapter
        {
            /// <summary>
            /// Invoked during serialization to handle writing out the specified <typeparamref name="TValue"/>.
            /// </summary>
            /// <param name="context">The serialization context.</param>
            /// <param name="value">The value to write.</param>
            void Serialize(IBinarySerializationContext context, TValue value);
            
            /// <summary>
            /// Invoked during deserialization to handle reading the specified <typeparamref name="TValue"/>.
            /// </summary>
            /// <param name="context">The deserialization context.</param>
            /// <returns>The deserialized value.</returns>
            object Deserialize(IBinaryDeserializationContext context);
        }
    }
}
#endif