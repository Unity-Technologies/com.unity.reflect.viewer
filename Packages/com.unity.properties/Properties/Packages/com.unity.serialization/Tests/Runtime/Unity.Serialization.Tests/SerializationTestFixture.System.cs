#if !NET_DOTS
using System;
using NUnit.Framework;
using Unity.Properties;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [GeneratePropertyBag]
        internal class ClassWithSystemTypes
        {
            public Guid Guid;
            public DateTime DateTime;
            public TimeSpan TimeSpan;
            public Version Version;
        }
        
        [Test]
        public void ClassWithSystemTypes_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithSystemTypes
            {
                Guid = Guid.NewGuid(),
                DateTime = DateTime.Now,
                TimeSpan = TimeSpan.FromSeconds(10),
                Version = new Version(1, 2, 3, 42)
            };

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst.Guid, Is.EqualTo(src.Guid));
            Assert.That(dst.DateTime, Is.EqualTo(src.DateTime));
            Assert.That(dst.TimeSpan, Is.EqualTo(src.TimeSpan));
            Assert.That(dst.Version, Is.EqualTo(src.Version));
        }
    }
}
#endif