using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [Test]
        public void ClassWithInt32ListList_WhenListHasListElements_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithLists
            {
                Int32ListList = new List<List<int>>
                {
                    new List<int> { 2, 4, 6 },
                    new List<int> { 3, 5, 7 }
                }
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Int32ListList, Is.Not.Null);
            Assert.That(dst.Int32ListList.Count, Is.EqualTo(src.Int32ListList.Count));
            
            Assert.That(dst.Int32ListList[0], Is.Not.Null);
            Assert.That(dst.Int32ListList[0].Count, Is.EqualTo(3));
            Assert.That(dst.Int32ListList[0][0], Is.EqualTo(2));
            Assert.That(dst.Int32ListList[0][1], Is.EqualTo(4));
            Assert.That(dst.Int32ListList[0][2], Is.EqualTo(6));
            
            Assert.That(dst.Int32ListList[1], Is.Not.Null);
            Assert.That(dst.Int32ListList[1].Count, Is.EqualTo(3));
            Assert.That(dst.Int32ListList[1][0], Is.EqualTo(3));
            Assert.That(dst.Int32ListList[1][1], Is.EqualTo(5));
            Assert.That(dst.Int32ListList[1][2], Is.EqualTo(7));
        }
    }
}