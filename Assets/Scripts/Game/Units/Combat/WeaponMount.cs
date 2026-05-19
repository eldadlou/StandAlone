using UnityEngine;
using MyGame.Presentation;

namespace MyGame.Core.Units.Combat
{
    /// <summary>
    /// Represents a single weapon mount on a vehicle.
    /// This is a reusable component that can be used on any vehicle type (tanks, jeeps, APCs, etc.)
    /// Each WeaponMount handles its own turret rotation, firing, and projectile creation.
    /// 
    /// SETUP:
    /// 1. Assign a WeaponPreset (REQUIRED) - contains all weapon stats (damage, range, prefab, etc.)
    /// 2. Assign the turret transform for this specific vehicle
    /// 3. Optionally assign a fire point transform
    /// </summary>
    [System.Serializable]
    public class WeaponMount
    {
        [Header("Weapon Configuration (REQUIRED)")]
        [Tooltip("The weapon preset containing all stats (damage, range, projectile, etc.)")]
        [SerializeField] private WeaponPreset weaponPreset;
        
        [Header("Vehicle-Specific Transforms")]
        [Tooltip("The turret transform on this specific vehicle")]
        [SerializeField] private Transform turretTransform;
        [Tooltip("Optional: specific fire point, defaults to turret position")]
        [SerializeField] private Transform firePoint;
        
        // Runtime state
        private float lastFireTime;
        private GunTurret gunTurretComponent;
        private bool isInitialized;
        
        // Properties - all stats come from the preset
        public WeaponPreset Preset => weaponPreset;
        public bool HasPreset => weaponPreset != null;
        
        public string WeaponName => weaponPreset?.WeaponName ?? "No Preset";
        public WeaponType Type => weaponPreset?.Type ?? WeaponType.MachineGun;
        public float Damage => weaponPreset?.Damage ?? 0f;
        public float Range => weaponPreset?.Range ?? 0f;
        public float Cooldown => weaponPreset?.Cooldown ?? 1f;
        public float Accuracy => weaponPreset?.Accuracy ?? 0f;
        public float RotationSpeed => weaponPreset?.RotationSpeed ?? 90f;
        public float RotationThreshold => weaponPreset?.RotationThreshold ?? 5f;
        public float ProjectileSpeed => weaponPreset?.ProjectileSpeed ?? 30f;
        public GameObject ProjectilePrefab => weaponPreset?.ProjectilePrefab;
        
        public Transform TurretTransform => turretTransform;
        public bool IsAvailable => turretTransform != null;
        public bool HasProjectile => ProjectilePrefab != null;
        public float LastFireTime => lastFireTime;
        public bool IsOnCooldown => Time.time - lastFireTime < Cooldown;
        public float CooldownRemaining => Mathf.Max(0, Cooldown - (Time.time - lastFireTime));
        
        /// <summary>
        /// Get the fire position (firePoint if set, otherwise turret position)
        /// </summary>
        public Vector3 FirePosition => firePoint != null ? firePoint.position : 
            (turretTransform != null ? turretTransform.position : Vector3.zero);
        
        /// <summary>
        /// Initialize the weapon mount. Call this in the owning unit's Awake/Start.
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;
            
            if (turretTransform != null)
            {
                gunTurretComponent = turretTransform.GetComponent<GunTurret>();
                if (gunTurretComponent != null)
                {
                    // Use property values (which respect preset if assigned)
                    gunTurretComponent.SetRotationSpeed(RotationSpeed);
                    gunTurretComponent.SetRotationThreshold(RotationThreshold);
                }
            }
            
            lastFireTime = -Cooldown; // Allow immediate first shot
            isInitialized = true;
            
            string presetInfo = HasPreset ? $" (using preset: {weaponPreset.name})" : "";
            Debug.Log($"WeaponMount '{WeaponName}' initialized{presetInfo} - Available: {IsAvailable}, HasProjectile: {HasProjectile}");
        }
        
        /// <summary>
        /// Check if this weapon can engage a target at the given distance
        /// </summary>
        public bool CanEngageAtDistance(float distance)
        {
            return IsAvailable && HasPreset && distance <= Range;
        }
        
        /// <summary>
        /// Check if the weapon can fire (available, has projectile, not on cooldown)
        /// </summary>
        public bool CanFire()
        {
            return IsAvailable && HasProjectile && !IsOnCooldown;
        }
        
        /// <summary>
        /// Rotate the turret towards a target position
        /// </summary>
        public void RotateTowards(Vector3 targetPosition)
        {
            if (!IsAvailable) return;
            
            if (gunTurretComponent != null)
            {
                gunTurretComponent.SetTarget(targetPosition);
            }
            else
            {
                // Manual rotation fallback
                Vector3 direction = (targetPosition - turretTransform.position).normalized;
                direction.y = 0;
                
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    turretTransform.rotation = Quaternion.RotateTowards(
                        turretTransform.rotation,
                        targetRotation,
                        RotationSpeed * Time.deltaTime
                    );
                }
            }
        }
        
        /// <summary>
        /// Check if the turret is facing the target
        /// </summary>
        public bool IsFacingTarget(Vector3 targetPosition)
        {
            if (!IsAvailable) return false;
            
            if (gunTurretComponent != null)
            {
                return gunTurretComponent.IsFacingTarget(targetPosition);
            }
            
            // Manual facing check fallback
            Vector3 direction = (targetPosition - turretTransform.position).normalized;
            direction.y = 0;
            
            Vector3 forward = turretTransform.forward;
            forward.y = 0;
            
            float angle = Vector3.Angle(forward, direction);
            return angle <= RotationThreshold;
        }
        
        /// <summary>
        /// Fire the weapon and create a projectile
        /// </summary>
        /// <param name="owner">The unit firing the weapon (for IUnit reference)</param>
        /// <param name="target">The target unit</param>
        /// <param name="hitResult">The accuracy hit result</param>
        /// <param name="actualDamage">The damage to deal</param>
        /// <returns>The created projectile GameObject, or null if firing failed</returns>
        public GameObject Fire(IUnit owner, IUnit target, HitResult hitResult, float actualDamage)
        {
            if (!CanFire())
            {
                Debug.LogWarning($"WeaponMount '{WeaponName}': Cannot fire - Available: {IsAvailable}, HasProjectile: {HasProjectile}, OnCooldown: {IsOnCooldown}");
                return null;
            }
            
            // Calculate target position with accuracy deviation
            Vector3 targetPosition = target.Position;
            if (hitResult != HitResult.FullHit)
            {
                float deviationAmount = hitResult == HitResult.Miss ? 3f : 1.5f;
                Vector3 deviation = Random.insideUnitSphere * deviationAmount;
                deviation.y = 0;
                targetPosition += deviation;
            }
            
            // Create projectile
            Vector3 spawnPosition = FirePosition;
            if (spawnPosition == Vector3.zero)
            {
                // Fallback to owner position if no fire point
                spawnPosition = owner.Position + Vector3.up;
            }
            
            // Use the property which respects preset
            GameObject prefabToUse = ProjectilePrefab;
            GameObject projectile = Object.Instantiate(prefabToUse, spawnPosition, Quaternion.identity);
            
            // Set projectile direction
            Vector3 direction = (targetPosition - spawnPosition).normalized;
            projectile.transform.rotation = Quaternion.LookRotation(direction);
            
            // Initialize projectile behavior (use ProjectileSpeed property which respects preset)
            var projectileBehavior = projectile.GetComponent<MyGame.RuntimeSystems.Combat.ProjectileBehavior>();
            if (projectileBehavior != null)
            {
                projectileBehavior.Initialize(owner, target, ProjectileSpeed, hitResult, actualDamage);
            }
            else
            {
                Debug.LogWarning($"WeaponMount '{WeaponName}': Projectile prefab missing ProjectileBehavior component!");
            }
            
            // Update cooldown
            lastFireTime = Time.time;
            
            Debug.Log($"WeaponMount '{WeaponName}': Fired at {target.Name} - HitResult: {hitResult}, Damage: {actualDamage:F1}");
            
            return projectile;
        }
        
        /// <summary>
        /// Reset cooldown (useful for special abilities or power-ups)
        /// </summary>
        public void ResetCooldown()
        {
            lastFireTime = -Cooldown;
        }
        
        /// <summary>
        /// Draw debug gizmos for this weapon
        /// </summary>
        public void DrawGizmos(Vector3 position, Color color)
        {
            if (!IsAvailable) return;
            
            Gizmos.color = color;
            Gizmos.DrawWireSphere(position, Range);
            
            // Draw turret forward direction
            if (turretTransform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(turretTransform.position, turretTransform.forward * 3f);
            }
        }
    }
}
