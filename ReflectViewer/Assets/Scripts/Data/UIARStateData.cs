using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public enum InstructionUIState
    {
        Init = 0,
        Started,
        Completed
    };

    public enum PlacementRule
    {
        None = 0,
        FloorPlacementRule = 1,
        TableTopPlacementRule = 2,
        WallPlacementRule = 3,
    }

    public interface IUIButtonValidator
    {
        bool ButtonValidate();
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ARToolStateData : IEquatable<ARToolStateData>
    {
        public bool selectionEnabled;
        public bool navigationEnabled;
        public bool previousStepEnabled;
        public bool okEnabled;
        public bool cancelEnabled;
        public bool scaleEnabled;
        public bool wallIndicatorsEnabled;
        public bool anchorPointsEnabled;
        public bool arWallIndicatorsEnabled;
        public bool arAnchorPointsEnabled;
        public bool rotateEnabled;
        public bool measureToolEnabled;
        public IUIButtonValidator okButtonValidator;

        public static readonly ARToolStateData defaultData = new ARToolStateData()
        {
            selectionEnabled = false,
            navigationEnabled = false,
            previousStepEnabled = false,
            okEnabled = true,
            cancelEnabled = false,
            scaleEnabled = false,
            wallIndicatorsEnabled = false,
            anchorPointsEnabled = false,
            okButtonValidator = null,
            arWallIndicatorsEnabled = false,
            arAnchorPointsEnabled = false,
            rotateEnabled = false,
            measureToolEnabled = false,
        };

        public ARToolStateData(bool selectionEnabled, bool navigationEnabled, bool previousStepEnabled, bool okEnabled,
            bool cancelEnabled, bool scaleEnabled, bool wallIndicatorsEnabled, bool anchorPointsEnabled, IUIButtonValidator okButtonValidator,
            bool arWallIndicatorsEnabled, bool arAnchorPointsEnabled, bool rotateEnabled, bool measureToolEnabled)
        {
            this.selectionEnabled = selectionEnabled;
            this.navigationEnabled = navigationEnabled;
            this.previousStepEnabled = previousStepEnabled;
            this.okEnabled = okEnabled;
            this.cancelEnabled = cancelEnabled;
            this.scaleEnabled = scaleEnabled;
            this.wallIndicatorsEnabled = wallIndicatorsEnabled;
            this.anchorPointsEnabled = anchorPointsEnabled;
            this.okButtonValidator = okButtonValidator;
            this.arWallIndicatorsEnabled = arWallIndicatorsEnabled;
            this.arAnchorPointsEnabled = arAnchorPointsEnabled;
            this.rotateEnabled = rotateEnabled;
            this.measureToolEnabled = measureToolEnabled;
        }

        public static ARToolStateData Validate(ARToolStateData stateData)
        {
            return stateData;
        }

        public override string ToString()
        {
            return $"selectionEnabled {selectionEnabled}, navigationEnabled {navigationEnabled}, previousStepEnabled {previousStepEnabled}, okEnabled {okEnabled}, " +
                $"cancelEnabled {cancelEnabled}, scaleEnabled {scaleEnabled}, wallIndicatorsEnabled {wallIndicatorsEnabled}, anchorPointsEnabled {anchorPointsEnabled}, " +
                $"okButtonValidator {okButtonValidator}, arWallIndicatorsEnabled {arWallIndicatorsEnabled}, arAnchorPointsEnabled {arAnchorPointsEnabled}, rotateEnabled {rotateEnabled}, measureToolEnabled {measureToolEnabled} ";
        }

        public bool Equals(ARToolStateData other)
        {
            return selectionEnabled == other.selectionEnabled &&
                   navigationEnabled == other.navigationEnabled &&
                   previousStepEnabled == other.previousStepEnabled &&
                   okEnabled == other.okEnabled &&
                   cancelEnabled == other.cancelEnabled &&
                   scaleEnabled == other.scaleEnabled &&
                   wallIndicatorsEnabled == other.wallIndicatorsEnabled &&
                   anchorPointsEnabled == other.anchorPointsEnabled &&
                   okButtonValidator == other.okButtonValidator &&
                   arWallIndicatorsEnabled == other.arWallIndicatorsEnabled &&
                   arAnchorPointsEnabled == other.arAnchorPointsEnabled &&
                   rotateEnabled == other.rotateEnabled &&
                   measureToolEnabled == other.measureToolEnabled;
        }

        public override bool Equals(object obj)
        {
            return obj is ARToolStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = selectionEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ navigationEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ previousStepEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ okEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ cancelEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ scaleEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ okButtonValidator.GetHashCode();
                hashCode = (hashCode * 397) ^ wallIndicatorsEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ anchorPointsEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ arWallIndicatorsEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ arAnchorPointsEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ rotateEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ measureToolEnabled.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ARToolStateData a, ARToolStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ARToolStateData a, ARToolStateData b)
        {
            return !(a == b);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ARPlacementStateData : IEquatable<ARPlacementStateData>
    {
        public GameObject modelFloor;
        public GameObject firstSelectedPlane;
        public GameObject secondSelectedPlane;
        public Vector3 modelPlacementLocation;
        public GameObject arFloor;
        public GameObject firstARSelectedPlane;
        public GameObject secondARSelectedPlane;
        public Vector3 arPlacementLocation;
        public Vector3 arPlacementAlignment;
        public PlacementRule placementRule;
        public bool validTarget;
        public float beamHeight;

        public static readonly ARPlacementStateData defaultData = new ARPlacementStateData()
        {
            modelFloor = null,
            firstSelectedPlane = null,
            secondSelectedPlane = null,
            modelPlacementLocation = Vector3.zero,
            arFloor = null,
            firstARSelectedPlane = null,
            secondARSelectedPlane = null,
            arPlacementLocation = Vector3.zero,
            arPlacementAlignment = Vector3.forward,
            placementRule = PlacementRule.None,
            validTarget = false,
            beamHeight = 0
        };

        public ARPlacementStateData(GameObject modelFloor, GameObject firstSelectedPlane, GameObject secondSelectedPlane, Vector3 modelPlacementLocation,
            GameObject arFloor, GameObject firstARSelectedPlane, GameObject secondARSelectedPlane, Vector3 arPlacementLocation,Vector3 arPlacementAlignment, PlacementRule placementRule, bool validTarget, float beamHeight)
        {
            this.modelFloor = modelFloor;
            this.firstSelectedPlane = firstSelectedPlane;
            this.secondSelectedPlane = secondSelectedPlane;
            this.modelPlacementLocation = modelPlacementLocation;
            this.arFloor = arFloor;
            this.firstARSelectedPlane = firstARSelectedPlane;
            this.secondARSelectedPlane = secondARSelectedPlane;
            this.arPlacementLocation = arPlacementLocation;
            this.arPlacementAlignment = arPlacementAlignment;
            this.placementRule = placementRule;
            this.validTarget = validTarget;
            this.beamHeight = beamHeight;
        }

        public static ARPlacementStateData Validate(ARPlacementStateData stateData)
        {
            return stateData;
        }

        public override string ToString()
        {
            return ToString("modelFloor{0}, firstSelectedPlane{1}, secondSelectedPlane{2}, modelPlacementLocation{3} arPlacementLocation{4} arPlacementAlignment{5}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                modelFloor,
                firstSelectedPlane,
                secondSelectedPlane,
                modelPlacementLocation,
                arPlacementLocation,
                arPlacementAlignment);
        }

        public bool Equals(ARPlacementStateData other)
        {
            return modelFloor == other.modelFloor &&
                firstSelectedPlane == other.firstSelectedPlane &&
                secondSelectedPlane == other.secondSelectedPlane &&
                modelPlacementLocation == other.modelPlacementLocation &&
                arFloor == other.arFloor &&
                placementRule == other.placementRule &&
                firstARSelectedPlane == other.firstARSelectedPlane &&
                secondARSelectedPlane == other.secondARSelectedPlane &&
                arPlacementLocation == other.arPlacementLocation &&
                arPlacementAlignment == other.arPlacementAlignment &&
                validTarget == other.validTarget &&
                Mathf.Approximately(beamHeight,other.beamHeight);
        }

        public override bool Equals(object obj)
        {
            return obj is ARPlacementStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = modelFloor.GetHashCode();
                hashCode = (hashCode * 397) ^ firstSelectedPlane.GetHashCode();
                hashCode = (hashCode * 397) ^ secondSelectedPlane.GetHashCode();
                hashCode = (hashCode * 397) ^ modelPlacementLocation.GetHashCode();
                hashCode = (hashCode * 397) ^ arFloor.GetHashCode();
                hashCode = (hashCode * 397) ^ placementRule.GetHashCode();
                hashCode = (hashCode * 397) ^ firstARSelectedPlane.GetHashCode();
                hashCode = (hashCode * 397) ^ secondARSelectedPlane.GetHashCode();
                hashCode = (hashCode * 397) ^ arPlacementLocation.GetHashCode();
                hashCode = (hashCode * 397) ^ arPlacementAlignment.GetHashCode();
                hashCode = (hashCode * 397) ^ validTarget.GetHashCode();
                hashCode = (hashCode *397) ^ beamHeight.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ARPlacementStateData a, ARPlacementStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ARPlacementStateData a, ARPlacementStateData b)
        {
            return !(a == b);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UIARStateData : IEquatable<UIARStateData>
    {
        public bool arEnabled;
        public InstructionUIState instructionUIState;
        public IInstructionUI currentInstructionUI;
        public int instructionUIStep;
        public int numProxyInstances;
        public ARToolStateData arToolStateData;
        public ARPlacementStateData placementStateData;
        public ARMode? arMode;

        public static readonly UIARStateData defaultData = new UIARStateData()
        {
            arEnabled = false,
            instructionUIState = InstructionUIState.Init,
            currentInstructionUI = null,
            numProxyInstances = 0,
            instructionUIStep = 0,
            arToolStateData = ARToolStateData.defaultData,
            placementStateData = ARPlacementStateData.defaultData,
            arMode = null
        };

        public UIARStateData(bool arEnabled, InstructionUIState instructionUIState, IInstructionUI currentInstructionUI, int numProxyInstances,
            int instructionUIStep, bool placementGesturesEnabled, ARToolStateData arToolStateData, ARPlacementStateData placementStateData, ARMode arMode)
        {
            this.arEnabled = arEnabled;
            this.instructionUIState = instructionUIState;
            this.currentInstructionUI = currentInstructionUI;
            this.numProxyInstances = numProxyInstances;
            this.instructionUIStep = instructionUIStep;
            this.arToolStateData = arToolStateData;
            this.placementStateData = placementStateData;
            this.arMode = arMode;
        }

        public static UIARStateData Validate(UIARStateData stateData)
        {
            return stateData;
        }

        public override string ToString()
        {
            return ToString("AREnabled{0}, instructionUIState{1}, numOfProxyInstances{2}, " +
                            "instructionUIStep{3}, placementGesturesEnabled{4}, ARToolStateData{5}, ARPlacementStateData{6}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                arEnabled,
                (object) instructionUIState,
                numProxyInstances,
                instructionUIStep,
                arToolStateData.ToString(),
                placementStateData.ToString());
        }

        public bool Equals(UIARStateData other)
        {
            return arEnabled == other.arEnabled &&
                   instructionUIState == other.instructionUIState &&
                   currentInstructionUI == other.currentInstructionUI &&
                   numProxyInstances == other.numProxyInstances &&
                   instructionUIStep == other.instructionUIStep &&
                   arToolStateData == other.arToolStateData &&
                   placementStateData == other.placementStateData;
        }

        public override bool Equals(object obj)
        {
            return obj is UIARStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = arEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ instructionUIState.GetHashCode();
                hashCode = (hashCode * 397) ^ currentInstructionUI.GetHashCode();
                hashCode = (hashCode * 397) ^ numProxyInstances;
                hashCode = (hashCode * 397) ^ instructionUIStep;
                hashCode = (hashCode * 397) ^ arToolStateData.GetHashCode();
                hashCode = (hashCode * 397) ^ placementStateData.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(UIARStateData a, UIARStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UIARStateData a, UIARStateData b)
        {
            return !(a == b);
        }
    }
}
