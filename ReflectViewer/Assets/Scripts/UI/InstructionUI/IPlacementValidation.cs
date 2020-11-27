using System.Linq;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public interface IPlacementValidation
    {
        bool IsValid(ARPlacementStateData placementState, out ModalPopup.ModalPopupData errorMessage);
    }

    public struct ParallelWallValidation : IPlacementValidation
    {
        public readonly float minPlaneNormalDot;

        public ParallelWallValidation(float minPlaneNormalDot)
        {
            this.minPlaneNormalDot = minPlaneNormalDot;
        }

        public bool IsValid(ARPlacementStateData placementState, out ModalPopup.ModalPopupData errorMessage)
        {
            errorMessage = default;
            var selected = UIStateManager.current.projectStateData.objectSelectionInfo.CurrentSelectedObject();
            if (selected == null)
            {
                errorMessage = UIStateManager.current.popUpManager.GetModalPopUpData();
                errorMessage.title = "Selection Error";
                errorMessage.text = "Select a wall before changing to the next step.";
                return false;
            }

            var selectedContext = selected.GetComponent<PlaneSelectionContext>();
            if (selectedContext == null)
            {
                errorMessage = UIStateManager.current.popUpManager.GetModalPopUpData();
                errorMessage.title = "Selection Error";
                errorMessage.text = "Selected object does not have a Plane Selection Context.";
                return false;
            }

            var firstPlane = placementState.firstSelectedPlane;
            if (firstPlane == null)
            {
                errorMessage = UIStateManager.current.popUpManager.GetModalPopUpData();
                errorMessage.title = "Placement Error";
                errorMessage.text = "Placement State does not have a first plane selected.";
                return false;
            }
            var firstPlaneContext = firstPlane.GetComponent<PlaneSelectionContext>();
            if (firstPlaneContext == null)
            {
                errorMessage = UIStateManager.current.popUpManager.GetModalPopUpData();
                errorMessage.title = "Placement Error";
                errorMessage.text = "First placement plane does not have a Plane Selection Context.";
                return false;
            }

            var selectedNormal = Vector3.ProjectOnPlane(selectedContext.LastContext.SelectedPlane.normal, Vector3.up);
            var firstPlaneNormal = Vector3.ProjectOnPlane(firstPlaneContext.LastContext.SelectedPlane.normal, Vector3.up);
            var dot = Mathf.Abs(Vector3.Dot(selectedNormal, firstPlaneNormal));

            if (dot > minPlaneNormalDot)
            {
                errorMessage = UIStateManager.current.popUpManager.GetModalPopUpData();
                errorMessage.title = "Placement Error";
                errorMessage.text = "Selected plane is not perpendicular to first plane.";
                return false;
            }
            return true;
        }
    }

    public struct WallSizeValidation : IPlacementValidation
    {
        public readonly float minBoundsMagnitude;

        public WallSizeValidation(float minBoundsMagnitude)
        {
            this.minBoundsMagnitude = minBoundsMagnitude;
        }

        public bool IsValid(ARPlacementStateData placementState, out ModalPopup.ModalPopupData errorMessage)
        {
            errorMessage = default;
            if (placementState.firstSelectedPlane != null)
            {
                var renderer = placementState.firstSelectedPlane.GetComponent<MeshRenderer>();
                if (renderer == null)
                {
                    errorMessage = UIStateManager.current.popUpManager.GetModalPopUpData();
                    errorMessage.title = "Placement Error";
                    errorMessage.text = "First placement plane does not have a renderer.";
                    return false;
                }
                if (renderer.bounds.size.magnitude < minBoundsMagnitude)
                {
                    errorMessage = UIStateManager.current.popUpManager.GetModalPopUpData();
                    errorMessage.title = "Placement Error";
                    errorMessage.text = "First placement plane model is too small.";
                    return false;
                }
            }
            if (placementState.secondSelectedPlane != null)
            {
                var renderer = placementState.secondSelectedPlane.GetComponent<MeshRenderer>();
                if (renderer == null)
                {
                    errorMessage = UIStateManager.current.popUpManager.GetModalPopUpData();
                    errorMessage.title = "Placement Error";
                    errorMessage.text = "Second placement plane does not have a renderer.";
                    return false;
                }
                if (renderer.bounds.size.magnitude < minBoundsMagnitude)
                {
                    errorMessage = UIStateManager.current.popUpManager.GetModalPopUpData();
                    errorMessage.title = "Placement Error";
                    errorMessage.text = "Second placement plane model is too small.";
                    return false;
                }
            }
            return true;
        }
    }
}
