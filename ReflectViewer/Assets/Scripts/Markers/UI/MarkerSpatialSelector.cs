using System;
using System.Collections.Generic;
using Unity.Reflect.Markers.UI;
using UnityEngine;
using UnityEngine.Reflect.Viewer;

namespace Unity.Reflect.Viewer.UI
{
    public class MarkerSpatialSelector : SpatialSelector
    {
        protected override void PostRaycast(List<Tuple<GameObject, RaycastHit>> results)
        {
            MarkerAnchorSelectionRaycast.PostRaycast(results);

            CleanCache();
        }
    }
}
