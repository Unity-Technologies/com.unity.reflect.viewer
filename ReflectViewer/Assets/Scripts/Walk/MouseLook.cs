using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Unity.Reflect.Viewer
{
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;

        InputAction m_InputAction;
        Quaternion m_CharacterTargetRot;
        Quaternion m_CameraTargetRot;
        bool m_CursorIsLocked = true;

        public void Init(Transform character, Transform camera, InputActionAsset inputActionAsset)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            m_InputAction = inputActionAsset["Camera Control Action"];
        }

        public void LookRotation(Transform character, Transform camera, ref TouchControl joystickTouch, ref TouchControl currentMouseTouch)
        {
            Vector2 mouseRot = Vector2.zero;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (currentMouseTouch is { isInProgress: false })
                currentMouseTouch = null;

            Touchscreen touchscreen = Touchscreen.current;

            if (touchscreen != null)
            {
                for (int i = 0; i < touchscreen.touches.Count; i++)
                {
                    TouchControl touch = touchscreen.touches[i];
                    if (touch.isInProgress && joystickTouch != touch &&
                        (IsRightSide(touch.position.ReadValue()) || currentMouseTouch == touch))
                    {
                        mouseRot = touch.delta.ReadValue() * 0.1f;
                        currentMouseTouch = touch;
                    }
                }
            }
#else
            mouseRot = m_InputAction.ReadValue<Vector2>();
#endif
            float yRot = mouseRot.x * XSensitivity;
            float xRot = mouseRot.y * YSensitivity;

            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            UpdateCursorLock();
        }

        bool IsRightSide(Vector2 coordinate)
        {
            bool retval = false;

            var boundRect = Screen.safeArea;
            boundRect.xMin += boundRect.width / 2f;

            if (boundRect.Contains(coordinate))
            {
                retval = true;
            }

            return retval;
        }

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if (!lockCursor)
            {
                // we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            // if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }

        void InternalLockUpdate()
        {
            m_CursorIsLocked = m_InputAction.controls[0].IsPressed();

            if (m_CursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_CursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}
