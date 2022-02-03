using NUnit.Framework;
using Unity.Serialization.Tests;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    partial class JsonSerializationTests
    {
        [Test]
        public void ToJson_WhenRootTypeIsKnown_TypeInformationIsNotIncludedInTheJsonString()
        {
            var container = new ClassDerivedA();

            // Serialize a polymorphic type but instruct the serialization that we know the root type at deserialization.
            var json = JsonSerialization.ToJson<IContainerInterface>(container, new JsonSerializationParameters { SerializedType = typeof(ClassDerivedA) });
            
            // Ensure no type information is included in the output.
            Assert.That(json, Does.Not.Contain("$type"));
        }
        
        [Test]
        public void FromJson_WhenRootTypeIsKnown_ReturnsClassInstanceWithCorrectValues()
        {
            const string json = @"{""AbstractInt32Value"": 5, ""DerivedAInt32Value"": 10}";
            
            // Serialize a polymorphic type but instruct the serialization that we know the root type at deserialization.
            var container = JsonSerialization.FromJson<IContainerInterface>(json, new JsonSerializationParameters { SerializedType = typeof(ClassDerivedA) });
            
            Assert.That(container, Is.TypeOf<ClassDerivedA>());

            var derived = container as ClassDerivedA;
            
            Assert.That(derived.AbstractInt32Value, Is.EqualTo(5));
            Assert.That(derived.DerivedAInt32Value, Is.EqualTo(10));
        }
    }
}