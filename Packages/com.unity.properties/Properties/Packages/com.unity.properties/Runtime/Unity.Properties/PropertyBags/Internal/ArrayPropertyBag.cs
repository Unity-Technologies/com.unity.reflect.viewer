using System;

namespace Unity.Properties.Internal
{
    /// <summary>
    /// An <see cref="IPropertyBag{T}"/> implementation for a built in array of <typeparamref name="TElement"/>.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    class ArrayPropertyBag<TElement> : IndexedCollectionPropertyBag<TElement[], TElement>
    {
        protected override ConstructionType ConstructionType => ConstructionType.PropertyBagOverride;
        protected override TElement[] ConstructWithCount(int count) => new TElement[count];
        protected override TElement[] Construct() => throw new InvalidOperationException();
    }
}