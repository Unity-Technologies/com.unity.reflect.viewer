using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Unity.Serialization.Json.Tests
{
    partial class JsonSerializationTests
    {
        class Container
        {
            public object Value;
        }
        
        [Test]
        public void ToJson_ListWithMetadata_IsFormattedCorrectly()
        {
            var json = JsonSerialization.ToJson(new Container
            {
                Value = new List<object>
                {
                    1, 2, 3, 4, 5, 6
                }
            });
            
            const string expected = @"{
    ""Value"": {
        ""$type"": ""System.Collections.Generic.List`1[System.Object], mscorlib"",
        ""$elements"": [
            1,
            2,
            3,
            4,
            5,
            6
        ]
    }
}";
            Assert.That(json , Is.EqualTo(expected.Replace("\r\n", "\n").Replace("\"\"", "\"")));
        }
    }
}