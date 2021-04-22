using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UIWalkStateData : IEquatable<UIWalkStateData>
    {
        public bool walkEnabled;
        public InstructionUIState instructionUIState;
        public IWalkInstructionUI instruction;

        public static readonly UIWalkStateData defaultData = new UIWalkStateData()
        {
            walkEnabled = false,
            instructionUIState = InstructionUIState.Init,
            instruction = null,
        };

        public UIWalkStateData(bool walkEnabled, InstructionUIState instructionUIState, IWalkInstructionUI instruction)
        {
            this.walkEnabled = walkEnabled;
            this.instructionUIState = instructionUIState;
            this.instruction = instruction;
        }

        public static UIWalkStateData Validate(UIWalkStateData stateData)
        {
            return stateData;
        }

        public override string ToString()
        {
            return ToString("WalkEnabled{0}, instructionUIState{1}, instructionUIStep{2}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                walkEnabled,
                (object)instructionUIState,
                instruction);
        }

        public bool Equals(UIWalkStateData other)
        {
            return walkEnabled == other.walkEnabled &&
                instructionUIState == other.instructionUIState &&
                instruction == other.instruction;
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
