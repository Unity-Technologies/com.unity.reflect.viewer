using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IForceNavigationModeDataProvider
    {
        public SetForceNavigationModeAction.ForceNavigationModeTrigger navigationMode { get; set; }
    }

    public class ForceNavigationModeContext : ContextBase<ForceNavigationModeContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IForceNavigationModeDataProvider)};
    }
}
