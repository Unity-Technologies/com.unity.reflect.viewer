using System.Text.RegularExpressions;
using Unity.Serialization.Tests;

namespace Unity.Serialization.Json.Tests
{
    partial class JsonSerializationTests : SerializationTestFixture
    {
        /// <summary>
        /// This is to say if the backend supports serializing reference to `UnityEngine.Object`
        /// when stored in a polymorphic field that does NOT inherit from `UnityEngine.Object.
        ///
        /// i.e.
        /// 
        /// struct Test
        /// {
        ///     IThing untypedReference = texture2DReference;
        /// }
        ///
        /// Currently we can do not support this type of structure in JSON.
        /// </summary>
        protected override bool SupportsPolymorphicUnityObjectReferences => false;

        static string UnFormat(string json)
        {
            return Regex.Replace(json, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");
        }

        protected override T SerializeAndDeserialize<T>(T value, CommonSerializationParameters parameters = default)
        {
            var jsonSerializationParameters = new JsonSerializationParameters
            {
                DisableSerializedReferences = parameters.DisableSerializedReferences
            };
            
            var json = JsonSerialization.ToJson(value, jsonSerializationParameters);
            return JsonSerialization.FromJson<T>(json, jsonSerializationParameters);
        }
    }
}