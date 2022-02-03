using UnityEngine.UIElements;

namespace Unity.Properties.Debug
{
    sealed class TypeSelectedEvent<T> : EventBase<TypeSelectedEvent<T>>
    {
        public T Value;
        
        internal static TypeSelectedEvent<T> GetPooled(T value)
        {
            var pooled = EventBase<TypeSelectedEvent<T>>.GetPooled();
            pooled.Value = value;
            return pooled;
        }
    }
}
