using System;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.MeasureTool
{
    public class PointAnchor : SelectObjectMeasureToolAction.IAnchor
    {
        public int objectId { get; }
        public ToggleMeasureToolAction.AnchorType type { get; }
        public Vector3 position { get; }
        public Vector3 normal { get; }

        public PointAnchor(int _objectid, ToggleMeasureToolAction.AnchorType _anchorType, Vector3 _position, Vector3 _normal)
        {
            objectId = _objectid;
            type = _anchorType;
            position = _position;
            normal = _normal;
        }
    }
}
