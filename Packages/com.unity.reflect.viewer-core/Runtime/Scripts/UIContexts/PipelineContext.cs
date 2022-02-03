using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{

    public interface IPipelineDataProvider
    {
        public Transform rootNode { get; set; }
        public SetVREnableAction.DeviceCapability deviceCapability { get; set; }
    }

    public class PipelineContext : ContextBase<PipelineContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IPipelineDataProvider)};

        public void ForceOnStateChanged()
        {
            OnStateChanged();
        }
    }
}
