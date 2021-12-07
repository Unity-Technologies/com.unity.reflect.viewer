using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetStatsInfoAction : ActionBase
    {
        public struct SetStatsInfoData
        {
            public int fpsMax;
            public int fpsAvg;
            public int fpsMin;
        }

        public object Data { get; }

        SetStatsInfoAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (SetStatsInfoData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IStatsInfoFPSDataProvider.fpsAvg), data.fpsAvg);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IStatsInfoFPSDataProvider.fpsMax), data.fpsMax);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IStatsInfoFPSDataProvider.fpsMin), data.fpsMin);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetStatsInfoAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == StatsInfoContext.current;
        }
    }
}
