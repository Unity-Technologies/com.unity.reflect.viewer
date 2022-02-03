using System.Collections.Generic;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core
{

    abstract class SimpleTransferVisitor<T> : IPropertyVisitor, IPropertyBagVisitor
    {
        public T Value;

        public void Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
        {
            var path = new PropertyPath(property.Name);

            if (!PropertyContainer.IsPathValid(ref Value, path))
                return;

            if (!PropertyContainer.TryGetValue(ref Value, path, out TValue oldValue))
            {
                // Could not cast to TValue
                return;
            }

            var currentValue = property.GetValue(ref container);
            PropertyContainer.SetValue(ref Value, path, currentValue);
            var newValue = PropertyContainer.GetValue<T, TValue>(ref Value, path);
            if (!EqualityComparer<TValue>.Default.Equals(oldValue, newValue))
            {
                // Value was changed
            }
            else
            {
                // Possible that the value was changed
            }
        }

        public void Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
        {
            foreach (var property in properties.GetProperties(ref container))
            {
                property.Accept(this, ref container);
            }
        }
    }
}
