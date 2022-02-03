using NUnit.Framework;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [Test]
        public void ClassWithInt32Array_WhenArrayIsNull_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithArrays
            {
                Int32Array = null
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Int32Array, Is.EqualTo(src.Int32Array));
        }
        
        [Test]
        public void ClassWithInt32Array_WhenArrayIsEmpty_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithArrays
            {
                Int32Array = new int[0]
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Int32Array.Length, Is.EqualTo(src.Int32Array.Length));
        }
        
        [Test]
        public void ClassWithInt32Array_WhenArrayHasElements_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithArrays
            {
                Int32Array = new[] { 3, 6, 9}
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Int32Array, Is.EquivalentTo(src.Int32Array));
        }
        
        [Test]
        public void ClassWithClassArray_WhenArrayHasSomeNullElements_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithArrays
            {
                ClassContainerArray = new[] { null, new ClassWithPrimitives { Int16Value = 356 }, null }
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.ClassContainerArray.Length, Is.EqualTo(src.ClassContainerArray.Length));
            Assert.That(dst.ClassContainerArray[0], Is.Null);
            Assert.That(dst.ClassContainerArray[1], Is.Not.Null);
            Assert.That(dst.ClassContainerArray[2], Is.Null);
            Assert.That(dst.ClassContainerArray[1].Int16Value, Is.EqualTo(356));
        }
    }
}