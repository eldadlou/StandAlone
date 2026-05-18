using System;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Core.Interfaces;
using MyGame.Core;
using MyGame.Core.Events;
using MyGame.Core.Services;

namespace MyGame.RuntimeSystems.Combat
{
    /// <summary>
    /// Handles explosions, area damage, and chain reactions
    /// </summary>
    public class ExplosionSystem : MonoBehaviour, IExplosionService
    {
        [Header("Explosion Settings")]
        public LayerMask destructibleLayers = -1;
        public bool enableChainReactions = true;
        public float chainReactionDelay = 0.2f;
        public float maxChainDistance = 15f;
        
        [Header("Damage Falloff")]
        public AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0.3f);
        
        private List<IDestructible> pendingExplosions = new List<IDestructible>();
        
        private void Awake()
        {
            var container = DependencyContainer.Instance;
            container.Register(this);
            container.RegisterAs<IExplosionService>(this);
        }
        
        /// <summary>
        /// Trigger an explosion at a specific location
        /// </summary>
        public void CreateExplosion(Vector3 position, float radius, float damage, IDestructible source = null)
        {
            Debug.Log($"ExplosionSystem: Creating explosion at {position} with radius {radius} and damage {damage}");
            
            // Find all destructible objects in range
            List<IDestructible> affectedObjects = FindDestructiblesInRange(position, radius);
            
            // Apply damage to each object based on distance
            foreach (var destructible in affectedObjects)
            {
                if (destructible == source) continue; // Don't damage the source of explosion
                
                float distance = Vector3.Distance(position, destructible.Position);
                float damageMultiplier = damageFalloff.Evaluate(distance / radius);
                float finalDamage = damage * damageMultiplier;
                
                if (finalDamage > 0)
                {
                    destructible.TakeDamage(finalDamage, source);
                    
                    // Check for chain reaction
                    if (enableChainReactions && destructible.CanExplode && 
                        distance <= maxChainDistance && finalDamage >= destructible.Health)
                    {
                        ScheduleChainExplosion(destructible);
                    }
                }
            }
            
            // Trigger explosion events
            GameEvents.TriggerExplosion(position, radius, damage, source);
        }
        
        /// <summary>
        /// Find all destructible objects within explosion radius
        /// </summary>
        private List<IDestructible> FindDestructiblesInRange(Vector3 center, float radius)
        {
            List<IDestructible> destructibles = new List<IDestructible>();
            
            // Use Physics.OverlapSphere to find potential targets
            Collider[] colliders = Physics.OverlapSphere(center, radius, destructibleLayers);
            
            foreach (var collider in colliders)
            {
                // Check if object implements IDestructible
                IDestructible destructible = collider.GetComponent<IDestructible>();
                if (destructible != null && !destructible.IsDestroyed)
                {
                    // Use the interface method to check if it's actually in range
                    if (destructible.IsInExplosionRange(center, radius))
                    {
                        destructibles.Add(destructible);
                    }
                }
            }
            
            return destructibles;
        }
        
        /// <summary>
        /// Schedule a chain explosion with delay
        /// </summary>
        private void ScheduleChainExplosion(IDestructible destructible)
        {
            if (!pendingExplosions.Contains(destructible))
            {
                pendingExplosions.Add(destructible);
                StartCoroutine(DelayedChainExplosion(destructible));
            }
        }
        
        /// <summary>
        /// Execute chain explosion after delay
        /// </summary>
        private System.Collections.IEnumerator DelayedChainExplosion(IDestructible destructible)
        {
            yield return new WaitForSeconds(chainReactionDelay);
            
            if (destructible != null && !destructible.IsDestroyed && destructible.CanExplode)
            {
                // Trigger the destructible's own explosion
                destructible.Explode();
            }
            
            pendingExplosions.Remove(destructible);
        }
        
        /// <summary>
        /// Handle explosion triggered by a destructible object
        /// </summary>
        public void HandleObjectExplosion(IDestructible source)
        {
            if (source == null || !source.CanExplode) return;
            
            CreateExplosion(source.Position, source.ExplosionRadius, source.ExplosionDamage, source);
        }
        
        /// <summary>
        /// Subscribe to a destructible object's explosion events
        /// </summary>
        public void SubscribeToDestructible(IDestructible destructible)
        {
            if (destructible != null)
            {
                destructible.OnExploded += (source, pos, radius, damage) => 
                    CreateExplosion(pos, radius, damage, source);
            }
        }
        
        /// <summary>
        /// Check if a position would be affected by an explosion
        /// </summary>
        public bool WouldBeAffectedByExplosion(Vector3 targetPosition, Vector3 explosionCenter, float radius)
        {
            return Vector3.Distance(targetPosition, explosionCenter) <= radius;
        }
        
        /// <summary>
        /// Calculate damage at a specific distance from explosion center
        /// </summary>
        public float CalculateDamageAtDistance(float baseDamage, float distance, float maxRadius)
        {
            if (distance >= maxRadius) return 0f;
            
            float normalizedDistance = distance / maxRadius;
            return baseDamage * damageFalloff.Evaluate(normalizedDistance);
        }
    }
}