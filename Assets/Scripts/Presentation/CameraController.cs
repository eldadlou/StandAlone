using UnityEngine;
using Unity.Cinemachine;
using MyGame.Input;

namespace MyGame.Presentation
{
    /// <summary>
    /// RTS pan: moves CameraTarget on XZ. Orbit and zoom are on Cinemachine Input Axis Controller (same vcam).
    /// </summary>
    [RequireComponent(typeof(CinemachineOrbitalFollow))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private bool enableDebugLogs;

        [Header("Pan")]
        [SerializeField] private float referenceRadius = 5f;
        [SerializeField] private float minSpeedScale = 0.25f;
        [SerializeField] private float maxSpeedScale = 2f;

        private CinemachineOrbitalFollow orbitalFollow;
        private Vector2 moveInput;

        private void Awake()
        {
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
            InputEvents.OnCameraMove += HandleCameraMove;

            if (cameraTarget == null)
                FindOrCreateCameraTarget();
        }

        private void OnDisable()
        {
            moveInput = Vector2.zero;
        }

        private void OnDestroy()
        {
            InputEvents.OnCameraMove -= HandleCameraMove;
        }

        private void LateUpdate()
        {
            if (cameraTarget == null || moveInput.sqrMagnitude < 0.01f)
                return;

            GetPanAxes(out var forward, out var right);
            var speed = moveSpeed * GetSpeedScale();
            cameraTarget.position += (forward * moveInput.y + right * moveInput.x) * (speed * Time.deltaTime);
        }

        private void HandleCameraMove(Vector2 direction) => moveInput = direction;

        private void FindOrCreateCameraTarget()
        {
            var existing = GameObject.Find("CameraTarget");
            if (existing != null)
            {
                cameraTarget = existing.transform;
                return;
            }

            cameraTarget = new GameObject("CameraTarget").transform;
            cameraTarget.position = Vector3.zero;
        }

        private void GetPanAxes(out Vector3 forward, out Vector3 right)
        {
            var yaw = orbitalFollow.HorizontalAxis.Value;
            var yawRotation = Quaternion.Euler(0f, yaw, 0f);
            forward = yawRotation * Vector3.forward;
            right = yawRotation * Vector3.right;
        }

        private float GetSpeedScale()
        {
            if (referenceRadius <= 0f)
                return 1f;

            return Mathf.Clamp(orbitalFollow.Radius / referenceRadius, minSpeedScale, maxSpeedScale);
        }

        public void SetCameraTarget(Transform target) => cameraTarget = target;
    }
}
