using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetQualitySettingsAction: ActionBase
    {
        public struct SetQualitySettingsData
        {
            public int fpsThresholdQualityDecrease;
            public int fpsThresholdQualityIncrease;
            public int qualityLevel;
            public bool isAutomatic;
            public float lastQualityChangeTimestamp;

            public static readonly SetQualitySettingsData defaultData = new SetQualitySettingsData()
            {
                fpsThresholdQualityDecrease = 15,
                fpsThresholdQualityIncrease = 60,
                qualityLevel = QualitySettings.GetQualityLevel(),
                isAutomatic = true,
                lastQualityChangeTimestamp = -1
            };
        }

        public object Data { get; }

        SetQualitySettingsAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (SetQualitySettingsData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            var prefixName = nameof(IApplicationSettingsDataProvider<T>.qualityStateData) + ".";

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefixName + nameof(IQualitySettingsDataProvider.fpsThresholdQualityDecrease), data.fpsThresholdQualityDecrease);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefixName + nameof(IQualitySettingsDataProvider.fpsThresholdQualityIncrease), data.fpsThresholdQualityIncrease);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefixName + nameof(IQualitySettingsDataProvider.qualityLevel), data.qualityLevel);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefixName + nameof(IQualitySettingsDataProvider.isAutomatic), data.isAutomatic);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefixName + nameof(IQualitySettingsDataProvider.lastQualityChangeTimestamp), data.lastQualityChangeTimestamp);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetQualitySettingsAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ApplicationSettingsContext.current;
        }
    }
}
