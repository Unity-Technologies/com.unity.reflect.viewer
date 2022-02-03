using NUnit.Framework;
using Unity.Properties;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        internal abstract class BaseClassWithGenericField<T>
        {
            public T BaseValue;
        }
    
        internal class ClassWithMultipleGenerics<T, V> : BaseClassWithGenericField<V>
        {
            public T FirstGeneric;
            public V SecondGeneric;
        }
        
        [GeneratePropertyBag]
        internal class ClassWithMultipleLevelsOfGenerics : ClassWithMultipleGenerics<int, float>
        {
        }

        [Test] 
        public void ClassWithMultipleLevelsOfGenerics_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithMultipleLevelsOfGenerics
            {
                FirstGeneric = 1,
                SecondGeneric = 2,
                BaseValue = 3
            };

            var dst = SerializeAndDeserialize(src);

            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.FirstGeneric, Is.EqualTo(src.FirstGeneric));
            Assert.That(dst.SecondGeneric, Is.EqualTo(src.SecondGeneric));
            Assert.That(dst.BaseValue, Is.EqualTo(src.BaseValue));
        }
    }
}