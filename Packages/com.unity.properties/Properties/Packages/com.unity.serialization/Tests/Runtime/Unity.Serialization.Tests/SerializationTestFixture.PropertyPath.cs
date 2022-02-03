using NUnit.Framework;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [Test]
        public void ClassWithPropertyPath_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithPropertyPath();
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Path.PartsCount, Is.EqualTo(src.Path.PartsCount));
            for (var i = 0; i < src.Path.PartsCount; ++i)
            {
                Assert.That(src.Path[i], Is.EqualTo(dst.Path[i]));
            }
        }
    }
}