using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IDebugOptionDataProvider
    {
        public bool gesturesTrackingEnabled { get; set; }
        public bool ARAxisTrackingEnabled { get; set; }
        public Vector3 spatialPriorityWeights { get; set; }
        public bool useDebugBoundingBoxMaterials { get; set; }
        public bool useCulling { get; set; }
        public bool useSpatialManifest { get; set; }
        public bool useHlods { get; set; }
        public int hlodDelayMode { get; set; }
        public int hlodPrioritizer { get; set; }
        public int targetFps { get; set; }
        public bool showActorDebug { get; set; }
    }

    public class DebugOptionContext : ContextBase<DebugOptionContext>
    {
        public override List<Type> implementsInterfaces { get; }
    }
}
