using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    public struct ProjectListFilterData : IEquatable<ProjectListFilterData>, IProjectListFilterDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetLandingScreenFilterProjectServerAction.ProjectServerType projectServerType { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string searchString { get; set; }

        public static bool operator ==(ProjectListFilterData a, ProjectListFilterData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ProjectListFilterData a, ProjectListFilterData b)
        {
            return !(a == b);
        }

        public bool Equals(ProjectListFilterData other)
        {
            return projectServerType == other.projectServerType && searchString == other.searchString;
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectListFilterData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)projectServerType;
                hashCode = (hashCode * 397) ^ (searchString != null ? searchString.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
