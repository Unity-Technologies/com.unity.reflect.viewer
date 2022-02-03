#if UNITY_EDITOR
namespace Unity.Serialization.Json.Adapters
{
    partial class JsonAdapter : IJsonAdapter
        , IJsonAdapter<UnityEditor.GUID>
        , IJsonAdapter<UnityEditor.GlobalObjectId>
    {
        void IJsonAdapter<UnityEditor.GUID>.Serialize(JsonSerializationContext<UnityEditor.GUID> context, UnityEditor.GUID value)
            => context.Writer.WriteValue(value.ToString());

        UnityEditor.GUID IJsonAdapter<UnityEditor.GUID>.Deserialize(JsonDeserializationContext<UnityEditor.GUID> context)
            => UnityEditor.GUID.TryParse(context.SerializedValue.ToString(), out var value) ? value : default;
        
        void IJsonAdapter<UnityEditor.GlobalObjectId>.Serialize(JsonSerializationContext<UnityEditor.GlobalObjectId> context, UnityEditor.GlobalObjectId value)
            => context.Writer.WriteValue(value.ToString());

        UnityEditor.GlobalObjectId IJsonAdapter<UnityEditor.GlobalObjectId>.Deserialize(JsonDeserializationContext<UnityEditor.GlobalObjectId> context)
            => UnityEditor.GlobalObjectId.TryParse(context.SerializedValue.ToString(), out var value) ? value : default;
    }
}
#endif
