#if !NET_DOTS
using System.IO;
using Unity.Collections;
using Unity.Properties;
using Unity.Properties.Internal;

namespace Unity.Serialization.Json
{
    public static partial class JsonSerialization
    {
        /// <summary>
        /// Serializes the given object to a json file at the specified path.
        /// </summary>
        /// <param name="file">The file to write to.</param>
        /// <param name="container">The object to serialize.</param>
        /// <param name="parameters">The parameters to use when writing.</param>
        /// <typeparam name="T">The type to serialize.</typeparam>
        public static void ToJson<T>(FileInfo file, T container, JsonSerializationParameters parameters = default)
        {
            File.WriteAllText(file.FullName, ToJson(container, parameters));
        }

        /// <summary>
        /// Writes a property container to a json string.
        /// </summary>
        /// <param name="value">The container to write.</param>
        /// <param name="parameters">The parameters to use when writing.</param>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <returns>A json string.</returns>
        public static string ToJson<T>(T value, JsonSerializationParameters parameters = default)
        {
            using (var writer = new JsonWriter(parameters.InitialCapacity, Allocator.Temp, new JsonWriterOptions {Minified = parameters.Minified, Simplified = parameters.Simplified}))
            {
                ToJson(writer, value, parameters);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Writes a property container the specified buffer.
        /// </summary>
        /// <param name="writer">The buffer to write the object to.</param>
        /// <param name="value">The container to write.</param>
        /// <param name="parameters">The parameters to use when writing.</param>
        /// <typeparam name="T">The type to serialize.</typeparam>
        public static void ToJson<T>(JsonWriter writer, T value, JsonSerializationParameters parameters = default)
        {
            var container = new PropertyWrapper<T>(value);
            
            var serializedReferences = default(SerializedReferences);
            var state = parameters.State ?? (parameters.RequiresThreadSafety ? new JsonSerializationState() : GetSharedState());

            if (!parameters.DisableSerializedReferences)
            {
                serializedReferences = state.GetSerializedReferences();
                var serializedReferenceVisitor = state.GetSerializedReferenceVisitor();
                serializedReferenceVisitor.SetSerializedReference(serializedReferences);
                PropertyContainer.Accept(serializedReferenceVisitor, ref container);
            }

            var visitor = state.GetJsonPropertyWriter();
            
            visitor.SetWriter(writer);
            visitor.SetSerializedType(parameters.SerializedType);
            visitor.SetDisableRootAdapters(parameters.DisableRootAdapters);
            visitor.SetGlobalAdapters(GetGlobalAdapters());
            visitor.SetUserDefinedAdapters(parameters.UserDefinedAdapters);
            visitor.SetGlobalMigrations(GetGlobalMigrations());
            visitor.SetUserDefinedMigration(parameters.UserDefinedMigrations);
            visitor.SetSerializedReferences(serializedReferences);
            
            using (visitor.Lock()) PropertyContainer.Accept(visitor, ref container);
        }
    }
}
#endif