using NUnit.Framework;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [Test]
        public void ClassWithMultidimensionalArray_WhenValueIsNull_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithMultidimensionalArray()
            {
                MultidimensionalArrayInt32 = null
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.MultidimensionalArrayInt32, Is.Null);
        }
        
        [Test]
        public void ClassWithMultidimensionalArray_WhenValueIsNotNull_DoesNotThrow()
        {
            var src = new ClassWithMultidimensionalArray()
            {
                MultidimensionalArrayInt32 = new int[2,2]
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.MultidimensionalArrayInt32, Is.Null);
        }
    }
}