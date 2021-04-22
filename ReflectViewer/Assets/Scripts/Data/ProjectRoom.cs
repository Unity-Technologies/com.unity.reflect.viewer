using System;
using System.Collections.Generic;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct ProjectRoom : IEquatable<ProjectRoom>
    {
        public Project project;
        public List<UserIdentity> users;

        public ProjectRoom(Project project, params UserIdentity[] users)
        {
            this.project = project;
            this.users = new List<UserIdentity>(users);
        }
        public bool Equals(ProjectRoom other)
        {
            return
                this.project == other.project &&
                EnumerableExtension.SafeSequenceEquals(users, other.users);
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectRoom other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = project.GetHashCode();
                hashCode = (hashCode * 397) ^ users.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ProjectRoom a, ProjectRoom b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ProjectRoom a, ProjectRoom b)
        {
            return !(a == b);
        }
    }
}
