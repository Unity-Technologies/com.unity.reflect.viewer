using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.Reflect.Viewer
{
    public class FreeFlyProvider : LocomotionProvider
    {
        #pragma warning disable 0649
        [SerializeField] XRController m_XrController;
        [SerializeField] float m_Speed = 1f;
        [SerializeField] float m_Acceleration = 1f;
        #pragma warning restore 0649

        Transform m_CamTransform;
        bool m_IsFlying;
        float m_CurrentSpeed;

        void Start()
        {
            m_CamTransform = system.xrRig.cameraGameObject.transform;
        }

        void Update()
        {
            if (!m_XrController.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out var isButtonPressed))
                return;

            if (m_IsFlying)
            {
                if (isButtonPressed)
                    Move();
                else
                    EndLocomotion();
            }
            else if (isButtonPressed && BeginLocomotion())
            {
                m_CurrentSpeed = m_Speed;
                m_IsFlying = true;
            }
        }

        void Move()
        {
            var dir = m_XrController.transform.forward;
            var deltaTime = Time.deltaTime;
            system.xrRig.MoveCameraToWorldLocation(m_CamTransform.position + deltaTime * m_CurrentSpeed * dir);
            m_CurrentSpeed += deltaTime * m_Acceleration;
        }
    }
}
