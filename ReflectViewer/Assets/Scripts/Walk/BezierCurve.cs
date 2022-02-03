using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Reflect.Viewer
{
    public class BezierCurve : MonoBehaviour
    {
        public Vector3 StartPosition;
        public Vector3 startControl;
        public Vector3 endControl;
        public float step = 4;
        public LineRenderer bezierLine;

        List<Vector3> pointsList;

        /// Returns point at time 't' (between 0 and 1)  along bezier curve defined by 4 points (a1, c1, a2, c2)
        public Vector3 EvaluateCurve(Vector3 a1, Vector3 c1, Vector3 c2, Vector3 a2, float t)
        {
            t = Mathf.Clamp01(t);
            return (1 - t) * (1 - t) * (1 - t) * a1 + 3 * (1 - t) * (1 - t) * t * c1 + 3 * (1 - t) * t * t * c2 + t * t * t * a2;
        }

        void Start()
        {
            pointsList = new List<Vector3>();
            bezierLine = GetComponent<LineRenderer>();
            StartPosition = Vector3.zero;
        }

        void Update()
        {
            pointsList.Clear();
            float pos = 0;
            for (int i = 0; i < step; ++i)
            {
                pos += 1f / step;
                pointsList.Add(EvaluateCurve(StartPosition, StartPosition + startControl, transform.position + endControl, transform.position, pos));
            }

            bezierLine.positionCount = (int)step;
            bezierLine.SetPositions(pointsList.ToArray());
        }
    }
}
