
using System;
using System.Collections;
using System.Linq;
using System.Text;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public interface IWalkInstructionUI:IInstructionUIIterable, IInstructionUICancelable
    {
        void Reset(Vector3 offset);
    }

    public interface IInstructionUIIterable
    {
        void Next();
        void Back();
    }

    public interface IInstructionUICancelable
    {
        void Cancel();
    }

    // TODO:Rename IInstructionUI to IARInstructionUI
    public interface IInstructionUI: IInstructionUIIterable, IInstructionUICancelable
    {
        void Initialize(ARModeUIController resolver);
        void Restart();
        ARMode arMode { get; }
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

    public static class InstructionExtensions
    {
        public static bool CheckValidations(this InstructionUIStep step)
        {
            if (step.validations != null)
            {
                foreach (var validation in step.validations)
                {
                    if (!validation.IsValid(UIStateManager.current.arStateData.placementStateData, out var message))
                    {
                        UIStateManager.current.popUpManager.DisplayModalPopUp(message);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
