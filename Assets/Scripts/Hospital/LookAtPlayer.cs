using UnityEngine;

namespace DigitalCurtain.Hospital
{
    /// <summary>
    /// Keeps a flat-shaped head (the glowing question mark) turned towards the player.
    /// Yaw only, so the symbol never tilts backwards or buries itself in the torso.
    /// </summary>
    public class LookAtPlayer : MonoBehaviour
    {
        private void LateUpdate()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 targetDirection = mainCamera.transform.position - transform.position;
            targetDirection.y = 0f;

            if (targetDirection.sqrMagnitude < 0.0001f) return;

            transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        }
    }
}
