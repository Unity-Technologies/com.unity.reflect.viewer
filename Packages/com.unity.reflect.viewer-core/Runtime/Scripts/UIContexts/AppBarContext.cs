using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IAppBarDataProvider
    {
        public IButtonVisibility buttonVisibility { get; set; }
        public IButtonInteractable buttonInteractable { get; set; }
    }

    public class AppBarContext : ContextBase<AppBarContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IAppBarDataProvider)};
    }
}
