using JetBrains.Annotations;
using NUnit.Framework;
using Unity.Properties;
#pragma warning disable 649

namespace Unity.Serialization.Json.Tests
{
    partial class JsonSerializationTests
    {
        [GeneratePropertyBag, UsedImplicitly]
        internal class TestClassWithFormerNameMembers
        {
            [UnityEngine.Serialization.FormerlySerializedAs("x")]
            public float RenamedX;
            
            [Unity.Serialization.FormerName("y")]
            public float RenamedY;
            
            [Unity.Serialization.FormerName("z")]
            [Unity.Properties.CreateProperty] public float RenamedZ { get; set; }

            public TestClassWithFormerName Nested;
        }
        
        [FormerName("Some.Other.Assembly.DerivedTypeThatIsConstructible, Some.Other.Assembly")]
        [UsedImplicitly]
        internal class TestClassWithFormerName
        {
            public int A;
        }

        [Test]
        public void FromJson_WithFormerNameMembers_ReturnsClassInstanceWithCorrectValues()
        {
            const string json = @"{""x"": 1, ""y"": 2, ""z"": 3, ""Nested"": {""$type"":""Some.Other.Assembly.DerivedTypeThatIsConstructible, Some.Other.Assembly"", ""A"": 42}}";
            
            var obj = JsonSerialization.FromJson<TestClassWithFormerNameMembers>(json);
            
            Assert.That(obj.RenamedX, Is.EqualTo(1));
            Assert.That(obj.RenamedY, Is.EqualTo(2));
            Assert.That(obj.RenamedZ, Is.EqualTo(3));
            Assert.That(obj.Nested, Is.Not.Null);
            Assert.That(obj.Nested.A, Is.EqualTo(42));
        }
    }
}