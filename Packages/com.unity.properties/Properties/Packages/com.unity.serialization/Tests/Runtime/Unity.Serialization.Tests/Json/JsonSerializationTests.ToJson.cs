using System.Collections.Generic;
using NUnit.Framework;
using Unity.Properties;
using UnityEngine;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    partial class JsonSerializationTests
    {
        [GeneratePropertyBag]
        internal struct TestStruct
        {
            public int A;
            public int B;
        }

        interface ITestInterface
        {
            
        }

        [GeneratePropertyBag]
        internal class TestConcreteA : ITestInterface
        {
            public int A;
        }

        [GeneratePropertyBag]
        internal class TestConcreteB : ITestInterface
        {
            public int B;
        }

        [GeneratePropertyBag]
        internal class ClassWithObjectField
        {
            public object Value;
        }

        [Test]
        public void ToJson_Null_ReturnsAStringThatSaysNull()
        {
            var json = JsonSerialization.ToJson<object>(default);
            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void ToJson_StructWithPrimitives_ReturnsValidJsonString()
        {
            var json = JsonSerialization.ToJson(new TestStruct {A = 10, B = 32});
            Assert.That(UnFormat(json), Is.EqualTo(@"{""A"":10,""B"":32}"));
        }

        [Test]
        public void ToJson_ArrayInt_ReturnsValidJsonString()
        {
            var json = JsonSerialization.ToJson(new[] {1, 2, 3});
            Assert.That(UnFormat(json), Is.EqualTo(@"[1,2,3]"));
        }

        [Test]
        public void ToJson_ListInt_ReturnsValidJsonString()
        {
            var json = JsonSerialization.ToJson(new List<int> {1, 2, 3});
            Assert.That(UnFormat(json), Is.EqualTo(@"[1,2,3]"));
        }

        [Test]
        public void ToJson_HashSetInt_ReturnsValidJsonString()
        {
            var json = JsonSerialization.ToJson(new HashSet<int> {1, 2, 3});
            Assert.That(UnFormat(json), Is.EqualTo(@"[1,2,3]"));
        }

        [Test]
        public void ToJson_Interface_ReturnsValidJsonStringWithTypeInfo()
        {
            var json = JsonSerialization.ToJson<ITestInterface>(new TestConcreteA { A = 42 });
            Debug.Log(UnFormat(json));
            Assert.That(UnFormat(json), Is.EqualTo(@"{""$type"":""Unity.Serialization.Json.Tests.JsonSerializationTests+TestConcreteA, Unity.Serialization.Tests"",""A"":42}"));
        }
        
        [Test]
        public void ToJson_ListInterface_ReturnsValidJsonStringWithTypeInfo()
        {
            var json = JsonSerialization.ToJson(new List<ITestInterface>
            {
                new TestConcreteA { A = 5 },
                new TestConcreteB { B = 6 }
            });
            Assert.That(UnFormat(json), Is.EqualTo(@"[{""$type"":""Unity.Serialization.Json.Tests.JsonSerializationTests+TestConcreteA, Unity.Serialization.Tests"",""A"":5},{""$type"":""Unity.Serialization.Json.Tests.JsonSerializationTests+TestConcreteB, Unity.Serialization.Tests"",""B"":6}]"));
        }

        [Test]
        public void ToJson_ObjectWithBoolValue_ReturnsValidJsonString()
        {
            var json = JsonSerialization.ToJson(new ClassWithObjectField { Value = true });
            Debug.Log(UnFormat(json));
            Assert.That(UnFormat(json), Is.EqualTo(@"{""Value"":true}"));
        }
    }
}