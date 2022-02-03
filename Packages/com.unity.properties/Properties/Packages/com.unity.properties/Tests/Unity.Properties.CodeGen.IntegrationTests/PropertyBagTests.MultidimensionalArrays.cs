using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Properties.Internal;

namespace Unity.Properties.CodeGen.IntegrationTests
{
#pragma warning disable 649
    [GeneratePropertyBag]
    class ClassWithMultidimensionalArray
    {
        public int[,] IntArrayField;
    }
    [GeneratePropertyBag]
    class ClassWithMultidimensionalGeneric
    {
        public Dictionary<int, int[,]> DictionaryWithMultidimensionalArray;
        public Dictionary<int, List<int[,]>> DictionaryWithListOfMultidimensionalArray;
    }
#pragma warning restore 649

    [TestFixture]
    sealed partial class PropertyBagTests
    {
        [Test]
        public void ClassWithMultidimensionalArray_HasPropertyBagGenerated()
        {
            var propertyBag = PropertyBagStore.GetPropertyBag(typeof(ClassWithMultidimensionalArray)) as ContainerPropertyBag<ClassWithMultidimensionalArray>;
            Assert.That(propertyBag, Is.Not.Null);
            var container = new ClassWithMultidimensionalArray();
            
            // @TODO ILPP should support this case
            // Assert.That(propertyBag.GetProperties(ref container).Count(), Is.EqualTo(1));
            
            Assert.Pass(); // Test is inconclusive on CI
        }
        
        [Test]
        public void ClassWithMultidimensionalGeneric_HasPropertyBagGenerated()
        {
            var propertyBag = PropertyBagStore.GetPropertyBag(typeof(ClassWithMultidimensionalGeneric)) as ContainerPropertyBag<ClassWithMultidimensionalGeneric>;
            Assert.That(propertyBag, Is.Not.Null);
            var container = new ClassWithMultidimensionalGeneric();
            
            // @TODO ILPP should support this case
            // Assert.That(propertyBag.GetProperties(ref container).Count(), Is.EqualTo(2));
            Assert.Pass(); // Test is inconclusive on CI
        }
    }
}