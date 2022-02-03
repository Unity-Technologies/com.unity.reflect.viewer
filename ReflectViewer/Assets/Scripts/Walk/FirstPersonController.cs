using System;
using Unity.Reflect.Geometry;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#pragma warning disable 618, 649
namespace Unity.Reflect.Viewer
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField]
        bool m_IsWalking;
        [SerializeField]
        float m_WalkSpeed;
        [SerializeField]
        float m_RunSpeed;
        [SerializeField]
        [Range(0f, 1f)]
        float m_RunstepLenghten;
        [SerializeField]
        float m_JumpSpeed;
        [SerializeField]
        float m_RotationSpeed;
        [SerializeField]
        float m_StickToGroundForce;
        [SerializeField]
        float m_GravityMultiplier;
        [SerializeField]
        MouseLook m_MouseLook;
        [SerializeField]
        bool m_UseFovKick;
        [SerializeField]
        FOVKick m_FovKick = new FOVKick();
        [SerializeField]
        bool m_UseHeadBob;
        [SerializeField]
        float m_GravityUpSpeed = 0.001f;
        [SerializeField]
        CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField]
        LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField]
        float m_StepInterval;
        [SerializeField]
        AudioClip[] m_FootstepSounds; // an array of footstep sounds that will be randomly selected from.
        [SerializeField]
        AudioClip m_JumpSound; // the sound played when character leaves the ground.
        [SerializeField]
        AudioClip m_LandSound; // the sound played when character touches back on ground.
        [SerializeField]
        InputActionAsset m_InputActionAsset;
        [SerializeField]
        float m_MaxFallDistance = 5f;

        JoystickControl m_JoystickControl;
        Camera m_Camera;
        bool m_Jump;
        Vector2 m_Input;
        Vector3 m_MoveDir = Vector3.zero;
        CharacterController m_CharacterController;
        CollisionFlags m_CollisionFlags;
        bool m_PreviouslyGrounded;
        Vector3 m_OriginalCameraPosition;
        float m_StepCycle;
        float m_NextStep;
        bool m_Jumping;
        AudioSource m_AudioSource;

        float m_Speed = 0;
        InputAction m_MovingAction;
        InputAction m_RunningAction;
        bool GamingNavigation = true;
        Vector3 m_ResetFallOffset = new Vector3(0, 1.3f, 0);
        Rigidbody m_Rigidbody;
        IUISelector<IWalkInstructionUI> m_WalkInstructionSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();
        IUISelector<Bounds> m_BoundsGetter;
        TouchControl m_CurrentJoystickTouch;
        TouchControl m_CurrentMouseTouch;

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_RunningAction = m_InputActionAsset["Run Action"];
            m_InputActionAsset["Jump Action"].performed += OnJump;
            m_MovingAction = m_InputActionAsset["Walk Navigation Action"];
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;
            m_Rigidbody = GetComponent<Rigidbody>();
            m_DisposeOnDestroy.Add(m_BoundsGetter = UISelectorFactory.createSelector<Bounds>(ProjectContext.current, "zoneBounds"));
            m_DisposeOnDestroy.Add(m_WalkInstructionSelector = UISelectorFactory.createSelector<IWalkInstructionUI>(WalkModeContext.current, nameof(IWalkModeDataProvider.instruction)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.moveEnabled),
                data =>
                {
                    if (data)
                    {
                        m_MovingAction.Enable();
                    }
                    else
                    {
                        m_MovingAction.Disable();
                    }
                }));
        }

        void Start()
        {
            m_Camera = GetComponentInChildren<Camera>();
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_AudioSource = GetComponent<AudioSource>();
            m_MouseLook.Init(transform, m_Camera.transform, m_InputActionAsset);
        }

        void OnEnable()
        {
            m_MovingAction.Reset();

            var aabb = m_BoundsGetter.GetValue().ToReflect().ToUnity();
            m_MaxFallDistance = aabb.min.y - m_CharacterController.height / 3f;
        }

        public void Init(JoystickControl joystickControl)
        {
            m_JoystickControl = joystickControl;
        }

        void OnNavigation()
        {
            m_Input = m_MovingAction.ReadValue<Vector2>();
            bool wasWalking = m_IsWalking;

#if !MOBILE_INPUT

            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !m_RunningAction.controls[0].IsPressed();
#endif

            // set the desired speed to be walking or running

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            m_Speed = Mathf.Clamp(m_JoystickControl.distanceFromCenter * m_RunSpeed, 0f, m_RunSpeed);
#else
            m_Speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
#endif

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != wasWalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }

        void OnJump(InputAction.CallbackContext obj)
        {
            if (!m_Jumping && m_Rigidbody.useGravity)
                m_Jump = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }

            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            m_MovingAction.Reset();
        }

        void FixedUpdate()
        {
            OnNavigation();

            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
            m_MoveDir.x = desiredMove.x * m_Speed;
            m_MoveDir.z = desiredMove.z * m_Speed;
            if (!GamingNavigation)
            {
                var offset = m_Input.y <= (-1f + Mathf.Epsilon) ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;

                if (m_Input != Vector2.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMove) * offset, m_RotationSpeed);
                }
            }

            RotateView();
            if (m_Rigidbody.useGravity)
            {
                if (m_CharacterController.isGrounded)
                {
                    m_MoveDir.y = -m_StickToGroundForce;

                    if (m_Jump)
                    {
                        m_MoveDir.y = m_JumpSpeed;
                        PlayJumpSound();
                        m_Jump = false;
                        m_Jumping = true;
                    }
                }
                else
                {
                    m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
                }
            }
            else
            {
                m_MoveDir.y = 0;
            }

            if (m_CharacterController.enabled)
                m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
            ProgressStepCycle(m_Speed);
            UpdateCameraPosition(m_Speed);
            m_MouseLook.UpdateCursorLock();
            if (transform.position.y < m_MaxFallDistance)
            {
                m_WalkInstructionSelector.GetValue().Reset(m_ResetFallOffset);
            }
        }

        bool IsFloorPresent()
        {
            return Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out RaycastHit hit, Mathf.Infinity);
        }

        void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }

        void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                    Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }

        void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }

            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);

            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }

        void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }

            newCameraPosition = m_Camera.transform.localPosition;
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                        (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition.y -= m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }

            m_Camera.transform.localPosition = newCameraPosition;
        }

        public void SetJoystickTouch(TouchControl touch)
        {
            m_CurrentJoystickTouch = touch;
        }

        public TouchControl GetJoystickTouch()
        {
            return m_CurrentJoystickTouch;
        }

        public bool IsTouchControlDifferent()
        {
            if (m_CurrentMouseTouch == null && m_CurrentJoystickTouch == null)
                return true;

            return m_CurrentMouseTouch != m_CurrentJoystickTouch;
        }

        void RotateView()
        {
            if (m_CurrentJoystickTouch is { isInProgress : false })
                m_CurrentJoystickTouch = null;

            m_MouseLook.LookRotation(transform, m_Camera.transform, ref m_CurrentJoystickTouch, ref m_CurrentMouseTouch);
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            if (!m_Rigidbody.useGravity)
                transform.position += Vector3.up * m_GravityUpSpeed;

            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }

            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
