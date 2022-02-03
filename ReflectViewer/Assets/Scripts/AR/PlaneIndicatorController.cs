using System;
using System.Collections.Generic;
using Unity.MARS.MARSUtils;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.UI
{
    public class PlaneIndicatorController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        GameObject m_ModelPlanesRoot;
        [SerializeField]
        GameObject m_ARPlanesRoot;
        [SerializeField]
        RectTransform m_PlaneAIndicator;
        [SerializeField]
        RectTransform m_PlaneBIndicator;
        [SerializeField]
        AnchorPoint m_AnchorPointIndicator;
#pragma warning restore CS0649

        Camera m_Camera;

        MeshRenderer m_MeshRendererA;
        MeshRenderer m_MeshRendererB;

        Vector3 m_HitpointA;
        Vector3 m_HitpointB;

        const float k_BeamHeightFactor = 1.5f;

        List<IDisposable> m_DisposableUISelectors = new List<IDisposable>();

        bool m_IsGetterReady = false;
        int m_PlaneSelectionLayer;

        // ARPlacementContext
        IUISelector<GameObject> m_ModelFloorSelector;
        IUISelector<GameObject> m_FirstSelectedPlaneSelector;
        IUISelector<GameObject> m_SecondSelectedPlaneSelector;
        IUISelector<GameObject> m_FirstARSelectedPlaneSelector;
        IUISelector<GameObject> m_SecondARSelectedPlaneSelector;
        IUISelector<GameObject> m_ARFloorSelector;
        IUISelector<Vector3> m_ModelPlacementLocationSelector;
        IUISelector<Vector3> m_ArPlacementLocationSelector;
        IUISelector<float> m_BeamHeightSelector;

        GameObject m_FirstSelectedPlane;
        GameObject m_SecondSelectedPlane;
        GameObject m_FirstARSelectedPlane;
        GameObject m_SecondARSelectedPlane;

        // ARToolStateContext
        IUISelector<bool> m_WallIndicatorEnabledSelector;
        IUISelector<bool> m_ARWallIndicatorEnabledSelector;
        IUISelector<bool> m_AnchorPointsEnabledSelector;
        IUISelector<bool> m_ARAnchorPointsEnabledSelector;

        bool m_WallIndicatorEnabled;
        bool m_ARWallIndicatorEnabled;

        // Start is called before the first frame update
        void OnEnable()
        {
            m_IsGetterReady = false;

            m_PlaneSelectionLayer = m_PlaneSelectionLayer;

            // ARPlacementContext
            m_DisposableUISelectors.Add(m_ModelFloorSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current,
                nameof(IARPlacementDataProvider.modelFloor), data =>
                {
                    if (data != null)
                    {
                        data.transform.parent = m_ModelPlanesRoot.transform;
                    }
                }));
            m_DisposableUISelectors.Add(m_FirstSelectedPlaneSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current,
                nameof(IARPlacementDataProvider.firstSelectedPlane), data =>
                {
                    m_FirstSelectedPlane = data;
                    UpdatePlacementState();
                }));
            m_DisposableUISelectors.Add(m_SecondSelectedPlaneSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current,
                nameof(IARPlacementDataProvider.secondSelectedPlane), data =>
                {
                    m_SecondSelectedPlane = data;
                    UpdatePlacementState();
                }));
            m_DisposableUISelectors.Add(m_FirstARSelectedPlaneSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current,
                nameof(IARPlacementDataProvider.firstARSelectedPlane), data =>
                {
                    m_FirstARSelectedPlane = data;
                    UpdatePlacementState();
                }));
            m_DisposableUISelectors.Add(m_SecondARSelectedPlaneSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current,
                nameof(IARPlacementDataProvider.secondARSelectedPlane), data =>
                {
                    m_SecondARSelectedPlane = data;
                    UpdatePlacementState();
                }));
            m_DisposableUISelectors.Add(m_ARFloorSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current,
                nameof(IARPlacementDataProvider.arFloor), data => { UpdatePlacementState(); }));
            m_DisposableUISelectors.Add(m_ModelPlacementLocationSelector = UISelectorFactory.createSelector<Vector3>(ARPlacementContext.current,
                nameof(IARPlacementDataProvider.modelPlacementLocation), data => { UpdateAnchorPoints(); }));
            m_DisposableUISelectors.Add(m_ArPlacementLocationSelector = UISelectorFactory.createSelector<Vector3>(ARPlacementContext.current,
                nameof(IARPlacementDataProvider.arPlacementLocation), data => { UpdateAnchorPoints(); }));
            m_DisposableUISelectors.Add(m_BeamHeightSelector = UISelectorFactory.createSelector<float>(ARPlacementContext.current,
                nameof(IARPlacementDataProvider.beamHeight), data => { UpdateAnchorPoints(); }));
            // ARToolStateContext
            m_DisposableUISelectors.Add(m_WallIndicatorEnabledSelector = UISelectorFactory.createSelector<bool>(ARToolStateContext.current,
                nameof(IARToolStatePropertiesDataProvider.wallIndicatorsEnabled),
                data =>
                {
                    m_WallIndicatorEnabled = data;
                    UpdatePlacementState();
                }));
            m_DisposableUISelectors.Add(m_ARWallIndicatorEnabledSelector = UISelectorFactory.createSelector<bool>(ARToolStateContext.current,
                nameof(IARToolStatePropertiesDataProvider.arWallIndicatorsEnabled),
                data =>
                {
                    m_ARWallIndicatorEnabled = data;
                    UpdatePlacementState();
                }));
            m_DisposableUISelectors.Add(m_AnchorPointsEnabledSelector = UISelectorFactory.createSelector<bool>(ARToolStateContext.current,
                nameof(IARToolStatePropertiesDataProvider.anchorPointsEnabled),
                data => { UpdateAnchorPoints(); }));
            m_DisposableUISelectors.Add(m_ARAnchorPointsEnabledSelector = UISelectorFactory.createSelector<bool>(ARToolStateContext.current,
                nameof(IARToolStatePropertiesDataProvider.arWallIndicatorsEnabled),
                data => { UpdateAnchorPoints(); }));
            m_DisposableUISelectors.Add(UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.selectionEnabled), data => { UpdatePlacementState(); }));

            m_IsGetterReady = true;

            UpdateAnchorPoints();
            UpdatePlacementState();
            ProjectContext.current.stateChanged += UpdatePlacementState;
        }

        void UpdateAnchorPoints()
        {
            if (!m_IsGetterReady)
                return;

            if (m_AnchorPointsEnabledSelector.GetValue())
            {
                if (m_ModelPlacementLocationSelector.GetValue() == Vector3.zero)
                {
                    m_AnchorPointIndicator.gameObject.SetActive(false);
                }
                else
                {
                    m_AnchorPointIndicator.transform.position = m_ModelPlacementLocationSelector.GetValue();
                    m_AnchorPointIndicator.beamHeight = k_BeamHeightFactor * m_BeamHeightSelector.GetValue();
                    m_AnchorPointIndicator.gameObject.SetActive(true);
                }
            }
            else
            {
                if (m_ARAnchorPointsEnabledSelector.GetValue())
                {
                    if (m_ArPlacementLocationSelector.GetValue() == Vector3.zero)
                    {
                        m_AnchorPointIndicator.gameObject.SetActive(false);
                    }
                    else
                    {
                        m_AnchorPointIndicator.transform.position = m_ArPlacementLocationSelector.GetValue();
                        m_AnchorPointIndicator.beamHeight = k_BeamHeightFactor * m_BeamHeightSelector.GetValue();
                        m_AnchorPointIndicator.gameObject.SetActive(true);
                    }
                }
                else
                {
                    m_AnchorPointIndicator.gameObject.SetActive(false);
                }
            }
        }

        void UpdatePlacementState()
        {
            if (!m_IsGetterReady)
                return;

            var modelFloor = m_ModelFloorSelector.GetValue();
            if (modelFloor != null)
            {
                modelFloor.transform.parent = m_ModelPlanesRoot.transform;
            }

            if (m_WallIndicatorEnabledSelector.GetValue())
            {
                m_ModelPlanesRoot.SetActive(true);
                var firstSelectedPlane = m_FirstSelectedPlaneSelector.GetValue();
                if (firstSelectedPlane == null)
                {
                    m_PlaneAIndicator.gameObject.SetActive(false);
                    m_HitpointA = Vector3.zero;
                }
                else
                {
                    firstSelectedPlane.transform.parent = m_ModelPlanesRoot.transform;
                    firstSelectedPlane.SetLayerRecursively(m_PlaneSelectionLayer);
                    m_PlaneAIndicator.gameObject.SetActive(true);
                    m_HitpointA = firstSelectedPlane.GetComponent<PlaneSelectionContext>().SelectionContextList[0].HitPoint;
                }

                var secondSelectedPlane = m_SecondSelectedPlaneSelector.GetValue();
                if (secondSelectedPlane == null)
                {
                    m_PlaneBIndicator.gameObject.SetActive(false);
                    m_HitpointB = Vector3.zero;
                }
                else
                {
                    secondSelectedPlane.transform.parent = m_ModelPlanesRoot.transform;
                    secondSelectedPlane.SetLayerRecursively(m_PlaneSelectionLayer);
                    m_PlaneBIndicator.gameObject.SetActive(true);
                    m_HitpointB = secondSelectedPlane.GetComponent<PlaneSelectionContext>().SelectionContextList[0].HitPoint;
                }
            }
            else
            {
                m_ModelPlanesRoot.SetActive(false);
                // indicators can show model or AR planes, not both at the same time
                if (m_ARWallIndicatorEnabledSelector.GetValue())
                {
                    m_ARPlanesRoot.SetActive(true);
                    var firstARSelectedPlane = m_FirstARSelectedPlaneSelector.GetValue();
                    if (firstARSelectedPlane == null)
                    {
                        m_PlaneAIndicator.gameObject.SetActive(false);
                        m_MeshRendererA = null;
                    }
                    else
                    {
                        firstARSelectedPlane.transform.parent = m_ARPlanesRoot.transform;
                        firstARSelectedPlane.SetLayerRecursively(m_PlaneSelectionLayer);
                        m_PlaneAIndicator.gameObject.SetActive(true);
                        m_MeshRendererA = firstARSelectedPlane.GetComponent<MeshRenderer>();
                    }

                    var secondARSelectedPlane = m_SecondARSelectedPlaneSelector.GetValue();
                    if (secondARSelectedPlane == null)
                    {
                        m_PlaneBIndicator.gameObject.SetActive(false);
                        m_MeshRendererB = null;
                    }
                    else
                    {
                        secondARSelectedPlane.transform.parent = m_ARPlanesRoot.transform;
                        secondARSelectedPlane.SetLayerRecursively(m_PlaneSelectionLayer);
                        m_PlaneBIndicator.gameObject.SetActive(true);
                        m_MeshRendererB = secondARSelectedPlane.GetComponent<MeshRenderer>();
                    }

                    var arFloor = m_ARFloorSelector.GetValue();
                    if (arFloor != null)
                    {
                        // we don't show the ar floor
                        arFloor.SetActive(false);
                        arFloor.transform.parent = m_ARPlanesRoot.transform;
                    }
                }
                else
                {
                    m_ARPlanesRoot.SetActive(false);
                    m_PlaneAIndicator.gameObject.SetActive(false);
                    m_PlaneBIndicator.gameObject.SetActive(false);
                }
            }

        }

        void Start()
        {
            m_Camera = MarsRuntimeUtils.GetActiveCamera(true);
        }

        // Update is called once per frame
        void Update()
        {

            if (m_WallIndicatorEnabled)
            {
                // Align indicator to the selected point on the plane
                if (m_FirstSelectedPlane != null && m_HitpointA != Vector3.zero)
                {
                    var pos = RectTransformUtility.WorldToScreenPoint (m_Camera, m_HitpointA);
                    m_PlaneAIndicator.position = pos;
                }

                if (m_SecondSelectedPlane != null && m_HitpointB != Vector3.zero)
                {
                    var pos = RectTransformUtility.WorldToScreenPoint (m_Camera, m_HitpointB);
                    m_PlaneBIndicator.position = pos;
                }
            }
            else
            {
                if (m_ARWallIndicatorEnabled)
                {
                    // Align indicator to the selected point on the plane
                    if (m_FirstARSelectedPlane != null && m_MeshRendererA != null)
                    {
                        var center = m_MeshRendererA.bounds.center;
                        var pos = RectTransformUtility.WorldToScreenPoint (m_Camera, center);
                        m_PlaneAIndicator.position = pos;
                    }

                    if (m_SecondARSelectedPlane != null && m_MeshRendererB != null)
                    {
                        var center = m_MeshRendererB.bounds.center;
                        var pos = RectTransformUtility.WorldToScreenPoint (m_Camera, center);
                        m_PlaneBIndicator.position = pos;
                    }
                }
            }
        }

        void OnDisable()
        {
            foreach (var disposable in m_DisposableUISelectors)
            {
                disposable.Dispose();
            }
            m_DisposableUISelectors.Clear();

            ProjectContext.current.stateChanged -= UpdatePlacementState;
        }
    }
}
