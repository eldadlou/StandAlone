using UnityEngine;
using System;
using MyGame.Core.Interfaces;
using MyGame.Core.Events;
using MyGame.Core;
using MyGame.Core.Services;

namespace MyGame.Core.Objects
{
    /// <summary>
    /// Base class for destructible objects like buildings, vehicles, environment objects
    /// </summary>
    public class DestructibleObject : MonoBehaviour, IDestructible
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        
        [Header("Explosion Settings")]
        [SerializeField] private bool canExplode = true;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float explosionDamage = 50f;
        [SerializeField] private bool hasExploded = false;
        
        [Header("Visual Settings")]
        [SerializeField] private GameObject destroyedPrefab; // Optional replacement object when destroyed
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float destroyDelay = 2f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip explosionSound;
        [SerializeField] private AudioClip damageSound;
        
        // Events
        public event Action<IDestructible> OnDestroyed;
        public event Action<IDestructible, float> OnDamaged;
        public event Action<IDestructible, Vector3, float, float> OnExploded;
        
        // Properties
        public float Health => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDestroyed => currentHealth <= 0f;
        public Vector3 Position => transform.position;
        public bool CanExplode => canExplode && !hasExploded;
        public float ExplosionRadius => explosionRadius;
        public float ExplosionDamage => explosionDamage;
        
        protected virtual void Awake()
        {
            currentHealth = maxHealth;
            
            // Register with explosion system
            var explosionService = DependencyContainer.Instance.TryResolve<IExplosionService>();
            explosionService?.SubscribeToDestructible(this);
        }
        
        protected virtual void Start()
        {
            // Notify systems about object creation
            GameEvents.TriggerDestructibleCreated(this);
        }
        
        public virtual void TakeDamage(float amount, IDestructible source = null)
        {
            if (IsDestroyed) return;
            
            float previousHealth = currentHealth;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            
            Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}/{maxHealth}");
            
            // Play damage sound
            if (damageSound != null && amount > 0)
            {
                AudioSource.PlayClipAtPoint(damageSound, Position);
            }
            
            // Trigger damage event
            OnDamaged?.Invoke(this, amount);
            GameEvents.TriggerObjectDamaged(this, amount, source);
            
            // Check if destroyed
            if (currentHealth <= 0f && previousHealth > 0f)
            {
                HandleDestruction();
            }
        }
        
        public virtual void Explode()
        {
            if (!CanExplode || hasExploded) return;
            
            hasExploded = true;
            
            Debug.Log($"{gameObject.name} exploding! Radius: {explosionRadius}, Damage: {explosionDamage}");
            
            // Play explosion sound
            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, Position);
            }
            
            // Trigger explosion event
            OnExploded?.Invoke(this, Position, explosionRadius, explosionDamage);
            GameEvents.TriggerObjectExploded(this, Position, explosionRadius, explosionDamage);
            
            // Destroy self (explosion damage is handled by ExplosionSystem)
            if (!IsDestroyed)
            {
                TakeDamage(currentHealth); // Kill self
            }
        }
        
        public virtual bool IsInExplosionRange(Vector3 explosionCenter, float radius)
        {
            // Simple distance check - can be overridden for complex shapes
            return Vector3.Distance(Position, explosionCenter) <= radius;
        }
        
        protected virtual void HandleDestruction()
        {
            Debug.Log($"{gameObject.name} destroyed!");
            
            // Trigger destroyed event
            OnDestroyed?.Invoke(this);
            GameEvents.TriggerDestructibleDestroyed(this);
            
            // Check if should explode on destruction
            if (CanExplode)
            {
                Explode();
            }
            
            // Spawn destroyed prefab if available
            if (destroyedPrefab != null)
            {
                GameObject destroyed = Instantiate(destroyedPrefab, Position, transform.rotation);
                
                // Copy any relevant properties to destroyed object
                var destroyedDestructible = destroyed.GetComponent<DestructibleObject>();
                if (destroyedDestructible != null)
                {
                    destroyedDestructible.canExplode = false; // Prevent destroyed objects from exploding again
                }
            }
            
            // Destroy or disable this object
            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
            else
            {
                // Just disable components instead of destroying
                GetComponent<Collider>().enabled = false;
                this.enabled = false;
            }
        }
        
        /// <summary>
        /// Heal the object (useful for repair mechanics)
        /// </summary>
        public virtual void Heal(float amount)
        {
            if (IsDestroyed) return;
            
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            Debug.Log($"{gameObject.name} healed for {amount}. Health: {currentHealth}/{maxHealth}");
        }
        
        /// <summary>
        /// Set health directly (useful for initial setup)
        /// </summary>
        public virtual void SetHealth(float health)
        {
            currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        }
        
        /// <summary>
        /// Check if object is critically damaged (below threshold)
        /// </summary>
        public virtual bool IsCriticallyDamaged(float threshold = 0.25f)
        {
            return (currentHealth / maxHealth) <= threshold;
        }
        
        // Gizmos for visualization in editor
        protected virtual void OnDrawGizmosSelected()
        {
            if (canExplode)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, explosionRadius);
            }
        }
    }
}