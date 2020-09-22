using System.Collections;
using Unity.MARS;
using SharpFlux;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.EventSystems;

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
        [Tooltip("The object to place.")]
        GameObject m_ObjectToPlace;

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
#pragma warning restore CS0649

        private const string instructionAimToPlaceText = "Aim at desired spot and tap on surface or press OK...";

        Vector3 m_ScreenPoint = new Vector3(0.5f, 0.5f, 0.0f);
        public Vector3 m_ObjectToPlaceOffset;

        Camera m_Camera;

        Transform m_CursorTransform;
        Transform m_CameraTransform;

        bool m_ValidTarget = false;

        PlacementTarget m_LastTarget;

        void Start()
        {
            m_Camera = MARS.MARSUtils.MarsRuntimeUtils.GetActiveCamera(true);
            m_CameraTransform = m_Camera.transform;
            m_ScreenPoint = new Vector3( 0.5f, 0.5f, 0.0f);

            if (m_Cursor != null)
            {
                m_CursorTransform = m_Cursor.transform;
                m_Cursor.SetActive(false);
            }

            if (m_ObjectToPlace != null)
                m_ObjectToPlace.SetActive(false);
        }

        void Update()
        {
            if (m_ObjectToPlace == null)
            {
                DisableCursor();
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
                return;
            }
            else
            {
                m_Cursor.SetActive(true);
                if (m_ValidTarget == false)
                    StartCoroutine(InstructionAimToPlace());
            }

            m_ValidTarget = true;

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
                pressed = !EventSystem.current.IsPointerOverGameObject();
            }

            if (pressed)
            {
                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, InstructionUI.ConfirmPlacement));
            }
        }

        IEnumerator InstructionAimToPlace()
        {
            yield return new WaitForSeconds(0);

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI,
                InstructionUI.AimToPlaceBoundingBox));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithLevel,
                new StatusMessageData() { text=instructionAimToPlaceText, level=StatusMessageLevel.Instruction }));
        }

        void DisableCursor()
        {
            m_ValidTarget = false;
            m_ScreenPoint = new Vector3(0.5f, 0.5f, 0.0f);
            m_Cursor.SetActive(false);
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

            // No cursor needed after the object is placed
            DisableCursor();

            // Can't place an object we do not have!
            if (m_ObjectToPlace == null)
                return;

            placementRootTransform.parent = null;
            placementRootTransform.position = m_CursorTransform.position;
            placementRootTransform.rotation = Quaternion.identity;

            var objectTransform = m_ObjectToPlace.transform;

            objectTransform.parent = null;
            // center BoundingBox root
            var offset  = Vector3.Scale( new Vector3(UIStateManager.current.projectStateData.rootBounds.center.x, UIStateManager.current.projectStateData.rootBounds.min.y, UIStateManager.current.projectStateData.rootBounds.center.z), objectTransform.localScale);
            objectTransform.position = m_CursorTransform.position - offset;
            objectTransform.rotation = Quaternion.identity;

            m_ObjectToPlace.transform.SetParent(UIStateManager.current.m_PlacementRoot.transform, true);
            m_ObjectToPlace.SetActive(true);

            m_ObjectToPlace = null;
        }

        /// <summary>
        /// Sets the object that will be placed when the user holds down the input
        /// </summary>
        /// <param name="toPlace">The object to place</param>
        public void SetObjectToPlace(GameObject toPlace)
        {
            m_ObjectToPlace = toPlace;

            if (m_ObjectToPlace != null)
            {
                m_ObjectToPlace.SetActive(false);
            }

        }

        public void SwapModel(GameObject boundingBoxes, GameObject model)
        {
            var boundingBoxesTransform = boundingBoxes.transform;

            var modelTransform = model.transform;
            modelTransform.parent = null;
            modelTransform.position = boundingBoxesTransform.position;
            modelTransform.rotation = boundingBoxesTransform.rotation;
            modelTransform.localScale = boundingBoxesTransform.localScale;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, true));

            var bbTransform = boundingBoxes.transform;
            bbTransform.parent = null;
            bbTransform.position = Vector3.zero;
            bbTransform.rotation = Quaternion.identity;
            bbTransform.localScale = Vector3.one / (float) UIStateManager.current.stateData.modelScale;

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, false));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, InstructionUI.OnBoardingComplete));
        }

        public void RestoreModel(GameObject boundingBoxes, GameObject model)
        {
            var boundingBoxesTransform = boundingBoxes.transform;

            var modelTransform = model.transform;
            modelTransform.parent = null;
            modelTransform.position = Vector3.zero;
            modelTransform.rotation = Quaternion.identity;
            modelTransform.localScale = Vector3.one;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, false));

            var bbTransform = boundingBoxes.transform;
            bbTransform.parent = null;
            bbTransform.position = Vector3.zero;
            bbTransform.rotation = Quaternion.identity;
            bbTransform.localScale = Vector3.one / (float) UIStateManager.current.stateData.modelScale;

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, true));
        }

        public void Reset()
        {
            DisableCursor();
            m_ObjectToPlace = null;
        }

        public void ResetTransformation()
        {
            var placementRootTransform = UIStateManager.current.m_PlacementRoot.transform;

            placementRootTransform.parent = null;
            placementRootTransform.rotation = Quaternion.identity;

            StartCoroutine(ResetScale());
        }

        IEnumerator ResetScale()
        {
            yield return new WaitForSeconds(0);

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, ArchitectureScale.OneToOneHundred));
        }
    }
}

