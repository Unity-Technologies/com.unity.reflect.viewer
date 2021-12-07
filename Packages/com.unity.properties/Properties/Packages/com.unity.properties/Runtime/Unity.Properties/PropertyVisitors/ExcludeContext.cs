namespace Unity.Properties
{
    /// <summary>
    /// Context object used during visitation to determine if a property should be visited or not.
    /// </summary>
    /// <typeparam name="TContainer">The container type of the <see cref="IProperty"/>.</typeparam>
    /// <typeparam name="TValue">The value type of the <see cref="IProperty"/>.</typeparam>
    public readonly struct ExcludeContext<TContainer, TValue>
    {
        internal static ExcludeContext<TContainer, TValue> FromProperty(
            PropertyVisitor visitor,
            Property<TContainer, TValue> property)
        {
            return new ExcludeContext<TContainer, TValue>(visitor, property);
        }
        
        readonly PropertyVisitor m_Visitor;
        
        /// <summary>
        /// The property being visited.
        /// </summary>
        public readonly Property<TContainer, TValue> Property;

        ExcludeContext(PropertyVisitor visitor, Property<TContainer, TValue> property)
        {
            m_Visitor = visitor;
            Property = property;
        }
    }

    /// <summary>
    /// Context object used during visitation to determine if a property should be visited or not.
    /// </summary>
    /// <typeparam name="TContainer">The container type of the <see cref="IProperty"/>.</typeparam>
    public readonly struct ExcludeContext<TContainer>
    {
        internal static ExcludeContext<TContainer> FromProperty<TValue>(
            PropertyVisitor visitor,
            Property<TContainer, TValue> property)
        {
            return new ExcludeContext<TContainer>(visitor, property);
        }
        
        readonly PropertyVisitor m_Visitor;
        
        /// <summary>
        /// The property being visited.
        /// </summary>
        public readonly IProperty<TContainer> Property;

        ExcludeContext(PropertyVisitor visitor, IProperty<TContainer> property)
        {
            m_Visitor = visitor;
            Property = property;
        }
    }
}