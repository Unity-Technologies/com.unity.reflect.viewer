using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IToolStateDataProvider
    {
        public SetActiveToolAction.ToolType activeTool { get; set; }
        public SetOrbitTypeAction.OrbitType orbitType { get; set; }
        public SetInfoTypeAction.InfoType infoType { get; set; }
        public SetClippingToolAction.ClippingTool clippingTool { get; set; }
    }

    public class ToolStateContext : ContextBase<ToolStateContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IToolStateDataProvider)};
    }
}
