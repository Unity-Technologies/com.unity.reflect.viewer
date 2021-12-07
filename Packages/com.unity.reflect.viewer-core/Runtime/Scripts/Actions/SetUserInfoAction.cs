using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetUserInfoAction : ActionBase
    {
        public struct SetUserInfoData
        {
            public string matchmakerId;
            public Vector3 dialogPosition;
        }

        public object Data { get; }

        SetUserInfoAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (SetUserInfoData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            var prefix = nameof(IUIStateDataProvider.SelectedUserData) + ".";

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefix + nameof(IUserInfoDialogDataProvider.matchmakerId), data.matchmakerId);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefix + nameof(IUserInfoDialogDataProvider.dialogPosition), data.dialogPosition);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefix + nameof(IUIStateDataProvider.SelectedUserData), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetUserInfoAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }
}
