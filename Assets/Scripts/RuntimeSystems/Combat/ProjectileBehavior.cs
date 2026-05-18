using UnityEngine;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;
using MyGame.Game;

namespace MyGame.RuntimeSystems.Combat
{
    /// <summary>
    /// Handles projectile movement and collision detection
    /// </summary>
    public class ProjectileBehavior : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 20f;
        [SerializeField] private float lifetime = 10f;
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private float hitEffectLifetime = 3f; // How long hit effects last
        
        // Private fields
        private Vector3 targetPosition;
        private IUnit target;
        private IUnit attacker;
        private float damage;
        private bool isInitialized = false;
        private bool hasHit = false; // Prevent multiple hits
        private float spawnTime;
        
        // Accuracy system fields
        private HitResult hitResult = HitResult.FullHit;
        private float actualDamage = 0f;
        private bool hasAccuracyInfo = false;
        
        private void Start()
        {
            spawnTime = Time.time;
        }
        
        private void Update()
        {
            
            
            // Check lifetime
            if (Time.time - spawnTime > lifetime)
            {
                DestroyProjectile();
                return;
            }
            if (!isInitialized || hasHit) return;
            // Move towards target
            MoveTowardsTarget();
            
            // Check if we've reached the target (distance-based hit detection)
            if (target != null && Vector3.Distance(transform.position, target.Position) < 0.5f)
            {
                OnHitTarget();
            }
        }
        
        /// <summary>
        /// Initialize the projectile with target and attacker information
        /// </summary>
        public void Initialize(IUnit attacker, IUnit target, float projectileSpeed)
        {
            Initialize(attacker, target, projectileSpeed, HitResult.FullHit, attacker.AttackDamage);
        }
        
        /// <summary>
        /// Initialize the projectile with accuracy information
        /// </summary>
        public void Initialize(IUnit attacker, IUnit target, float projectileSpeed, HitResult hitResult, float actualDamage)
        {
            this.attacker = attacker;
            this.target = target;
            this.speed = projectileSpeed;
            this.targetPosition = target.Position;
            this.damage = actualDamage; // Use actual damage from accuracy system
            this.hitResult = hitResult;
            this.actualDamage = actualDamage;
            this.isInitialized = true;
            this.hasHit = false; // Reset hit state
            this.hasAccuracyInfo = true;
            
            // Set initial rotation towards target
            if (target != null)
            {
                Vector3 direction = (target.Position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
        
        private void MoveTowardsTarget()
        {
            if (target == null) return;
            
            // Update target position (in case target is moving)
            targetPosition = target.Position;
            
            // Move towards target
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            
            // Update rotation to face movement direction
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        private void OnHitTarget()
        {
            // Prevent multiple hits
            if (hasHit) return;
            hasHit = true;
            
            if (target != null)
            {
                // Apply damage to target
                ApplyDamage();
                
                // Create hit effect with automatic cleanup
                if (hitEffect != null)
                {
                    GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
                    // Destroy the hit effect after specified lifetime
                    Destroy(effect, hitEffectLifetime);
                }
                
                // Notify LightweightFireSystem using dependency injection
                var fireCoordinator = SystemInitializer.GetSystem<ICombatFireCoordinator>();
                if (fireCoordinator != null)
                    fireCoordinator.OnProjectileHit(target, transform.position, attacker);
                else
                {
                    // Debug.LogWarning("ProjectileBehavior: LightweightFireSystem not found in dependency container");
                }
            }
            
            if (destroyOnHit)
            {
                DestroyProjectile();
            }
        }
        
        private void ApplyDamage()
        {
            if (target == null || attacker == null) return;
            
            // Apply damage based on accuracy system
            if (hasAccuracyInfo)
            {
                // Use the damage calculated from accuracy system
                ApplyDamageWithAccuracy();
            }
            else
            {
                // Fallback to original damage system
                ApplyDamageLegacy();
            }
        }
        
        private void ApplyDamageWithAccuracy()
        {
            if (target == null || attacker == null) return;
            
            // Log hit result and damage
            string hitResultText = hitResult switch
            {
                HitResult.FullHit => "FULL HIT",
                HitResult.PartialHit => "PARTIAL HIT",
                HitResult.Miss => "MISS",
                _ => "UNKNOWN"
            };
            
            // Debug.Log($"{attacker.Name} -> {target.Name}: {hitResultText} for {actualDamage:F1} damage");
            
            // Only apply damage if not a complete miss
            if (hitResult != HitResult.Miss && actualDamage > 0)
            {
                // Try to get the target's damage system
                var damageable = target as IDamageable;
                if (damageable != null)
                {
                    damageable.TakeDamage(actualDamage);
                }
                else
                {
                    // Fallback: try to find a Unit component
                    Unit unit = target as Unit;
                    if (unit != null)
                    {
                        unit.TakeDamage(actualDamage);
                    }
                }
                
                // Notify AlliedSupportSystem to alert nearby friendly units
                NotifyAlliedSupportSystem(actualDamage);
            }
            else if (hitResult == HitResult.Miss)
            {
                // Debug.Log($"{attacker.Name} completely missed {target.Name}!");
            }
        }
        
        private void ApplyDamageLegacy()
        {
            if (target == null || attacker == null) return;
            
            // Try to get the target's damage system
            var damageable = target as IDamageable;
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            else
            {
                // Fallback: try to find a Unit component
                Unit unit = target as Unit;
                if (unit != null)
                {
                    unit.TakeDamage(damage);
                }
            }
            
            // Notify AlliedSupportSystem to alert nearby friendly units
            NotifyAlliedSupportSystem(damage);
        }
        
        /// <summary>
        /// Notify the AlliedSupportSystem that a unit was attacked
        /// This allows nearby friendly units to respond and help
        /// </summary>
        private void NotifyAlliedSupportSystem(float damageDealt)
        {
            if (target == null || attacker == null) return;
            
            // Get AlliedSupportSystem using dependency injection
            var support = SystemInitializer.GetSystem<IAlliedCombatSupport>();
            support?.NotifyUnitAttacked(target, attacker, damageDealt);
        }
        
        private void DestroyProjectile()
        {
            Destroy(gameObject);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Prevent multiple hits
            if (hasHit) return;
            
            // Use GetComponentInParent to find IUnit even if collider is on a child object
            IUnit hitUnit = other.GetComponentInParent<IUnit>();
            
            // If we hit a unit, check if it's friendly or enemy
            if (hitUnit != null)
            {
                // Ignore friendly units completely - projectile passes through them
                if (IsFriendlyUnit(hitUnit))
                {
                    return; // Pass through friendly units
                }
                
                // Check if we hit the intended target (enemy)
                if (target != null && hitUnit == target)
                {
                    OnHitTarget();
                }
                // Optionally: hit any enemy unit (not just the target)
                // Uncomment the following if you want projectiles to hit ANY enemy, not just the target
                // else if (IsEnemyUnit(hitUnit))
                // {
                //     // Hit a different enemy - could handle this differently
                //     OnHitTarget();
                // }
            }
            else
            {
                // Hit something that's not a unit (ground, obstacle, etc.)
                // Destroy the projectile on impact with environment
                if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || 
                    other.gameObject.layer == LayerMask.NameToLayer("Default") ||
                    other.CompareTag("Ground") || other.CompareTag("Terrain"))
                {
                    hasHit = true;
                    DestroyProjectile();
                }
            }
        }
        
        /// <summary>
        /// Check if the hit unit is friendly (same team as attacker)
        /// </summary>
        private bool IsFriendlyUnit(IUnit unit)
        {
            if (attacker == null || unit == null) return false;
            if (attacker.Owner == null || unit.Owner == null) return false;
            
            return attacker.Owner.Team == unit.Owner.Team;
        }
        
        /// <summary>
        /// Check if the hit unit is an enemy (different team from attacker)
        /// </summary>
        private bool IsEnemyUnit(IUnit unit)
        {
            if (attacker == null || unit == null) return false;
            if (attacker.Owner == null || unit.Owner == null) return false;
            
            return attacker.Owner.Team != unit.Owner.Team;
        }
        
        // Visual debugging
        private void OnDrawGizmos()
        {
            if (!isInitialized) return;
            
            // Draw projectile path
            Gizmos.color = hasHit ? Color.gray : Color.red;
            Gizmos.DrawLine(transform.position, targetPosition);
            
            // Draw speed indicator
            Gizmos.color = hasHit ? Color.gray : Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
    
    /// <summary>
    /// Interface for objects that can take damage
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
}
