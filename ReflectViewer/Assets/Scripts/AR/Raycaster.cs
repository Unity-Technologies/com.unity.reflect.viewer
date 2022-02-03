using System;
using System.Collections;
using Unity.MARS.MARSUtils;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer
{
    /// <summary>
    /// Raycasts against colliders and specialized placement objects
    /// Allows objects to be placed in the given location
    /// </summary>
    public class Raycaster : MonoBehaviour
    {
        static RaycastHit[] s_RaycastResults = new RaycastHit[10];

#pragma warning disable CS0649
        [SerializeField]
        [Tooltip("True drives this raycaster internally via mouse/tap events. Set to false to drive this raycaster via a parent transform and events.")]
        bool m_ScreenMode = true;

        [SerializeField]
        [Tooltip("Gameobject that is set to the hit location of the raycast. Disabled when no target is available.")]
        GameObject m_Cursor;

        [SerializeField]
        [Tooltip("Collider mask for raycast targets.")]
        LayerMask m_Mask = 1;

        [SerializeField]
        [Tooltip("Only allow raycasts against objects with PlacementTarget scripts.")]
        bool m_PlacementTargetsOnly;

        [SerializeField]
        [Tooltip("Maximum distance to check for raycasting.")]
        float m_MaskDistance = 10.0f;

        [SerializeField]
        [Tooltip("If the cursor should align to the raycast surface normal.")]
        bool m_AlignToTargetNormal;

        [SerializeField]
        [Tooltip("If the target is not valid, show this instruction UI canvas.")]
        Canvas m_InstructionUICanvas;
#pragma warning restore CS0649

        Vector3 m_ScreenPoint = new Vector3(0.5f, 0.5f, 0.0f);
        Camera m_Camera;
        Transform m_CursorTransform;
        Transform m_CameraTransform;
        bool m_ValidTarget;
        bool? m_CachedValidTarget;
        PlacementTarget m_LastTarget;
        bool m_ViewBasedMode;
        bool m_ViewBasedPlaceMode;
        Vector3 m_InversePosition;
        Quaternion m_InverseRotation;
        IUISelector<IARInstructionUI> m_ARInstructionUISelector;
        IUISelector<SetModelScaleAction.ArchitectureScale> m_ModelScaleSelector;
        IUISelector<Transform> m_RootSelector;
        IUISelector<bool> m_ARAxisTrackingSelector;
        IUISelector<Transform> m_PlacementRootSelector;
        IUISelector<Transform> m_BoundingBoxRootNodeSelector;
        IUISelector<Bounds> m_ZoneBoundsSelector;

        public bool ValidTarget
        {
            get => m_ValidTarget;
        }

        public PlacementTarget LastTarget { get => m_LastTarget; }

        public bool ActiveScanning { get; set; }

        void Awake()
        {
            m_PlacementRootSelector = UISelectorFactory.createSelector<Transform>(ARPlacementContext.current, nameof(IARPlacementDataProvider.placementRoot));
            m_BoundingBoxRootNodeSelector = UISelectorFactory.createSelector<Transform>(ARPlacementContext.current, nameof(IARPlacementDataProvider.boundingBoxRootNode));
            m_ARInstructionUISelector = UISelectorFactory.createSelector<IARInstructionUI>(ARContext.current, nameof(IARModeDataProvider.currentARInstructionUI));
            m_ModelScaleSelector = UISelectorFactory.createSelector<SetModelScaleAction.ArchitectureScale>(ARPlacementContext.current, nameof(IARPlacementDataProvider.modelScale));
            m_RootSelector = UISelectorFactory.createSelector<Transform>(PipelineContext.current, nameof(IPipelineDataProvider.rootNode));
            m_ARAxisTrackingSelector = UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.ARAxisTrackingEnabled));
            m_ZoneBoundsSelector = UISelectorFactory.createSelector<Bounds>(ProjectContext.current, nameof(IProjectBound.zoneBounds));
        }

        void OnDestroy()
        {
            m_PlacementRootSelector?.Dispose();
            m_BoundingBoxRootNodeSelector?.Dispose();
            m_ARInstructionUISelector?.Dispose();
            m_ModelScaleSelector?.Dispose();
            m_RootSelector?.Dispose();
            m_ARAxisTrackingSelector?.Dispose();
            m_ZoneBoundsSelector?.Dispose();
        }

        void Start()
        {
            m_Camera = MarsRuntimeUtils.GetActiveCamera(true);
            m_CameraTransform = m_Camera.transform;
            m_ScreenPoint = new Vector3( 0.5f, 0.5f, 0.0f);

            if (m_Cursor != null)
            {
                m_CursorTransform = m_Cursor.transform;
                m_Cursor.SetActive(false);
            }
        }

        void Update()
        {
            if (m_ViewBasedMode && m_ViewBasedPlaceMode)
            {
                AlignViewModelWithARView(m_PlacementRootSelector.GetValue());
            }

            if (!ActiveScanning)
            {
                return;
            }

            // Make a ray - set to the last touched screen position in screen mode. Otherwise just mirror the camera
            var screenRay = new Ray(m_CameraTransform.position, m_CameraTransform.forward);

            if (m_ScreenMode)
            {
                var screenCoords = Vector3.Scale(new Vector3(Screen.width, Screen.height, 0.0f), m_ScreenPoint);
                screenRay = m_Camera.ScreenPointToRay(screenCoords, Camera.MonoOrStereoscopicEye.Mono);
            }

            // Use this ray to find any potential colliders
            var foundHits = Physics.RaycastNonAlloc(screenRay, s_RaycastResults, m_MaskDistance, m_Mask);
            var rayHits = foundHits;

            // Filter out non-placement targets if the user has requested it
            var rayCounter = 0;
            if (m_PlacementTargetsOnly)
            {
                while (rayCounter < foundHits)
                {
                    if (s_RaycastResults[rayCounter].collider.GetComponent<PlacementTarget>() == null)
                    {
                        s_RaycastResults[rayCounter].distance = m_MaskDistance + 1.0f;
                        rayHits--;
                    }
                    rayCounter++;
                }
            }

            // No hits? Turn off the target
            if (rayHits <= 0)
            {
                DisableCursor();
                UpdateTargetUI(false);
                return;
            }

            m_Cursor.SetActive(true);
            UpdateTargetUI(true);

            // Find the closest target and align the cursor to that target
            var closestHit = 0;
            var closestHitDistance = m_MaskDistance;
            rayCounter = 0;
            while (rayCounter < foundHits)
            {
                if (s_RaycastResults[rayCounter].distance < closestHitDistance)
                {
                    closestHit = rayCounter;
                    closestHitDistance = s_RaycastResults[rayCounter].distance;
                }
                rayCounter++;
            }

            var closestPlacementTarget = s_RaycastResults[closestHit].collider.GetComponent<PlacementTarget>();
            if (closestPlacementTarget != null && closestPlacementTarget != m_LastTarget)
                closestPlacementTarget.HoverBegin();

            if (m_LastTarget != closestPlacementTarget && m_LastTarget != null)
                m_LastTarget.HoverEnd();

            m_LastTarget = closestPlacementTarget;

            // Align the cursor to this target
            m_CursorTransform.position = s_RaycastResults[closestHit].point;
            var normal = m_AlignToTargetNormal ? s_RaycastResults[closestHit].normal : Vector3.up;
            var forward = m_CameraTransform.forward;
            if (Mathf.Abs(Vector3.Dot(normal, forward)) > 0.5f)
                forward = m_CameraTransform.up;

            m_CursorTransform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.Cross(forward, normal), normal), normal);

            var pressed = Input.GetMouseButtonDown(0);
            if (pressed)
            {
                pressed = !OrphanUIController.isTouchBlockedByUI;
            }

            if (pressed)
            {
                StartCoroutine(MoveNext());
            }
        }

        void UpdateTargetUI(bool valid)
        {
            m_ValidTarget = valid;
            m_InstructionUICanvas.enabled = !valid;

            if (m_CachedValidTarget != m_ValidTarget)
            {
                StartCoroutine(UpdateValidTarget());
                m_CachedValidTarget = m_ValidTarget;
            }
        }

        IEnumerator UpdateValidTarget()
        {
            // TODO: find a better way to update UI
            yield return new WaitForSeconds(0);
            Dispatcher.Dispatch(SetValidTargetAction.From( m_ValidTarget));
        }

        IEnumerator MoveNext()
        {
            // TODO: move to new input system so we don't need this
            yield return new WaitForSeconds(0);
            m_ARInstructionUISelector.GetValue().Next();
        }

        public void DisableCursor()
        {
            m_ScreenPoint = new Vector3(0.5f, 0.5f, 0.0f);
            m_Cursor.SetActive(false);
            m_InstructionUICanvas.enabled = false;
        }

        /// <summary>
        /// Places the loaded object at the cursor's location
        /// Call this method manually if driving via XRI or other non-screen based approaches
        /// </summary>
        public void PlaceObject()
        {
            var placementRootTransform = m_PlacementRootSelector.GetValue();

            // Can't place if we don't have a valid target
            if (!m_ValidTarget && placementRootTransform.position == Vector3.zero)
                return;

            DisableCursor();

            placementRootTransform.position = m_CursorTransform.position;
            placementRootTransform.rotation = Quaternion.identity;

            var boundingBoxRoot = m_BoundingBoxRootNodeSelector.GetValue();
            boundingBoxRoot.gameObject.SetActive(true);

            if (m_ViewBasedMode)
            {
                boundingBoxRoot.localPosition = Vector3.zero;
                m_RootSelector.GetValue().localPosition = Vector3.zero;
                AlignViewModelWithARView(placementRootTransform);
            }
            else
            {
                var offset = GetBoundingBoxOffset();
                boundingBoxRoot.localPosition = -offset;
                m_RootSelector.GetValue().localPosition = -offset;
            }
        }

        public void RestoreModel(Transform boundingBoxes, Transform model)
        {
            model.position = Vector3.zero;
            model.rotation = Quaternion.identity;
            model.localScale = Vector3.one;

            boundingBoxes.position = Vector3.zero;
            boundingBoxes.rotation = Quaternion.identity;
            boundingBoxes.localScale = Vector3.one / (float) m_ModelScaleSelector.GetValue();
        }

        public void Reset()
        {
            DisableCursor();
            m_ValidTarget = false;
            m_CachedValidTarget = null;
            m_ViewBasedMode = false;
            m_ViewBasedPlaceMode = false;
        }

        public void ResetTransformation()
        {
            m_PlacementRootSelector.GetValue().rotation = Quaternion.identity;
            StartCoroutine(ResetScale());
        }

        IEnumerator ResetScale()
        {
            yield return new WaitForSeconds(0);

            Dispatcher.Dispatch(SetModelScaleAction.From(SetModelScaleAction.ArchitectureScale.OneToOneHundred));
        }

        public void SetViewBaseARMode(Transform cameraTransform)
        {
            m_ViewBasedMode = true;
            m_InversePosition = cameraTransform.InverseTransformDirection( - cameraTransform.position);
            m_InverseRotation = Quaternion.Inverse(cameraTransform.rotation);
        }

        public void SetViewBasedPlaceMode(bool value)
        {
            m_ViewBasedPlaceMode = value;
        }

        void AlignViewModelWithARView(Transform objectTransform)
        {
            var transformRotation = m_CameraTransform.rotation;
            m_CameraTransform.rotation = Quaternion.Euler(0, transformRotation.eulerAngles.y, 0);

            var transformPoint = m_CameraTransform.TransformPoint(m_InversePosition);
            transformPoint.y = m_CursorTransform.position.y + 0.01f;
            objectTransform.position = transformPoint;

            Quaternion cameraTransformRotation = m_CameraTransform.rotation * m_InverseRotation;
            objectTransform.rotation = Quaternion.Euler(0, cameraTransformRotation.eulerAngles.y, 0);

            m_CameraTransform.rotation = transformRotation;
        }

        Vector3 GetBoundingBoxOffset()
        {
            var bounds = m_ZoneBoundsSelector.GetValue();
            return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }

        public void AlignModelWithAnchor(Transform model, Vector3 modelPlaneNormal, Vector3 arPlaneNormal, Vector3 modelAnchor, Vector3 arAnchor)
        {
            var distance = modelAnchor - model.transform.position;
            model.position = arAnchor - distance;

            // orient model with first plane
            float differenceAngle = Vector3.SignedAngle(modelPlaneNormal, arPlaneNormal, Vector3.up);
            model.RotateAround(arAnchor, Vector3.up, differenceAngle);

            if (m_ARAxisTrackingSelector.GetValue())
            {
                Dispatcher.Dispatch(SetStatusMessageWithType.From(
                    new StatusMessageData { text=$"the AR Alignment angle is {differenceAngle}", type= StatusMessageType.Instruction }));
            }
        }
    }
}

