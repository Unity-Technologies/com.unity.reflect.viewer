using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.MeasureTool
{
    public static class RawMeasure
    {
        public static float GetDistanceBetweenAnchors(SelectObjectMeasureToolAction.IAnchor anc1, SelectObjectMeasureToolAction.IAnchor anc2)
        {
            var origin1 = GetAnchorOrigin(anc1);
            var origin2 = GetAnchorOrigin(anc2);

            return Vector3.Distance(origin1, origin2);
        }


        static Vector3 GetAnchorOrigin(SelectObjectMeasureToolAction.IAnchor anchor)
        {
            switch (anchor.type)
            {
                case ToggleMeasureToolAction.AnchorType.Point:
                    return ((PointAnchor)anchor).position;
            }

            return Vector3.zero;
        }
    }
}
