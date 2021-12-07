namespace Unity.Properties
{
    /// <summary>
    /// Interface for accepting type visitation.
    /// </summary>
    /// <remarks>
    /// This code path is NOT safe for AOT platforms.
    /// </remarks>
    public interface ITypeAccept
    {
        /// <summary>
        /// Call this method to invoke <see cref="ITypeVisitor.Visit{TContainer}"/> with the strongly typed container type.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        void Accept(ITypeVisitor visitor);
    }
    
    /// <summary>
    /// Interface to accept property bag visitation on an untyped object.
    /// </summary>
    public interface IPropertyBagAccept
    {
        /// <summary>
        /// Call this method to invoke <see cref="IPropertyBagVisitor.Visit{TContainer}"/> with the strongly typed container for the given <see cref="container"/> object.
        /// </summary>
        /// <param name="visitor">The visitor to invoke the visit callback on.</param>
        /// <param name="container">The container being visited.</param>
        void Accept(IPropertyBagVisitor visitor, ref object container);
    }
    
    /// <summary>
    /// Interface to accept property bag visitation on a strongly typed object.
    /// </summary>
    public interface IPropertyBagAccept<TContainer>
    {
        /// <summary>
        /// Call this method to invoke <see cref="IPropertyBagVisitor.Visit{TContainer}"/> with the strongly typed container.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="container">The container being visited.</param>
        void Accept(IPropertyBagVisitor visitor, ref TContainer container);
    }
    
    /// <summary>
    /// Interface for accepting collection property bags visitation. This is an internal interface.
    /// </summary>
    public interface ICollectionPropertyBagAccept<TContainer>
    {
        /// <summary>
        /// Call this method to invoke <see cref="ICollectionPropertyBagVisitor.Visit{TCollection,TElement}"/> with the strongly typed container.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="container">The container being visited.</param>
        void Accept(ICollectionPropertyBagVisitor visitor, ref TContainer container);
    }
    
    /// <summary>
    /// Interface for accepting list property bags visitation. This is an internal interface.
    /// </summary>
    public interface IListPropertyBagAccept<TContainer>
    {
        /// <summary>
        /// Call this method to invoke <see cref="IListPropertyBagVisitor.Visit{TList,TElement}"/> with the strongly typed container.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="container">The container being visited.</param>
        void Accept(IListPropertyBagVisitor visitor, ref TContainer container);
    }
    
    /// <summary>
    /// Interface for accepting list property bags visitation. This is an internal interface.
    /// </summary>
    public interface ISetPropertyBagAccept<TContainer>
    {
        /// <summary>
        /// Call this method to invoke <see cref="ISetPropertyBagVisitor.Visit{TSet,TValue}"/> with the strongly typed container.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="container">The container being visited.</param>
        void Accept(ISetPropertyBagVisitor visitor, ref TContainer container);
    }
    
    /// <summary>
    /// Interface for accepting list property bags visitation. This is an internal interface.
    /// </summary>
    public interface IDictionaryPropertyBagAccept<TContainer>
    {
        /// <summary>
        /// Call this method to invoke <see cref="IDictionaryPropertyBagVisitor.Visit{TDictionary,TKey,TValue}"/> with the strongly typed container.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="container">The container being visited.</param>
        void Accept(IDictionaryPropertyBagVisitor visitor, ref TContainer container);
    }

    /// <summary>
    /// Interface for accepting property visitation. This is an internal interface.
    /// </summary>
    public interface IPropertyAccept<TContainer>
    {
        /// <summary>
        /// Call this method to invoke <see cref="IPropertyVisitor.Visit{TContainer,TValue}"/> with the strongly typed container and value.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="container">The container being visited.</param>
        void Accept(IPropertyVisitor visitor, ref TContainer container);
    }
    
    /// <summary>
    /// Interface for accepting collection property visitation. This is an internal interface.
    /// </summary>
    public interface ICollectionPropertyAccept<TCollection>
    {
        /// <summary>
        /// Call this method to invoke <see cref="ICollectionPropertyVisitor.Visit{TContainer,TCollection,TElement}"/>.
        /// </summary>
        /// <remarks>
        /// This method is used to join the container type and element type.
        /// </remarks>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="property">The property being visited.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="collection">The collection value</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        void Accept<TContainer>(ICollectionPropertyVisitor visitor, Property<TContainer, TCollection> property, ref TContainer container, ref TCollection collection);
    }
    
    /// <summary>
    /// Interface for accepting list property visitation. This is an internal interface.
    /// </summary>
    public interface IListPropertyAccept<TList>
    {
        /// <summary>
        /// Call this method to invoke <see cref="IListPropertyVisitor.Visit{TContainer,TList,TElement}"/>.
        /// </summary>
        /// <remarks>
        /// This method is used to join the container type and element type.
        /// </remarks>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="property">The property being visited.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="list">The list value.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        void Accept<TContainer>(IListPropertyVisitor visitor, Property<TContainer, TList> property, ref TContainer container, ref TList list);
    }
    
    /// <summary>
    /// Interface for accepting hash set property visitation. This is an internal interface.
    /// </summary>
    public interface ISetPropertyAccept<TSet>
    {
        /// <summary>
        /// Call this method to invoke <see cref="ISetPropertyVisitor.Visit{TContainer,TSet,TValue}"/>.
        /// </summary>
        /// <remarks>
        /// This method is used to join the container, the key and the value type.
        /// </remarks>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="property">The property being visited.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="set">The set value.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        void Accept<TContainer>(ISetPropertyVisitor visitor, Property<TContainer, TSet> property, ref TContainer container, ref TSet set);
    }
    
    /// <summary>
    /// Interface for accepting dictionary property visitation. This is an internal interface.
    /// </summary>
    public interface IDictionaryPropertyAccept<TDictionary>
    {
        /// <summary>
        /// Call this method to invoke <see cref="IDictionaryPropertyVisitor.Visit{TContainer,TDictionary,TKey, TValue}"/>.
        /// </summary>
        /// <remarks>
        /// This method is used to join the container, the key and the value type.
        /// </remarks>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="property">The property being visited.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="dictionary">The dictionary value.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        void Accept<TContainer>(IDictionaryPropertyVisitor visitor, Property<TContainer, TDictionary> property, ref TContainer container, ref TDictionary dictionary);
    }
}