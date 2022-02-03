using System.Collections.Generic;

namespace Unity.Properties.Internal
{    
    /// <summary>
    /// A <see cref="IPropertyBag{T}"/> implementation for a <see cref="List{TElement}"/> type.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    class ListPropertyBag<TElement> : IndexedCollectionPropertyBag<List<TElement>, TElement>
    {
        protected override ConstructionType ConstructionType => ConstructionType.PropertyBagOverride;
        protected override List<TElement> ConstructWithCount(int count) => new List<TElement>(count);
        protected override List<TElement> Construct() => new List<TElement>();
    }
}