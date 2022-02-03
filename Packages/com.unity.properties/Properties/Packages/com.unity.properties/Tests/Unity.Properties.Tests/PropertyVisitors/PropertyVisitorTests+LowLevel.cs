using NUnit.Framework;

namespace Unity.Properties.Tests
{
    partial class PropertyVisitorTests
    {
        class MyContainerType
        {
        }

        [Test]
        public void VisitTest()
        {
            var container = new MyContainerType();
            PropertyContainer.Accept(new LowLevelVisitor(), container);
        }
        
        class LowLevelVisitor : 
            IPropertyBagVisitor, 
            IPropertyVisitor
        {
            void IPropertyBagVisitor.Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
            {
                foreach (var property in properties.GetProperties(ref container))
                {
                    property.Accept(this, ref container);
                }
            }
            
            /// <summary>
            /// Invoked by property.Accept and provides the strongly typed value.
            /// </summary>
            void IPropertyVisitor.Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
            {
                var value = property.GetValue(ref container);
                
                // Re-entry will invoke the strongly typed visit callback for this container.
                PropertyContainer.Accept(this, value);
            }
        }
    }
}