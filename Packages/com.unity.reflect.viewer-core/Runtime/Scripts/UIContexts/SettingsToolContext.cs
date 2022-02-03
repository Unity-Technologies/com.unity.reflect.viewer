using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface ISettingsToolDataProvider
    {
        public bool bimFilterEnabled { get; set; }
        public bool sceneSettingsEnabled { get; set; }
        public bool sunStudyEnabled { get; set; }
        public bool markerSettingsEnabled { get; set; }
    }

    public class SettingsToolContext : ContextBase<SettingsToolContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(ISettingsToolDataProvider)};
    }
}
