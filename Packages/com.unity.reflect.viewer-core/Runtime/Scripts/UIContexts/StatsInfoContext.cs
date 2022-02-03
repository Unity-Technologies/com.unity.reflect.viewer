using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IStreamCountDataProvider
    {
        public int addedCount { get; set; }
        public int changedCount { get; set; }
        public int removedCount { get; set; }
    }

    public interface IStatsInfoFPSDataProvider
    {
        public int fpsMax { get; set; }
        public int fpsAvg { get; set; }
        public int fpsMin { get; set; }
    }

    public interface IStatsInfoStreamDataProvider<T>
    {
        public T assetsCountData { get; set; }
        public T instancesCountData { get; set; }
        public T gameObjectsCountData { get; set; }
    }

    public class StatsInfoContext : ContextBase<StatsInfoContext>
    {
        public override List<Type> implementsInterfaces => new List<Type>{typeof(IStatsInfoFPSDataProvider)};
    }
}
