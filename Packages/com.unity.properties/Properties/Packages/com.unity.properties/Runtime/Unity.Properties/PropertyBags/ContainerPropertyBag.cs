using System;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// Base class for implementing a static property bag for a specified container type. This is an abstract class.
    /// </summary>
    /// <remarks>
    /// A <see cref="ContainerPropertyBag{TContainer}"/> is used to describe and traverse the properties for a specified <typeparamref name="TContainer"/> type.
    ///
    /// In order for properties to operate on a type, a <see cref="ContainerPropertyBag{TContainer}"/> must exist and be pre-registered for that type.
    ///
    /// _NOTE_ In editor use cases property bags can be generated dynamically through reflection. (see Unity.Properties.Reflection)
    /// </remarks>
    /// <typeparam name="TContainer">The container type.</typeparam>
    public abstract class ContainerPropertyBag<TContainer> : PropertyBag<TContainer>, IPropertiesNamed<TContainer>
    {
        static ContainerPropertyBag()
        {
            if (!RuntimeTypeInfoCache.IsContainerType(typeof(TContainer)))
            {
                throw new InvalidOperationException($"Failed to create a property bag for Type=[{typeof(TContainer)}]. The type is not a valid container type.");
            }
        }
        
        readonly List<IProperty<TContainer>> m_PropertiesList = new List<IProperty<TContainer>>();
        readonly Dictionary<string, IProperty<TContainer>> m_PropertiesHash = new Dictionary<string, IProperty<TContainer>>();

        /// <summary>
        /// Adds a <see cref="Property{TContainer,TValue}"/> to the property bag.
        /// </summary>
        /// <param name="property">The <see cref="Property{TContainer,TValue}"/> to add.</param>
        /// <typeparam name="TValue">The value type for the given property.</typeparam>
        protected void AddProperty<TValue>(Property<TContainer, TValue> property)
        {
            m_PropertiesList.Add(property);
            m_PropertiesHash.Add(property.Name, property);
        }

        /// <inheritdoc/>
        public override PropertyCollection<TContainer> GetProperties()
            => new PropertyCollection<TContainer>(m_PropertiesList);

        /// <inheritdoc/>
        public override PropertyCollection<TContainer> GetProperties(ref TContainer container)
            => new PropertyCollection<TContainer>(m_PropertiesList);

        /// <inheritdoc/>
        public bool TryGetProperty(ref TContainer container, string name, out IProperty<TContainer> property)
            => m_PropertiesHash.TryGetValue(name, out property);
    }
}