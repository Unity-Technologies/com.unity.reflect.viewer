using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.UI
{
    public enum LoginState
    {
        LoggedOut = 0,
        LoggingIn,
        LoggedIn,
        LoggingOut,
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SessionState : IEquatable<SessionState>
    {
        public static readonly SessionState defaultData = new SessionState()
        {
            loggedState = LoginState.LoggedOut,
            user = null,
            projects = new Project[]{}
        };

        public LoginState loggedState;
        public UnityUser user;
        public Project[] projects;

        public SessionState(LoginState loggedState, UnityUser user, Project[] projects)
        {
            this.loggedState = loggedState;
            this.user = user;
            this.projects = projects;
        }

        public static SessionState Validate(SessionState state)
        {
            return state;
        }

        public override string ToString()
        {
            return ToString("(loggedIn {0}, displayName {1}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                this.loggedState,
                this.user);
        }

        public bool Equals(SessionState other)
        {
            return
                this.loggedState == other.loggedState &&
                this.user.Equals(other.user) &&
                this.projects.Equals(other.projects);
        }

        public override bool Equals(object obj)
        {
            return obj is SessionState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)loggedState;
                hashCode = (hashCode * 397) ^ user.GetHashCode();
                hashCode = (hashCode * 397) ^ projects.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SessionState a, SessionState b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SessionState a, SessionState b)
        {
            return !(a == b);
        }
    }
}
