using System;
using System.Linq;
using NUnit.Framework;
using Unity.Properties.Internal;

namespace Unity.Properties.CodeGen.IntegrationTests
{
    [GeneratePropertyBag]
    class ClassWithPointerTypes
    {
#pragma warning disable 649
        public unsafe int* IntPointer;
        public IntPtr IntPtr;
#pragma warning restore 649
    }
    
    [TestFixture]
    sealed partial class PropertyBagTests
    {
        [Test]
        public void ClassWithPointerTypes_HasHasPropertyBagGenerated()
        {
            var propertyBag = PropertyBagStore.GetPropertyBag(typeof(ClassWithPointerTypes)) as ContainerPropertyBag<ClassWithPointerTypes>;
            Assert.That(propertyBag, Is.Not.Null);
            Assert.That(propertyBag.GetProperties().Count(), Is.EqualTo(1));
        }
    }
}