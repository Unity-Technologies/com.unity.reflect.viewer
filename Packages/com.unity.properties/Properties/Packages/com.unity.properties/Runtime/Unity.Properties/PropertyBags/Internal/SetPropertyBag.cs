using System;
using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    /// <summary>
    /// A <see cref="IPropertyBag{T}"/> implementation for a generic set of elements using the <see cref="ISet{TElement}"/> interface.
    /// </summary>
    /// <typeparam name="TSet">The collection type.</typeparam>
    /// <typeparam name="TElement">The element type.</typeparam>
    class SetPropertyBagBase<TSet, TElement> : PropertyBag<TSet>, ISetPropertyBag<TSet, TElement>, IPropertiesKeyed<TSet, object>
        where TSet : ISet<TElement>
    {
        class SetElementProperty : Property<TSet, TElement>, ICollectionElementProperty
        {
            internal TElement m_Value;

            public override string Name => m_Value.ToString();
            public override bool IsReadOnly => true;

            public override TElement GetValue(ref TSet container) => m_Value;
            public override void SetValue(ref TSet container, TElement value) => throw new InvalidOperationException("Property is ReadOnly.");
        }

        /// <summary>
        /// Shared instance of a set element property. We re-use the same instance to avoid allocations.
        /// </summary>
        readonly SetElementProperty m_Property = new SetElementProperty();

        public override PropertyCollection<TSet> GetProperties()
        {
            return PropertyCollection<TSet>.Empty;
        }

        public override PropertyCollection<TSet> GetProperties(ref TSet container)
        {
            return new PropertyCollection<TSet>(GetPropertiesEnumerable(container));
        }

        IEnumerable<IProperty<TSet>> GetPropertiesEnumerable(TSet container)
        {
            foreach (var element in container)
            {
                m_Property.m_Value = element;
                yield return m_Property;
            }
        }

        void ICollectionPropertyBagAccept<TSet>.Accept(ICollectionPropertyBagVisitor visitor, ref TSet container)
        {
            visitor.Visit(this, ref container); 
        }
        
        void ISetPropertyBagAccept<TSet>.Accept(ISetPropertyBagVisitor visitor, ref TSet container)
        {
            visitor.Visit(this, ref container);
        }

        void ISetPropertyAccept<TSet>.Accept<TContainer>(ISetPropertyVisitor visitor, Property<TContainer, TSet> property, ref TContainer container, ref TSet dictionary)
        {
            using ((m_Property as IAttributes).CreateAttributesScope(property))
            {
                visitor.Visit<TContainer, TSet, TElement>(property, ref container, ref dictionary);
            }
        }

        public bool TryGetProperty(ref TSet container, object key, out IProperty<TSet> property)
        {
            if (container.Contains((TElement) key))
            {
                property = new SetElementProperty {m_Value = (TElement) key};
                return true;
            }

            property = default;
            return false;
        }
    }
}