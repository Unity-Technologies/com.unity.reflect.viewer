using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface ISunstudyDataProvider
    {
        public int timeOfDay { get; set; }
        public int timeOfYear { get; set; }
        public int utcOffset { get; set; }
        public int latitude { get; set; }
        public int longitude { get; set; }
        public int northAngle { get; set; }
        public float altitude { get; set; }
        public float azimuth { get; set; }
        public bool isStaticMode { get; set; }
    }
    public class SunStudyContext : ContextBase<SunStudyContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(ISunstudyDataProvider)};
    }
}
