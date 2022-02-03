using System;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Viewer.UI;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer
{
    public class ARAlignmentObject : MonoBehaviour, IAlignmentObject
    {
        [SerializeField]
        MarkerController m_MarkerController;

        IUISelector<Transform> m_RootGetter;
        IUISelector<Transform> m_PlacementRootGetter;
        IUISelector<Transform> m_BoundingBoxRootNodeGetter;
        IUISelector<bool> m_ARModeGetter;

        void Awake()
        {
            m_PlacementRootGetter = UISelectorFactory.createSelector<Transform>(ARPlacementContext.current, "placementRoot");
            m_BoundingBoxRootNodeGetter = UISelectorFactory.createSelector<Transform>(ARPlacementContext.current, "boundingBoxRootNode");
            m_RootGetter = UISelectorFactory.createSelector<Transform>(PipelineContext.current, "rootNode");
            m_ARModeGetter = UISelectorFactory.createSelector<bool>(ARContext.current, nameof(IARModeDataProvider.arEnabled));
            m_MarkerController.AlignedObject = this;
        }

        void OnDestroy()
        {
            m_RootGetter?.Dispose();
            m_PlacementRootGetter?.Dispose();
            m_BoundingBoxRootNodeGetter?.Dispose();
            m_ARModeGetter?.Dispose();
        }

        public void Move(TransformData worldSpaceTransformData)
        {
            // Don't move if not in AR
            if (!m_ARModeGetter.GetValue())
                return;
            // Zero the bounding boxes & children
            var boundingBoxRoot = m_BoundingBoxRootNodeGetter.GetValue();
            boundingBoxRoot.gameObject.SetActive(true);
            boundingBoxRoot.localPosition = Vector3.zero;
            m_RootGetter.GetValue().localPosition = Vector3.zero;
            // Move the placement root
            var obj = m_PlacementRootGetter.GetValue().transform;
            obj.localScale = worldSpaceTransformData.scale;
            obj.rotation = worldSpaceTransformData.rotation;
            obj.position = worldSpaceTransformData.position;
        }

        public TransformData Get()
        {
            return new TransformData(m_PlacementRootGetter.GetValue());
        }

        public Transform Transform
        {
            get => m_PlacementRootGetter.GetValue().transform;
        }
    }
}
