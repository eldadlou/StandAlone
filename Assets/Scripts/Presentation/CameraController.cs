using UnityEngine;
using Unity.Cinemachine;
using MyGame.Input;

namespace MyGame.Presentation
{
    /// <summary>
    /// Handles camera movement and controls, responding to input events
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private bool enableDebugLogs = false;
        
        [Header("Cinemachine")]
        [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
        
        [Header("Rotation Settings")]
        [SerializeField] private float horizontalRotationSpeed = 2f;
        [SerializeField] private float verticalRotationSpeed = 0.5f; // Reduced from 2f to 0.5f for smoother vertical rotation
        
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minRadius = 0.2f; // Updated to match CinemachineOrbitalFollow RadialAxis range
        [SerializeField] private float maxRadius = 10f;  // Updated to match CinemachineOrbitalFollow RadialAxis range
        
        private Vector2 moveInput;
        private Vector2 lookInput;
        private Vector2 scrollInput;
        private bool middleClickInput = true; // FOR NOW IS ALWAYS ON

        private void Awake()
        {
            // Subscribe to input events
            InputEvents.OnCameraMove += HandleCameraMove;
            InputEvents.OnCameraLook += HandleCameraLook;
            InputEvents.OnCameraScroll += HandleCameraScroll;
            InputEvents.OnMiddleClick += HandleMiddleClick;
            
            // Try to find camera target if not assigned
            if (cameraTarget == null)
            {
                FindOrCreateCameraTarget();
            }
            
            // Try to find orbital follow if not assigned
            if (orbitalFollow == null)
            {
                FindOrbitalFollow();
            }
        }

        private void FindOrCreateCameraTarget()
        {
            // Look for existing camera target
            var existingTarget = GameObject.Find("CameraTarget");
            if (existingTarget != null)
            {
                cameraTarget = existingTarget.transform;
                if (enableDebugLogs)
                    Debug.Log($"Found existing camera target: {cameraTarget.name}");
                return;
            }
            
            // Create a new camera target
            GameObject targetGO = new GameObject("CameraTarget");
            cameraTarget = targetGO.transform;
            cameraTarget.position = Vector3.zero;
            
            if (enableDebugLogs)
                Debug.Log("Created new camera target at origin");
        }

        private void FindOrbitalFollow()
        {
            // Look for Cinemachine Virtual Camera with Orbital Follow
            var virtualCamera = FindFirstObjectByType<CinemachineVirtualCamera>();
            if (virtualCamera != null)
            {
                orbitalFollow = virtualCamera.GetCinemachineComponent<CinemachineOrbitalFollow>();
                if (orbitalFollow != null && enableDebugLogs)
                    Debug.Log($"Found orbital follow on virtual camera: {virtualCamera.name}");
            }
            
            if (orbitalFollow == null && enableDebugLogs)
                Debug.LogWarning("No CinemachineOrbitalFollow found! Camera rotation and zoom may not work.");
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            InputEvents.OnCameraMove -= HandleCameraMove;
            InputEvents.OnCameraLook -= HandleCameraLook;
            InputEvents.OnCameraScroll -= HandleCameraScroll;
            InputEvents.OnMiddleClick -= HandleMiddleClick;
        }

        private void Update()
        {
            var dt = Time.deltaTime; // Use regular deltaTime for smoother movement
            UpdateMovement(dt);
            UpdateRotation(dt);
            UpdateZoom(dt);
        }

        private void HandleCameraMove(Vector2 direction)
        {
            moveInput = direction;
            if (enableDebugLogs && direction.magnitude > 0.1f)
                Debug.Log($"Camera move input: {direction}");
        }

        private void HandleCameraLook(Vector2 lookDelta)
        {
            lookInput = lookDelta;
            if (enableDebugLogs && lookDelta.magnitude > 0.1f)
                Debug.Log($"Camera look input: {lookDelta}");
        }

        private void HandleCameraScroll(Vector2 scrollDelta)
        {
            scrollInput = scrollDelta;
            if (enableDebugLogs && scrollDelta.magnitude > 0.1f)
                Debug.Log($"Camera scroll input: {scrollDelta}");
        }

        private void HandleMiddleClick(Vector2 position)
        {
            // Could use position for camera focus, etc.
            middleClickInput = true;
        }

        private void UpdateMovement(float deltaTime)
        {
            if (cameraTarget == null)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("Camera target is null! Cannot move camera.");
                return;
            }

            // Only move if there's meaningful input
            if (moveInput.magnitude < 0.1f) return;

            // Get camera forward and right vectors for movement
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;
            
            // If we have orbital follow, use its orientation
            if (orbitalFollow != null)
            {
                // Get the camera's forward and right vectors from the orbital follow
                var virtualCamera = orbitalFollow.VirtualCamera;
                if (virtualCamera != null)
                {
                    forward = virtualCamera.transform.forward;
                    right = virtualCamera.transform.right;
                }
            }
            
            // Flatten Y component for ground movement
            forward.y = 0;
            forward.Normalize();
            right.y = 0;
            right.Normalize();
            
            Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed;
            
            Vector3 motion = targetVelocity * deltaTime;
            cameraTarget.position += forward * motion.z + right * motion.x;
            
            if (enableDebugLogs)
                Debug.Log($"Moving camera target to: {cameraTarget.position}");
        }

        private void UpdateRotation(float dt)
        {
            if (orbitalFollow == null)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("Orbital follow is null! Cannot rotate camera.");
                return;
            }
            
            Vector2 rotationInput = lookInput * (middleClickInput ? 1f : 0f);
            
            if (!middleClickInput) return;
            
            // Only rotate if there's meaningful input
            if (rotationInput.magnitude < 0.1f) return;
            
            // Update TargetOffset for rotation
            Vector3 currentOffset = orbitalFollow.TargetOffset;
            
            // Horizontal rotation (left/right) - affects X offset
            currentOffset.x += rotationInput.x * horizontalRotationSpeed * dt;
            
            // Vertical rotation (up/down) - affects Y offset
            // CinemachineOrbitalFollow handles boundary checking with VerticalAxis.Wrap disabled
            currentOffset.y += rotationInput.y * verticalRotationSpeed * dt;
            
            // Apply the new offset
            orbitalFollow.TargetOffset = currentOffset;
            
            if (enableDebugLogs)
                Debug.Log($"Rotating camera - TargetOffset: {currentOffset}, Vertical Delta: {rotationInput.y * verticalRotationSpeed * dt:F3}");
        }

        private void UpdateZoom(float dt)
        {
            if (orbitalFollow == null)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("Orbital follow is null! Cannot zoom camera.");
                return;
            }
            
            // Only zoom if there's meaningful scroll input
            if (scrollInput.magnitude < 0.1f) return;
            
            // Get zoom direction from scroll input (usually Y axis)
            float zoomDelta = scrollInput.y * zoomSpeed * dt;
            
            // Update radius for zoom
            float currentRadius = orbitalFollow.Radius;
            currentRadius -= zoomDelta; // Negative because scroll up should zoom in
            currentRadius = Mathf.Clamp(currentRadius, minRadius, maxRadius);
            
            // Apply the new radius
            orbitalFollow.Radius = currentRadius;
            
            if (enableDebugLogs)
                Debug.Log($"Zooming camera - Radius: {currentRadius:F2}, Delta: {zoomDelta:F2}");
        }

        public void SetCameraTarget(Transform target)
        {
            cameraTarget = target;
            if (enableDebugLogs)
                Debug.Log($"Camera target set to: {target?.name ?? "null"}");
        }

        public void SetOrbitalFollow(CinemachineOrbitalFollow follow)
        {
            orbitalFollow = follow;
            if (enableDebugLogs)
                Debug.Log($"Orbital follow set to: {follow?.name ?? "null"}");
        }

        // Public getters for debugging
        public Transform CameraTarget 
        { 
            get { return cameraTarget; } 
        }
        
        public CinemachineOrbitalFollow OrbitalFollow
        {
            get { return orbitalFollow; }
        }
        
        public Vector2 CurrentMoveInput 
        { 
            get { return moveInput; } 
        }
        
        public Vector2 CurrentLookInput 
        { 
            get { return lookInput; } 
        }
        
        public float CurrentRadius
        {
            get { return orbitalFollow != null ? orbitalFollow.Radius : 0f; }
        }
    }
} 