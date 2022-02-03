using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Properties;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UIWalkStateData : IEquatable<UIWalkStateData>, IWalkModeDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool walkEnabled { get; set; }

        // TODO: Move all teleport variable to it's own store
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool isTeleportFinish { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetInstructionUIStateAction.InstructionUIState instructionUIState { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IWalkInstructionUI instruction { get; set; }

        public UIWalkStateData(bool walkEnabled, SetInstructionUIStateAction.InstructionUIState instructionUIState, IWalkInstructionUI instruction)
        {
            this.walkEnabled = walkEnabled;
            this.instructionUIState = instructionUIState;
            this.instruction = instruction;
            isTeleportFinish = false;
        }

        public static UIWalkStateData Validate(UIWalkStateData stateData)
        {
            return stateData;
        }

        public override string ToString()
        {
            return ToString("WalkEnabled{0}, instructionUIState{1}, instructionUIStep{2}, TeleportFinish{3}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                walkEnabled,
                (object)instructionUIState,
                instruction, isTeleportFinish);
        }

        public bool Equals(UIWalkStateData other)
        {
            return walkEnabled == other.walkEnabled &&
                instructionUIState == other.instructionUIState &&
                instruction == other.instruction
                && isTeleportFinish == other.isTeleportFinish;
        }

        public override bool Equals(object obj)
        {
            return obj is UIWalkStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = walkEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ instructionUIState.GetHashCode();
                hashCode = (hashCode * 397) ^ instruction.GetHashCode();
                hashCode = (hashCode * 397) ^ isTeleportFinish.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(UIWalkStateData a, UIWalkStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UIWalkStateData a, UIWalkStateData b)
        {
            return !(a == b);
        }
    }
}
