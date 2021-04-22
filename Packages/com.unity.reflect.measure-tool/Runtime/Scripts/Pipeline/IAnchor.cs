using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.MeasureTool
{
    public interface IAnchor
    {
        // Reference of the selected object
        // TODO: may be used later to make responsive Anchors
        int objectId { get; }
        AnchorType type { get; }
    }

    public enum AnchorType
    {
        Point = 0,
        Edge = 1,
        Plane = 2,
        Object = 3
    };

    public class PointAnchor : IAnchor
    {
        public int objectId { get; }
        public AnchorType type { get; }
        public Vector3 position { get; }
        public Vector3 normal { get; }

        public PointAnchor(int _objectid, AnchorType _anchorType, Vector3 _position, Vector3 _normal)
        {
            objectId = _objectid;
            type = _anchorType;
            position = _position;
            normal = _normal;
        }
    }

    public class EdgeAnchor : IAnchor
    {
        public int objectId { get; }
        public AnchorType type { get; }
        public List<Vector3> points { get; }

        public EdgeAnchor(int _objectid, AnchorType _anchorType, List<Vector3> _points)
        {
            objectId = _objectid;
            type = _anchorType;
            points = _points;
        }
    }
}
