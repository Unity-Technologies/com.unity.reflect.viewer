#if !NET_DOTS
using System.IO;

namespace Unity.Serialization
{
    static class DirectoryInfoExtensions
    {
        internal static string GetRelativePath(this DirectoryInfo directoryInfo)
        {
            var relativePath = new DirectoryInfo(".").FullName.ToForwardSlash();
            var path = directoryInfo.FullName.ToForwardSlash();
            return path.StartsWith(relativePath) ? path.Substring(relativePath.Length).TrimStart('/') : path;
        }
    }

    static class FileInfoExtensions
    {
        internal static string GetRelativePath(this FileInfo fileInfo)
        {
            var relativePath = new DirectoryInfo(".").FullName.ToForwardSlash();
            var path = fileInfo.FullName.ToForwardSlash();
            return path.StartsWith(relativePath) ? path.Substring(relativePath.Length).TrimStart('/') : path;
        }
    }
    
    static class StringExtensions
    {
        internal static string ToForwardSlash(this string value)
        {
            return value.Replace('\\', '/');
        }
    }
}
#endif