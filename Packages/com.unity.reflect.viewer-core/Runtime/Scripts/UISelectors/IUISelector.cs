using System;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IUISelector: IDisposable
    {
        object GetValue();
    }

    public interface IUISelector<TValue>: IUISelector
    {
        new TValue GetValue();
    }
}
