using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetLoginSettingActions<TValue>: ActionBase
    {
        public object Data { get; }

        SetLoginSettingActions(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ILoginSettingDataProvider.environmentInfo), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetLoginSettingActions<TValue>(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == LoginSettingContext<TValue>.current;
        }
    }

    public class DeleteCloudEnvironmentSetting<TValue>: ActionBase
    {
        public object Data { get; }

        DeleteCloudEnvironmentSetting(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ILoginSettingDataProvider.deleteCloudEnvironmentSetting), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new DeleteCloudEnvironmentSetting<TValue>(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == LoginSettingContext<TValue>.current;
        }
    }
}
