using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IVREnableDataProvider
    {
        public bool VREnable { get; set; }
        public Transform RightController { get; set; }
    }

    public class VRContext : ContextBase<VRContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IVREnableDataProvider)};
    }
}
