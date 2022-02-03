using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    sealed class JsonObjectTests
    {
        const string JsonWithAllDataTypes = @"{
    ""hello"": ""world"",
    ""nested"": {
        ""num_value"": 123.0
    },
    ""collection"": [
        1, 2, 3, {
            ""bool_value"": true
        }
    ],
    ""some_secret"": ""!!secret!!"",
    ""remove_me"": ""!!removed!!""
}";
        const string JsonWithNestedArray = @"{
    ""collection"": [
        1, 2, 3, [4,5,6]
    ]
}";        
        static string UnFormat(string json)
        {
            return Regex.Replace(json, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");
        }

        [Test]
        public void JsonObject_Deserialize_ReturnsJsonObjectWithCorrectData()
        {
            var obj = JsonSerialization.FromJson<JsonObject>(JsonWithAllDataTypes);
            Assert.That(obj["hello"], Is.EqualTo("world"));
            Assert.That((obj["nested"] as JsonObject)["num_value"], Is.EqualTo(123.0f));
            Assert.That((obj["collection"] as JsonArray)[2], Is.EqualTo(3));
            Assert.That(((obj["collection"] as JsonArray)[3] as JsonObject)["bool_value"], Is.EqualTo(true));
        }
        
        [Test]
        public void JsonObject_DeserializeMutateAndSerialize_ReturnsCorrectJsonString()
        {
            var obj = JsonSerialization.FromJson<JsonObject>(JsonWithAllDataTypes);

            obj["hello"] = "!!replaced!!";
            obj.Remove("remove_me");

            var json = JsonSerialization.ToJson(obj);
            
            Assert.That(json, Does.Contain("!!replaced!!"));
            Assert.That(json, Does.Contain("!!secret!!"));
            Assert.That(json, Does.Not.Contain("!!removed!!"));
        }
        
        [Test]
        public void JsonObject_DeserializeJsonWithNestedArray_ReturnsJsonObjectWithCorrectData()
        {
            var obj = JsonSerialization.FromJson<JsonObject>(JsonWithNestedArray);
            Assert.That(((obj["collection"]as JsonArray)[3] as JsonArray)[2], Is.EqualTo(6));
            var json = JsonSerialization.ToJson(obj);
            Assert.That(UnFormat(json), Is.EqualTo(UnFormat(JsonWithNestedArray)));
        }
    }
}