#if UNITY_EDITOR
namespace Unity.Serialization.Binary.Adapters
{
    unsafe partial class BinaryAdapter : IBinaryAdapter
        , IBinaryAdapter<UnityEditor.GUID>
        , IBinaryAdapter<UnityEditor.GlobalObjectId>
    {
        void IBinaryAdapter<UnityEditor.GUID>.Serialize(BinarySerializationContext<UnityEditor.GUID> context, UnityEditor.GUID value)
        {
            context.Writer->Add(value.ToString());
        }

        UnityEditor.GUID IBinaryAdapter<UnityEditor.GUID>.Deserialize(BinaryDeserializationContext<UnityEditor.GUID> context)
        {
            context.Reader->ReadNext(out string str);
            return UnityEditor.GUID.TryParse(str, out var value) ? value : default;
        }

        void IBinaryAdapter<UnityEditor.GlobalObjectId>.Serialize(BinarySerializationContext<UnityEditor.GlobalObjectId> context, UnityEditor.GlobalObjectId value)
        {
            context.Writer->Add(value.ToString());
        }
        
        UnityEditor.GlobalObjectId IBinaryAdapter<UnityEditor.GlobalObjectId>.Deserialize(BinaryDeserializationContext<UnityEditor.GlobalObjectId> context)
        {
            context.Reader->ReadNext(out string str);
            return UnityEditor.GlobalObjectId.TryParse(str, out var value) ? value : default;
        }
    }
}
#endif