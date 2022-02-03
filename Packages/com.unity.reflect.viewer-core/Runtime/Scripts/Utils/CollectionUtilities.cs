using System.Collections;

namespace UnityEngine.Reflect.Viewer.Core
{
    public static class CollectionUtilities
    {
        public static ICollection Copy(ICollection obj)
        {
            return new ArrayList(obj);
        }

        public static bool AreEqual(ICollection collection, ICollection other)
        {
            if (collection == null && other == null)
            {
                return true;
            }

            if (collection == null || other == null)
            {
                return false;
            }

            if (collection.Count != other.Count)
            {
                return false;
            }

            var collectionEnumerator = collection.GetEnumerator();
            var otherEnumerator = other.GetEnumerator();

            var continueLoop = collectionEnumerator.MoveNext() && otherEnumerator.MoveNext();

            while (continueLoop)
            {
                if (!collectionEnumerator.Current.Equals(otherEnumerator.Current))
                {
                    return false;
                }

                continueLoop = collectionEnumerator.MoveNext() && otherEnumerator.MoveNext();
            }

            return true;
        }
    }
}
