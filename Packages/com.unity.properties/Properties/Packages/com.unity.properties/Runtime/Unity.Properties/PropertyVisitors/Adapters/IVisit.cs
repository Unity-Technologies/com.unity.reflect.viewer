namespace Unity.Properties
{
    namespace Adapters
    {
        /// <summary>
        /// Implement this interface to intercept the visitation for a specific <see cref="TContainer"/> and <see cref="TValue"/> pair.
        /// </summary>
        /// <remarks>
        /// * <seealso cref="IVisit{TValue}"/>
        /// * <seealso cref="IVisit"/>
        /// </remarks>
        /// <typeparam name="TContainer">The container type being visited.</typeparam>
        /// <typeparam name="TValue">The value type being visited.</typeparam>
        public interface IVisit<TContainer, TValue> : IPropertyVisitorAdapter
        {
            /// <summary>
            /// Invoked when the visitor encounters specific a <see cref="TContainer"/> and <see cref="TValue"/> pair.
            /// </summary>
            /// <param name="context">The context being visited.</param>
            /// <param name="container">The container being visited.</param>
            /// <param name="value">The value being visited.</param>
            void Visit(VisitContext<TContainer, TValue> context, ref TContainer container, ref TValue value);
        }

        /// <summary>
        /// Implement this interface to intercept the visitation for a specific <see cref="TValue"/> type.
        /// </summary>
        /// <remarks>
        /// <seealso cref="IVisit{TContainer,TValue}"/>
        /// <seealso cref="IVisit"/>
        /// </remarks>
        /// <typeparam name="TValue">The value type being visited.</typeparam>
        public interface IVisit<TValue> : IPropertyVisitorAdapter
        {
            /// <summary>
            /// Invoked when the visitor encounters specific <see cref="TValue"/> type with any container.
            /// </summary>
            /// <param name="context">The context being visited.</param>
            /// <param name="container">The container being visited.</param>
            /// <param name="value">The value being visited.</param>
            /// <typeparam name="TContainer">The container type being visited.</typeparam>
            void Visit<TContainer>(VisitContext<TContainer, TValue> context, ref TContainer container, ref TValue value);
        }

        /// <summary>
        /// Implement this interface to handle visitation for all properties.
        /// </summary>
        /// <remarks>
        /// <seealso cref="IVisit{TContainer,TValue}"/>
        /// <seealso cref="IVisit{TValue}"/>
        /// </remarks>
        public interface IVisit : IPropertyVisitorAdapter
        {
            /// <summary>
            /// Invoked when the visitor encounters any property.
            /// </summary>
            /// <param name="context">The context being visited.</param>
            /// <param name="container">The container being visited.</param>
            /// <param name="value">The value being visited.</param>
            /// <typeparam name="TValue">The value type being visited.</typeparam>
            /// <typeparam name="TContainer">The container type being visited.</typeparam>
            void Visit<TContainer, TValue>(VisitContext<TContainer, TValue> context, ref TContainer container, ref TValue value);
        }
    }

    namespace Adapters.Contravariant
    {
        /// <summary>
        /// Implement this interface to intercept the visitation for a specific <see cref="TContainer"/> and <see cref="TValue"/> pair.
        /// </summary>
        /// <remarks>
        /// * <seealso cref="IVisit{TValue}"/>
        /// * <seealso cref="IVisit"/>
        /// </remarks>
        /// <typeparam name="TContainer">The container type being visited.</typeparam>
        /// <typeparam name="TValue">The value type being visited.</typeparam>
        public interface IVisit<TContainer, in TValue> : IPropertyVisitorAdapter
        {
            /// <summary>
            /// Invoked when the visitor encounters specific a <see cref="TContainer"/> and <see cref="TValue"/> pair.
            /// </summary>
            /// <param name="context">The context being visited.</param>
            /// <param name="container">The container being visited.</param>
            /// <param name="value">The value being visited.</param>
            void Visit(VisitContext<TContainer> context, ref TContainer container, TValue value);
        }
        
        /// <summary>
        /// Implement this interface to intercept the visitation for a specific <see cref="TValue"/> type.
        /// </summary>
        /// <remarks>
        /// <seealso cref="IVisit{TContainer,TValue}"/>
        /// <seealso cref="IVisit"/>
        /// </remarks>
        /// <typeparam name="TValue">The value type being visited.</typeparam>
        public interface IVisit<in TValue> : IPropertyVisitorAdapter
        {
            /// <summary>
            /// Invoked when the visitor encounters specific <see cref="TValue"/> type with any container.
            /// </summary>
            /// <param name="context">The context being visited.</param>
            /// <param name="container">The container being visited.</param>
            /// <param name="value">The value being visited.</param>
            /// <typeparam name="TContainer">The container type being visited.</typeparam>
            void Visit<TContainer>(VisitContext<TContainer> context, ref TContainer container, TValue value);
        }
    }
}