using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface INavigationDataProvider
    {
        public SetNavigationModeAction.NavigationMode navigationMode { get; set; }
        public bool freeFlyCameraEnabled { get; set; }
        public bool orbitEnabled { get; set; }
        public bool panEnabled { get; set; }
        public bool zoomEnabled { get; set; }
        public bool moveEnabled { get; set; }
        public bool worldOrbitEnabled { get; set; }
        public bool teleportEnabled { get; set; }
        public bool gizmoEnabled { get; set; }
        public bool showScaleReference { get; set; }
    }

    public interface INavigationModeInfosDataProvider<TNavInfos>
    {
        public List<TNavInfos> navigationModeInfos { get; set; }
    }

    public class NavigationContext : ContextBase<NavigationContext>
    {
        public override List<Type> implementsInterfaces => new List<Type>{typeof(INavigationDataProvider), typeof(INavigationModeInfosDataProvider<>)};
    }
}
