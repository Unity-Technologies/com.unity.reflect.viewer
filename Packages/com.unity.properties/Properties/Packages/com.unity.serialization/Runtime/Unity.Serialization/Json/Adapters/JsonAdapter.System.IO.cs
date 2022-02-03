#if !NET_DOTS
using System.IO;

namespace Unity.Serialization.Json.Adapters
{
    partial class JsonAdapter :
        IJsonAdapter<DirectoryInfo>,
        IJsonAdapter<FileInfo>
    {
        void IJsonAdapter<DirectoryInfo>.Serialize(JsonSerializationContext<DirectoryInfo> context, DirectoryInfo value)
        {
            if (null == value) 
                context.Writer.WriteNull();
            else 
                context.Writer.WriteValue(value.GetRelativePath());
        }

        DirectoryInfo IJsonAdapter<DirectoryInfo>.Deserialize(JsonDeserializationContext<DirectoryInfo> context)
        {
            return context.SerializedValue.AsStringView().Equals("null") ? null : new DirectoryInfo(context.SerializedValue.ToString());
        }

        void IJsonAdapter<FileInfo>.Serialize(JsonSerializationContext<FileInfo> context, FileInfo value)
        {
            if (null == value) 
                context.Writer.WriteNull();
            else 
                context.Writer.WriteValue(value.GetRelativePath());
        }

        FileInfo IJsonAdapter<FileInfo>.Deserialize(JsonDeserializationContext<FileInfo> context)
        {
            return context.SerializedValue.AsStringView().Equals("null") ? null : new FileInfo(context.SerializedValue.ToString());
        }
    }
}
#endif