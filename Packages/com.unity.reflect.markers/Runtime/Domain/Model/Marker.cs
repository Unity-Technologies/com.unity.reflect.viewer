using System;
using Unity.Reflect.Markers.Placement;
using UnityEngine;
using Unity.Reflect.Model;
using Unity.TouchFramework;
using UnityEngine.Reflect;

namespace Unity.Reflect.Markers.Storage
{
    [Serializable]
    public struct Marker : IMarker
    {
        SyncId m_Id;
        DateTime m_CreatedTime;
        DateTime m_LastUpdatedTime;
        DateTime m_LastUsedTime;
        string m_Name;
        Vector3 m_RelativePosition;
        Vector3 m_RelativeRotationEuler;
        Vector3 m_ObjectScale;

        /// <summary>
        /// Returns a World Space Pose from a point and rotation, relative to a transform.
        /// </summary>
        /// <param name="transform">Root transform</param>
        /// <param name="relativePosition">Position related to root transform</param>
        /// <param name="relativeRotation">Rotation related to root transform</param>
        /// <returns>World Space Pose (Position, Rotation)</returns>
        public static Pose GetWorldPose(TransformData origin, Vector3 relativePosition, Quaternion relativeRotation)
        {

            Pose pose = new Pose()
            {
                position = origin.rotation * Vector3.Scale(relativePosition, origin.scale) + origin.position,
                rotation = origin.rotation * relativeRotation
            };
            return pose;
        }

        /// <summary>
        /// Returns a world space pose, based off a given transform.
        /// </summary>
        /// <param name="transform">Root transform</param>
        /// <returns>World Space Pose (Position, Rotation)</returns>
        public Pose GetWorldPose(TransformData transform)
        {
            return GetWorldPose(transform, RelativePosition, Quaternion.Euler(RelativeRotationEuler));
        }

        /// <summary>
        /// Align a transform to the destination pose using this marker.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public TransformData AlignObject(TransformData origin, Pose destination)
        {
            return AlignObject(this, origin, destination);
        }

        public static TransformData AlignObject(IMarker marker, TransformData origin, Pose destination)
        {
            // Get a world pose from the marker relative to the spawned model.
            Pose currentPose = GetWorldPose(origin, marker.RelativePosition, Quaternion.Euler(marker.RelativeRotationEuler));
            // Rotate around the anchor point, to match the pose.
            var postRot = PivotTransform.RotateAroundPivot(origin, currentPose, destination.rotation);

            // Update world pose
            currentPose = GetWorldPose(postRot, marker.RelativePosition, Quaternion.Euler(marker.RelativeRotationEuler));
            // Translate around a pivot
            return PivotTransform.MoveWithPivot(postRot, currentPose.position, destination.position);
        }

        /// <summary>
        /// Inverse transforms a World Pose into a Pose relative to the passed transform.
        /// </summary>
        /// <param name="transform">Transform to get a local postion from</param>
        /// <param name="worldPose">World pose</param>
        /// <returns>Pose localized to the passed transform</returns>
        public static Pose InverseTransformPose(Transform transform, Pose worldPose)
        {
            return new Pose()
            {
                position = transform.InverseTransformPoint(worldPose.position),
                rotation = Quaternion.Inverse(transform.rotation) * worldPose.rotation
            };
        }

        /// <summary>
        /// Updates the RelativePosition and RelativeRotation values to a new World-space Pose
        /// </summary>
        /// <param name="transform">Marker relative transform</param>
        /// <param name="worldPose"></param>
        public void SetWorldPosition(Transform transform, Pose worldPose)
        {
            Pose localPose = InverseTransformPose(transform, worldPose);
            RelativePosition = localPose.position;
            RelativeRotationEuler = localPose.rotation.eulerAngles;
        }

        /// <summary>
        /// Updates the RelativePosition and RelativeRotation values to a new World-space Pose
        /// </summary>
        /// <param name="transform">Marker relative transform</param>
        /// <param name="worldPose"></param>
        public static void SetWorldPosition(IMarker marker, Transform transform, Pose worldPose)
        {
            Pose localPose = InverseTransformPose(transform, worldPose);
            marker.RelativePosition = localPose.position;
            marker.RelativeRotationEuler = localPose.rotation.eulerAngles;
        }

        public static SyncMarker ToSyncMarker(IMarker marker)
        {
            if (marker == null)
                return null;

            SyncMarker response = new SyncMarker(marker.Id, marker.Name);
            System.Numerics.Vector3 syncPos = new System.Numerics.Vector3(marker.RelativePosition.x, marker.RelativePosition.y, marker.RelativePosition.z);
            System.Numerics.Vector3 syncScale = new System.Numerics.Vector3(marker.ObjectScale.x, marker.ObjectScale.y, marker.ObjectScale.z);
            Quaternion rot = Quaternion.Euler(marker.RelativeRotationEuler);
            System.Numerics.Quaternion syncRot = new System.Numerics.Quaternion(rot.x, rot.y, rot.z, rot.w);
            response.Transform = new SyncTransform(syncPos, syncRot, syncScale);
            response.CreatedTime = marker.CreatedTime;
            response.LastUpdatedTime = marker.LastUpdatedTime;
            response.LastUsedTime = marker.LastUsedTime;
            return response;
        }

        public static Marker FromProjectMarker(ProjectMarker projectMarker)
        {
            return new Marker()
            {
                Name = projectMarker.Name,
                Id = projectMarker.Id,
                CreatedTime = projectMarker.CreatedTime,
                LastUpdatedTime = projectMarker.LastUpdatedTime,
                LastUsedTime = projectMarker.LastUsedTime,
                RelativePosition = projectMarker.Position,
                RelativeRotationEuler = projectMarker.Rotation,
                ObjectScale = projectMarker.Scale
            };
        }

        public Marker(SyncMarker syncMarker)
        {
            m_Id = syncMarker.Id;
            m_Name = syncMarker.Name;

            m_RelativePosition = new Vector3(syncMarker.Transform.Position.X, syncMarker.Transform.Position.Y, syncMarker.Transform.Position.Z);
            m_RelativeRotationEuler = new Quaternion(syncMarker.Transform.Rotation.X, syncMarker.Transform.Rotation.Y, syncMarker.Transform.Rotation.Z, syncMarker.Transform.Rotation.W).eulerAngles;
            m_ObjectScale = new Vector3(syncMarker.Transform.Scale.X, syncMarker.Transform.Scale.Y, syncMarker.Transform.Scale.Z);
            m_CreatedTime = syncMarker.CreatedTime;
            m_LastUpdatedTime = syncMarker.LastUpdatedTime;
            m_LastUsedTime = syncMarker.LastUsedTime;
        }

        public Marker(string name = "Reflect Marker")
        {
            var id = Guid.NewGuid();
            m_Id = new SyncId(id.ToString("N"));
            m_Name = name;

            m_RelativePosition = Vector3.zero;
            m_RelativeRotationEuler = Vector3.zero;
            m_ObjectScale = Vector3.one;

            m_CreatedTime =
                m_LastUpdatedTime =
                    m_LastUsedTime = DateTime.Now.ToUniversalTime();
        }

        public override string ToString()
        {
            return $"[{Id}] {Name} Position: {RelativePosition} Rotation: {RelativeRotationEuler} Scale: {ObjectScale} LastUpdate:{LastUpdatedTime} Created:{CreatedTime} LastUsed:{LastUsedTime}";
        }

        public bool Equals(Marker other)
        {
            return Equals((IMarker)other);
        }

        public bool Equals(IMarker other)
        {
            return
                other != null
                && Id.Equals(other.Id)
                && Name == other.Name
                && RelativePosition.Equals(other.RelativePosition)
                && RelativeRotationEuler.Equals(other.RelativeRotationEuler)
                && ObjectScale.Equals(other.ObjectScale)
                && CreatedTime.Equals(other.CreatedTime)
                && LastUpdatedTime.Equals(other.LastUpdatedTime)
                && LastUsedTime.Equals(other.LastUsedTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Marker)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RelativePosition.GetHashCode();
                hashCode = (hashCode * 397) ^ RelativeRotationEuler.GetHashCode();
                hashCode = (hashCode * 397) ^ ObjectScale.GetHashCode();
                hashCode = (hashCode * 397) ^ CreatedTime.GetHashCode();
                hashCode = (hashCode * 397) ^ LastUpdatedTime.GetHashCode();
                hashCode = (hashCode * 397) ^ LastUsedTime.GetHashCode();
                return hashCode;
            }
        }

        public SyncId Id
        {
            get => m_Id;
            set => m_Id = value;
        }

        public DateTime CreatedTime
        {
            get => m_CreatedTime;
            set => m_CreatedTime = value;
        }

        public DateTime LastUpdatedTime
        {
            get => m_LastUpdatedTime;
            set => m_LastUpdatedTime = value;
        }

        public DateTime LastUsedTime
        {
            get => m_LastUsedTime;
            set => m_LastUsedTime = value;
        }

        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        public Vector3 RelativePosition
        {
            get => m_RelativePosition;
            set => m_RelativePosition = value;
        }

        public Vector3 RelativeRotationEuler
        {
            get => m_RelativeRotationEuler;
            set => m_RelativeRotationEuler = value;
        }

        public Vector3 ObjectScale
        {
            get => m_ObjectScale;
            set => m_ObjectScale = value;
        }
    }
}
