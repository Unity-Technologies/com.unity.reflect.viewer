
using System;
using System.Collections;
using System.Linq;
using System.Text;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public interface IInstructionUI
    {
        void Initialize(ARModeUIController resolver);
        void Restart();
        void Cancel();
        ARMode arMode { get; }
        void Next();
        void Back();
        InstructionUIStep CurrentInstructionStep { get; }
    }

    public struct InstructionUIStep
    {
        public int stepIndex;
        public delegate void transition();

        public transition onNext;
        public transition onBack;

        public IPlacementValidation[] validations;
    }
}
