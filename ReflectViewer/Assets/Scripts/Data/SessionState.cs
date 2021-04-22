using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.UI
{
    [Flags]
    public enum CollaborationState
    {
        Disconnected= 0,
        ConnectedMatchmaker = 1,
        ConnectedRoom = 2,
        ConnectedNetcode = 4
    }

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
            rooms = new ProjectRoom[] { }
        };

        public CollaborationState collaborationState;
        public LoginState loggedState;
        public UnityUser user;
        public UserIdentity userIdentity;
        public bool isInPrivateMode;
        public ProjectRoom[] rooms;
        public bool linkShareLoggedOut;
        public LinkPermission linkSharePermission;
        public ProjectRoom linkSharedProjectRoom;

        public SessionState(LoginState loggedState, UnityUser user, ProjectRoom[] rooms)
        {
            this.loggedState = loggedState;
            this.user = user;
            this.rooms = rooms;
            userIdentity = default;
            linkShareLoggedOut = false;
            linkSharePermission = LinkPermission.Private;
            isInPrivateMode = false;
            collaborationState = CollaborationState.Disconnected;
            linkSharedProjectRoom = default;
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
                this.collaborationState == other.collaborationState &&
                this.loggedState == other.loggedState &&
                this.isInPrivateMode == other.isInPrivateMode &&
                this.linkShareLoggedOut == other.linkShareLoggedOut &&
                this.linkSharePermission == other.linkSharePermission &&
                this.user.Equals(other.user) &&
                this.linkSharedProjectRoom.Equals(other.linkSharedProjectRoom) &&
                this.rooms.Equals(other.rooms);
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
                hashCode = (hashCode * 397) ^ (int)collaborationState;
                hashCode = (hashCode * 397) ^ user.GetHashCode();
                hashCode = (hashCode * 397) ^ isInPrivateMode.GetHashCode();
                hashCode = (hashCode * 397) ^ linkShareLoggedOut.GetHashCode();
                hashCode = (hashCode * 397) ^ linkSharePermission.GetHashCode();
                hashCode = (hashCode * 397) ^ rooms.GetHashCode();
                hashCode = (hashCode * 397) ^ linkSharedProjectRoom.GetHashCode();
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
