using NUnit.Framework;
using UnityEngine;

namespace Unity.Serialization.Json.Tests
{
    partial class JsonSerializationTests
    {
        class TestUnityObject : ScriptableObject
        {
            public int Int32Value;
        }
        
        [Test]
        public void ToJson_ScriptableObjectWithDisableRootAdapters_DoesNotThrow()
        {
            var src = ScriptableObject.CreateInstance<TestUnityObject>();
            var dst = ScriptableObject.CreateInstance<TestUnityObject>();

            src.Int32Value = 100;

            try
            {
                var parameters = new JsonSerializationParameters
                {
                    DisableRootAdapters = true
                };
                
                var json = JsonSerialization.ToJson(src, parameters);
                Assert.That(UnFormat(json), Is.EqualTo("{\"Int32Value\":100}"));
                
                JsonSerialization.FromJsonOverride(json, ref dst, parameters);
                Assert.That(dst.Int32Value, Is.EqualTo(src.Int32Value));
                
                /* @TODO uncomment when TypeConstruction lands
                var clone = JsonSerialization.FromJson<TestUnityObject>(json, parameters);
                Assert.That(clone.Int32Value, Is.EqualTo(src.Int32Value));
                */
            }
            finally
            {
                Object.DestroyImmediate(src);
            }
        }
    }
}