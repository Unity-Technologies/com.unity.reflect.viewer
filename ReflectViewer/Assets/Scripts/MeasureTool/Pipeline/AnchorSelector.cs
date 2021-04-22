using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer;

namespace UnityEngine.Reflect.MeasureTool
{
    public class AnchorSelector : SpatialSelector
    {
        public AnchorType CurrentAnchorTypeSelection { get; set; }

        protected override void PostRaycast(List<Tuple<GameObject, RaycastHit>> results)
        {
            AnchorSelectionRaycast.PostRaycast(results, CurrentAnchorTypeSelection);

            foreach (var c in m_ColliderCache.Values)
                Object.Destroy(c);

            m_ColliderCache.Clear();
        }
    }
}
