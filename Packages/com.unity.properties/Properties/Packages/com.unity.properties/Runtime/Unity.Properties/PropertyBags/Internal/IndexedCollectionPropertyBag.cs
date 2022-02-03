using System.Collections;
using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    /// <summary>
    /// An <see cref="IPropertyBag{T}"/> implementation for a generic collection of elements which can be accessed by index. This is based on the <see cref="IList{TElement}"/> interface.
    /// </summary>
    /// <typeparam name="TList">The collection type.</typeparam>
    /// <typeparam name="TElement">The element type.</typeparam>
    class IndexedCollectionPropertyBag<TList, TElement> : PropertyBag<TList>, IListPropertyBag<TList, TElement>, IConstructorWithCount<TList>
        where TList : IList<TElement>
    {
        class ListElementProperty : Property<TList, TElement>, IListElementProperty
        {
            internal int m_Index;
            internal bool m_IsReadOnly;

            /// <inheritdoc/>
            public int Index => m_Index;
        
            /// <inheritdoc/>
            public override string Name => Index.ToString();
        
            /// <inheritdoc/>
            public override bool IsReadOnly => m_IsReadOnly;
        
            /// <inheritdoc/>
            public override TElement GetValue(ref TList container) => container[m_Index];
        
            /// <inheritdoc/>
            public override void SetValue(ref TList container, TElement value) => container[m_Index] = value;
        }

        /// <summary>
        /// Internal collection used to dynamically return the same instance pointing to a different index.
        /// </summary>
        readonly struct Enumerable : IEnumerable<IProperty<TList>>
        {
            struct Enumerator : IEnumerator<IProperty<TList>>
            {
                readonly TList m_List;
                readonly ListElementProperty m_Property;
                readonly int m_Previous;
                int m_Position;

                internal Enumerator(TList list, ListElementProperty property)
                {
                    m_List = list;
                    m_Property = property;
                    m_Previous = property.m_Index;
                    m_Position = -1;
                }

                /// <inheritdoc/>
                public IProperty<TList> Current => m_Property;

                /// <inheritdoc/>
                object IEnumerator.Current => Current;

                /// <inheritdoc/>
                public bool MoveNext()
                {
                    m_Position++;
                    
                    if (m_Position < m_List.Count)
                    {
                        m_Property.m_Index = m_Position;
                        m_Property.m_IsReadOnly = false;
                        return true;
                    }
                    
                    m_Property.m_Index = m_Previous;
                    m_Property.m_IsReadOnly = false;
                    return false;
                }

                /// <inheritdoc/>
                public void Reset()
                {
                    m_Position = -1;
                    m_Property.m_Index = m_Previous;
                    m_Property.m_IsReadOnly = false;
                }

                /// <inheritdoc/>
                public void Dispose()
                {
                }
            }

            readonly TList m_List;
            readonly ListElementProperty m_Property;
            
            public Enumerable(TList list, ListElementProperty property)
            {
                m_List = list;
                m_Property = property;
            }
            
            /// <inheritdoc/>
            IEnumerator<IProperty<TList>> IEnumerable<IProperty<TList>>.GetEnumerator() 
                => new Enumerator(m_List, m_Property);
            
            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() 
                => new Enumerator(m_List, m_Property);
        }
        
        /// <summary>
        /// Shared instance of a list element property. We re-use the same instance to avoid allocations.
        /// </summary>
        readonly ListElementProperty m_Property = new ListElementProperty();

        /// <inheritdoc cref="IPropertyBag{T}.GetProperties()"/>
        public override PropertyCollection<TList> GetProperties()
        {
            return PropertyCollection<TList>.Empty;
        }
        
        /// <inheritdoc cref="IPropertyBag{T}.GetProperties(ref T)"/>
        public override PropertyCollection<TList> GetProperties(ref TList container)
        {
            return new PropertyCollection<TList>(new Enumerable(container, m_Property));
        }

        /// <inheritdoc/>
        public bool TryGetProperty(ref TList container, int index, out IProperty<TList> property)
        {
            if ((uint) index >= (uint) container.Count)
            {
                property = null;
                return false;
            }
            
            property = new ListElementProperty
            {
                m_Index = index,
                m_IsReadOnly = false
            };

            return true;
        }
        
        void ICollectionPropertyBagAccept<TList>.Accept(ICollectionPropertyBagVisitor visitor, ref TList container)
        {
            visitor.Visit(this, ref container); 
        }
        
        void IListPropertyBagAccept<TList>.Accept(IListPropertyBagVisitor visitor, ref TList list)
        {
            visitor.Visit(this, ref list);
        }
        
        void IListPropertyAccept<TList>.Accept<TContainer>(IListPropertyVisitor visitor, Property<TContainer, TList> property, ref TContainer container, ref TList list)
        {
            using ((m_Property as IAttributes).CreateAttributesScope(property))
            {
                visitor.Visit<TContainer, TList, TElement>(property, ref container, ref list);
            }
        }
        
        TList IConstructorWithCount<TList>.ConstructWithCount(int count)
        {
            return ConstructWithCount(count);
        }
        
        /// <summary>
        /// Implement this method to provide custom type construction with a count value for the container type.
        /// </summary>
        /// <remarks>
        /// You MUST also override <see cref="ConstructionType"/> to return <see langword="ConstructionType.PropertyBagOverride"/> for this method to be called.
        /// </remarks>
        protected virtual TList ConstructWithCount(int count)
        {
            return default;
        }
    }
}