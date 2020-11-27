using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.Reflect.Viewer
{
    public class FreeFlyProvider : LocomotionProvider
    {
        #pragma warning disable 0649
        [SerializeField] InputActionAsset m_InputActionAsset;
        [SerializeField] Transform m_ControllerTransform;
        [SerializeField] float m_Speed = 1f;
        [SerializeField] float m_Acceleration = 1f;
        #pragma warning restore 0649

        InputAction m_FlyAction;
        Transform m_CamTransform;
        bool m_IsFlying;
        float m_CurrentSpeed;

        void Start()
        {
            m_CamTransform = system.xrRig.cameraGameObject.transform;

            m_FlyAction = m_InputActionAsset["VR/Fly"];
        }

        void Update()
        {
            var isButtonPressed = m_FlyAction.ReadValue<float>() > 0;

            if (m_IsFlying)
            {
                if (isButtonPressed)
                    Move();
                else
                {
                    EndLocomotion();
                    m_IsFlying = false;
                }
            }
            else if (isButtonPressed && BeginLocomotion())
            {
                m_CurrentSpeed = m_Speed;
                m_IsFlying = true;
            }
        }

        void Move()
        {
            var dir = m_ControllerTransform.forward;
            var deltaTime = Time.deltaTime;
            system.xrRig.MoveCameraToWorldLocation(m_CamTransform.position + deltaTime * m_CurrentSpeed * dir);
            m_CurrentSpeed += deltaTime * m_Acceleration;
        }
    }
}
