using System.Collections;
using Unity.MARS.MARSUtils;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.UI;
using UnityEngine;

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

        public bool ValidTarget
        {
            get => m_ValidTarget;
        }

        PlacementTarget m_LastTarget;
        public PlacementTarget LastTarget { get => m_LastTarget; }

        public bool ActiveScanning { get; set; }

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
                AlignViewModelWithARView(UIStateManager.current.m_PlacementRoot.transform);
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
            var placementData = UIStateManager.current.arStateData.placementStateData;
            placementData.validTarget = m_ValidTarget;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementState, placementData));
        }

        IEnumerator MoveNext()
        {
            // TODO: move to new input system so we don't need this
            yield return new WaitForSeconds(0);
            UIStateManager.current.arStateData.currentInstructionUI.Next();
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
            var placementRootTransform = UIStateManager.current.m_PlacementRoot.transform;

            // Can't place if we don't have a valid target
            if (!m_ValidTarget && placementRootTransform.position == Vector3.zero)
                return;

            DisableCursor();

            placementRootTransform.position = m_CursorTransform.position;
            placementRootTransform.rotation = Quaternion.identity;

            var boundingBoxRoot = UIStateManager.current.m_BoundingBoxRootNode;
            boundingBoxRoot.SetActive(true);

            var modelRootTransform = UIStateManager.current.m_RootNode.transform;

            if (m_ViewBasedMode)
            {
                boundingBoxRoot.transform.localPosition = Vector3.zero;
                modelRootTransform.localPosition = Vector3.zero;
                AlignViewModelWithARView(placementRootTransform);
            }
            else
            {
                var offset = GetBoundingBoxOffset();
                boundingBoxRoot.transform.localPosition = -offset;
                modelRootTransform.localPosition = -offset;
            }
        }

        public void RestoreModel(GameObject boundingBoxes, GameObject model)
        {
            var modelTransform = model.transform;
            modelTransform.position = Vector3.zero;
            modelTransform.rotation = Quaternion.identity;
            modelTransform.localScale = Vector3.one;

            var bbTransform = boundingBoxes.transform;
            bbTransform.position = Vector3.zero;
            bbTransform.rotation = Quaternion.identity;
            bbTransform.localScale = Vector3.one / (float) UIStateManager.current.stateData.modelScale;
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
            var placementRootTransform = UIStateManager.current.m_PlacementRoot.transform;
            placementRootTransform.rotation = Quaternion.identity;
            StartCoroutine(ResetScale());
        }

        IEnumerator ResetScale()
        {
            yield return new WaitForSeconds(0);

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, ArchitectureScale.OneToOneHundred));
        }

        bool m_ViewBasedMode;
        bool m_ViewBasedPlaceMode;
        Vector3 m_InversePosition;
        Quaternion m_InverseRotation;
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
            return new Vector3(UIStateManager.current.projectStateData.rootBounds.center.x,
                UIStateManager.current.projectStateData.rootBounds.min.y,
                UIStateManager.current.projectStateData.rootBounds.center.z);
        }

        public void AlignModelWithAnchor(GameObject model, Vector3 modelPlaneNormal, Vector3 arPlaneNormal, Vector3 modelAnchor, Vector3 arAnchor)
        {
            var distance = modelAnchor - model.transform.position;
            model.transform.position = arAnchor - distance;

            // orient model with first plane
            float differenceAngle = Vector3.SignedAngle(modelPlaneNormal, arPlaneNormal, Vector3.up);
            model.transform.RotateAround(arAnchor, Vector3.up, differenceAngle);
            if (UIStateManager.current.debugStateData.debugOptionsData.ARAxisTrackingEnabled)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                    new StatusMessageData { text=$"the AR Alignment angle is {differenceAngle}", type= StatusMessageType.Instruction }));
            }
        }
    }
}

