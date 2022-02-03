using System;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public enum JoystickPreference
    {
        RightHanded = 0,
        LeftHanded = 1
    }

    public class SetCameraProjectionTypeAction: ActionBase
    {
        public enum CameraProjectionType
        {
            Perspective = 0,
            Orthographic = 1
        }

        public object Data { get; }

        public SetCameraProjectionTypeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (CameraProjectionType)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ICameraOptionsDataProvider.cameraProjectionType), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetCameraProjectionTypeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == CameraOptionsContext.current;
        }
    }

    public class SetCameraViewTypeAction: ActionBase
    {
        public enum CameraViewType
        {
            Default = 0,
            Top = 1,
            Left = 2,
            Right = 3
        }

        public object Data { get; }

        public SetCameraViewTypeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (CameraViewType)viewerActionData;
            object boxed = stateData;

            if(SetPropertyValue(ref stateData, ref boxed, nameof(ICameraOptionsDataProvider.cameraViewOption.cameraViewType), data))
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetCameraViewTypeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == CameraOptionsContext.current;
        }
    }

    public class SetCameraNumberClickAction: ActionBase
    {
        public object Data { get; }

        public SetCameraNumberClickAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (int)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ICameraOptionsDataProvider.cameraViewOption.numberOfClick), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetCameraNumberClickAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == CameraOptionsContext.current;
        }
    }

    public class SetCameraViewOptionAction: ActionBase
    {
        public object Data { get; }

        public SetCameraViewOptionAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ICameraViewOption)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;


            var prefPropertyName = nameof(ICameraOptionsDataProvider.cameraViewOption);
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                var oldValue = PropertyContainer.GetValue<ICameraViewOption>(ref boxed, prefPropertyName);

                if (oldValue == null || oldValue.cameraViewType != data.cameraViewType)
                {
                    data.numberOfClick = 0;
                }
                else
                {
                    data.numberOfClick = oldValue.numberOfClick + 1;
                }
            }

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ICameraOptionsDataProvider.cameraViewOption), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetCameraViewOptionAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == CameraOptionsContext.current;
        }
    }
}
