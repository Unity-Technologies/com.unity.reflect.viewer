using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetBimGroupAction : ActionBase
    {
        public object Data { get; }

        SetBimGroupAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IUIStateDataProvider.bimGroup), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetBimGroupAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }

    public class CancelAction : ActionBase
    {
        public object Data { get; }

        CancelAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;


            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IUIStateDataProvider.operationCancelled), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new CancelAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }

    public class SetThemeAction : ActionBase
    {
        public object Data { get; }

        SetThemeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;


            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IUIStateDataProvider.themeName), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetThemeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }

    public class SetDisplayAction : ActionBase
    {
        [Serializable]
        public enum ScreenSizeQualifier
        {
            XSmall,
            Small,
            Medium,
            Large,
            XLarge
        }

        public enum DisplayType
        {
            Desktop,
            Tablet,
            Phone
        }

        public struct SetDisplayData
        {
            public Vector2 screenSize;
            public Vector2 scaledScreenSize;
            public ScreenSizeQualifier screenSizeQualifier;
            public float targetDpi;
            public float dpi;
            public float scaleFactor;
            public DisplayType displayType;
        }

        public object Data { get; }

        SetDisplayAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (SetDisplayData) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            var prefix = nameof(IUIStateDisplayProvider<T>.display) + ".";

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefix + nameof(IDisplayDataProvider.screenSize), data.screenSize);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefix + nameof(IDisplayDataProvider.scaledScreenSize), data.scaledScreenSize);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefix + nameof(IDisplayDataProvider.screenSizeQualifier), data.screenSizeQualifier);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefix + nameof(IDisplayDataProvider.targetDpi), data.targetDpi);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefix + nameof(IDisplayDataProvider.dpi), data.dpi);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefix + nameof(IDisplayDataProvider.scaleFactor), data.scaleFactor);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefix + nameof(IDisplayDataProvider.displayType), data.displayType);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetDisplayAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }
}
