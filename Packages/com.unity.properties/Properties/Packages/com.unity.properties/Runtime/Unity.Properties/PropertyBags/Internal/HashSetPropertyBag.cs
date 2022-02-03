using System.Collections.Generic;

namespace Unity.Properties.Internal
{    
    /// <summary>
    /// An <see cref="IPropertyBag{T}"/> implementation for a <see cref="HashSet{TElement}"/> type.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    class HashSetPropertyBag<TElement> : SetPropertyBagBase<HashSet<TElement>, TElement>
    {
        protected override ConstructionType ConstructionType => ConstructionType.PropertyBagOverride;
        protected override HashSet<TElement> Construct() => new HashSet<TElement>();
    }
}