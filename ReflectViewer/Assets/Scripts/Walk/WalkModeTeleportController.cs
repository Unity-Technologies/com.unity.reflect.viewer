using System;
using JetBrains.Annotations;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.Reflect.Viewer
{
    public class WalkModeTeleportController : MonoBehaviour
    {
        [SerializeField]
        OrbitModeUIController m_OrbitModeUIController;
        [SerializeField]
        GameObject m_PointMesh;

        WalkTargetAnimation m_TargetAnimation;
        Vector2 m_CurrentPosition;
        Vector2 m_TeleportDestination;
        bool m_IsTeleporting = false;

        public Vector3 GetWalkReticlePosition()
        {
            return m_PointMesh.transform.position;
        }

        public void GetAsyncTargetPosition(Action<Vector3> callback)
        {
            if (!m_IsTeleporting)
            {
                m_IsTeleporting = true;
                m_CurrentPosition = Pointer.current.position.ReadValue();

                IsTargetPositionValid(result =>
                {
                    if (!result)
                        return;

                    m_OrbitModeUIController.AsyncGetTeleportTarget(m_CurrentPosition, result =>
                    {
                        m_IsTeleporting = false;
                        callback(result);
                    });
                });
            }
        }

        void ResizeMesh(Vector3 target)
        {
            m_PointMesh.transform.position = target;
            float size = (Camera.main.transform.position - target).magnitude;
            m_PointMesh.transform.localScale = Vector3.one * size;
        }

        void Awake()
        {
            m_PointMesh = Instantiate(m_PointMesh);
            m_PointMesh.SetActive(false);
            m_TargetAnimation = m_PointMesh.GetComponentInChildren<WalkTargetAnimation>();
        }

        public void SetRotation(Vector3 rotation)
        {
            var ray = Camera.main.ScreenPointToRay(Pointer.current.position.ReadValue());
            Plane groundPlane = new Plane(m_PointMesh.transform.up,m_PointMesh.transform.position);
            if (groundPlane.Raycast(ray, out float rayDistance))
            {
                m_PointMesh.transform.LookAt(ray.GetPoint(rayDistance));
            }
        }

        public float GetRotation()
        {
            return m_PointMesh.transform.eulerAngles.y;
        }

        public void IsTargetPositionValid(Action<bool> callback)
        {
            m_CurrentPosition = Pointer.current.position.ReadValue();
            m_OrbitModeUIController.AsyncGetTeleportTarget(m_CurrentPosition, result =>
            {
                var isValidPosition = result != Vector3.zero;

                callback(isValidPosition);

                if (isValidPosition)
                    ResizeMesh(result);
            });
        }

        public void OnGetTeleportTarget(bool placementMeshActive, bool teleport, [CanBeNull] Action<bool> callback)
        {
            m_TeleportDestination = teleport ? m_TeleportDestination : Pointer.current.position.ReadValue();

            IsTargetPositionValid(result =>
            {
                EnableTarget(placementMeshActive && result);
                if (teleport && !m_IsTeleporting)
                {
                    m_IsTeleporting = true;
                    m_OrbitModeUIController.TriggerTeleport(m_TeleportDestination);
                    result = true;
                }

                callback?.Invoke(result);
            });
        }

        public void SetTeleportDestination(Vector2 destination)
        {
            m_TeleportDestination = destination;
        }

        public void SetCurrentPosition(Vector2 position)
        {
            m_CurrentPosition = position;
        }

        public void TeleportFinish()
        {
            m_IsTeleporting = false;
            m_PointMesh.transform.position = Vector3.zero;
            m_PointMesh.transform.rotation = Quaternion.identity;
            StartAnimation(false);
            EnableTarget(false);
        }

        public bool IsTeleporting()
        {
            return m_IsTeleporting;
        }

        public void EnableTarget(bool isEnable)
        {
            m_PointMesh.SetActive(isEnable);
        }

        void StartAnimation(bool isEnable)
        {
            m_TargetAnimation.StartAnimation(isEnable);
        }

        public void StartDistanceAnimation()
        {
            m_TargetAnimation.DistanceAnimation(m_CurrentPosition, Pointer.current.position.ReadValue());
        }
    }
}
