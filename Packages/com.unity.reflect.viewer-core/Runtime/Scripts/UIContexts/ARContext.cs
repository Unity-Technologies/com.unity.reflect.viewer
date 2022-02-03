using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IInstructionUIIterable
    {
        void Next();
        void Back();
    }

    public interface IInstructionUICancelable
    {
        void Cancel();
    }

    public interface IARModeUIController { }

    public interface IARInstructionUI : IInstructionUIIterable, IInstructionUICancelable
    {
        void Initialize(IARModeUIController resolver);
        void Restart();
        void Reset();
        SetARModeAction.ARMode arMode { get; }
        SetARInstructionUIAction.InstructionUIStep CurrentInstructionStep { get; }
    }

    public interface IARModeDataProvider
    {
        public bool arEnabled { get; set; }
        public SetARModeAction.ARMode arMode { get; set; }
        public int instructionUIStep { get; set; }
        SetInstructionUIStateAction.InstructionUIState instructionUIState { get; set; }
        IARInstructionUI currentARInstructionUI { get; set; }
    }

    public interface IARPlacement<T>
    {
        public T placementStateData { get; set; }
    }

    public class ARContext : ContextBase<ARContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> { typeof(IARModeDataProvider), typeof(IARPlacement<IARPlacementDataProvider>)};
    }
}
