using UnityEngine;
using MyGame.Core.Units;

namespace MyGame.Presentation
{
    /// <summary>
    /// Handles visual rotation of gun turrets on units
    /// Works in conjunction with FireSystem for combat targeting
    /// </summary>
    public class GunTurret : MonoBehaviour
    {
        [Header("Turret Settings")]
        public float rotationSpeed = 90f; // degrees per second
        public float rotationThreshold = 5f; // degrees tolerance for "facing target"
        public bool smoothRotation = true;
        public bool limitRotation = false;
        
        [Header("Rotation Limits")]
        public float minRotationAngle = -180f;
        public float maxRotationAngle = 180f;
        
        [Header("Visual Effects")]
        public bool showRotationGizmos = true;
        public Color gizmoColor = Color.red;
        
        // Private fields
        private Quaternion targetRotation;
        private bool isRotating = false;
        private Vector3 lastTargetPosition;
        
        private void Start()
        {
            Debug.Log($"GunTurret {name}: Start called");
            
            // Initialize target rotation to current rotation
           // targetRotation = transform.rotation;
            targetRotation = Quaternion.identity;
            
            Debug.Log($"GunTurret {name}: Initialized with rotation speed {rotationSpeed}°/s, threshold {rotationThreshold}°");
        }
        
        private void Update()
        {
            if (isRotating)
            {
                // Only log occasionally to avoid spam
                if (Time.frameCount % 60 == 0) // Log every 60 frames (about once per second)
                {
                    Debug.Log($"GunTurret {name}: Update called - isRotating: {isRotating}");
                }
                
                UpdateRotation();
            }
        }
        
        /// <summary>
        /// Set the target direction for the turret to rotate towards
        /// </summary>
        /// <param name="targetPosition">World position to aim at</param>
        /// <param name="immediate">If true, instantly rotate to target</param>
        public void SetTarget(Vector3 targetPosition, bool immediate = false)
        {
//            Debug.Log($"GunTurret {name}: SetTarget called for position {targetPosition}");
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0; // Keep rotation on horizontal plane
            
            if (direction != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(direction);
                
                if (immediate)
                {
                    transform.rotation = targetRotation;
                    isRotating = false;
             //       Debug.Log($"GunTurret {name}: Immediate rotation to target");
                }
                else
                {
                    isRotating = true;
//                    Debug.Log($"GunTurret {name}: Starting rotation to target");
                }
                
                lastTargetPosition = targetPosition;
            }
            else
            {
                Debug.LogWarning($"GunTurret {name}: SetTarget failed - zero direction vector");
            }
        }
        
        /// <summary>
        /// Stop rotating and maintain current orientation
        /// </summary>
        public void StopRotation()
        {
            isRotating = false;
        }
        
        /// <summary>
        /// Check if the turret is facing the target position
        /// </summary>
        /// <param name="targetPosition">Position to check against</param>
        /// <returns>True if facing target within threshold</returns>
        public bool IsFacingTarget(Vector3 targetPosition)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            directionToTarget.y = 0;
            
            Vector3 turretForward = transform.forward;
            turretForward.y = 0;
            
            float angle = Vector3.Angle(turretForward, directionToTarget);
            bool isFacing = angle <= rotationThreshold;
            
      //      Debug.Log($"GunTurret {name}: IsFacingTarget - Angle: {angle:F1}°, Threshold: {rotationThreshold:F1}°, Facing: {isFacing}");
            
            return isFacing;
        }
        
        /// <summary>
        /// Get the current rotation progress (0-1)
        /// </summary>
        /// <returns>Rotation progress as a value between 0 and 1</returns>
        public float GetRotationProgress()
        {
            if (!isRotating) return 1f;
            
            float currentAngle = Vector3.Angle(transform.forward, targetRotation * Vector3.forward);
            float totalAngle = Vector3.Angle(Quaternion.identity * Vector3.forward, targetRotation * Vector3.forward);
            
            if (totalAngle == 0) return 1f;
            
            return 1f - (currentAngle / totalAngle);
        }
        
        private void UpdateRotation()
        {
            if (smoothRotation)
            {
                // Smooth rotation towards target
                Quaternion oldRotation = transform.rotation;
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
                
                float rotationDelta = Quaternion.Angle(oldRotation, transform.rotation);
                if (rotationDelta > 0.1f) // Only log if there's significant rotation
                {
//                    Debug.Log($"GunTurret {name}: Rotating by {rotationDelta:F1}° towards target");
                }
            }
            else
            {
                // Instant rotation
                transform.rotation = targetRotation;
        //        Debug.Log($"GunTurret {name}: Instant rotation to target");
            }
            
            // Apply rotation limits if enabled
            if (limitRotation)
            {
                ApplyRotationLimits();
            }
            
            // Check if we've reached the target rotation
            float angleToTarget = Quaternion.Angle(transform.rotation, targetRotation);
            if (angleToTarget <= rotationThreshold)
            {
                isRotating = false;
//                Debug.Log($"GunTurret {name}: Rotation complete - angle to target: {angleToTarget:F1}°");
            }
        }
        
        private void ApplyRotationLimits()
        {
            Vector3 eulerAngles = transform.rotation.eulerAngles;
            
            // Normalize Y angle to -180 to 180 range
            float yAngle = eulerAngles.y;
            if (yAngle > 180f)
                yAngle -= 360f;
            
            // Clamp to limits
            yAngle = Mathf.Clamp(yAngle, minRotationAngle, maxRotationAngle);
            
            // Apply clamped rotation
            transform.rotation = Quaternion.Euler(eulerAngles.x, yAngle, eulerAngles.z);
        }
        
        /// <summary>
        /// Set rotation speed
        /// </summary>
        /// <param name="speed">Rotation speed in degrees per second</param>
        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = speed;
        }
        
        /// <summary>
        /// Set rotation threshold
        /// </summary>
        /// <param name="threshold">Angle threshold in degrees</param>
        public void SetRotationThreshold(float threshold)
        {
            rotationThreshold = threshold;
        }
        
        /// <summary>
        /// Enable or disable rotation limits
        /// </summary>
        /// <param name="enabled">Whether to enable rotation limits</param>
        /// <param name="minAngle">Minimum rotation angle</param>
        /// <param name="maxAngle">Maximum rotation angle</param>
        public void SetRotationLimits(bool enabled, float minAngle = -180f, float maxAngle = 180f)
        {
            limitRotation = enabled;
            minRotationAngle = minAngle;
            maxRotationAngle = maxAngle;
        }
        
        // Debug visualization
        private void OnDrawGizmosSelected()
        {
            if (!showRotationGizmos) return;
            
            Gizmos.color = gizmoColor;
            
            // Draw turret forward direction
            Vector3 forward = transform.forward * 2f;
            Gizmos.DrawRay(transform.position, forward);
            
            // Draw target direction if we have a target
            if (isRotating)
            {
                Gizmos.color = Color.yellow;
                Vector3 targetDirection = (lastTargetPosition - transform.position).normalized * 2f;
                Gizmos.DrawRay(transform.position, targetDirection);
            }
            
            // Draw rotation limits if enabled
            if (limitRotation)
            {
                Gizmos.color = Color.cyan;
                float radius = 1.5f;
                
                // Draw min angle
                Vector3 minDirection = Quaternion.Euler(0, minRotationAngle, 0) * Vector3.forward * radius;
                Gizmos.DrawRay(transform.position, minDirection);
                
                // Draw max angle
                Vector3 maxDirection = Quaternion.Euler(0, maxRotationAngle, 0) * Vector3.forward * radius;
                Gizmos.DrawRay(transform.position, maxDirection);
                
                // Draw arc between limits
                int segments = 20;
                Vector3 prevPoint = transform.position + minDirection;
                for (int i = 1; i <= segments; i++)
                {
                    float angle = Mathf.Lerp(minRotationAngle, maxRotationAngle, (float)i / segments);
                    Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
                    Vector3 currentPoint = transform.position + direction;
                    
                    Gizmos.DrawLine(prevPoint, currentPoint);
                    prevPoint = currentPoint;
                }
            }
        }
    }
}
