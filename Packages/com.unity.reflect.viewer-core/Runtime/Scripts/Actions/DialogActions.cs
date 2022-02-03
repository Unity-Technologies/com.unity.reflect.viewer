using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class OpenDialogAction: ActionBase
    {
        [Serializable]
        public enum DialogType
        {
            None = 0,
            Account = 1,
            Projects = 2,
            Filters = 3,
            MeasureTool = 4,
            ClippingTool = 5,
            OrbitSelect = 6,
            CameraOptions = 7,
            SceneOptions = 8,
            Sequence = 9,
            SelectTool = 10,
            SunStudy = 11,
            BimInfo = 12,
            LandingScreen = 13,
            NavigationMode = 14,
            // = 15,
            ARCardSelection = 16,
            GizmoMode = 17,
            InfoSelect = 18,
            // = 19,
            CollaborationMenu = 20,
            CollaborationUserList = 21,
            CollaborationUserInfo = 22,
            LoginScreen = 23,
            LinkSharing = 24,
            SceneSettings = 25,
            Timeline = 26,
            SectioningTool = 27,
            ModelAlignment = 28,
            Marker = 29,
            LeftSidebarMore = 30
        }

        public object Data { get; }

        OpenDialogAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (DialogType) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IDialogDataProvider.activeDialog), data);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IDialogDataProvider.activeSubDialog), DialogType.None);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new OpenDialogAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }

    public class OpenSubDialogAction: ActionBase
    {
        public object Data { get; }

        OpenSubDialogAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (OpenDialogAction.DialogType) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IDialogDataProvider.activeSubDialog), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new OpenSubDialogAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }

    public class SetDialogModeAction: ActionBase
    {
        /// <summary>
        /// Defines a global mode for dialog buttons. For example Help Mode, which makes clicking any dialog button open a help dialog.
        /// </summary>
        public enum DialogMode
        {
            Normal,
            Help,
            //Options,
        }

        public object Data { get; }

        SetDialogModeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (DialogMode) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IDialogDataProvider.dialogMode), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetDialogModeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }

    public class CloseAllDialogsAction: ActionBase
    {
        public enum OptionDialogType
        {
            None = 0,
            ProjectOptions = 1,
        }

        public object Data { get; }

        CloseAllDialogsAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IDialogDataProvider.activeDialog), OpenDialogAction.DialogType.None);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IDialogDataProvider.activeSubDialog), OpenDialogAction.DialogType.None);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IDialogDataProvider.activeOptionDialog), OptionDialogType.None);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new CloseAllDialogsAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }
}
