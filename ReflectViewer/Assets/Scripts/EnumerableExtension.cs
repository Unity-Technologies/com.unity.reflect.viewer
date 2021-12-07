using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Reflect
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// Compares the content of 2 enumerables, no matter the order, and consider null == empty enumerables
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool SafeSequenceEquals<T>(this IEnumerable<T> obj, IEnumerable<T> other)
        {
            if (obj == null)
                return other == null || !other.Any();
            else if (other == null)
                return !obj.Any();
            else
                return obj.SequenceEqual(other);
        }
    }
}
