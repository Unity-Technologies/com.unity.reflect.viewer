using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UISessionStateData : IEquatable<UISessionStateData>, ISessionStateDataProvider<UnityUser, LinkPermission>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public CollaborationState collaborationState { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public LoginState loggedState { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public ProjectListState projectListState { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public UnityUser user { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IUserIdentity userIdentity { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool isInPrivateMode { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IProjectRoom[] rooms { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool linkShareLoggedOut { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool isOpenWithLinkSharing { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string cachedLinkToken { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public LinkPermission linkSharePermission { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IProjectRoom linkSharedProjectRoom { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public NetworkReachability networkReachability { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool projectServerConnection { get; set; }


        public bool IsNetworkConnected =>
            networkReachability != NetworkReachability.NotReachable && projectServerConnection;

        public static readonly UISessionStateData defaultData = new UISessionStateData()
        {
            loggedState = LoginState.LoginSessionFromCache,
            user = null,
            rooms = new IProjectRoom[] { },
            projectListState = ProjectListState.AwaitingUser,
            userIdentity = new UserIdentity("", 0, "", DateTime.Now, null),
            linkSharedProjectRoom = new ProjectRoom(new Project(null))
        };

        public UISessionStateData(LoginState loggedState, UnityUser user, ProjectRoom[] rooms)
        {
            this.loggedState = loggedState;
            this.user = user;
            this.rooms = rooms.Cast<IProjectRoom>().ToArray();
            projectListState = loggedState == LoginState.LoggedIn ? ProjectListState.AwaitingUserData : ProjectListState.AwaitingUser;
            userIdentity = new UserIdentity();
            linkShareLoggedOut = false;
            isOpenWithLinkSharing = false;
            linkSharePermission = LinkPermission.Private;
            isInPrivateMode = false;
            cachedLinkToken = string.Empty;
            collaborationState = CollaborationState.Disconnected;
            linkSharedProjectRoom = default;
            networkReachability = default;
            projectServerConnection = true;
        }

        public static UISessionStateData Validate(UISessionStateData state)
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

        public bool Equals(UISessionStateData other)
        {
            return
                this.collaborationState == other.collaborationState &&
                this.loggedState == other.loggedState &&
                this.projectListState == other.projectListState &&
                this.isInPrivateMode == other.isInPrivateMode &&
                this.linkShareLoggedOut == other.linkShareLoggedOut &&
                this.isOpenWithLinkSharing == other.isOpenWithLinkSharing &&
                this.cachedLinkToken == other.cachedLinkToken &&
                this.linkSharePermission == other.linkSharePermission &&
                this.user == other.user &&
                this.linkSharedProjectRoom.Equals(other.linkSharedProjectRoom) &&
                this.rooms.Equals(other.rooms) &&
                this.networkReachability == other.networkReachability &&
                this.projectServerConnection == other.projectServerConnection;
        }

        public override bool Equals(object obj)
        {
            return obj is UISessionStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)loggedState;
                hashCode = (hashCode * 397) ^ (int)collaborationState;
                hashCode = (hashCode * 397) ^ (int)projectListState;
                hashCode = (hashCode * 397) ^ user.GetHashCode();
                hashCode = (hashCode * 397) ^ isInPrivateMode.GetHashCode();
                hashCode = (hashCode * 397) ^ linkShareLoggedOut.GetHashCode();
                hashCode = (hashCode * 397) ^ isOpenWithLinkSharing.GetHashCode();
                hashCode = (hashCode * 397) ^ cachedLinkToken.GetHashCode();
                hashCode = (hashCode * 397) ^ linkSharePermission.GetHashCode();
                hashCode = (hashCode * 397) ^ rooms.GetHashCode();
                hashCode = (hashCode * 397) ^ linkSharedProjectRoom.GetHashCode();
                hashCode = (hashCode * 397) ^ networkReachability.GetHashCode();
                hashCode = (hashCode * 397) ^ projectServerConnection.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(UISessionStateData a, UISessionStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UISessionStateData a, UISessionStateData b)
        {
            return !(a == b);
        }
    }
}
