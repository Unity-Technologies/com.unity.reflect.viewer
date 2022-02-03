#if !NET_DOTS
using System;
using System.Collections.Generic;
using Unity.Serialization.Json.Adapters;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// This class is used to store state between multiple serialization calls.
    /// By passing this to <see cref="JsonSerializationParameters"/> will allow visitors and serialized references to be re-used.
    /// </summary>
    class JsonSerializationState
    {
        JsonPropertyWriter m_JsonPropertyWriter;
        JsonPropertyReader m_JsonPropertyReader;
        SerializedReferences m_SerializedReferences;
        SerializedReferenceVisitor m_SerializedReferenceVisitor;
        List<DeserializationEvent> m_DeserializationEvents;

        /// <summary>
        /// Returns true if the given state is in use by either serialization or de-serialization.
        /// </summary>
        internal bool IsLocked => (m_JsonPropertyWriter != null && m_JsonPropertyWriter.IsLocked) || (m_JsonPropertyReader != null && m_JsonPropertyReader.IsLocked);
        
        /// <summary>
        /// Gets the shared <see cref="JsonPropertyWriter"/>.
        /// </summary>
        /// <returns>The <see cref="JsonPropertyWriter"/>.</returns>
        internal JsonPropertyWriter GetJsonPropertyWriter()
        { 
            if (null != m_JsonPropertyWriter) 
                return m_JsonPropertyWriter.IsLocked ? new JsonPropertyWriter() : m_JsonPropertyWriter;
            
            m_JsonPropertyWriter = new JsonPropertyWriter();
            return m_JsonPropertyWriter;
        }
        
        /// <summary>
        /// Gets the shared <see cref="JsonPropertyReader"/>.
        /// </summary>
        /// <returns>The <see cref="JsonPropertyReader"/>.</returns>
        internal JsonPropertyReader GetJsonPropertyReader()
        {
            if (null != m_JsonPropertyReader) 
                return m_JsonPropertyReader.IsLocked ? new JsonPropertyReader() : m_JsonPropertyReader;
            
            m_JsonPropertyReader = new JsonPropertyReader();
            return m_JsonPropertyReader;
        }

        /// <summary>
        /// Gets the shared <see cref="SerializedReferences"/>.
        /// </summary>
        /// <returns>The <see cref="SerializedReferences"/>.</returns>
        internal SerializedReferences GetSerializedReferences()
            => m_SerializedReferences ?? (m_SerializedReferences = new SerializedReferences());
        
        /// <summary>
        /// Gets the shared <see cref="SerializedReferenceVisitor"/>.
        /// </summary>
        /// <returns>The <see cref="SerializedReferenceVisitor"/>.</returns>
        internal SerializedReferenceVisitor GetSerializedReferenceVisitor()
            => m_SerializedReferenceVisitor ?? (m_SerializedReferenceVisitor = new SerializedReferenceVisitor());

        /// <summary>
        /// Gets the shared <see cref="List{DeserializationEvent}"/>.
        /// </summary>
        /// <returns>The <see cref="List{DeserializationEvent}"/>.</returns>
        internal List<DeserializationEvent> GetDeserializationEvents()
            => m_DeserializationEvents ?? (m_DeserializationEvents = new List<DeserializationEvent>());

        /// <summary>
        /// Clears the serialized references state.
        /// </summary>
        internal void ClearSerializedReferences()
            => m_SerializedReferences?.Clear();
        
        /// <summary>
        /// Clears the deserialization events state.
        /// </summary>
        internal void ClearDeserializationEvents()
            => m_DeserializationEvents?.Clear();
    }
    
    /// <summary>
    /// Custom parameters to use for json serialization or deserialization.
    /// </summary>
    public struct JsonSerializationParameters
    {
        /// <summary>
        /// By default, a polymorphic root type will have it's assembly qualified type name written to the output in the "$type" field.
        /// Use this parameter to provide a known root type at both serialize and deserialize time to avoid writing this information OR
        /// if this information is missing from the json string.
        /// </summary>
        public Type SerializedType { get; set; }
        
        /// <summary>
        /// By default, adapters are evaluated for root objects. Use this to change the default behaviour.
        /// </summary>
        public bool DisableRootAdapters { get; set; }
        
        /// <summary>
        /// Provide a custom set of adapters for the serialization. These adapters will be evaluated first before any global or built in adapters.
        /// </summary>
        /// <remarks>
        /// To register a global adapter see <see cref="JsonSerialization.AddGlobalAdapter"/>.
        /// </remarks>
        public List<IJsonAdapter> UserDefinedAdapters { get; set; }
        
        /// <summary>
        /// Provide a custom set of migration adapters for the serialization. These adapters will be evaluated first before any global or built in adapters.
        /// </summary>
        /// <remarks>
        /// To register a global migration see <see cref="JsonSerialization.AddGlobalMigration"/>.
        /// </remarks>
        public List<IJsonMigration> UserDefinedMigrations { get; set; }

        /// <summary>
        /// Stores user data which is passed to the migration adapter context.
        /// </summary>
        public object UserData { get; set; }

        /// <summary>
        /// The initial capacity (in characters) to use for the internal writer if none is provided. The default value is 32.
        /// </summary>
        public int InitialCapacity { get; set; }

        /// <summary>
        /// This parameter indicates if the serialization should be thread safe. The default value is false.
        /// </summary>
        /// <remarks>
        /// Setting this to true will cause managed allocations for the internal visitor.
        /// </remarks>
        public bool RequiresThreadSafety { get; set; }
        
        /// <summary>
        /// By default, references between objects are serialized. Use this to always write a copy of the object to the output.
        /// </summary>
        public bool DisableSerializedReferences { get; set; }
        
        /// <summary>
        /// Use this parameter to write minified json.
        /// </summary>
        public bool Minified { get; set; }
        
        /// <summary>
        /// Use this parameter to write simplified json.
        /// </summary>
        public bool Simplified { get; set; }
        
        /// <summary>
        /// Sets the state object for serialization. This can be used to share resources across multiple calls to serialize and deserialize.
        /// </summary>
        internal JsonSerializationState State { get; set; }
    }
    
    /// <summary>
    /// High level API for serializing or deserializing json data from string, file or stream.
    /// </summary>
    public partial class JsonSerialization
    {
        static readonly List<IJsonAdapter> s_Adapters = new List<IJsonAdapter>();
        static readonly List<IJsonMigration> s_Migrations = new List<IJsonMigration>();
        static readonly JsonSerializationState s_SharedState = new JsonSerializationState();
        
        static JsonSerializationState GetSharedState()
        {
            // The current state is in use by the current stack. We must return a new instance to avoid trashing the serialized references and deserialization events.
            if (s_SharedState.IsLocked)
                return new JsonSerializationState();
            
            s_SharedState.ClearSerializedReferences();
            s_SharedState.ClearDeserializationEvents();
            return s_SharedState;
        }

        /// <summary>
        /// Adds the specified <see cref="IJsonAdapter"/> to the set of global adapters. This is be included by default in all JsonSerialization calls.
        /// </summary>
        /// <param name="adapter">The adapter to add.</param>
        /// <exception cref="ArgumentException">The given adapter is already registered.</exception>
        public static void AddGlobalAdapter(IJsonAdapter adapter)
        {
            if (s_Adapters.Contains(adapter))
                throw new ArgumentException("IJsonAdapter has already been registered.");
            
            s_Adapters.Add(adapter);
        }
        
        /// <summary>
        /// Removes the specified <see cref="IJsonAdapter"/> from the set of global adapters. 
        /// </summary>
        /// <param name="adapter">The adapter to remove.</param>
        /// <exception cref="ArgumentException">The given adapter has not been registered.</exception>
        public static void RemoveGlobalAdapter(IJsonAdapter adapter)
        {
            if (!s_Adapters.Contains(adapter))
                throw new ArgumentException("IJsonAdapter has not been registered.");
            
            s_Adapters.Remove(adapter);
        }
        
        /// <summary>
        /// Adds the specified <see cref="IJsonMigration"/> to the set of global adapters. This is be included by default in all JsonSerialization calls.
        /// </summary>
        /// <param name="migration">The migration to add.</param>
        /// <exception cref="ArgumentException">The given migration is already registered.</exception>
        public static void AddGlobalMigration(IJsonMigration migration)
        {
            if (s_Migrations.Contains(migration))
                throw new ArgumentException("IJsonMigration has already been registered.");
            
            s_Migrations.Add(migration);
        }
        
        /// <summary>
        /// Removes the specified <see cref="IJsonAdapter"/> from the set of global adapters. 
        /// </summary>
        /// <param name="migration">The migration to remove.</param>
        /// <exception cref="ArgumentException">The given migration has not been registered.</exception>
        public static void RemoveGlobalMigration(IJsonMigration migration)
        {
            if (!s_Migrations.Contains(migration))
                throw new ArgumentException("IJsonMigration has not been registered.");
            
            s_Migrations.Add(migration);
        }

        /// <summary>
        /// Returns the currently registered set of global adapters.
        /// </summary>
        /// <returns>The internal list of global adapters.</returns>
        static List<IJsonAdapter> GetGlobalAdapters() => s_Adapters;
        
        /// <summary>
        /// Returns the currently registered set of global migration.
        /// </summary>
        /// <returns>The internal list of global migration.</returns>
        static List<IJsonMigration> GetGlobalMigrations() => s_Migrations;
    }
}
#endif