using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IFollowUserDataProvider
    {
        public string userId { get; set; }
        public GameObject userObject { get; set; }
        public bool isFollowing { get; set; }
    }

    public class FollowUserContext : ContextBase<FollowUserContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IFollowUserDataProvider)};
    }
}
