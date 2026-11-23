using UnityEngine;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DigitalCurtain.Hospital
{
    /// <summary>
    /// Desktop fallback for looking around while the XR headset is not connected.
    /// Rotates the rig node that the breathing script does not animate, so the
    /// two never fight over the same transform.
    /// </summary>
    public class PatientLookController : MonoBehaviour
    {
        public float sensitivity = 2.0f;
        [Tooltip("Base tilt of the bed pose. 75 degrees keeps the player lying down, looking towards the feet.")]
        public float basePitch = 75f;
        public float minPitch = -72f;
        public float maxPitch = 66f;
        public bool lockCursor = true;
        public PatientBreathingAndIntro intro;

        private float _yaw;
        private float _pitch;
        private bool _cursorLocked;

        private void Start()
        {
            if (lockCursor) SetCursorLock(true);
        }

        private void Update()
        {
            if (XRSettings.isDeviceActive) return;

            if (intro != null && intro.SequenceRunning && intro.handlerTorso != null)
            {
                FocusOnHandler(intro.handlerTorso);
                return;
            }

            HandleCursorLock();
            if (lockCursor && !_cursorLocked) return;

            Vector2 delta = ReadLookDelta();
            _yaw += delta.x * sensitivity;
            _pitch = Mathf.Clamp(_pitch - delta.y * sensitivity, minPitch, maxPitch);
            transform.localRotation = Quaternion.Euler(basePitch + _pitch, _yaw, 0f);
        }

        private void HandleCursorLock()
        {
            if (!lockCursor) return;

            bool escapePressed = false;
            bool clickPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null) escapePressed = Keyboard.current.escapeKey.wasPressedThisFrame;
            if (Mouse.current != null) clickPressed = Mouse.current.leftButton.wasPressedThisFrame;
#else
            escapePressed = Input.GetKeyDown(KeyCode.Escape);
            clickPressed = Input.GetMouseButtonDown(0);
#endif

            if (escapePressed) SetCursorLock(false);
            else if (clickPressed && !_cursorLocked) SetCursorLock(true);
        }

        private void SetCursorLock(bool locked)
        {
            _cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private Vector2 ReadLookDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.delta.ReadValue() * 0.12f;
            }
            return Vector2.zero;
#else
            return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
#endif
        }

        private void FocusOnHandler(Transform handlerTorso)
        {
            if (_cursorLocked) SetCursorLock(false);

            Vector3 point = handlerTorso.position + Vector3.up * 0.78f;
            Vector3 direction = point - transform.position;
            if (direction.sqrMagnitude < 0.0001f) return;

            Quaternion worldRotation = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion localRotation = transform.parent != null
                ? Quaternion.Inverse(transform.parent.rotation) * worldRotation
                : worldRotation;

            transform.localRotation = Quaternion.Slerp(transform.localRotation, localRotation, 1f - Mathf.Exp(-6f * Time.deltaTime));
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
