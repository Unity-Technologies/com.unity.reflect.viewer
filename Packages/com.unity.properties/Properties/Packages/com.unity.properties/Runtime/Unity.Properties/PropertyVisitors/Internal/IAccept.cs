namespace Unity.Properties.Internal
{
    /// <summary>
    /// Interface for accepting property visitation with adapters. This is an internal interface.
    /// </summary>
    interface IPropertyAcceptWithAdapters<TContainer>
    {
        /// <summary>
        /// Calls the next visitation adapter.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="enumerator">The adapter collection enumerator.</param>
        /// <param name="container">The container being visited.</param>
        void ContinueVisitation(PropertyVisitor visitor, ReadOnlyAdapterCollection.Enumerator enumerator, ref TContainer container);
        
        /// <summary>
        /// Skips the next adapters and calls the default visitation behaviour.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="enumerator">The adapter collection enumerator.</param>
        /// <param name="container">The container being visited.</param>
        void ContinueVisitationWithoutAdapters(PropertyVisitor visitor, ReadOnlyAdapterCollection.Enumerator enumerator, ref TContainer container);
    }
    
    /// <summary>
    /// Interface for accepting property visitation with adapters. This is an internal interface.
    /// </summary>
    interface IPropertyAcceptWithAdapters<TContainer, TValue> : IPropertyAcceptWithAdapters<TContainer>
    {
        /// <summary>
        /// Returns if visitation should proceed for the current value. 
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="enumerator">The adapter collection enumerator.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="value">The value being visited.</param>
        /// <returns><see langword="true"/> if visitation should be skipped, <see langword="false"/> otherwise.</returns>
        bool IsExcluded(PropertyVisitor visitor, ReadOnlyAdapterCollection.Enumerator enumerator, ref TContainer container, ref TValue value);
     
        /// <summary>
        /// Calls the next visitation adapter.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="enumerator">The adapter collection enumerator.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="value">The value being visited.</param>
        void ContinueVisitation(PropertyVisitor visitor, ReadOnlyAdapterCollection.Enumerator enumerator, ref TContainer container, ref TValue value);
        
        /// <summary>
        /// Skips the next adapters and calls the default visitation behaviour.
        /// </summary>
        /// <param name="visitor">The visitor being run.</param>
        /// <param name="enumerator">The adapter collection enumerator.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="value">The value being visited.</param>
        void ContinueVisitationWithoutAdapters(PropertyVisitor visitor, ReadOnlyAdapterCollection.Enumerator enumerator, ref TContainer container, ref TValue value);
    }
}