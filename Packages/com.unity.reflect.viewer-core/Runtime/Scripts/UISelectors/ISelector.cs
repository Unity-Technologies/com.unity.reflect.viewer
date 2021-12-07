using System;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface ISelector<TValue>
    {
        TValue getValueByName(string name);
    }

    public interface ISelectorComponent
    {
        IUIContext context { get; }
        string propertyName { get; }
        Type ProperType { get; }
    }
}
