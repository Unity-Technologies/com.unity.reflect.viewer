using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IMeasureToolDataProvider
    {
        public bool toolState { get; set; }
        public ToggleMeasureToolAction.AnchorType selectionType { get; set; }
        public ToggleMeasureToolAction.MeasureMode measureMode { get; set; }
        public ToggleMeasureToolAction.MeasureFormat measureFormat { get; set; }
        public SelectObjectMeasureToolAction.IAnchor selectedAnchor { get; set; }
    }

    public class MeasureToolContext : ContextBase<MeasureToolContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IMeasureToolDataProvider)};
    }
}
