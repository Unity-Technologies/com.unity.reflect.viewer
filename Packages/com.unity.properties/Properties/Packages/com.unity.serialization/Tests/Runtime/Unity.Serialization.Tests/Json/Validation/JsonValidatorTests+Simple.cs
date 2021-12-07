using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    partial class JsonValidatorTests
    {
        [Test]
        [TestCase("{}", true, JsonType.EOF, JsonType.EOF, 1, 3)]
        [TestCase("\n{\n \t}", true, JsonType.EOF, JsonType.EOF, 3, 4)]
        [TestCase("{", false, JsonType.EndObject | JsonType.String, JsonType.EOF, 1, 2)]
        [TestCase("a = 1234", true, JsonType.Value | JsonType.EOF, JsonType.EOF, 1, 9)]
        [TestCase("a = { b : q }", true, JsonType.EOF, JsonType.EOF, 1, 14)]
        [TestCase(@"""a""", true, JsonType.MemberSeparator | JsonType.EOF, JsonType.EOF, 1, 4)]
        [TestCase(@"a b", false, JsonType.MemberSeparator, JsonType.Value, 1, 3)]
        [TestCase(@"a : ""b:a""", true,  JsonType.ValueSeparator | JsonType.String | JsonType.EOF, JsonType.EOF, 1, 10)]
        [TestCase(@"{""test"": -3.814697E-06}", true,  JsonType.EOF, JsonType.EOF, 1, 24)]
        public unsafe void JsonValidatorSimple_Validate(string json, bool valid, JsonType expected, JsonType actual, int line, int character)
        {
            using (var validator = new JsonValidator(JsonValidationType.Simple, Allocator.TempJob))
            {
                fixed (char* ptr = json)
                {
                    var result = validator.Validate(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    Debug.Log(result.ToString());
                    Assert.AreEqual(valid, result.IsValid());
                    Assert.AreEqual(expected, result.ExpectedType);
                    Assert.AreEqual(actual, result.ActualType);
                    Assert.AreEqual(line, result.LineCount);
                    Assert.AreEqual(character, result.CharCount);
                }
            }
        }
        
        [Test]
        [TestCase(@"a = 12|34", true)]
        public unsafe void JsonValidatorSimple_Validate_Parts(string parts, bool valid)
        {
            using (var validator = new JsonValidator(JsonValidationType.Simple, Allocator.TempJob))
            {
                foreach (var json in parts.Split('|'))
                {
                    fixed (char* ptr = json)
                    {
                        validator.Validate(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    }
                }

                var result = validator.GetResult();
                Debug.Log(result.ToString());
                Assert.AreEqual(true, result.IsValid());
            }
        }
        
        [Test]
        [TestCase(@"{""test"": 0}", true)]
        [TestCase(@"{""test"": -}", true)]
        [TestCase(@"{""test"": -0}", true)]
        [TestCase(@"{""test"": 1}", true)]
        [TestCase(@"{""test"": -1}", true)]
        [TestCase(@"{""test"": 10.0}", true)]
        [TestCase(@"{""test"": 10.}", true)]
        [TestCase(@"{""test"": 1e5}", true)]
        [TestCase(@"{""test"": -1.0e5}", true)]
        [TestCase(@"{""test"": 1.e5}", true)]
        [TestCase(@"{""test"": 1e5.0}", true)]
        [TestCase(@"{""test"": --42}", true)]
        [TestCase(@"{""test"": -3.814697E-06}", true)]
        public unsafe void JsonValidatorSimple_Validate_Numbers(string json, bool valid)
        {
            using (var validator = new JsonValidator(JsonValidationType.Simple, Allocator.TempJob))
            {
                fixed (char* ptr = json)
                {
                    var result = validator.Validate(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    Debug.Log(result);
                    Assert.AreEqual(valid, result.IsValid());
                }
            }
        }
    }
}