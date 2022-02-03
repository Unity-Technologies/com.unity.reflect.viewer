using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    /// <summary>
    /// An <see cref="IPropertyBag{T}"/> implementation for a <see cref="Dictionary{TKey, TValue}"/> type.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    class DictionaryPropertyBag<TKey, TValue> : KeyValueCollectionPropertyBag<Dictionary<TKey, TValue>, TKey, TValue>
    {
        protected override ConstructionType ConstructionType => ConstructionType.PropertyBagOverride;

        protected override Dictionary<TKey, TValue> Construct()
            => new Dictionary<TKey, TValue>();
    }
}