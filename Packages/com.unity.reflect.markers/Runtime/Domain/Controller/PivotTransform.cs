using System.Collections;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Markers.Placement
{
    public class PivotTransform
    {
        public static TransformData MoveWithPivot(TransformData origin, Vector3 worldStartPoint, Vector3 worldEndPoint)
        {
            GameObject pivotObject = new GameObject();
            pivotObject.transform.localScale = Vector3.one;
            pivotObject.transform.rotation = Quaternion.identity;
            pivotObject.transform.position = worldStartPoint;
            pivotObject.transform.SetParent(null, true);

            GameObject target = new GameObject();
            target.transform.position = origin.position;
            target.transform.rotation = origin.rotation;
            target.transform.localScale = origin.scale;

            target.transform.SetParent(pivotObject.transform, true);
            target.gameObject.SetActive(true);

            //Move the pivot
            pivotObject.transform.position = worldEndPoint;

            //Unparent the model
            target.transform.SetParent(null, true);
            var response = new TransformData(target.transform);
            GameObject.Destroy(pivotObject);
            GameObject.Destroy(target);
            return response;
        }

        public static void RotateAroundPivot(Transform target, float angle, Vector3 axis, Vector3 worldPivotPoint)
        {
            GameObject pivotObject = new GameObject();
            pivotObject.transform.localScale = Vector3.one;
            pivotObject.transform.rotation = Quaternion.identity;
            pivotObject.transform.position = worldPivotPoint;
            pivotObject.transform.SetParent(null, true);

            //Parent the model to it's pivot
            Transform oldParent = target.transform.parent;

            target.transform.SetParent(pivotObject.transform, true);

            //Rotate the pivot
            pivotObject.transform.Rotate(axis, angle);

            //Unparent the model
            target.transform.SetParent(oldParent, true);
            GameObject.Destroy(pivotObject);
        }

        /// <summary>
        /// Rotate target around pivot with the given target rotation.
        /// If KeepTargetUpright is True, then the rotation will only turn the target about on the Yaw (Y) axis.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pivot"></param>
        /// <param name="targetRotation"></param>
        /// <param name="keepTargetUpright">
        /// When true, X & Z rotations are discarded from target rotation.
        /// </param>

        public static TransformData RotateAroundPivot(TransformData origin, Pose pivot, Quaternion targetRotation, bool keepTargetUpright = true)
        {
            GameObject pivotObject = new GameObject();
            pivotObject.transform.localScale = Vector3.one;
            pivotObject.transform.rotation = pivot.rotation;
            pivotObject.transform.position = pivot.position;
            pivotObject.transform.SetParent(null, true);

            //Parent the model to it's pivot
            GameObject target = new GameObject();
            target.transform.position = origin.position;
            target.transform.rotation = origin.rotation;
            target.transform.localScale = origin.scale;

            target.transform.SetParent(pivotObject.transform, true);

            if (keepTargetUpright)
            {
                float y = targetRotation.eulerAngles.y + 180;
                Vector3 source = pivotObject.transform.rotation.eulerAngles;
                targetRotation.eulerAngles = new Vector3(source.x, y, source.z);
            }

            //Update pivot rotation
            pivotObject.transform.rotation = targetRotation;
            target.transform.SetParent(null, true);
            var response = new TransformData(target.transform);
            GameObject.Destroy(pivotObject);
            GameObject.Destroy(target);
            return response;
        }

        /// <summary>
        /// Returns true if the two floats are similar within tolerance
        /// </summary>
        /// <param name="a">First float</param>
        /// <param name="b">Other float</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Yes if A ~= B within Tolerance</returns>
        public static bool Similar(float a, float b, float tolerance = 0.0001f)
        {
            return Mathf.Abs(a - b) <= tolerance;
        }


        /// <summary>
        /// Returns true if the two Vector3 are similar within tolerance
        /// </summary>
        /// <param name="a">First Vector3</param>
        /// <param name="b">Other Vector3</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Yes if A ~= B within Tolerance</returns>
        public static bool Similar(Vector3 a, Vector3 b, float tolerance = 0.0001f)
        {
            return Similar(a.x, b.x, tolerance) &&
                Similar(a.y, b.y, tolerance) &&
                Similar(a.z, b.z, tolerance);
        }

        /// <summary>
        /// Returns true if the two Vector2 are similar within tolerance
        /// </summary>
        /// <param name="a">First Vector2</param>
        /// <param name="b">Other Vector2</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Yes if A ~= B within Tolerance</returns>
        public static bool Similar(Vector2 a, Vector2 b, float tolerance = 0.0001f)
        {
            return Similar(a.x, b.x, tolerance) &&
                Similar(a.y, b.y, tolerance);
        }

        /// <summary>
        /// Returns true if the two Vector4 are similar within tolerance
        /// </summary>
        /// <param name="a">First Vector4</param>
        /// <param name="b">Other Vector4</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Yes if A ~= B within Tolerance</returns>
        public static bool Similar(Vector4 a, Vector4 b, float tolerance = 0.0001f)
        {
            return Similar(a.x, b.x, tolerance) &&
                Similar(a.y, b.y, tolerance) &&
                Similar(a.z, b.z, tolerance) &&
                Similar(a.w, b.w, tolerance);
        }

        /// <summary>
        /// Returns true if the two Quaternion are similar within tolerance
        /// </summary>
        /// <param name="a">First Quaternion</param>
        /// <param name="b">Other Quaternion</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Yes if A ~= B within Tolerance</returns>
        public static bool Similar(Quaternion a, Quaternion b, float tolerance = 0.0001f)
        {
            return Similar(a.x, b.x, tolerance) &&
                Similar(a.y, b.y, tolerance) &&
                Similar(a.z, b.z, tolerance) &&
                Similar(a.w, b.w, tolerance);
        }

    }
}
