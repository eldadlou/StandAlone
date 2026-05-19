using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;

namespace MyGame.RuntimeSystems.Combat
{
    /// <summary>
    /// Lightweight FireSystem that coordinates with individual combat units
    /// Handles global combat effects, projectiles, and coordination
    /// </summary>
    public class LightweightFireSystem : MonoBehaviour, ICombatFireCoordinator
    {
        [Header("Global Combat Settings")]
        [SerializeField] private bool enableProjectiles = true;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 20f;
        
        [Header("Combat Effects")]
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private float explosionLifetime = 3f; // How long explosion effects last
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip explosionSound;
        
        // Tracking
        private List<ICombatUnit> registeredCombatUnits = new List<ICombatUnit>();
        private Dictionary<IUnit, float> globalCooldowns = new Dictionary<IUnit, float>();
        
        // Events
        public System.Action<IUnit, IUnit> OnUnitAttack; // Attacker, Target
        public System.Action<IUnit> OnUnitDeath;
        
        private void Awake()
        {
            // Find all combat units in the scene
            FindCombatUnits();
        }

        /// <summary>
        /// Registers this instance with the dependency container as concrete type and as <see cref="ICombatFireCoordinator"/>.
        /// </summary>
        public void RegisterWithDependencyContainer()
        {
            var container = DependencyContainer.Instance;
            container.Register(this);
            container.RegisterAs<ICombatFireCoordinator>(this);
        }
        
        private void FindCombatUnits()
        {
            var combatUnits = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ICombatUnit>();
            foreach (var combatUnit in combatUnits)
                RegisterCombatUnit(combatUnit);
        }
        
        public void RegisterCombatUnit(ICombatUnit combatUnit)
        {
            if (!registeredCombatUnits.Contains(combatUnit))
            {
                registeredCombatUnits.Add(combatUnit);
                // Debug.Log($"Registered combat unit: {combatUnit}");
            }
        }
        
        public void UnregisterCombatUnit(ICombatUnit combatUnit)
        {
            if (registeredCombatUnits.Contains(combatUnit))
            {
                registeredCombatUnits.Remove(combatUnit);
                // Debug.Log($"Unregistered combat unit: {combatUnit}");
            }
        }
        
        /// <summary>
        /// Called by combat units when they want to perform an attack
        /// This system handles the visual/audio effects and coordination
        /// </summary>
        public bool ProcessAttack(IUnit attacker, IUnit target)
        {
            if (attacker == null || target == null) return false;
            
            // Check global cooldown if needed
            if (globalCooldowns.ContainsKey(attacker))
            {
                if (Time.time < globalCooldowns[attacker]) return false;
            }
            
            // Create attack effects
            CreateAttackEffects(attacker, target);
            
            // Set global cooldown
            globalCooldowns[attacker] = Time.time + 0.1f; // Small global cooldown
            
            // Notify listeners
            OnUnitAttack?.Invoke(attacker, target);
            
            return true;
        }
        
        private void CreateAttackEffects(IUnit attacker, IUnit target)
        {
            // Create projectile if enabled
            if (enableProjectiles && projectilePrefab)
            {
                Vector3 firePosition = attacker.Position + Vector3.up * 1f; // Fire from slightly above
                GameObject projectile = Instantiate(projectilePrefab, firePosition, Quaternion.identity);
                
                // Set projectile direction
                Vector3 direction = (target.Position - firePosition).normalized;
                projectile.transform.rotation = Quaternion.LookRotation(direction);
                
                // Add projectile behavior
                ProjectileBehavior projectileBehavior = projectile.GetComponent<ProjectileBehavior>();
                projectileBehavior?.Initialize(attacker, target, projectileSpeed);
            }
            
            // Play fire sound
            if (fireSound != null)
            {
                AudioSource.PlayClipAtPoint(fireSound, attacker.Position);
            }
        }
        
        /// <summary>
        /// Called when a projectile hits its target
        /// </summary>
        public void OnProjectileHit(IUnit target, Vector3 hitPosition, IUnit attacker)
        {
            // Create explosion effect with automatic cleanup
            if (explosionPrefab is not null)
            {
                GameObject explosion = Instantiate(explosionPrefab, hitPosition, Quaternion.identity);
                // Destroy the explosion effect after specified lifetime
                Destroy(explosion, explosionLifetime);
            }
            
            // Play explosion sound
            if (explosionSound!= null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, hitPosition);
            }
            
            // Apply damage to target
            if (target != null)
            {
                // This would typically be handled by the target's damage system
                string targetInfo = target is ICombatUnit combatTarget ? 
                    $"{combatTarget.Name} ({target.Owner?.Team.ToString() ?? "Unknown"})" : 
                    $"{target.Owner?.Team.ToString() ?? "Unknown"}";
                // Debug.Log($"Projectile hit {targetInfo} at {hitPosition}");
            }
        }
        
        /// <summary>
        /// Get all combat units in the system
        /// </summary>
        public ICombatUnit[] GetCombatUnits()
        {
            return registeredCombatUnits.ToArray();
        }
        
        /// <summary>
        /// Get combat units by team
        /// </summary>
        public ICombatUnit[] GetCombatUnitsByTeam(Team team)
        {
            return registeredCombatUnits.Where(unit => 
                unit.Owner != null && unit.Owner.Team == team).ToArray();
        }
        
        /// <summary>
        /// Find enemies within range of a position
        /// </summary>
        public IUnit[] FindEnemiesInRange(Vector3 position, float range, Team excludeTeam)
        {
            List<IUnit> enemies = new List<IUnit>();
            
            foreach (var combatUnit in registeredCombatUnits)
            {
                if (combatUnit.Owner != null && combatUnit.Owner.Team != excludeTeam)
                {
                    float distance = Vector3.Distance(position, combatUnit.Position);
                    if (distance <= range)
                    {
                        enemies.Add(combatUnit);
                    }
                }
            }
            
            return enemies.ToArray();
        }
        
        // Visual debugging
        private void OnDrawGizmos()
        {
            // Show registered combat units
            foreach (var combatUnit in registeredCombatUnits)
            {
                if (combatUnit != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(combatUnit.Position, 1f);
                }
            }
        }
    }
}
