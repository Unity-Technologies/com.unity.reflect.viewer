using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IWalkModeDataProvider
    {
        public bool walkEnabled { get; set; }
        public bool isTeleportFinish { get; set; }
        public SetInstructionUIStateAction.InstructionUIState instructionUIState{ get; set; }
        public IWalkInstructionUI instruction{ get; set; }
    }

    public class WalkModeContext : ContextBase<WalkModeContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IWalkModeDataProvider)};
    }
}
