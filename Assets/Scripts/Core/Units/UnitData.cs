using System;
using System.Collections.Generic;
using MyGame.Core.Skills;
using MyGame.Game;
using UnityEngine;

namespace MyGame.Core.Units
{
    /// <summary>
    /// Pure logic class containing unit data and behavior without Unity dependencies
    /// Now serializable for inspector visibility
    /// </summary>
    [System.Serializable]
    public class UnitData
    {
        // Core properties
        [SerializeField] public float Health { get; set; }
        [SerializeField] public float Speed { get; set; }
        [SerializeField] public Player Owner { get; set; }
        [SerializeField] public UnitType Type { get; set; }
        [SerializeField] public List<Skill> Skills { get; set; } = new List<Skill>();
        
        // Combat properties
        [SerializeField] public float AttackDamage { get; set; }
        [SerializeField] public float AttackRange { get; set; }
        [SerializeField] public float AttackCooldown { get; set; }
        [SerializeField] public float LastAttackTime { get; set; }
        
        // Movement state
        [SerializeField] public bool IsMoving { get; set; }
        [SerializeField] public Vector3 Destination { get; set; }
        
        // Events (pure logic)
        public event Action<UnitData> OnDeath;
        public event Action<UnitData, UnitData> OnAttack; // Attacker, Target
        public event Action<UnitData, Vector3> OnMove; // Unit, Destination
        public event Action<string> OnAnimationEvent;

        public UnitData(UnitType type, float health, float speed, Player owner)
        {
            Type = type;
            Health = health;
            Speed = speed;
            Owner = owner;
            
            // Initialize combat properties based on unit type
            InitializeCombatProperties();
        }

        private void InitializeCombatProperties()
        {
            var config = MyGame.Core.Configuration.GameConfig.Instance;
            AttackDamage = config.GetAttackDamage(Type);
            AttackRange = config.GetAttackRange(Type);
            AttackCooldown = config.GetAttackCooldown(Type);
        }

        // Pure logic methods
        public virtual void TakeDamage(float amount)
        {
            float oldHealth = Health;
            Health -= amount;
            
            // Debug.Log($"💥 UnitData {Type}: Took {amount} damage. Health: {oldHealth} → {Health}");
            
            if (Health <= 0)
            {
                // Debug.Log($"💀 UnitData {Type}: Health reached 0! Triggering OnDeath event");
                OnDeath?.Invoke(this);
                // Debug.Log($"💀 UnitData {Type}: OnDeath event triggered with {OnDeath?.GetInvocationList().Length ?? 0} subscribers");
            }
        }

        public virtual bool CanAttack(UnitData target)
        {
            if (target == null) 
            {
                // Debug.LogWarning($"UnitData CanAttack failed - target is null");
                return false;
            }
            
            // Check if target is on different team (no friendly fire)
            if (Owner != null && target.Owner != null && Owner.Team == target.Owner.Team)
            {
                // Debug.LogWarning($"UnitData CanAttack failed - friendly fire prevented (both on {Owner.Team} team)");
                return false;
            }
            
            // Check cooldown
            if (Time.time - LastAttackTime < AttackCooldown)
            {
                // Debug.LogWarning($"UnitData CanAttack failed - cooldown not ready ({(Time.time - LastAttackTime):F1}s < {AttackCooldown:F1}s)");
                return false;
            }
            
            // Debug.Log($"UnitData CanAttack passed all checks for target {target.Type}");
            return true;
        }

        public virtual bool Attack(UnitData target)
        {
            // Debug.Log($"UnitData Attack called - Attacker: {Type}, Target: {target?.Type}");
            
            if (!CanAttack(target))
            {
                // Debug.LogWarning($"UnitData CanAttack returned false for target {target?.Type}");
                return false;
            }
            
            // Apply damage to target
            target.TakeDamage(AttackDamage);
            
            // Update last attack time
            LastAttackTime = Time.time;
            
            // Trigger attack event
            OnAttack?.Invoke(this, target);
            
            // Debug.Log($"UnitData Attack successful - Applied {AttackDamage} damage to {target.Type}");
            
            return true;
        }

        public virtual void UseSkill(int skillIndex) 
        { 
            /* Skill logic - pure logic */ 
        }

        public virtual void Upgrade() 
        { 
            /* Upgrade logic - pure logic */ 
        }

        public virtual void PlayAnimation(string animationName)
        {
            OnAnimationEvent?.Invoke(animationName);
        }

        public virtual void SetMoving(Vector3 destination)
        {
            Destination = destination;
            IsMoving = true;
            OnMove?.Invoke(this, destination);
        }

        public virtual void UpdatePosition(Vector3 newPosition)
        {
            // Pure logic - no Unity dependencies
            if (Vector3.Distance(newPosition, Destination) < 0.1f)
                IsMoving = false;
        }
    }
} 