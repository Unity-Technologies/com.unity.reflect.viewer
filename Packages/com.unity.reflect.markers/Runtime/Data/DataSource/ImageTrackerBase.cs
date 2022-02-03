using System;
using System.Collections.Generic;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

namespace Unity.Reflect.Markers.ImageTracker
{
    public abstract class ImageTrackerBase : MonoBehaviour, IImageTracker
    {
        [SerializeField] private bool flattenMarker = false;
        [SerializeField]
        GameObject m_TrackedImagePrefab= null;
        GameObject spawnedPrefab = null;
        public Action<Pose, string> OnTrackedFound { get; set; }
        public Action<Pose, string> OnTrackedPositionUpdate { get; set; }
        public Action OnTrackingLost { get; set; }
        public Pose Value { get; protected set; }
        public virtual bool IsAvailable { get; protected set; }
        public virtual bool IsTracking { get; protected set; }
        public abstract void Run();

        public abstract void Stop();

        [SerializeField]
        internal MarkerController markerController;

        /// <summary>
        /// Flattens Value to a wall or floor.
        /// </summary>
        public static Pose Flatten(Pose value)
        {
            Pose result = value;
            Vector3 euler = value.rotation.eulerAngles;
            // Snap to 90 degree segments.
            float x = SnapAngleQuarters(euler.x);
            float z = SnapAngleQuarters(euler.z);
            result.rotation = Quaternion.Euler(x, euler.y, z);

            // If y is down, rotate it upwards.
            if ((result.forward + result.position).y < result.position.y)
            {
                euler = result.rotation.eulerAngles;
                result.rotation = Quaternion.Euler(euler.x + 180, euler.y, euler.z);
            }
            return result;
        }

        static float SnapAngleQuarters(float angle)
        {
            // Break 360 into quarters. Modulo the result to 0-3.
            int quarter = Mathf.RoundToInt(((angle * 4) / 360)) % 4;
            // Explode back to 360
            return quarter * 90f;
        }

        internal void UpdateValue(Pose pose, string id = null)
        {
            if (flattenMarker)
            {
                Pose flattenedPose = Flatten(pose);
                pose = flattenedPose;
            }

            // Show the marker position after processing
            spawnedPrefab.transform.position = pose.position;
            spawnedPrefab.transform.rotation = pose.rotation;

            Value = pose;
            try
            {
                OnTrackedPositionUpdate?.Invoke(Value, id);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        internal void ShowGizmo(bool visible)
        {
            if (spawnedPrefab == null)
                spawnedPrefab = Instantiate(m_TrackedImagePrefab);
            spawnedPrefab.SetActive(visible);
        }
    }
}
