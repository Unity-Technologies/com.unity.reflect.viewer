#if !UNITY_DOTSPLAYER
namespace Unity.Serialization.Binary.Adapters
{
    unsafe partial class BinaryAdapter :
        Contravariant.IBinaryAdapter<UnityEngine.Object>
    {
        void Contravariant.IBinaryAdapter<UnityEngine.Object>.Serialize(IBinarySerializationContext context, UnityEngine.Object value)
        {
#if UNITY_EDITOR
            var id = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(value).ToString();
            context.Writer->Add(id);
#endif
        }

        object Contravariant.IBinaryAdapter<UnityEngine.Object>.Deserialize(IBinaryDeserializationContext context)
        {
#if UNITY_EDITOR
            context.Reader->ReadNext(out string value);
            
            if (UnityEditor.GlobalObjectId.TryParse(value, out var id))
            {
                return UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
            }
#endif
            return null;
        }
    }
}
#endif