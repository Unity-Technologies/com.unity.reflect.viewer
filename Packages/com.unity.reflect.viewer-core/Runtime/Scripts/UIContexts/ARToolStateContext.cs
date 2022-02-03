using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IARToolStatePropertiesDataProvider
    {
        public bool selectionEnabled { get; set; }
        public bool navigationEnabled { get; set; }
        public bool previousStepEnabled { get; set; }
        public bool okEnabled { get; set; }
        public bool cancelEnabled { get; set; }
        public bool scaleEnabled { get; set; }
        public bool wallIndicatorsEnabled { get; set; }
        public bool anchorPointsEnabled { get; set; }
        public bool arWallIndicatorsEnabled { get; set; }
        public bool arAnchorPointsEnabled { get; set; }
        public bool rotateEnabled { get; set; }
        public bool measureToolEnabled { get; set; }
        public SetARToolStateAction.IUIButtonValidator okButtonValidator { get; set; }
    }

    public class ARToolStateContext : ContextBase<ARToolStateContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IARToolStatePropertiesDataProvider)};
    }

}
