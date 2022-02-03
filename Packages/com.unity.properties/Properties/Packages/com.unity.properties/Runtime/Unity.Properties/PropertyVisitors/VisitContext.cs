using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// Context object used during visitation when a <see cref="IProperty{TContainer}"/> is visited.
    /// </summary>
    /// <typeparam name="TContainer">The container type of the <see cref="IProperty"/>.</typeparam>
    /// <typeparam name="TValue">The value type of the <see cref="IProperty"/>.</typeparam>
    public readonly struct VisitContext<TContainer, TValue>
    {
        internal static VisitContext<TContainer, TValue> FromProperty(
            PropertyVisitor visitor,
            ReadOnlyAdapterCollection.Enumerator enumerator,
            Property<TContainer, TValue> property)
        {
            return new VisitContext<TContainer, TValue>(visitor, enumerator, property);
        }

        readonly ReadOnlyAdapterCollection.Enumerator m_Enumerator;
        readonly IPropertyAcceptWithAdapters<TContainer, TValue> m_Internal;
        readonly PropertyVisitor m_Visitor;

        /// <summary>
        /// The property being visited.
        /// </summary>
        public readonly Property<TContainer, TValue> Property;

        VisitContext(
            PropertyVisitor visitor,
            ReadOnlyAdapterCollection.Enumerator enumerator,
            Property<TContainer, TValue> property)
        {
            m_Visitor = visitor;
            m_Enumerator = enumerator;
            Property = property;
            m_Internal = property;
        }

        /// <summary>
        /// Continues visitation through the next visitation adapter.
        /// </summary>
        /// <param name="container">The container being visited.</param>
        public void ContinueVisitation(ref TContainer container, ref TValue value)
        {
            m_Internal.ContinueVisitation(m_Visitor, m_Enumerator, ref container, ref value);
        }

        /// <summary>
        /// Continues visitation while skipping the next visitation adapters.
        /// </summary>
        /// <param name="container">The container being visited.</param>
        /// <param name="value">The value being visited.</param>
        public void ContinueVisitationWithoutAdapters(ref TContainer container, ref TValue value)
        {
            m_Internal.ContinueVisitationWithoutAdapters(m_Visitor, m_Enumerator, ref container, ref value);
        }
    }

    /// <summary>
    /// Context object used during visitation when a <see cref="IProperty{TContainer}"/> is visited.
    /// </summary>
    /// <typeparam name="TContainer">The container type of the <see cref="IProperty"/>.</typeparam>
    public readonly struct VisitContext<TContainer>
    {
        internal static VisitContext<TContainer> FromProperty<TValue>(
            PropertyVisitor visitor,
            ReadOnlyAdapterCollection.Enumerator enumerator,
            Property<TContainer, TValue> property)
        {
            return new VisitContext<TContainer>(visitor, enumerator, property, property);
        }

        readonly ReadOnlyAdapterCollection.Enumerator m_Enumerator;
        readonly IPropertyAcceptWithAdapters<TContainer> m_Internal;
        readonly PropertyVisitor m_Visitor;

        /// <summary>
        /// The property being visited.
        /// </summary>
        public readonly IProperty<TContainer> Property;

        VisitContext(
            PropertyVisitor visitor,
            ReadOnlyAdapterCollection.Enumerator enumerator,
            IProperty<TContainer> property,
            IPropertyAcceptWithAdapters<TContainer> accept)
        {
            m_Visitor = visitor;
            m_Enumerator = enumerator;
            Property = property;
            m_Internal = accept;
        }

        /// <summary>
        /// Continues visitation through the next visitation adapter.
        /// </summary>
        /// <param name="container">The container being visited.</param>
        public void ContinueVisitation(ref TContainer container)
        {
            m_Internal.ContinueVisitation(m_Visitor, m_Enumerator, ref container);
        }

        /// <summary>
        /// Continues visitation while skipping the next visitation adapters.
        /// </summary>
        /// <param name="container">The container being visited.</param>
        public void ContinueVisitationWithoutAdapters(ref TContainer container)
        {
            m_Internal.ContinueVisitationWithoutAdapters(m_Visitor, m_Enumerator, ref container);
        }
    }
}
