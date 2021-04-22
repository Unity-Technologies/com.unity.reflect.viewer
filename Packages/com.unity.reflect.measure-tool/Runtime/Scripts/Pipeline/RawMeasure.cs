namespace UnityEngine.Reflect.MeasureTool
{
    public static class RawMeasure
    {
        public static float GetDistanceBetweenAnchors(IAnchor anc1, IAnchor anc2)
        {
            var origin1 = GetAnchorOrigin(anc1);
            var origin2 = GetAnchorOrigin(anc2);

            return Vector3.Distance(origin1, origin2);
        }


        static Vector3 GetAnchorOrigin(IAnchor anchor)
        {
            switch (anchor.type)
            {
                case AnchorType.Point:
                    return ((PointAnchor)anchor).position;
            }

            return Vector3.zero;
        }
    }
}
