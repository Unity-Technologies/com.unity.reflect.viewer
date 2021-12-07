using System;
using Unity.SpatialFramework.Input;
using Unity.SpatialFramework.Interaction;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// A manipulator that converts handle drags into forces applied to a rigidbody on the GameObject.
    /// The handle is scaled and rotated to match the bounds of the targets.
    /// </summary>
    public class PhysicsBasedManipulator : MonoBehaviour, IManipulator, IUsesXRPointers
    {
#pragma warning disable 649
        [SerializeField, Tooltip("The handle whose translate events will be converted into translational force on the rigidbody")]
        BaseHandle m_Handle;

        [SerializeField, Tooltip("(Optional) A scroll controller script. Vertical scroll will scale the manipulated target, horizontal scroll will rotate it around the Y axis.")]
        ScrollInputController m_ScrollInputController;

        [SerializeField, Tooltip("If enabled, the handle will be positioned and scaled to encompass the world bounds of the target objects (faster). Otherwise it will be oriented to match the active object's first mesh's rotation.")]
        bool m_UseWorldBounds;

        [SerializeField, Tooltip("The magnitude of the force applied to rigidbody for every unit the handle is dragged per second.")]
        float m_DragForceMagnitude = 100f;

        [SerializeField, Tooltip("The amount of degrees of rotation that will be applied for every 1 unit of scrolling per second.")]
        float m_DegreesPerScroll = 90f;

        [SerializeField, Tooltip("The minimum amount the handle must be attempted to be moved in order to start actually moving the manipulator. " +
             "This makes it easier to select an object to use scroll Scale and Rotate without accidentally moving it.")]
        float m_MoveThreshold = 0.01f;

        [SerializeField, Tooltip("The mode that is used to apply force to the rigidbody. By default, this is VelocityChange so that the rigidbody mass is ignored.")]
        ForceMode m_ForceMode = ForceMode.VelocityChange;
#pragma warning restore 649

        bool m_CollisionDisabled = true;
        bool m_Dragging;
        Rigidbody m_Rigidbody;
        Vector3 m_StartPosition;
        Vector3 m_CurrentHandleDelta;
        Vector3 m_PreviousPosition;
        Transform m_ActiveTransform;
        Transform[] m_SelectionTransforms;

        public event Action<Vector3> translate;
        public event Action<Quaternion> rotate;
        public event Action<Vector3> scale;
        public event Action dragStarted;
        public event Action dragEnded;
        public bool dragging => m_Dragging;

        IProvidesXRPointers IFunctionalitySubscriber<IProvidesXRPointers>.provider { get; set; }

        void IManipulator.SetTargetTransforms(Transform[] targetTransforms, Transform activeTargetTransform)
        {
            m_SelectionTransforms = targetTransforms;
            m_ActiveTransform = activeTargetTransform;
            var noSelection = m_SelectionTransforms == null || m_SelectionTransforms.Length <= 0;

            if (noSelection)
            {
                m_Handle.gameObject.SetActive(false);
            }
            else if (!m_Handle.gameObject.activeSelf)
            {
                m_Handle.gameObject.SetActive(true);
            }

            UpdateBoundingBox();
        }

        void IManipulator.SetPose(Vector3 position, Quaternion rotation)
        {
            var manipulatorTransform = transform;

            if (!m_Dragging)
            {
                manipulatorTransform.position = position;
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }
        }

        void UpdateBoundingBox()
        {
            var activeMesh = m_ActiveTransform != null ? m_ActiveTransform.GetComponentInChildren<MeshFilter>() : null;
            Bounds selectionBounds;
            Quaternion selectionBoundsRotation;
            Vector3 boundsSize;
            Vector3 boundsWorldCenter;

            if (m_UseWorldBounds || activeMesh == null)
            {
                var noSelection = m_SelectionTransforms == null || m_SelectionTransforms.Length <= 0;
                selectionBounds = noSelection ? new Bounds() : BoundsUtils.GetBounds(m_SelectionTransforms);
                selectionBoundsRotation = Quaternion.identity;
                boundsWorldCenter = noSelection ? Vector3.zero : selectionBounds.center;
                boundsSize = noSelection ? Vector3.zero : selectionBounds.size;
            }
            else
            {
                // Using the rotation of the active transform's first mesh, expand the bounds to encapsulate all other selected objects
                var activeMeshTransform = activeMesh.transform;

                // Corners of other objects will be converted from world to the active mesh's space
                var worldToLocalMatrix = activeMeshTransform.worldToLocalMatrix;
                selectionBounds = activeMesh.mesh.bounds;
                selectionBoundsRotation = activeMeshTransform.rotation;

                foreach (var selectedTransform in m_SelectionTransforms)
                {
                    var selectionMeshes = selectedTransform.GetComponentsInChildren<MeshFilter>();
                    foreach (var selectedMesh in selectionMeshes)
                    {
                        if (selectedMesh == activeMesh)
                            continue;

                        EncapsulateMesh(selectedMesh, worldToLocalMatrix, ref selectionBounds);
                    }
                }

                var toWorldMatrix = activeMeshTransform.localToWorldMatrix;
                boundsWorldCenter = toWorldMatrix.MultiplyPoint3x4(selectionBounds.center);
                boundsSize = Vector3.Scale(selectionBounds.size, toWorldMatrix.lossyScale);
            }

            var boundsTransform = m_Handle.transform;
            boundsTransform.localScale = boundsSize.Abs();
            boundsTransform.localPosition = boundsWorldCenter - transform.position;
            boundsTransform.rotation = selectionBoundsRotation;
        }

        static void EncapsulateMesh(MeshFilter selectedMesh, Matrix4x4 worldToActiveBoundsMatrix, ref Bounds selectionBounds)
        {
            var meshLocalToWorldMatrix = selectedMesh.transform.localToWorldMatrix;
            var bounds = selectedMesh.mesh.bounds;

            var meshLocalToActiveBoundsMatrix = worldToActiveBoundsMatrix * meshLocalToWorldMatrix;
            var size = bounds.size;

            // Back 4 corners
            // Note can use faster 3x4 version of MultiplyPoint because matrix does not include projection
            var corner = bounds.min;
            var cornerInWorldSpace = meshLocalToActiveBoundsMatrix.MultiplyPoint3x4(corner);
            selectionBounds.Encapsulate(cornerInWorldSpace);
            corner.x += size.x;
            cornerInWorldSpace = meshLocalToActiveBoundsMatrix.MultiplyPoint3x4(corner);
            selectionBounds.Encapsulate(cornerInWorldSpace);
            corner.y += size.y;
            cornerInWorldSpace = meshLocalToActiveBoundsMatrix.MultiplyPoint3x4(corner);
            selectionBounds.Encapsulate(cornerInWorldSpace);
            corner.x -= size.x;
            cornerInWorldSpace = meshLocalToActiveBoundsMatrix.MultiplyPoint3x4(corner);
            selectionBounds.Encapsulate(cornerInWorldSpace);
            corner.y -= size.y;

            // Front 4 corners
            corner.z += size.z;
            cornerInWorldSpace = meshLocalToActiveBoundsMatrix.MultiplyPoint3x4(corner);
            selectionBounds.Encapsulate(cornerInWorldSpace);
            corner.x += size.x;
            cornerInWorldSpace = meshLocalToActiveBoundsMatrix.MultiplyPoint3x4(corner);
            selectionBounds.Encapsulate(cornerInWorldSpace);
            corner.y += size.y;
            cornerInWorldSpace = meshLocalToActiveBoundsMatrix.MultiplyPoint3x4(corner);
            selectionBounds.Encapsulate(cornerInWorldSpace);
            corner.x -= size.x;
            cornerInWorldSpace = meshLocalToActiveBoundsMatrix.MultiplyPoint3x4(corner);
            selectionBounds.Encapsulate(cornerInWorldSpace);
        }

        void Awake()
        {
            m_Handle.dragStarted += OnHandleDragStarted;
            m_Handle.dragging += OnHandleDragging;
            m_Handle.dragEnded += OnHandleDragEnded;

            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            m_Rigidbody.freezeRotation = true;
            m_Rigidbody.useGravity = false;
            m_Dragging = false;
            m_Rigidbody.isKinematic = !m_Dragging;
        }

        void OnHandleDragStarted(BaseHandle handle, HandleEventData eventData)
        {
            m_StartPosition = eventData.worldPosition;

            var device = this.GetDeviceForRayOrigin(eventData.rayOrigin);
            if (device != null)
                m_ScrollInputController.CreateActions(device);

            m_ScrollInputController.onInput = value =>
            {
                var timeDelta = Time.unscaledDeltaTime;
                rotate?.Invoke(Quaternion.AngleAxis(m_DegreesPerScroll * -value.x * timeDelta, Vector3.up));
                scale?.Invoke(value.y * timeDelta * Vector3.one);
            };

            m_ScrollInputController.enabled = true;

            foreach (var c in m_Handle.colliders)
                c.isTrigger = m_CollisionDisabled;

            dragStarted?.Invoke();
        }

        void OnHandleDragEnded(BaseHandle handle, HandleEventData eventData)
        {
            m_Dragging = false;
            m_Rigidbody.isKinematic = true;
            if (m_ScrollInputController != null)
            {
                m_ScrollInputController.RemoveActions();
                m_ScrollInputController.enabled = false;
            }

            m_CurrentHandleDelta = Vector3.zero;

            foreach (var c in m_Handle.colliders)
                c.isTrigger = true;

            dragEnded?.Invoke();
        }

        void OnHandleDragging(BaseHandle handle, HandleEventData eventData)
        {
            if (Vector3.SqrMagnitude(eventData.worldPosition - m_StartPosition) > m_MoveThreshold)
            {
                m_Dragging = true;
                m_Rigidbody.isKinematic = false;
            }

            if (m_Dragging)
                m_CurrentHandleDelta = eventData.deltaPosition; // Force vector will be applied in fixed updated

            UpdateBoundingBox();
        }

        void FixedUpdate()
        {
            m_Rigidbody.AddForce(m_CurrentHandleDelta * (m_DragForceMagnitude * Time.fixedUnscaledDeltaTime), m_ForceMode);
            var position = m_Rigidbody.position;
            translate?.Invoke(position - m_PreviousPosition);
            m_PreviousPosition = position;
        }
    }
}
