using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace UnityEngine.Reflect.Viewer.Core
{
    [Flags]
    public enum CollaborationState
    {
        Disconnected = 0,
        ConnectedMatchmaker = 1,
        ConnectedRoom = 2,
        ConnectedNetcode = 4
    }

    public enum LoginState
    {
        LoginSessionFromCache = 0,
        LoggedOut,
        LoggingIn,
        ProcessingToken,
        LoggedIn,
        LoggingOut,
    }

    public enum ProjectListState
    {
        AwaitingUser,
        AwaitingUserData,
        Ready,
    }

    public interface IUserIdentity { }
    public interface IProjectRoom { }

    public interface ISessionStateDataProvider<T,U>
    {
        public CollaborationState collaborationState { get; set; }
        public LoginState loggedState { get; set; }
        public ProjectListState projectListState { get; set; }
        public T user { get; set; }
        public IUserIdentity userIdentity { get; set; }
        public bool isInPrivateMode { get; set; }
        public IProjectRoom[] rooms { get; set; }
        public bool linkShareLoggedOut { get; set; }
        public bool isOpenWithLinkSharing { get; set; }
        public string cachedLinkToken { get; set; }
        public U linkSharePermission { get; set; }
        public IProjectRoom linkSharedProjectRoom { get; set; }
        public NetworkReachability networkReachability { get; set; }
        public bool projectServerConnection { get; set; }
    }

    public class SessionStateContext<T,U> : ContextBase<SessionStateContext<T,U>>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(ISessionStateDataProvider<T,U>) };
    }
}
