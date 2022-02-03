using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IQualitySettingsDataProvider
    {
        public int fpsThresholdQualityDecrease { get; set; }
        public int fpsThresholdQualityIncrease { get; set; }
        public int qualityLevel { get; set; }
        public bool isAutomatic { get; set; }
        public float lastQualityChangeTimestamp { get; set; }
    }

    public interface IApplicationSettingsDataProvider<T>
    {
        public T qualityStateData { get; set; }
    }

    public class ApplicationSettingsContext : ContextBase<ApplicationSettingsContext>
    {
        public override List<Type> implementsInterfaces { get; }
    }
}
