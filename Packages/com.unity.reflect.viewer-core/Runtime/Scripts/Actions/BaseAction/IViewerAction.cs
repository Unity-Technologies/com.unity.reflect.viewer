using System;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IViewerAction
    {
        void ApplyPayload<T>(object viewerActionData, ref T stateData, Action changedFunc);
        bool RequiresContext(IUIContext context, object viewerActionData);
    }
}
