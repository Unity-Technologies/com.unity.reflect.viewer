using Unity.Reflect.Viewer.UI;

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

namespace UnityEngine.Reflect
{
    /// <summary>
    ///     A generic free fly camera with pan, move, rotation, orbit and automatic positioning features.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FreeFlyCamera : MonoBehaviour
    {
        [SerializeField]
        FreeFlyCameraSettings m_Settings = null;

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

        Vector3 m_CameraCenter;
        float m_SqrCameraMaxDistance;
        Vector2 m_AngleOffset;

        public new Camera camera => m_Camera;

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
        }

        public void ForceStop()
        {
            m_MovingSpeed = 0;
            m_MovingDirection = Vector3.zero;
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
            offset = m_DesiredRotation * offset * GetDistanceFromLookAt() * m_Settings.panScaling;
            MovePosition(offset, LookAtConstraint.Follow);

            UpdateSphericalMovement(false);
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
            // Don't block max distance from look at (requested)
            //if (nbUnits < 0.0f && (pos - m_DesiredLookAt).sqrMagnitude > m_SqrCameraMaxDistance)
            //{
            //    return;
            //}

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
            if(m_DesiredLookAt != value)
                m_DesiredLookAt = value;
            if(m_DesiredPosition != targetCameraPos)
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
            m_CameraCenter = bb.center;
            m_SqrCameraMaxDistance = bb.extents.magnitude * bb.extents.magnitude * m_Settings.maxLookAtDistanceScaling * m_Settings.maxLookAtDistanceScaling;

            FitInView(bb, pitch, percentOfView);
            SetupCameraSpeed(bb);
            m_AngleOffset = Vector2.zero;
        }

        public void SetupInitialCameraPosition()
        {
            m_Camera.transform.rotation = Quaternion.identity;
            m_Camera.transform.position = new Vector3(0,0, -10);
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
            var fitPosition = CalculateViewFitPosition(bb, pitch, percentOfView, m_Camera.fieldOfView);

            m_DesiredPosition = fitPosition.position;
            m_DesiredRotation = Quaternion.Euler(fitPosition.rotation);
            m_DesiredRotationEuler = fitPosition.rotation;
            m_DesiredLookAt = bb.center;

            m_Camera.transform.rotation = m_DesiredRotation;
            m_Camera.transform.position = m_DesiredPosition;
        }

        public static CameraTransformInfo CalculateViewFitPosition(Bounds bb, float pitch, float percentOfView, float fov)
        {
            var adjacent = bb.extents.x;
            var angle = (180.0f - fov) / 2.0f;
            var ratio = Mathf.Tan(Mathf.Deg2Rad * angle);
            var opposite = ratio * adjacent;
            var distanceFromBoundSurface = opposite;

            var lookAt = bb.center;

            var distanceFromLookAt = lookAt.z - bb.min.z + distanceFromBoundSurface * percentOfView;
            var desiredEuler = new Vector3(pitch, 0, 0);

            return new CameraTransformInfo()
            {
                rotation = UIStateManager.current.m_RootNode.transform.rotation*desiredEuler,
                position = UIStateManager.current.m_RootNode.transform.TransformPoint(lookAt - distanceFromLookAt * (Quaternion.Euler(desiredEuler) * Vector3.forward)),
            };
        }

        void SetupCameraSpeed(Bounds bb)
        {
            var maxDistanceToMove = bb.extents.magnitude;

            m_MinSpeed = maxDistanceToMove / m_Settings.maxTimeToTravelMinSpeed * m_Settings.minSpeedScaling;
            m_MaxSpeed = maxDistanceToMove / m_Settings.maxTimeToTravelFullSpeed* m_Settings.maxSpeedScaling;
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
