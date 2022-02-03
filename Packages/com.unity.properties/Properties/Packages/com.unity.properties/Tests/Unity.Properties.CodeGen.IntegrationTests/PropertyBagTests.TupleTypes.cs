using System;
using System.Linq;
using NUnit.Framework;
using Unity.Properties.Internal;

namespace Unity.Properties.CodeGen.IntegrationTests
{
    [GeneratePropertyBag]
    public class ClassWithAnonymousType
    {
        public (int, float) AnonymousValue;
    }
    
    [GeneratePropertyBag]
    public class ClassWithNamedAnonymousType
    {
        public (int A, string B) AnonymousValue1;
        public (int C, string D) AnonymousValue2;
        public Tuple<int, string> TupleValue;
    }

    [TestFixture]
    sealed partial class PropertyBagTests
    {
        [Test]
        public void ClassWithAnonymousType_HasPropertyBagGenerated()
        {
            // Check properties are generated for anonymous field types.
            {
                var propertyBag = PropertyBagStore.GetPropertyBag(typeof(ClassWithNamedAnonymousType));
            
                Assert.That(propertyBag, Is.InstanceOf(typeof(ContainerPropertyBag<ClassWithNamedAnonymousType>)));

                var typed = propertyBag as ContainerPropertyBag<ClassWithNamedAnonymousType>;
                var container = default(ClassWithNamedAnonymousType);
                var properties = typed.GetProperties(ref container);
            
                Assert.That(properties.Count(), Is.EqualTo(3));
                Assert.That(properties.ElementAt(0), Is.InstanceOf(typeof(Property<ClassWithNamedAnonymousType, (int A, string B)>)));
                Assert.That(properties.ElementAt(1), Is.InstanceOf(typeof(Property<ClassWithNamedAnonymousType, (int C, string D)>)));
                Assert.That(properties.ElementAt(2), Is.InstanceOf(typeof(Property<ClassWithNamedAnonymousType, Tuple<int, string>>)));
            }

            // Check that the anonymous type has a property bag generated
            {
                var propertyBag = PropertyBagStore.GetPropertyBag(typeof((int A, string B)));
                Assert.That(propertyBag, Is.InstanceOf(typeof(ContainerPropertyBag<(int A, string B)>)));
                
                var typed = propertyBag as ContainerPropertyBag<(int A, string B)>;
                var container = default((int A, string B));
                var properties = typed.GetProperties(ref container);
                
                Assert.That(properties.Count(), Is.EqualTo(2));
                Assert.That(properties.ElementAt(0), Is.InstanceOf(typeof(Property<(int A, string B), int>)));
                Assert.That(properties.ElementAt(1), Is.InstanceOf(typeof(Property<(int A, string B), string>)));
            }
        }
    }
}