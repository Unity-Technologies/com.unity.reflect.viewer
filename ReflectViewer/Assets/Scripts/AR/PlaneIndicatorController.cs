using System;
using Unity.MARS.MARSUtils;
using UnityEngine;
using UnityEngine.Reflect;

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

        ARToolStateData? m_CachedARToolStateData;
        ARPlacementStateData? m_CachedARPlacementStateData;
        UIProjectStateData ? m_CachedProjectStateData;
        Camera m_Camera;

        MeshRenderer m_MeshRendererA;
        MeshRenderer m_MeshRendererB;

        Vector3 m_HitpointA;
        Vector3 m_HitpointB;

        const float k_BeamHeightFactor = 1.5f;

        // Start is called before the first frame update
        void OnEnable()
        {
            UIStateManager.arStateChanged += OnARStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateChanged;
        }

        void OnProjectStateChanged(UIProjectStateData projectData)
        {
            if (m_CachedProjectStateData != projectData && m_CachedARToolStateData != null)
            {
                m_CachedProjectStateData = projectData;
                UpdatePlacementState();
            }
        }

        void OnARStateDataChanged(UIARStateData arData)
        {
            bool somethingChanged = false;
            if (m_CachedARToolStateData != arData.arToolStateData)
            {
                m_CachedARToolStateData = arData.arToolStateData;
                somethingChanged = true;
            }

            if (m_CachedARPlacementStateData != arData.placementStateData)
            {
                m_CachedARPlacementStateData = arData.placementStateData;
                somethingChanged = true;
            }

            if (somethingChanged)
                UpdatePlacementState();
        }

        void UpdatePlacementState()
        {
            if (m_CachedARPlacementStateData == null)
                return;

            var placementData = m_CachedARPlacementStateData.Value;
            if (placementData.modelFloor != null)
            {
                placementData.modelFloor.transform.parent = m_ModelPlanesRoot.transform;
            }

            if (m_CachedARToolStateData.Value.wallIndicatorsEnabled)
            {
                m_ModelPlanesRoot.SetActive(true);
                if (placementData.firstSelectedPlane == null)
                {
                    m_PlaneAIndicator?.gameObject.SetActive(false);
                    m_HitpointA = Vector3.zero;
                }
                else
                {
                    placementData.firstSelectedPlane.transform.parent = m_ModelPlanesRoot.transform;
                    placementData.firstSelectedPlane.SetLayerRecursively(LayerMask.NameToLayer("PlaneSelection"));
                    m_PlaneAIndicator?.gameObject.SetActive(true);
                    m_HitpointA = placementData.firstSelectedPlane.GetComponent<PlaneSelectionContext>().SelectionContextList[0].HitPoint;
                }

                if (placementData.secondSelectedPlane == null)
                {
                    m_PlaneBIndicator?.gameObject.SetActive(false);
                    m_HitpointB = Vector3.zero;
                }
                else
                {
                    placementData.secondSelectedPlane.transform.parent = m_ModelPlanesRoot.transform;
                    placementData.secondSelectedPlane.SetLayerRecursively(LayerMask.NameToLayer("PlaneSelection"));
                    m_PlaneBIndicator?.gameObject.SetActive(true);
                    m_HitpointB = placementData.secondSelectedPlane.GetComponent<PlaneSelectionContext>().SelectionContextList[0].HitPoint;
                }
            }
            else
            {
                m_ModelPlanesRoot.SetActive(false);
                // indicators can show model or AR planes, not both at the same time
                if (m_CachedARToolStateData.Value.arWallIndicatorsEnabled)
                {
                    m_ARPlanesRoot.SetActive(true);
                    if (placementData.firstARSelectedPlane == null)
                    {
                        m_PlaneAIndicator.gameObject.SetActive(false);
                        m_MeshRendererA = null;
                    }
                    else
                    {
                        placementData.firstARSelectedPlane.transform.parent = m_ARPlanesRoot.transform;
                        placementData.firstARSelectedPlane.SetLayerRecursively(LayerMask.NameToLayer("PlaneSelection"));
                        m_PlaneAIndicator.gameObject.SetActive(true);
                        m_MeshRendererA = placementData.firstARSelectedPlane.GetComponent<MeshRenderer>();
                    }

                    if (placementData.secondARSelectedPlane == null)
                    {
                        m_PlaneBIndicator.gameObject.SetActive(false);
                        m_MeshRendererB = null;
                    }
                    else
                    {
                        placementData.secondARSelectedPlane.transform.parent = m_ARPlanesRoot.transform;
                        placementData.secondARSelectedPlane.SetLayerRecursively(LayerMask.NameToLayer("PlaneSelection"));
                        m_PlaneBIndicator.gameObject.SetActive(true);
                        m_MeshRendererB = placementData.secondARSelectedPlane.GetComponent<MeshRenderer>();
                    }

                    if (placementData.arFloor != null)
                    {
                        // we don't show the ar floor
                        placementData.arFloor.SetActive(false);
                        placementData.arFloor.transform.parent = m_ARPlanesRoot.transform;
                    }
                }
                else
                {
                    m_ARPlanesRoot.SetActive(false);
                    m_PlaneAIndicator.gameObject.SetActive(false);
                    m_PlaneBIndicator.gameObject.SetActive(false);
                }
            }

            if (m_CachedARToolStateData.Value.anchorPointsEnabled)
            {
                if (placementData.modelPlacementLocation == Vector3.zero)
                {
                    m_AnchorPointIndicator.gameObject.SetActive(false);
                }
                else
                {
                    m_AnchorPointIndicator.transform.position = placementData.modelPlacementLocation;
                    m_AnchorPointIndicator.beamHeight = k_BeamHeightFactor * placementData.beamHeight;
                    m_AnchorPointIndicator.gameObject.SetActive(true);
                }
            }
            else
            {
                if (m_CachedARToolStateData.Value.arAnchorPointsEnabled)
                {
                    if (placementData.arPlacementLocation == Vector3.zero)
                    {
                        m_AnchorPointIndicator.gameObject.SetActive(false);
                    }
                    else
                    {
                        m_AnchorPointIndicator.transform.position = placementData.arPlacementLocation;
                        m_AnchorPointIndicator.beamHeight = k_BeamHeightFactor * placementData.beamHeight;
                        m_AnchorPointIndicator.gameObject.SetActive(true);
                    }
                }
                else
                {
                    m_AnchorPointIndicator.gameObject.SetActive(false);
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
            if (m_CachedARToolStateData != null && m_CachedARToolStateData.Value.wallIndicatorsEnabled)
            {
                // Align indicator to the selected point on the plane
                if (m_CachedARPlacementStateData?.firstSelectedPlane != null && m_HitpointA != Vector3.zero)
                {
                    var pos = RectTransformUtility.WorldToScreenPoint (m_Camera, m_HitpointA);
                    m_PlaneAIndicator.position = pos;
                }

                if (m_CachedARPlacementStateData?.secondSelectedPlane != null && m_HitpointB != Vector3.zero)
                {
                    var pos = RectTransformUtility.WorldToScreenPoint (m_Camera, m_HitpointB);
                    m_PlaneBIndicator.position = pos;
                }
            }
            else
            {
                if (m_CachedARToolStateData != null && m_CachedARToolStateData.Value.arWallIndicatorsEnabled)
                {
                    // Align indicator to the selected point on the plane
                    if (m_CachedARPlacementStateData?.firstARSelectedPlane != null && m_MeshRendererA != null)
                    {
                        var center = m_MeshRendererA.bounds.center;
                        var pos = RectTransformUtility.WorldToScreenPoint (m_Camera, center);
                        m_PlaneAIndicator.position = pos;
                    }

                    if (m_CachedARPlacementStateData?.secondARSelectedPlane != null && m_MeshRendererB != null)
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
            UIStateManager.arStateChanged -= OnARStateDataChanged;
            UIStateManager.projectStateChanged -= OnProjectStateChanged;
        }
    }
}
