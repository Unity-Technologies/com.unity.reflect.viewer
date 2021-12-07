using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ARToolStateData : IEquatable<ARToolStateData>, IARToolStatePropertiesDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool selectionEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool navigationEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool previousStepEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool okEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool cancelEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool scaleEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool wallIndicatorsEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool anchorPointsEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool arWallIndicatorsEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool arAnchorPointsEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool rotateEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool measureToolEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetARToolStateAction.IUIButtonValidator okButtonValidator { get; set; }

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
            okButtonValidator = new SetARToolStateAction.EmptyUIButtonValidator(),
            arWallIndicatorsEnabled = false,
            arAnchorPointsEnabled = false,
            rotateEnabled = false,
            measureToolEnabled = false,
        };

        public ARToolStateData(bool selectionEnabled, bool navigationEnabled, bool previousStepEnabled, bool okEnabled,
            bool cancelEnabled, bool scaleEnabled, bool wallIndicatorsEnabled, bool anchorPointsEnabled, SetARToolStateAction.IUIButtonValidator okButtonValidator,
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

    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ARPlacementStateData : IEquatable<ARPlacementStateData>, IARPlacementDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool showModel { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool showBoundingBoxModelAction { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public GameObject modelFloor { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public GameObject firstSelectedPlane { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public GameObject secondSelectedPlane { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Vector3 modelPlacementLocation { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public GameObject arFloor { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public GameObject firstARSelectedPlane { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public GameObject secondARSelectedPlane { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Vector3 arPlacementLocation { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Vector3 arPlacementAlignment { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetModelFloorAction.PlacementRule placementRule { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool validTarget  { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float beamHeight { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetModelScaleAction.ArchitectureScale modelScale { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Transform placementRoot { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public GameObject placementRulesGameObject { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public List<GameObject> placementRulesPrefabs { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Transform boundingBoxRootNode { get; set; }

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
            placementRule = SetModelFloorAction.PlacementRule.None,
            validTarget = false,
            beamHeight = 0,
            modelScale = SetModelScaleAction.ArchitectureScale.OneToOne
        };

        public ARPlacementStateData(GameObject modelFloor, GameObject firstSelectedPlane, GameObject secondSelectedPlane, Vector3 modelPlacementLocation,
            GameObject arFloor, GameObject firstARSelectedPlane, GameObject secondARSelectedPlane, Vector3 arPlacementLocation,Vector3 arPlacementAlignment,
            SetModelFloorAction.PlacementRule placementRule, bool validTarget, float beamHeight, SetModelScaleAction.ArchitectureScale modelScale, Transform placementRoot,
            GameObject placementRulesGameObject, List<GameObject> placementRulesPrefabs, Transform boundingBoxRootNode)
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
            this.modelScale = modelScale;
            this.placementRoot = placementRoot;
            this.placementRulesGameObject = placementRulesGameObject;
            this.placementRulesPrefabs = placementRulesPrefabs;
            this.boundingBoxRootNode = boundingBoxRootNode;
            showModel = true;
            showBoundingBoxModelAction = false;
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
                Mathf.Approximately(beamHeight,other.beamHeight) &&
                modelScale == other.modelScale &&
                placementRoot == other.placementRoot &&
                placementRulesGameObject == other.placementRulesGameObject &&
                placementRulesPrefabs == other.placementRulesPrefabs &&
                showModel == other.showModel &&
                showBoundingBoxModelAction == other.showBoundingBoxModelAction &&
                boundingBoxRootNode == other.boundingBoxRootNode;
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
                hashCode = (hashCode *397) ^ modelScale.GetHashCode();
                hashCode = (hashCode *397) ^ placementRoot.GetHashCode();
                hashCode = (hashCode *397) ^ placementRulesGameObject.GetHashCode();
                hashCode = (hashCode *397) ^ placementRulesPrefabs.GetHashCode();
                hashCode = (hashCode *397) ^ boundingBoxRootNode.GetHashCode();
                hashCode = (hashCode *397) ^ showModel.GetHashCode();
                hashCode = (hashCode *397) ^ showBoundingBoxModelAction.GetHashCode();
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

    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UIARStateData : IEquatable<UIARStateData>, IARModeDataProvider, IARPlacement<ARPlacementStateData>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool arEnabled  { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetInstructionUIStateAction.InstructionUIState instructionUIState  { get; set; }
        [CreateProperty]
        [field: DontCreateProperty]
        public IARInstructionUI currentARInstructionUI  { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int numProxyInstances  { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int instructionUIStep  { get; set; }
        public ARToolStateData arToolStateData;
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public ARPlacementStateData placementStateData{ get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetARModeAction.ARMode arMode  { get; set; }

        public static readonly UIARStateData defaultData = new UIARStateData()
        {
            arEnabled = false,
            instructionUIState = SetInstructionUIStateAction.InstructionUIState.None,
            currentARInstructionUI = null,
            numProxyInstances = 0,
            instructionUIStep = 0,
            arToolStateData = ARToolStateData.defaultData,
            placementStateData = ARPlacementStateData.defaultData,
            arMode = SetARModeAction.ARMode.None
        };

        public UIARStateData(bool arEnabled, SetInstructionUIStateAction.InstructionUIState instructionUIState, IARInstructionUI currentARInstructionUI, int numProxyInstances,
            int instructionUIStep, ARToolStateData arToolStateData, ARPlacementStateData placementStateData, SetARModeAction.ARMode arMode)
        {
            this.arEnabled = arEnabled;
            this.instructionUIState = instructionUIState;
            this.currentARInstructionUI = currentARInstructionUI;
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
            return ToString("AREnabled{0}, instructionUIState{1}, currentARInstructionUI{2}, umOfProxyInstances{3}, " +
                            "instructionUIStep{4}, ARToolStateData{5}, ARPlacementStateData{6}, ARMode{7}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                arEnabled,
                (object) instructionUIState,
                currentARInstructionUI,
                numProxyInstances,
                instructionUIStep,
                arToolStateData.ToString(),
                placementStateData.ToString(),
                arMode.ToString());
        }

        public bool Equals(UIARStateData other)
        {
            return arEnabled == other.arEnabled &&
                   instructionUIState == other.instructionUIState &&
                   currentARInstructionUI == other.currentARInstructionUI &&
                   numProxyInstances == other.numProxyInstances &&
                   instructionUIStep == other.instructionUIStep &&
                   arToolStateData == other.arToolStateData &&
                   placementStateData == other.placementStateData &&
                   arMode == other.arMode;
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
                hashCode = (hashCode * 397) ^ currentARInstructionUI.GetHashCode();
                hashCode = (hashCode * 397) ^ numProxyInstances.GetHashCode();
                hashCode = (hashCode * 397) ^ instructionUIStep.GetHashCode();
                hashCode = (hashCode * 397) ^ arToolStateData.GetHashCode();
                hashCode = (hashCode * 397) ^ placementStateData.GetHashCode();
                hashCode = (hashCode * 397) ^ arMode.GetHashCode();
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
