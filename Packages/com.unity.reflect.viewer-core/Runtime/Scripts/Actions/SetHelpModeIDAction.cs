using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetHelpModeIDAction : ActionBase
    {
        /// <summary>
        /// Defines a global mode for HelpMode Buttons that don't have dialog/subdialog.
        /// </summary>
        public enum HelpModeEntryID
        {
            None = 0,
            Sync = 1,
            HomeReset = 2,

            // AR Sidebars
            Back = 3,
            Scale = 4,
            Target = 5, // Not used yet
            Ok = 6,
            Cancel = 7,
            ARSelect = 8, // Select button without opening subdialog
            MeasureTool = 9,
            Microphone = 10,
            LinkSharing = 11,
            Projects = 12
        }

        public object Data { get; }

        SetHelpModeIDAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (HelpModeEntryID)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IHelpModeDataProvider.helpModeEntryId), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetHelpModeIDAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }
}
