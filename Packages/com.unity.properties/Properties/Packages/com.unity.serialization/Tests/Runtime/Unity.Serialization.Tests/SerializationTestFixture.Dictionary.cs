using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [Test]
        public void ClassWithDictionaryStringInt32_WhenDictionaryIsNull_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithDictionaries
            {
                DictionaryStringInt32 = null
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.DictionaryStringInt32, Is.Null);
        }
        
        [Test]
        public void ClassWithDictionaryStringInt32_WhenDictionaryIsEmpty_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithDictionaries
            {
                DictionaryStringInt32 = new Dictionary<string, int>()
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.DictionaryStringInt32, Is.Not.Null);
            Assert.That(dst.DictionaryStringInt32.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void ClassWithDictionaryStringInt32_WhenDictionaryHasElements_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithDictionaries
            {
                DictionaryStringInt32 = new Dictionary<string, int>
                {
                    { "a", 10 },
                    { "b", 20 },
                    { "c", 30 },
                }
            };
            
            var dst = SerializeAndDeserialize(src);

            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.DictionaryStringInt32, Is.Not.Null);
            Assert.That(dst.DictionaryStringInt32.Count, Is.EqualTo(3));
            Assert.That(dst.DictionaryStringInt32["a"], Is.EqualTo(10));
            Assert.That(dst.DictionaryStringInt32["b"], Is.EqualTo(20));
            Assert.That(dst.DictionaryStringInt32["c"], Is.EqualTo(30));
        }
    }
}