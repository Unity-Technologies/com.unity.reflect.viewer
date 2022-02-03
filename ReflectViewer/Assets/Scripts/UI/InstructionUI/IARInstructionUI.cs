using System;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    public static class InstructionExtensions
    {
        public static bool CheckValidations(this SetARInstructionUIAction.InstructionUIStep step, GameObject firstSelectedPlane, GameObject secondSelectedPlane, GameObject currentSelectedObject = null)
        {
            if (step.validations != null)
            {
                foreach (var validation in step.validations)
                {
                    if (!validation.IsValid(firstSelectedPlane, secondSelectedPlane, out var message, currentSelectedObject))
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
