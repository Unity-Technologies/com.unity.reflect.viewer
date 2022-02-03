using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Reflect.Collections;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Utils;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    public enum LookAtConstraint
    {
        /// <summary>
        ///     The lookAt point does not move with the camera, resulting
        ///     in the camera to continue to look at the same point.
        /// </summary>
        StandBy,

        /// <summary>
        ///     The lookAt point moves with the position, resulting in
        ///     the camera to keep its current rotation while moving.
        /// </summary>
        Follow
    }

    /// <summary>
    ///     A generic free fly camera with pan, move, rotation, orbit and automatic positioning features.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FreeFlyCamera : MonoBehaviour
    {
        [SerializeField]
        FreeFlyCameraSettings m_Settings = null;

        [SerializeField, Tooltip("Once set, the camera will follow this transform")]

        Camera m_Camera;

        // Default values to hide missing call to SetupCameraSpeed (hotfix)
        float m_MinSpeed = 1.0f;
        float m_MaxSpeed = 1000.0f;
        float m_Acceleration = 10.0f;
        float m_WaitingDeceleration = 40.0f;

        Vector3 m_DesiredLookAt;
        Vector3 m_DesiredPosition;
        Vector2 m_DesiredRotationEuler;
        Quaternion m_DesiredRotation;

        bool m_IsSphericalMovement;
        float m_MovingSpeed;
        Vector3 m_MovingDirection;
        LookAtConstraint m_LookAtConstraint;

        Vector2 m_AngleOffset;

        public new Camera camera => m_Camera;

        readonly string k_CameraTransformKey = "ct";
        Dictionary<string, string> m_CameraTransformQueryValue = new Dictionary<string, string>();
        string m_CurrentServerProjectId = "";

        IUISelector<IPicker> m_TeleportPickerSelector;
        float m_PanningScale = 1.0f;
        public FreeFlyCameraSettings settings
        {
            get => m_Settings;
            set => m_Settings = value;
        }

        void Awake()
        {
            m_Camera = GetComponent<Camera>();

            m_DesiredLookAt = m_Settings.initialLookAt;
            m_DesiredPosition = m_Camera.transform.position;
            m_DesiredRotation = m_Camera.transform.rotation;
            m_DesiredRotationEuler = m_DesiredRotation.eulerAngles;

            m_IsSphericalMovement = false;

            QueryArgHandler.Register(this, k_CameraTransformKey, CameraTransformFromQueryValue, CameraTransformToQueryValue);
            UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), OnActiveProjectChanged);

            m_TeleportPickerSelector = UISelectorFactory.createSelector<IPicker>(ProjectContext.current, nameof(ITeleportDataProvider.teleportPicker));
        }

        void OnDestroy()
        {
            QueryArgHandler.Unregister(this);
            m_TeleportPickerSelector?.Dispose();
        }

        void OnActiveProjectChanged(Project newData)
        {
            if (m_CameraTransformQueryValue.ContainsKey(m_CurrentServerProjectId))
            {
                m_CameraTransformQueryValue.Remove(m_CurrentServerProjectId);
            }
            m_CurrentServerProjectId = newData?.serverProjectId ?? "";
        }

        public string CameraTransformToQueryValue()
        {
            return $"{UIUtils.Vector3UrlFormat(m_Camera.transform.position)},"
                + $"{UIUtils.Vector3UrlFormat(m_Camera.transform.rotation.eulerAngles)},"
                + $"{UIUtils.Vector3UrlFormat(m_Camera.transform.forward)}";
        }

        public void CameraTransformFromQueryValue(string cameraTransformStringValue)
        {
            // Save to memory to later apply in SetupInitialCameraPosition
            if (!m_CameraTransformQueryValue.ContainsKey(m_CurrentServerProjectId))
            {
                m_CameraTransformQueryValue.Add(m_CurrentServerProjectId, cameraTransformStringValue);
            }
            else
            {
                m_CameraTransformQueryValue[m_CurrentServerProjectId] = cameraTransformStringValue;
            }
            ApplyQueryStringToTransform(cameraTransformStringValue);
        }

        void ApplyQueryStringToTransform(string queryValue)
        {
            var splitValue = queryValue.Split(',').Select(i => float.TryParse(i, out float result) ? result : 0.0f).ToList();
            if (splitValue.Count > 8)
            {
                var newPosition = new Vector3(splitValue[0], splitValue[1], splitValue[2]);
                var newEulerAngle = new Vector3(splitValue[3], splitValue[4], splitValue[5]);
                var newRotation = Quaternion.Euler(newEulerAngle);
                var newForward = new Vector3(splitValue[6], splitValue[7], splitValue[8]);

                m_Camera.transform.position = newPosition;
                m_Camera.transform.rotation = newRotation;
                m_DesiredRotation = newRotation;
                m_DesiredRotationEuler = newRotation.eulerAngles;
                m_DesiredLookAt = newForward;
                m_DesiredPosition = newPosition;
                m_IsSphericalMovement = false;
            }
        }

        public void ForceStop()
        {
            m_MovingSpeed = 0;
            m_MovingDirection = Vector3.zero;
        }

        public void SetRotation(Quaternion quaternion)
        {
            m_Camera.transform.rotation = quaternion;
            m_DesiredRotation = quaternion;
            m_DesiredRotationEuler = quaternion.eulerAngles;
            m_DesiredLookAt = m_DesiredRotation * new Vector3(0.0f, 0.0f, (m_DesiredLookAt - m_DesiredPosition).magnitude) + m_DesiredPosition;
        }

        public void TransformTo(Transform newTransform)
        {
            m_DesiredRotation = newTransform.rotation;
            m_DesiredRotationEuler = newTransform.rotation.eulerAngles;
            m_DesiredLookAt = newTransform.forward;
            m_DesiredPosition = newTransform.position;
            m_IsSphericalMovement = false;
        }

        void Update()
        {
            var delta = Time.unscaledDeltaTime;

            if (m_MovingDirection != Vector3.zero)
            {
                var offset = m_DesiredRotation * m_MovingDirection * m_MovingSpeed * delta;

                m_DesiredPosition += offset;

                if (m_LookAtConstraint == LookAtConstraint.Follow)
                {
                    m_DesiredLookAt += offset;
                }

                m_MovingSpeed = Mathf.Clamp(m_MovingSpeed + m_Acceleration * delta, m_MinSpeed, m_MaxSpeed);
            }
            else
            {
                if (delta < 0.1f) // Should be based on UINavigationControllerSettings' inputLagSkipThreshold, but it's not in the scope.  Using default value for now.
                {
                    m_MovingSpeed = Mathf.Clamp(m_MovingSpeed - m_WaitingDeceleration * delta, m_MinSpeed, m_MaxSpeed);
                }
                else
                {
                    m_MovingSpeed = 0;
                }
            }

            var rotation = Quaternion.Lerp(m_Camera.transform.rotation, m_DesiredRotation, Mathf.Clamp(delta / m_Settings.rotationElasticity, 0.0f, 1.0f));
            m_Camera.transform.rotation = rotation;

            Vector3 position;
            if (m_IsSphericalMovement)
            {
                position = m_DesiredLookAt + rotation * Vector3.back * GetDistanceFromLookAt();
            }
            else
            {
                position = Vector3.Lerp(m_Camera.transform.position, m_DesiredPosition, Mathf.Clamp(delta / m_Settings.positionElasticity, 0.0f, 1.0f));
            }

            m_Camera.transform.position = position;
        }

        /// <summary>
        ///     Move the camera in the specified local direction.
        /// </summary>
        /// <remarks>
        ///     After being called once, this method will continue to move the camera in
        ///     the specified direction. You need to call it with a zero vector to stop the camera from moving.
        /// </remarks>
        /// <param name="unitDir">A unit vector indicating the local direction in which the camera should move.</param>
        /// <param name="constraint">Specifies if the lookAt point is affected or not by this movement.</param>
        public void MoveInLocalDirection(Vector3 unitDir, LookAtConstraint constraint)
        {
            m_MovingDirection = unitDir;
            m_LookAtConstraint = constraint;

            UpdateSphericalMovement(false);
        }

        /// <summary>
        ///     Move the current position of the camera by an offset.
        /// </summary>
        /// <param name="offset">The offset by which the camera should be moved.</param>
        /// <param name="constraint">The constraint on lookAt point.</param>
        public void MovePosition(Vector3 offset, LookAtConstraint constraint)
        {
            m_DesiredPosition += offset;

            if (constraint == LookAtConstraint.Follow)
            {
                m_DesiredLookAt += offset;
            }

            UpdateSphericalMovement(false);
        }

        /// <summary>
        ///     Drag the camera on the current frustum plane. If the
        ///     camera is looking forward, <see cref="Pan"/> will drag
        ///     the camera on its local up and right vectors.
        /// </summary>
        /// <param name="offset"></param>
        public void Pan(Vector3 offset)
        {
            offset = m_DesiredRotation * (offset * m_PanningScale);
            MovePosition(offset, LookAtConstraint.Follow);
            UpdateSphericalMovement(false);
        }

        public void PanStart(Vector2 pos)
        {
            Vector3[] frustumCorners = new Vector3[4];
            var depth = -m_DesiredPosition.magnitude;
            camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), depth, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
            m_PanningScale = Mathf.Abs((frustumCorners[2].x - frustumCorners[1].x) / Screen.width);

            var picker = (ISpatialPickerAsync<Tuple<GameObject, RaycastHit>>) m_TeleportPickerSelector.GetValue();
            picker.Pick(m_Camera.ScreenPointToRay(pos), result =>
            {
                var selected = result.Select(x => x.Item2).ToList();
                if(selected.Count > 0)
                {
                    var selectedPoint = selected[0].point;
                    Vector3[] frustumCorners = new Vector3[4];
                    var depth = (selectedPoint - m_DesiredPosition).magnitude;
                    camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), depth, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
                    m_PanningScale = Mathf.Abs((frustumCorners[2].x - frustumCorners[1].x) / Screen.width);
                }
            }, new string[] {});
        }

        /// <summary>
        ///     Rotate the camera by adding an offset to the current rotation.
        /// </summary>
        /// <remarks>
        ///     The <see cref="angleOffset"/> is a rotation in azimuth coordinate where Y is up
        ///     axis (azimuth angle, clockwise) and X, right axis (altitude, clockwise)
        /// </remarks>
        /// <param name="angleOffset">A rotation around the y axis, then the x axis</param>
        public void Rotate(Vector2 angleOffset)
        {
            m_DesiredRotationEuler += angleOffset;
            m_DesiredRotationEuler.x = Mathf.Clamp(m_DesiredRotationEuler.x, -m_Settings.maxPitchAngle, m_Settings.maxPitchAngle);

            m_DesiredRotation =
                Quaternion.AngleAxis(m_DesiredRotationEuler.y, Vector3.up) *
                Quaternion.AngleAxis(m_DesiredRotationEuler.x, Vector3.right);

            m_DesiredLookAt = m_DesiredRotation * new Vector3(0.0f, 0.0f, (m_DesiredLookAt - m_DesiredPosition).magnitude) + m_DesiredPosition;

            UpdateSphericalMovement(false);
        }

        /// <summary>
        ///     Orbit around the look at point with up vector fixed to Y.
        /// </summary>
        /// <remarks>
        ///     The <see cref="angleOffset"/> is the same value that is provided in <see cref="Rotate"/>
        ///     because the orbit rotation and camera rotation matches when orbiting.
        /// </remarks>
        /// <param name="angleOffset"></param>
        public void OrbitAroundLookAt(Vector2 angleOffset)
        {
            m_DesiredRotationEuler += angleOffset;
            m_DesiredRotationEuler.x = Mathf.Clamp(m_DesiredRotationEuler.x, -m_Settings.maxPitchAngle, m_Settings.maxPitchAngle);

            m_DesiredRotation =
                Quaternion.AngleAxis(m_DesiredRotationEuler.y, Vector3.up) *
                Quaternion.AngleAxis(m_DesiredRotationEuler.x, Vector3.right);

            var negDistance = new Vector3(0.0f, 0.0f, -GetDistanceFromLookAt());
            m_DesiredPosition = m_DesiredRotation * negDistance + m_DesiredLookAt;

            UpdateSphericalMovement(true);
        }

        public void FixedOrbitAroundLookAt(Vector2 angleOffset)
        {
            m_DesiredRotationEuler = angleOffset;
            m_DesiredRotation = Quaternion.Euler(angleOffset);

            var negDistance = new Vector3(0.0f, 0.0f, -GetDistanceFromLookAt());
            m_DesiredPosition = m_DesiredRotation * negDistance + m_DesiredLookAt;

            UpdateSphericalMovement(true);
        }

        public void ContinuousOrbitAroundLookAt(Vector2 angleOffset, bool isXAxis)
        {
            ResetDesiredPosition();
            m_AngleOffset += angleOffset;

            if (isXAxis)
            {
                m_AngleOffset = angleOffset;
            }
            else
            {
                m_AngleOffset.x = 0;
            }
            FixedOrbitAroundLookAt(m_AngleOffset);
        }

        /// <summary>
        ///     Move on the forward axis of the camera. The operation is similar
        ///     to a zoom without changing FOV.
        /// </summary>
        /// <remarks>
        ///     This function does nothing if the new distance from lookAt is greater than
        ///     the maximum camera distance.
        /// </remarks>
        /// <param name="nbUnits">The number of units to move forward. A negative value
        ///     will move the camera away from the look at point.</param>
        public void MoveOnLookAtAxis(float nbUnits)
        {
            nbUnits *= GetDistanceFromLookAt() * m_Settings.moveOnAxisScaling;
            var originalDistanceFromLookAt = GetDistanceFromLookAt();

            var forward = m_DesiredRotation * Vector3.forward;

            var pos = m_DesiredPosition + forward * nbUnits;

            m_DesiredPosition = pos;

            if (originalDistanceFromLookAt - nbUnits < m_Settings.minDistanceFromLookAt)
            {
                m_DesiredLookAt = m_DesiredPosition + forward * m_Settings.minDistanceFromLookAt;
            }

            UpdateSphericalMovement(false);
        }

        public void FocusOnPoint(Vector3 value)
        {
            var cameraPlane = new Plane(transform.forward, transform.position);
            var targetCameraPos = cameraPlane.ClosestPointOnPlane(value);
            m_DesiredLookAt = value;
            m_DesiredPosition = targetCameraPos;

            UpdateSphericalMovement(true);
        }

        /// <summary>
        ///     Setup the camera at a position where <see cref="percentOfView"/> of the entire scene will be visible from.
        /// </summary>
        /// <param name="bb">The AABB of the scene</param>
        /// <param name="pitch">The pitch of the camera</param>
        /// <param name="percentOfView">The percentage of the <see cref="bb"/> that will be visible</param>
        public void SetupInitialCameraPosition(Bounds bb, float pitch, float percentOfView)
        {
            if (!string.IsNullOrEmpty(m_CurrentServerProjectId))
            {
                if (m_CameraTransformQueryValue.ContainsKey(m_CurrentServerProjectId))
                {
                    ApplyQueryStringToTransform(m_CameraTransformQueryValue[m_CurrentServerProjectId]);
                    SetupCameraSpeed(bb);
                }
                else
                {
                    FitInView(bb, pitch, percentOfView);
                    SetupCameraSpeed(bb);
                    m_AngleOffset = Vector2.zero;
                }
            }
        }

        public void SetupInitialCameraPosition()
        {
            m_Camera.transform.rotation = Quaternion.identity;
            m_Camera.transform.position = new Vector3(0, 0, -10);
            ForceStop();
        }

        void UpdateSphericalMovement(bool isSphericalMovement)
        {
            m_IsSphericalMovement = isSphericalMovement;
        }

        public float GetDistanceFromLookAt()
        {
            return (m_DesiredLookAt - m_DesiredPosition).magnitude;
        }

        public void SetDistanceFromLookAt(float distance)
        {
            if (distance < m_Settings.minDistanceFromLookAt)
            {
                distance = m_Settings.minDistanceFromLookAt;
            }
            m_DesiredPosition = m_DesiredLookAt + (m_DesiredPosition - m_DesiredLookAt).normalized * distance;
        }

        void FitInView(Bounds bb, float pitch, float percentOfView)
        {
            var fitPosition = CalculateViewFitPosition(bb, pitch, percentOfView, m_Camera.fieldOfView, m_Camera.aspect);

            m_DesiredPosition = fitPosition.position;
            m_DesiredRotation = Quaternion.Euler(fitPosition.rotation);
            m_DesiredRotationEuler = fitPosition.rotation;
            m_DesiredLookAt = bb.center;

            m_Camera.transform.rotation = m_DesiredRotation;
            m_Camera.transform.position = m_DesiredPosition;
        }

        public static CameraTransformInfo CalculateViewFitPosition(Bounds bb, float pitch, float percentOfView, float fov, float aspectRatio)
        {
            var desiredEuler = new Vector3(pitch, 0, 0);

            using (var rootGetter = UISelectorFactory.createSelector<Transform>(PipelineContext.current, nameof(IPipelineDataProvider.rootNode), null))
            {

                var (lookAt, distanceFromLookAtX, distanceFomFromLookAtY) = GetDistanceFromLookAt(bb, percentOfView, fov, aspectRatio);

                var distanceFromLookAt = Mathf.Max(distanceFromLookAtX, distanceFomFromLookAtY);

                return new CameraTransformInfo
                {
                    rotation = rootGetter.GetValue().rotation * desiredEuler,
                    position = rootGetter.GetValue().TransformPoint(lookAt - distanceFromLookAt * (Quaternion.Euler(desiredEuler) * Vector3.forward))
                };
            }
        }

        static (Vector3 LookAt, float DistanceFromLookAtX, float DistanceFromLookAtY) GetDistanceFromLookAt(Bounds bb, float percentOfView, float fov, float aspectRatio)
        {
            var lookAt = bb.center;

            var adjacent = bb.extents.x;
            var angle = (180.0f - fov) / 2.0f;
            var ratio = Mathf.Tan(Mathf.Deg2Rad * angle);
            var opposite = ratio * adjacent;
            var distanceFromBoundSurfaceX = opposite;
            var distanceFromLookAtX = lookAt.z - bb.min.z + distanceFromBoundSurfaceX / (aspectRatio * percentOfView);

            adjacent = bb.extents.y;
            angle = (180.0f - fov) / 2.0f;
            ratio = Mathf.Tan(Mathf.Deg2Rad * angle);
            opposite = ratio * adjacent;
            var distanceFromBoundSurfaceY = opposite;
            var distanceFromLookAtY = lookAt.z - bb.min.z + distanceFromBoundSurfaceY * aspectRatio / percentOfView;

            return (lookAt, distanceFromLookAtX, distanceFromLookAtY);
        }

        void SetupCameraSpeed(Bounds bb)
        {
            var maxDistanceToMove = bb.extents.magnitude;

            m_MinSpeed = maxDistanceToMove / m_Settings.maxTimeToTravelMinSpeed * m_Settings.minSpeedScaling;
            m_MaxSpeed = maxDistanceToMove / m_Settings.maxTimeToTravelFullSpeed * m_Settings.maxSpeedScaling;
            m_Acceleration = (m_MaxSpeed - m_MinSpeed) / m_Settings.maxTimeToAccelerate * m_Settings.accelerationScaling;
            m_WaitingDeceleration = m_Acceleration * m_Settings.waitingDecelerationScaling;
        }

        public void ResetDesiredPosition()
        {
            var lookAtOffset = m_DesiredLookAt - m_DesiredPosition;
            m_DesiredPosition = m_Camera.transform.position;
            m_DesiredLookAt = m_DesiredPosition + lookAtOffset;
        }
    }
}
