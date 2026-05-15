using UnityEngine;

namespace Kaleidoscope2.DiamondFocus.RealMesh.Proof
{
    [DisallowMultipleComponent]
    public sealed class Crystal3DProofCameraOrbit : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Range(1f, 12f)] private float distance = 5.2f;
        [SerializeField, Range(-2f, 3f)] private float height = 0.35f;
        [SerializeField, Range(0f, 90f)] private float orbitDegreesPerSecond = 18f;
        [SerializeField, Range(-45f, 45f)] private float pitchDegrees = 4f;

        private float orbitAngle;

        public void SetTarget(Transform value)
        {
            target = value;
            ApplyPose();
        }

        private void Start()
        {
            ApplyPose();
        }

        private void LateUpdate()
        {
            orbitAngle += orbitDegreesPerSecond * Time.deltaTime;
            ApplyPose();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                ApplyPose();
            }
        }

        private void ApplyPose()
        {
            if (target == null)
            {
                return;
            }

            Quaternion orbitRotation = Quaternion.Euler(0f, orbitAngle, 0f);
            Vector3 offset = orbitRotation * new Vector3(0f, height, -distance);
            transform.position = target.position + offset;
            Vector3 lookTarget = target.position + Vector3.up * pitchDegrees * 0.01f;
            Vector3 direction = lookTarget - transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }
    }
}
