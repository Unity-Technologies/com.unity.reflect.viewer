using NUnit.Framework;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [Test]
        public void ClassWithInt32_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithPrimitives
            {
                Int32Value = 42,
                CharValue = '\0'
            };

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Int32Value, Is.EqualTo(src.Int32Value));
        }
        
        [Test]
        public void ClassWithFloat32_WhenValueIsVeryLarge_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithPrimitives
            {
                Float32Value = 3.402823E+38f
            };

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Float32Value, Is.EqualTo(src.Float32Value));
        }
        
        [Test]
        public void ClassWithCharValue_CanBeSerializedAndDeserialized()
        {
            void TestChar(char c)
            {
                var src = new ClassWithPrimitives
                {
                    CharValue = c
                };

                var dst = SerializeAndDeserialize(src);
            
                Assert.That(dst, Is.Not.SameAs(src));
                Assert.That(dst.CharValue, Is.EqualTo(src.CharValue));
            }

            TestChar('a');
            TestChar('0');
            TestChar('/');
            TestChar('\0');
            TestChar('\t');
            TestChar('\n');
            TestChar('\b');
        }
    }
}