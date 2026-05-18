using UnityEngine;
using System;

namespace MyGame.Core.Interfaces
{
    /// <summary>
    /// Interface for objects that can be destroyed (units, buildings, environment objects)
    /// </summary>
    public interface IDestructible
    {
        // Health and destruction
        float Health { get; }
        float MaxHealth { get; }
        bool IsDestroyed { get; }
        
        // Position for targeting and effects
        Vector3 Position { get; }
        
        // Explosion properties
        bool CanExplode { get; }
        float ExplosionRadius { get; }
        float ExplosionDamage { get; }
        
        // Events
        event Action<IDestructible> OnDestroyed;
        event Action<IDestructible, float> OnDamaged;
        event Action<IDestructible, Vector3, float, float> OnExploded; // (source, position, radius, damage)
        
        // Methods
        void TakeDamage(float amount, IDestructible source = null);
        void Explode();
        bool IsInExplosionRange(Vector3 explosionCenter, float radius);
    }
}