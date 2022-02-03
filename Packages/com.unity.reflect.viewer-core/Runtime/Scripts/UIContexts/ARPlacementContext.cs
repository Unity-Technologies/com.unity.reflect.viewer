using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IARPlacementDataProvider
    {
        public bool showModel{ get; set; }
        public bool showBoundingBoxModelAction{ get; set; }
        public GameObject modelFloor { get; set; }
        public GameObject firstSelectedPlane { get; set; }
        public GameObject secondSelectedPlane { get; set; }
        public Vector3 modelPlacementLocation { get; set; }
        public GameObject arFloor { get; set; }
        public GameObject firstARSelectedPlane { get; set; }
        public GameObject secondARSelectedPlane { get; set; }
        public Vector3 arPlacementLocation { get; set; }
        public Vector3 arPlacementAlignment { get; set; }
        public SetModelFloorAction.PlacementRule placementRule { get; set; }
        public GameObject placementRulesGameObject { get; set; }
        public bool validTarget { get; set; }
        public float beamHeight { get; set; }
        public SetModelScaleAction.ArchitectureScale modelScale { get; set; }
        public Transform boundingBoxRootNode { get; set; }
        public Transform placementRoot { get; set; }
    }

    public class ARPlacementContext : ContextBase<ARPlacementContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IARPlacementDataProvider)};
    }
}
