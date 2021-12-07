using System;
using SharpFlux;
using Unity.Reflect;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetSessionStateAction : ActionBase
    {
        public object Data { get; }

        SetSessionStateAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetSessionStateAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SessionStateContext<UnityUser, LinkPermission>.current;
        }
    }

    public class SetLoginAction : ActionBase
    {
        public object Data { get; }

        SetLoginAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (LoginState)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetLoginAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SessionStateContext<UnityUser, LinkPermission>.current;
        }
    }

    public class SetProjectRoomAction : ActionBase
    {
        public object Data { get; }

        SetProjectRoomAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.rooms), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetProjectRoomAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SessionStateContext<UnityUser, LinkPermission>.current;
        }
    }

    public class RefreshProjectListAction : ActionBase
    {
        public object Data { get; }

        RefreshProjectListAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            var data = (ProjectListState)viewerActionData;
            SetPropertyValue(ref stateData, ref boxed, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.projectListState), data);

            onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new RefreshProjectListAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SessionStateContext<UnityUser, LinkPermission>.current;
        }
    }

    public class SetIsOpenWithLinkSharingAction : ActionBase
    {
        public object Data { get; }

        SetIsOpenWithLinkSharingAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.isOpenWithLinkSharing), viewerActionData);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetIsOpenWithLinkSharingAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SessionStateContext<UnityUser, LinkPermission>.current;
        }
    }

    public class SetCachedLinkTokenAction : ActionBase
    {
        public object Data { get; }

        SetCachedLinkTokenAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.cachedLinkToken), viewerActionData);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetCachedLinkTokenAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SessionStateContext<UnityUser, LinkPermission>.current;
        }
    }

    public class SetPrivateModeAction : ActionBase
    {
        public object Data { get; }

        SetPrivateModeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.isInPrivateMode), viewerActionData);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetPrivateModeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SessionStateContext<UnityUser, LinkPermission>.current;
        }
    }

    public class SetUnityUserAction<TValue> : ActionBase
    {
        public object Data { get; }

        SetUnityUserAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            var hasChanged = false;
            var data = (TValue)boxed;

            // TODO other actions see UIStateManagerActions
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.user), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetUnityUserAction<TValue>(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SessionStateContext<UnityUser, LinkPermission>.current;
        }
    }

    public class SetRoomAction : ActionBase
    {
        public object Data { get; }

        SetRoomAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            var hasChanged = false;
            var data = (IProjectRoom[])boxed;

            // TODO other actions see UIStateManagerActions
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.rooms), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetRoomAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SessionStateContext<UnityUser, LinkPermission>.current;
        }
    }

    public class SetLinkSharePermissionAction : ActionBase
    {
        public object Data { get; }

        SetLinkSharePermissionAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            var hasChanged = false;
            var data = (LinkPermission)viewerActionData;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.linkSharePermission), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
            // TODO other actions see UIStateManagerActions
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetLinkSharePermissionAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SessionStateContext<UnityUser, LinkPermission>.current;
        }
    }
}
