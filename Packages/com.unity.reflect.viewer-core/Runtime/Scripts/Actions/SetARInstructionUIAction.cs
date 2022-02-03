using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetARInstructionUIAction : ActionBase
    {
        public struct InstructionUIStep
        {
            public int stepIndex;

            public delegate void transition();

            public transition onNext;
            public transition onBack;

            public IPlacementValidation[] validations;
        }

        public object Data { get; }

        SetARInstructionUIAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetARInstructionUIAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARContext.current;
        }
    }
}
