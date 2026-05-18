using UnityEngine;
using System;
using System.Collections.Generic;
using MyGame.Core.Units.Combat;
using UnityEngine.Serialization;

namespace MyGame.Core.Units
{
    /// <summary>
    /// Concrete Tank implementation that extends the abstract Unit class
    /// Provides tank-specific stats and behavior
    /// Now organized to avoid duplicate properties with UnitData
    /// </summary>
    public class Tank : Unit
    {
        [FormerlySerializedAs("tankHealth")]
        [Header("Tank Base Stats")]
        [SerializeField] private float tankInitialHealth = 150f;
        [SerializeField] private float tankSpeed = 5f;
        [SerializeField] private string ownerName;
        
        [Header("Tank Combat (Weapon-specific stats handled by VehicleCombatUnit)")]
        [SerializeField] private bool showCombatInfo = true;
        
        [Header("Tank Armor")]
        [SerializeField] private float armorDamageReduction = 0.2f; // 20% damage reduction
        
        public override UnitType Type => UnitType.Tank;

        protected override float GetInitialHealth() => tankInitialHealth;
        protected override float GetInitialSpeed() => tankSpeed;

        protected override void Awake()
        {
            base.Awake();
            
            // Tank-specific initialization
            Debug.Log($"Tank {name} initialized with {tankInitialHealth} health and {tankSpeed} speed");
        }

        // Override combat methods if Tank has special behavior
        public override bool CanAttack(IUnit target)
        {
            // Tank-specific attack validation
            if (!base.CanAttack(target)) return false;
            
            // Add any tank-specific attack conditions here
            // For example, tanks might not be able to attack while moving
            // if (IsMoving) return false; // REMOVED: This was preventing attacks while moving
            
            return true;
        }

        public override bool Attack(IUnit target)
        {
            // Tank-specific attack behavior
            bool attackSuccess = base.Attack(target);
            
            if (attackSuccess)
            {
                // Add tank-specific attack effects here
                // For example, recoil, sound effects, etc.
                Debug.Log($"Tank {name} fired at {target.Name}!");
            }
            
            return attackSuccess;
        }

        // Override movement methods if Tank has special behavior
        public override void MoveTo(Vector3 destination)
        {
            // Tank-specific movement behavior
            // For example, tanks might be slower on certain terrain
            base.MoveTo(destination);
            
//            Debug.Log($"Tank {name} moving to {destination}");
        }

        // Override damage methods if Tank has special behavior
        public override void TakeDamage(float amount)
        {
            // Tank-specific damage behavior
            // For example, tanks might have armor that reduces damage
            float reducedDamage = amount * (1f - armorDamageReduction);
            
            base.TakeDamage(reducedDamage);
            Debug.Log($"Tank {name} took {reducedDamage} damage (reduced from {amount} due to {armorDamageReduction * 100}% armor)");
        }
        
        // Helper method to get combat info for display
        public string GetCombatInfo()
        {
            if (!showCombatInfo) return "Combat info hidden";
            
            var combatUnit = GetComponent<VehicleCombatUnit>();
            if (combatUnit != null)
            {
                return combatUnit.GetWeaponsDescription();
            }
            
            return "No VehicleCombatUnit component found";
        }
    }
}
