using NUnit.Framework;
using Unity.Properties;
using Unity.Properties.CodeGen.IntegrationTests;
using Unity.Properties.Internal;

[assembly: GeneratePropertyBagsForType(typeof(ClassWithOpenGeneric<>))]
[assembly: GeneratePropertyBagsForTypesQualifiedWith(typeof(IGeneratePropertyBag))]

namespace Unity.Properties.CodeGen.IntegrationTests
{
#pragma warning disable 649
    [GeneratePropertyBag]
    class ClassWithOpenGeneric<T>
    {
        public T Value;
    }

    interface IGeneratePropertyBag
    {
        
    } 
    
    public class ClassWithOpenGenericInterface<T> : IGeneratePropertyBag
    {
        public T Value;
    }
#pragma warning restore 649

    [TestFixture]
    sealed partial class PropertyBagTests
    {
        [Test]
        public void ClassWithOpenGeneric_DoesNotHavePropertyBagGenerated()
        {
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(ClassWithOpenGeneric<>)), Is.Null);
        }
        
        [Test]
        public void ClassWithOpenGenericInterface_DoesNotHavePropertyBagGenerated()
        {
            Assert.That(PropertyBagStore.GetPropertyBag(typeof(ClassWithOpenGenericInterface<>)), Is.Null);
        }
    }
}