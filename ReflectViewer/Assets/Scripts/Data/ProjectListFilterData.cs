using System;

namespace Unity.Reflect.Viewer.UI
{
    [Flags]
    public enum ProjectServerType
    {
        None = 0,
        Local = 1 << 0,
        Network = 1 << 1,
        Cloud = 1 << 2,
        All = (Cloud << 1) - 1
    }

    [Serializable]
    public struct ProjectListFilterData : IEquatable<ProjectListFilterData>
    {
        public ProjectServerType projectServerType;
        public string searchString;

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
