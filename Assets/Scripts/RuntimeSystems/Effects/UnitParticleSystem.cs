using UnityEngine;
using System;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Services;
using Random = UnityEngine.Random;

namespace MyGame.RuntimeSystems.Effects
{
    /// <summary>
    /// Handles particle effects in response to unit events
    /// </summary>
    public class UnitParticleSystem : MonoBehaviour, ISelectionParticleFeedback
    {
        [Header("Death Effects")]
        public GameObject deathExplosionPrefab;
        public GameObject deathSmokePrefab;

        [Header("Attack Effects")]
        public GameObject fireEffectPrefab;
        public GameObject impactEffectPrefab;
        public GameObject muzzleFlashPrefab;

        [Header("Movement Effects")]
        public GameObject dustTrailPrefab;
        public GameObject footstepEffectPrefab;

        [Header("Selection Effects")]
        public GameObject selectionRingPrefab;
        public GameObject highlightEffectPrefab;

        [Header("Explosion Effects")]
        public GameObject largeExplosionPrefab;
        public GameObject shockwavePrefab;
        public GameObject debrisPrefab;
        public GameObject fireballPrefab;

        private void Awake()
        {
            // Don't auto-register - let SystemInitializer handle registration
            // DependencyContainer.Instance.Register(this);
            
            Debug.Log($"✨ UnitParticleSystem {gameObject.name}: Awake called at {Time.time}");
            
            // Subscribe to explosion events
            MyGame.Core.Events.GameEvents.OnExplosion += HandleExplosion;
            MyGame.Core.Events.GameEvents.OnObjectExploded += HandleObjectExploded;
            
            //Debug.Log($"✨ UnitParticleSystem {gameObject.name}: Subscribed to explosion events - OnExplosion subscribers: {MyGame.Core.Events.GameEvents.OnExplosion?.GetInvocationList().Length ?? 0}, OnObjectExploded subscribers: {MyGame.Core.Events.GameEvents.OnObjectExploded?.GetInvocationList().Length ?? 0}");
            
            Debug.Log($"✨ UnitParticleSystem {gameObject.name}: Awake completed, waiting for SystemInitializer registration");
        }
        
        /// <summary>
        /// Manual registration method for SystemInitializer
        /// </summary>
        public void RegisterWithDependencyContainer()
        {
            var container = DependencyContainer.Instance;
            container.Register(this);
            container.RegisterAs<ISelectionParticleFeedback>(this);
            Debug.Log($"✨ UnitParticleSystem {gameObject.name}: Manually registered with DependencyContainer");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from explosion events
            MyGame.Core.Events.GameEvents.OnExplosion -= HandleExplosion;
            MyGame.Core.Events.GameEvents.OnObjectExploded -= HandleObjectExploded;
        }

        public void SubscribeToUnit(IUnit unit)
        {
            if (unit == null) return;

            Debug.Log($"✨ UnitParticleSystem: Subscribing to unit {unit.Name} ({unit.Type})");

            unit.OnDeath += HandleUnitDeath;
            unit.OnAttack += HandleUnitAttack;
            unit.OnMove += HandleUnitMove;
            unit.OnAnimationEvent += HandleAnimationEvent;
            
           // Debug.Log($"✨ UnitParticleSystem: Successfully subscribed to {unit.Name} - OnDeath subscribers: {unit.OnDeath?.GetInvocationList().Length ?? 0}");
        }

        public void UnsubscribeFromUnit(IUnit unit)
        {
            if (unit == null) return;

            unit.OnDeath -= HandleUnitDeath;
            unit.OnAttack -= HandleUnitAttack;
            unit.OnMove -= HandleUnitMove;
            unit.OnAnimationEvent -= HandleAnimationEvent;
        }

        private void HandleUnitDeath(IUnit unit)
        {
            Debug.Log($"💀 UnitParticleSystem: HandleUnitDeath called for {unit.Name} ({unit.Type})");
            
            Vector3 unitPosition = (unit as MonoBehaviour)?.transform.position ?? Vector3.zero;

            // Spawn death explosion
            if (deathExplosionPrefab != null)
            {
                GameObject explosion = Instantiate(deathExplosionPrefab, unitPosition, Quaternion.identity);
                Destroy(explosion, 3f);
                Debug.Log($"✨ UnitParticleSystem: Spawned death explosion for {unit.Name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ UnitParticleSystem: No deathExplosionPrefab assigned for {unit.Name}");
            }

            // Spawn death smoke
            if (deathSmokePrefab != null)
            {
                GameObject smoke = Instantiate(deathSmokePrefab, unitPosition, Quaternion.identity);
                Destroy(smoke, 5f);
                Debug.Log($"✨ UnitParticleSystem: Spawned death smoke for {unit.Name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ UnitParticleSystem: No deathSmokePrefab assigned for {unit.Name}");
            }

            Debug.Log($"✨ Particles: Death effects completed for {unit.Type}");
        }

        private void HandleUnitAttack(IUnit attacker, IUnit target)
        {
            Vector3 attackerPosition = (attacker as MonoBehaviour)?.transform.position ?? Vector3.zero;
            Vector3 targetPosition = (target as MonoBehaviour)?.transform.position ?? Vector3.zero;

            // Spawn muzzle flash at attacker position
            if (muzzleFlashPrefab != null)
            {
                GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, attackerPosition, Quaternion.identity);
                Destroy(muzzleFlash, 0.5f);
            }

            // Spawn fire effect
            if (fireEffectPrefab != null)
            {
                GameObject fireEffect = Instantiate(fireEffectPrefab, attackerPosition, Quaternion.identity);
                Destroy(fireEffect, 1f);
            }

            // Spawn impact effect at target position
            if (impactEffectPrefab != null)
            {
                GameObject impact = Instantiate(impactEffectPrefab, targetPosition, Quaternion.identity);
                Destroy(impact, 2f);
            }

          //  Debug.Log($"✨ Particles: Attack effects for {attacker.Type} -> {target.Type}");
        }

        private void HandleUnitMove(IUnit unit, Vector3 destination)
        {
            Vector3 unitPosition = (unit as MonoBehaviour)?.transform.position ?? Vector3.zero;

            // Spawn dust trail
            if (dustTrailPrefab != null)
            {
                GameObject dustTrail = Instantiate(dustTrailPrefab, unitPosition, Quaternion.identity);
                Destroy(dustTrail, 2f);
            }

            // Spawn footstep effect
            if (footstepEffectPrefab != null)
            {
                GameObject footstep = Instantiate(footstepEffectPrefab, unitPosition, Quaternion.identity);
                Destroy(footstep, 1f);
            }

//            Debug.Log($"✨ Particles: Movement effects for {unit.Type}");
        }

        private void HandleAnimationEvent(string eventName)
        {
            // Handle animation-specific particle effects
            switch (eventName.ToLower())
            {
                case "footstep":
                    // Could spawn footstep effects here
                    break;
                case "attack":
                    // Could spawn attack effects here
                    break;
                case "death":
                    // Could spawn death effects here
                    break;
            }
        }

        /// <summary>
        /// Spawn selection effect for a unit
        /// </summary>
        public void SpawnSelectionEffect(IUnit unit)
        {
            Vector3 unitPosition = (unit as MonoBehaviour)?.transform.position ?? Vector3.zero;

            // Spawn selection ring
            if (selectionRingPrefab != null)
            {
                GameObject selectionRing = Instantiate(selectionRingPrefab, unitPosition, Quaternion.identity);
                Destroy(selectionRing, 2f);
            }

            // Spawn highlight effect
            if (highlightEffectPrefab != null)
            {
                GameObject highlight = Instantiate(highlightEffectPrefab, unitPosition, Quaternion.identity);
                Destroy(highlight, 1.5f);
            }

//            Debug.Log($"✨ Particles: Selection effects for {unit.Type}");
        }

        /// <summary>
        /// Spawn footstep effect for a unit
        /// </summary>
        public void SpawnFootstepEffect(IUnit unit)
        {
            Vector3 unitPosition = (unit as MonoBehaviour)?.transform.position ?? Vector3.zero;

            if (footstepEffectPrefab != null)
            {
                GameObject footstep = Instantiate(footstepEffectPrefab, unitPosition, Quaternion.identity);
                Destroy(footstep, 1f);
            }
        }

        /// <summary>
        /// Handle explosion events from the explosion system
        /// </summary>
        private void HandleExplosion(Vector3 position, float radius, float damage, MyGame.Core.Interfaces.IDestructible source)
        {
            Debug.Log($"✨ UnitParticleSystem: HandleExplosion called at {position} (radius: {radius}, damage: {damage})");
            SpawnExplosionEffects(position, radius, damage);
        }

        /// <summary>
        /// Handle object explosion events
        /// </summary>
        private void HandleObjectExploded(MyGame.Core.Interfaces.IDestructible source, Vector3 position, float radius, float damage)
        {
            Debug.Log($"✨ UnitParticleSystem: HandleObjectExploded called for {source?.GetType().Name} at {position} (radius: {radius}, damage: {damage})");
            SpawnExplosionEffects(position, radius, damage);
        }

        /// <summary>
        /// Spawn explosion effects at the specified location
        /// </summary>
        public void SpawnExplosionEffects(Vector3 position, float radius, float damage)
        {
            // Scale effects based on explosion size
            float effectScale = Mathf.Clamp(radius / 5f, 0.5f, 3f);

            // Spawn main explosion effect
            if (largeExplosionPrefab != null)
            {
                GameObject explosion = Instantiate(largeExplosionPrefab, position, Quaternion.identity);
                explosion.transform.localScale = Vector3.one * effectScale;
                Destroy(explosion, 4f);
            }

            // Spawn shockwave effect
            if (shockwavePrefab != null)
            {
                GameObject shockwave = Instantiate(shockwavePrefab, position, Quaternion.identity);
                shockwave.transform.localScale = Vector3.one * effectScale;
                Destroy(shockwave, 2f);
            }

            // Spawn fireball for large explosions
            if (fireballPrefab != null && radius > 3f)
            {
                GameObject fireball = Instantiate(fireballPrefab, position, Quaternion.identity);
                fireball.transform.localScale = Vector3.one * effectScale;
                Destroy(fireball, 3f);
            }

            // Spawn debris effects
            if (debrisPrefab != null)
            {
                int debrisCount = Mathf.RoundToInt(radius * 2f);
                for (int i = 0; i < debrisCount; i++)
                {
                    Vector3 debrisPosition = position + Random.insideUnitSphere * radius * 0.5f;
                    GameObject debris = Instantiate(debrisPrefab, debrisPosition, Random.rotation);
                    Destroy(debris, Random.Range(3f, 6f));
                }
            }

            Debug.Log($"✨ Particles: Explosion effects at {position} (radius: {radius}, damage: {damage})");
        }
    }
} 