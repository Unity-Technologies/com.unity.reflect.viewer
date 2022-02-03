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
        public unsafe void JsonValidatorStandard_Validate(string json, bool valid, JsonType expected, JsonType actual, int line, int character)
        {
            using (var validator = new JsonValidator(JsonValidationType.Standard, Allocator.TempJob))
            {
                fixed (char* ptr = json)
                {
                    var result = validator.Validate(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    Debug.Log(result);
                    Assert.AreEqual(valid, result.IsValid());
                    Assert.AreEqual(expected, result.ExpectedType);
                    Assert.AreEqual(actual, result.ActualType);
                    Assert.AreEqual(line, result.LineCount);
                    Assert.AreEqual(character, result.CharCount);
                }
            }
        }
        
        // Tests streaming sliced input
        [Test]
        [TestCase(@"{""fo|o"":|t|ru|e}", true)]
        public unsafe void JsonValidatorStandard_Validate_Parts(string parts, bool valid)
        {
            using (var validator = new JsonValidator(JsonValidationType.Standard, Allocator.TempJob))
            {
                foreach (var json in parts.Split('|'))
                {
                    fixed (char* ptr = json)
                    {
                        validator.Validate(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    }
                }

                Assert.AreEqual(true, validator.GetResult().IsValid());
            }
        }

        [Test]
        [TestCase(@"{""test"": 0}", true)]
        [TestCase(@"{""test"": -}", false)]
        [TestCase(@"{""test"": -0}", true)]
        [TestCase(@"{""test"": 1}", true)]
        [TestCase(@"{""test"": -1}", true)]
        [TestCase(@"{""test"": 10.0}", true)]
        [TestCase(@"{""test"": 10.}", false)]
        [TestCase(@"{""test"": 1e5}", true)]
        [TestCase(@"{""test"": -1.0e5}", true)]
        [TestCase(@"{""test"": 1.e5}", false)]
        [TestCase(@"{""test"": 1e5.0}", false)]
        [TestCase(@"{""test"": --42}", false)]
        [TestCase(@"{""test"": -3.814697E-06}", true)]
        public unsafe void JsonValidatorStandard_Validate_Numbers(string json, bool valid)
        {
            using (var validator = new JsonValidator(JsonValidationType.Standard, Allocator.TempJob))
            {
                fixed (char* ptr = json)
                {
                    var result = validator.Validate(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    Debug.Log(result);
                    Assert.AreEqual(valid, result.IsValid());
                }
            }
        }
        
        [Test]
        [TestCase(@"{""test"": NaN}", true)]
        [TestCase(@"{""test"": nan}", true)]
        [TestCase(@"{""test"": n}", false)]
        [TestCase(@"{""test"": na}", false)]
        [TestCase(@"{""test"": naa}", false)]
        [TestCase(@"{""test"": naan}", false)]
        [TestCase(@"{""test"": nann}", false)]
        public unsafe void JsonValidatorStandard_Validate_NaN(string json, bool valid)
        {
            using (var validator = new JsonValidator(JsonValidationType.Standard, Allocator.TempJob))
            {
                fixed (char* ptr = json)
                {
                    var result = validator.Validate(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    Debug.Log(result);
                    Assert.AreEqual(valid, result.IsValid());
                }
            }
        }
        
        [Test]
        [TestCase(@"{""test"": Null}", true)]
        [TestCase(@"{""test"": null}", true)]
        [TestCase(@"{""test"": n}", false)]
        [TestCase(@"{""test"": nu}", false)]
        [TestCase(@"{""test"": nul}", false)]
        [TestCase(@"{""test"": nulll}", false)]
        public unsafe void JsonValidatorStandard_Validate_Null(string json, bool valid)
        {
            using (var validator = new JsonValidator(JsonValidationType.Standard, Allocator.TempJob))
            {
                fixed (char* ptr = json)
                {
                    var result = validator.Validate(new UnsafeBuffer<char>(ptr, json.Length), 0, json.Length);
                    Debug.Log(result);
                    Assert.AreEqual(valid, result.IsValid());
                }
            }
        }
        
        [Test]
        [TestCase(@"{""test"": Infinity}", true)]
        [TestCase(@"{""test"": -Infinity}", true)]
        [TestCase(@"{""test"": inf}", false)]
        [TestCase(@"{""test"": -inf}", false)]
        public unsafe void JsonValidatorStandard_Validate_Infinity(string json, bool valid)
        {
            using (var validator = new JsonValidator(JsonValidationType.Standard, Allocator.TempJob))
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
