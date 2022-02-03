using System;
using UnityEngine.Events;

namespace Unity.TouchFramework
{
    public interface IPropertyValue
    {
        Type type { get; }
        object objectValue { get; set; }
        void AddListener(Action eventFunc);
        void RemoveListener(Action eventFunc);
    }

    public interface IPropertyValue<T> : IPropertyValue
    {
        T value { get; }
    }
}
