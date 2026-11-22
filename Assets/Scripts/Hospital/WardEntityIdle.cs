using UnityEngine;

namespace DigitalCurtain.Hospital
{
    /// <summary>
    /// A silent ward figure that only ever idles: it sways in place, breathes and
    /// slowly turns its head toward the player. It carries no AI and never takes a
    /// step - it exists to be looked at.
    /// PatientBreathingAndIntro finds these components and builds unease while the
    /// player keeps staring at one of them.
    /// </summary>
    public class WardEntityIdle : MonoBehaviour
    {
        [Header("Idle Motion")]
        public float swayAmplitude = 0.012f;
        public float swaySpeed = 0.42f;
        public float swayRoll = 1.4f;
        public float breathAmplitude = 0.010f;
        public float breathSpeed = 0.23f;

        [Header("Head Tracking")]
        public bool trackPlayer = true;
        public float headTurnSharpness = 1.6f;
        public float maxHeadYaw = 55f;
        public float maxHeadPitch = 20f;

        [Header("Bones")]
        public Transform headBone;
        public Transform torsoBone;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseRotation;
        private Vector3 _torsoBaseScale;
        private Quaternion _headBaseRotation;
        private float _phase;
        private float _yaw;
        private float _pitch;
        private Transform _playerCamera;

        private void Awake()
        {
            _baseLocalPosition = transform.localPosition;
            _baseRotation = transform.localRotation;
            _phase = Random.value * Mathf.PI * 2f;

            if (headBone != null) _headBaseRotation = headBone.localRotation;
            if (torsoBone != null) _torsoBaseScale = torsoBone.localScale;
        }

        private void Start()
        {
            if (!trackPlayer) return;

            Camera cam = Camera.main;
            if (cam == null)
            {
                Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
                for (int i = 0; i < cameras.Length; i++)
                {
                    if (cam == null || cameras[i].CompareTag("MainCamera")) cam = cameras[i];
                }
            }

            if (cam != null) _playerCamera = cam.transform;
        }

        private void Update()
        {
            float t = Time.time + _phase;

            float sway = Mathf.Sin(t * swaySpeed * Mathf.PI * 2f);
            transform.localPosition = _baseLocalPosition + Vector3.up * (sway * swayAmplitude);
            transform.localRotation = _baseRotation * Quaternion.Euler(0f, 0f, sway * swayRoll);

            if (torsoBone != null)
            {
                float breath = 1f + Mathf.Sin(t * breathSpeed * Mathf.PI * 2f) * breathAmplitude;
                torsoBone.localScale = new Vector3(_torsoBaseScale.x, _torsoBaseScale.y * breath, _torsoBaseScale.z);
            }

            if (!trackPlayer || headBone == null || _playerCamera == null) return;

            Vector3 toPlayer = transform.InverseTransformDirection(_playerCamera.position - headBone.position);
            float targetYaw = Mathf.Clamp(Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg, -maxHeadYaw, maxHeadYaw);

            float flatDistance = new Vector2(toPlayer.x, toPlayer.z).magnitude;
            float targetPitch = Mathf.Clamp(-Mathf.Atan2(toPlayer.y, flatDistance) * Mathf.Rad2Deg, -maxHeadPitch, maxHeadPitch);

            float blend = 1f - Mathf.Exp(-headTurnSharpness * Time.deltaTime);
            _yaw = Mathf.Lerp(_yaw, targetYaw, blend);
            _pitch = Mathf.Lerp(_pitch, targetPitch, blend);

            headBone.localRotation = _headBaseRotation * Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
}
