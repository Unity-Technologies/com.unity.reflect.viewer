#if !NET_DOTS
using System.IO;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Serialization.Binary.Adapters
{
    unsafe partial class BinaryAdapter :
        IBinaryAdapter<DirectoryInfo>,
        IBinaryAdapter<FileInfo>
    {
        void IBinaryAdapter<DirectoryInfo>.Serialize(BinarySerializationContext<DirectoryInfo> context, DirectoryInfo value)
        {
            if (null == value) 
                context.Writer->Add("null");
            else 
                context.Writer->Add(value.GetRelativePath());
        }

        DirectoryInfo IBinaryAdapter<DirectoryInfo>.Deserialize(BinaryDeserializationContext<DirectoryInfo> context)
        {
            context.Reader->ReadNext(out string str);
            return str.Equals("null") ? null : new DirectoryInfo(str);
        }

        void IBinaryAdapter<FileInfo>.Serialize(BinarySerializationContext<FileInfo> context, FileInfo value)
        {
            if (null == value) 
                context.Writer->Add("null");
            else 
                context.Writer->Add(value.GetRelativePath());
        }

        FileInfo IBinaryAdapter<FileInfo>.Deserialize(BinaryDeserializationContext<FileInfo> context)
        {
            context.Reader->ReadNext(out string str);
            return str.Equals("null") ? null : new FileInfo(str);
        }
    }
}
#endif