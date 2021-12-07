using System;
using System.Linq;
using NUnit.Framework;
using Unity.Properties.Internal;

namespace Unity.Properties.CodeGen.IntegrationTests
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    class CustomAttribute : Attribute
    {
            
    }
        
    class ClassWithAttributeOnPrivateField
    {
        // ReSharper disable once InconsistentNaming
        [CustomAttribute, CreateProperty] int Value;
    }

    [GeneratePropertyBag]
    class SuperClassWithAttributeOnPrivateField : ClassWithAttributeOnPrivateField
    {
            
    }

    [TestFixture]
    partial class PropertyBagTests
    {
        [Test]
        public void ClassThatInheritsClassWithAttributes_HasPropertyBagGenerated()
        {
            var container = default(SuperClassWithAttributeOnPrivateField);
            var properties = (PropertyBagStore.GetPropertyBag(typeof(SuperClassWithAttributeOnPrivateField)) as IPropertyBag<SuperClassWithAttributeOnPrivateField>).GetProperties(ref container);

            Assert.That(properties.Count(), Is.EqualTo(1));
            Assert.That(properties.First().HasAttribute<CustomAttribute>());
        }
    }
}