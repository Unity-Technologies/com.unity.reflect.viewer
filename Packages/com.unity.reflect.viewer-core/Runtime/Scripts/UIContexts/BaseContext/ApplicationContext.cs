using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class ApplicationContext : ContextBase<ApplicationContext>
    {
        public override List<Type> implementsInterfaces { get; }
    }
}
