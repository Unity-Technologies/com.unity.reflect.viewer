using System;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// The <see cref="PropertyBag"/> class provides access to registered property bag instances.
    /// </summary>
    public static partial class PropertyBag
    {
        /// <summary>
        /// Gets an interface to the <see cref="PropertyBag{TContainer}"/> for the given type.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="IPropertyBag"/> can be used to get the strongly typed generic using the <see cref="IPropertyBagAccept"/> interface method. 
        /// </remarks>
        /// <param name="type">The container type to resolve the property bag for.</param>
        /// <returns>The resolved property bag.</returns>
        public static IPropertyBag GetPropertyBag(Type type)
        {
            return PropertyBagStore.GetPropertyBag(type);
        }

        /// <summary>
        /// Gets the strongly typed <see cref="PropertyBag{TContainer}"/> for the given <typeparamref name="TContainer"/>.
        /// </summary>
        /// <typeparam name="TContainer">The container type to resolve the property bag for.</typeparam>
        /// <returns>The resolved property bag, strongly typed.</returns>
        public static IPropertyBag<TContainer> GetPropertyBag<TContainer>()
        {
            return PropertyBagStore.GetPropertyBag<TContainer>();
        }

        /// <summary>
        /// Gets a property bag for the concrete type of the given value.
        /// </summary>
        /// <param name="value">The value type to retrieve a property bag for.</param>
        /// <param name="propertyBag">When this method returns, contains the property bag associated with the specified value, if the bag is found; otherwise, null.</param>
        /// <typeparam name="TValue"></typeparam>
        /// <returns><see langword="true"/> if the property bag was found for the specified value; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetPropertyBagForValue<TValue>(ref TValue value, out IPropertyBag propertyBag)
        {
            return PropertyBagStore.TryGetPropertyBagForValue(ref value, out propertyBag);
        }
    }
    
    /// <summary>
    /// Base class for implementing a property bag for a specified container type. This is an abstract class.
    /// </summary>
    /// <remarks>
    /// This is used as the base class internally and should NOT be extended.
    ///
    /// When implementing custom property bags use:
    /// * <seealso cref="ContainerPropertyBag{TContainer}"/>.
    /// * <seealso cref="IndexedCollectionPropertyBag{TContainer,TValue}"/>.
    /// </remarks>
    /// <typeparam name="TContainer">The container type.</typeparam>
    public abstract class PropertyBag<TContainer> : IPropertyBag<TContainer>, IPropertyBagRegister, IConstructor<TContainer>
    {
        static PropertyBag()
        {
            AOT.PropertyBagGenerator<TContainer>.Preserve();

            if (!RuntimeTypeInfoCache.IsContainerType(typeof(TContainer)))
            {
                throw new InvalidOperationException($"Failed to create a property bag for Type=[{typeof(TContainer)}]. The type is not a valid container type.");
            }
        }

        /// <inheritdoc/>
        void IPropertyBagRegister.Register()
        {
            PropertyBagStore.AddPropertyBag(this);
        }
            
        /// <summary>
        /// Accepts visitation from a specified <see cref="ITypeVisitor"/>.
        /// </summary>
        /// <param name="visitor">The visitor handling visitation.</param>
        /// <exception cref="ArgumentNullException">The visitor is null.</exception>
        public void Accept(ITypeVisitor visitor)
        {
            if (null == visitor)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            visitor.Visit<TContainer>();
        }

        /// <summary>
        /// Accepts visitation from a specified <see cref="IPropertyBagVisitor"/> using an object as the container.
        /// </summary>
        /// <param name="visitor">The visitor handling the visitation.</param>
        /// <param name="container">The container being visited.</param>
        /// <exception cref="ArgumentNullException">The container is null.</exception>
        /// <exception cref="InvalidCastException">The container type does not match the property bag type.</exception>
        void IPropertyBagAccept.Accept(IPropertyBagVisitor visitor, ref object container)
        {
            if (null == container)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (!(container is TContainer typedContainer))
            {
                throw new ArgumentException($"The given ContainerType=[{container.GetType()}] does not match the PropertyBagType=[{typeof(TContainer)}]");
            }

            PropertyBag.AcceptWithSpecializedVisitor(this, visitor, ref typedContainer);

            container = typedContainer;
        }

        /// <summary>
        /// Accepts visitation from a specified <see cref="IPropertyBagVisitor"/> using a strongly typed container.
        /// </summary>
        /// <param name="visitor">The visitor handling the visitation.</param>
        /// <param name="container">The container being visited.</param>
        void IPropertyBagAccept<TContainer>.Accept(IPropertyBagVisitor visitor, ref TContainer container)
        {
            visitor.Visit(this, ref container);
        }

        /// <inheritdoc/>
        PropertyCollection<TContainer> IPropertyBag<TContainer>.GetProperties()
        {
            return GetProperties();
        }

        /// <inheritdoc/>
        PropertyCollection<TContainer> IPropertyBag<TContainer>.GetProperties(ref TContainer container)
        {
            return GetProperties(ref container);
        }

        /// <inheritdoc/>
        ConstructionType IConstructor.ConstructionType => ConstructionType;
        
        /// <inheritdoc/>
        TContainer IConstructor<TContainer>.Construct()
        {
            return Construct();
        }
        
        /// <summary>
        /// Implement this method to return a <see cref="PropertyCollection{TContainer}"/> that can enumerate through all properties for the <typeparamref name="TContainer"/>.
        /// </summary>
        /// <returns>A <see cref="PropertyCollection{TContainer}"/> structure which can enumerate each property.</returns>
        public abstract PropertyCollection<TContainer> GetProperties();
        
        /// <summary>
        /// Implement this method to return a <see cref="PropertyCollection{TContainer}"/> that can enumerate through all properties for the <typeparamref name="TContainer"/>.
        /// </summary>
        /// <param name="container">The container hosting the data.</param>
        /// <returns>A <see cref="PropertyCollection{TContainer}"/> structure which can enumerate each property.</returns>
        public abstract PropertyCollection<TContainer> GetProperties(ref TContainer container);
        
        /// <summary>
        /// Implement this property and return true to provide custom type construction for the container type.
        /// </summary>
        protected virtual ConstructionType ConstructionType { get; } = ConstructionType.Activator;

        /// <summary>
        /// Implement this method to provide custom type construction for the container type.
        /// </summary>
        /// <remarks>
        /// You MUST also override <see cref="ConstructionType"/> to return <see langword="ConstructionType.PropertyBagOverride"/> for this method to be called.
        /// </remarks>
        protected virtual TContainer Construct()
        {
            return default;
        }
        
        /// <inheritdoc/>
        public TContainer CreateInstance() => TypeConstruction.Construct<TContainer>();

        /// <inheritdoc/>
        public bool TryCreateInstance(out TContainer instance) => TypeConstruction.TryConstruct(out instance);
    }
}