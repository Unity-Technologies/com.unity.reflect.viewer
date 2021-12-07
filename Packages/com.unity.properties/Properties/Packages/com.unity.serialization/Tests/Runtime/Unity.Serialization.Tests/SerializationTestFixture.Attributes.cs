using NUnit.Framework;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [Test]
        public void ClassWithNonSerializeFields_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithNonSerializeFields()
            {
                A = 2,
                B = 4,
                C = 6
            };

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.A, Is.EqualTo(src.A));
            Assert.That(dst.B, Is.EqualTo(src.B));
            Assert.That(dst.C, Is.EqualTo(default(int)));
        }
    }
}