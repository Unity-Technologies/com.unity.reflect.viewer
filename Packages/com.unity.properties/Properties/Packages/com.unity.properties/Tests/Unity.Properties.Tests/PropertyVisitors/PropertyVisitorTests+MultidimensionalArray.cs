using NUnit.Framework;

namespace Unity.Properties.Tests
{
    partial class PropertyVisitorTests
    {
        [GeneratePropertyBag]
        public class ClassWithMultidimensionalArray
        {
            public int[,] Int32MultidimensionalArray;
        }
        
        class EmptyVisitor : PropertyVisitor
        {
        }
        
        class EmptyLowLevelVisitor : 
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
            
            void IPropertyVisitor.Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
            {
                var value = property.GetValue(ref container);
                PropertyContainer.TryAccept(this, ref value);
            }
        }

        [Test]
        public void PropertyVisitor_ClassWithMultidimensionalArray_WhenArrayIsNull()
        {
            var container = new ClassWithMultidimensionalArray();
            
            PropertyContainer.Accept(new EmptyVisitor(), container);
            PropertyContainer.Accept(new EmptyLowLevelVisitor(), container);
        }

        [Test]
        public void PropertyVisitor_ClassWithMultidimensionalArray_WhenArrayIsNotNull()
        {
            var container = new ClassWithMultidimensionalArray
            {
                Int32MultidimensionalArray = new [,]
                {
                    {1, 2},
                    {3, 4}
                }
            };
            
            PropertyContainer.Accept(new EmptyVisitor(), container);
            PropertyContainer.Accept(new EmptyLowLevelVisitor(), container);
        }
    }
}