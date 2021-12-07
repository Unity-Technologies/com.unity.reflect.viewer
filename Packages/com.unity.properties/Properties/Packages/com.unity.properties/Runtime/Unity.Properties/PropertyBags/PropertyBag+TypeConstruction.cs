using Unity.Properties.Internal;

namespace Unity.Properties
{
    static partial class PropertyBag
    {
        /// <summary>
        /// Constructs a new instance of the given <see cref="TContainer"/> type.
        /// </summary>
        /// <typeparam name="TContainer">The container type to construct.</typeparam>
        /// <returns>A new instance of <see cref="TContainer"/>.</returns>
        public static TContainer CreateInstance<TContainer>()
        {
            var propertyBag = PropertyBagStore.GetPropertyBag<TContainer>();
            
            if (null == propertyBag)
                throw new MissingPropertyBagException(typeof(TContainer));

            return propertyBag.CreateInstance();
        }
    }
}