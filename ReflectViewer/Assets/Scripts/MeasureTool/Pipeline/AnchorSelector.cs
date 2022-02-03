using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.MeasureTool
{
    public class AnchorSelector : SpatialSelector
    {
        public ToggleMeasureToolAction.AnchorType CurrentAnchorTypeSelection { get; set; }

        protected override void PostRaycast(List<Tuple<GameObject, RaycastHit>> results)
        {
            AnchorSelectionRaycast.PostRaycast(results, CurrentAnchorTypeSelection);

            CleanCache();
        }
    }
}
