using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Viewer.Core
{
    public enum DragState
    {
        None = 0,
        OnStart = 1,
        OnUpdate = 2,
        OnEnd = 3
    }

    public interface IDragStateData
    {
        public DragState dragState { get; set; }
        public Vector3 position { get; set; }
        public int hashObjectDragged { get; set; }
    }

    public class DragStateContext : ContextBase<DragStateContext>
    {
        public override List<Type> implementsInterfaces { get; }
    }
}
