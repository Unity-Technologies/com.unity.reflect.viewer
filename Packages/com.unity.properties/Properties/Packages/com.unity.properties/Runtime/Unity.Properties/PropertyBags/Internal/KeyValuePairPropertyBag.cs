using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    /// <summary>
    /// A <see cref="IPropertyBag{T}"/> implementation for a generic key/value pair.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    class KeyValuePairPropertyBag<TKey, TValue> : PropertyBag<KeyValuePair<TKey, TValue>>, IPropertiesNamed<KeyValuePair<TKey, TValue>>
    {
        static readonly DelegateProperty<KeyValuePair<TKey, TValue>, TKey> s_KeyProperty =
            new DelegateProperty<KeyValuePair<TKey, TValue>, TKey>(
                nameof(KeyValuePair<TKey, TValue>.Key),
                (ref KeyValuePair<TKey, TValue> container) => container.Key,
                null);

        static readonly DelegateProperty<KeyValuePair<TKey, TValue>, TValue> s_ValueProperty =
            new DelegateProperty<KeyValuePair<TKey, TValue>, TValue>(
                nameof(KeyValuePair<TKey, TValue>.Value),
                (ref KeyValuePair<TKey, TValue> container) => container.Value,
                null);

        /// <inheritdoc cref="IPropertyBag{T}.GetProperties()"/>
        public override PropertyCollection<KeyValuePair<TKey, TValue>> GetProperties()
        {
            return new PropertyCollection<KeyValuePair<TKey, TValue>>(GetPropertiesEnumerable());
        }
        
        /// <inheritdoc cref="IPropertyBag{T}.GetProperties(ref T)"/>
        public override PropertyCollection<KeyValuePair<TKey, TValue>> GetProperties(ref KeyValuePair<TKey, TValue> container)
        {
            return new PropertyCollection<KeyValuePair<TKey, TValue>>(GetPropertiesEnumerable());
        }

        static IEnumerable<IProperty<KeyValuePair<TKey, TValue>>> GetPropertiesEnumerable()
        {
            yield return s_KeyProperty;
            yield return s_ValueProperty;
        }

        public bool TryGetProperty(ref KeyValuePair<TKey, TValue> container, string name,
            out IProperty<KeyValuePair<TKey, TValue>> property)
        {
            if (name == nameof(KeyValuePair<TKey, TValue>.Key))
            {
                property = s_KeyProperty;
                return true;
            }

            if (name == nameof(KeyValuePair<TKey, TValue>.Value))
            {
                property = s_ValueProperty;
                return true;
            }

            property = default;
            return false;
        }
    }
}