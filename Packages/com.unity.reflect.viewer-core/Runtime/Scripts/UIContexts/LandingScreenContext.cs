using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IProjectListFilterDataProvider
    {
        public SetLandingScreenFilterProjectServerAction.ProjectServerType projectServerType { get; set; }
        public string searchString { get; set; }
    }

    public class LandingScreenContext : ContextBase<LandingScreenContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IProjectListFilterDataProvider)};
    }
}
