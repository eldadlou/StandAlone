using UnityEngine;
using System;
using System.Collections.Generic;
using MyGame.Core.Units;
using MyGame.Game;
using MyGame.Core.Skills;
using MyGame.Presentation;

namespace MyGame.Core.Units.Combat
{
    /// <summary>
    /// Generic vehicle combat unit that can handle any number of weapon mounts.
    /// Use this for any vehicle: tanks, jeeps, APCs, trucks with mounted guns, etc.
    /// Configure weapons in the Inspector by adding WeaponMount entries.
    /// 
    /// SETUP:
    /// 1. Add this component to your vehicle
    /// 2. Add WeaponMount entries to the weapons array
    /// 3. Assign turret transforms and projectile prefabs
    /// 4. Configure weapon stats (damage, range, cooldown, etc.)
    /// </summary>
    public class VehicleCombatUnit : CombatUnit, IAccurateUnit
    {
        [Header("Vehicle Weapons")]
        [SerializeField] private List<WeaponMount> weapons = new List<WeaponMount>();
        
        [Header("Weapon Selection")]
        [SerializeField] private WeaponSelectionMode selectionMode = WeaponSelectionMode.BestForRange;
        [SerializeField] private bool allowMultipleWeaponsFiring = false; // Fire all weapons in range simultaneously
        
        // Cached components
        private Unit unitComponent;
        
        // Current selected weapon
        private int currentWeaponIndex = 0;
        private WeaponMount CurrentWeapon => weapons.Count > 0 ? weapons[currentWeaponIndex] : null;
        
        // Properties
        public override float AttackDamage => CurrentWeapon?.Damage ?? 0f;
        public override float AttackRange => GetMaxWeaponRange();
        public override float AttackCooldown => CurrentWeapon?.Cooldown ?? 1f;
        public override float Health => unitComponent?.Health ?? 100f;
        public override Vector3 Position => transform.position;
        
        // Vehicle identification
        public override string Name => gameObject.name;
        
        // IUnit interface properties
        public override UnitType Type => unitComponent?.Type ?? UnitType.Tank;
        public override Player Owner => unitComponent?.Owner;
        public override float LastAttackTime => lastAttackTime;
        
        // IMovable interface
        public override float Speed => unitComponent?.Speed ?? 5f;
        public override bool IsMoving => unitComponent?.IsMoving ?? false;
        public override Vector3 Destination => unitComponent?.Destination ?? transform.position;
        
        // ISkillUser interface - cached empty list to avoid GC allocation
        private static readonly List<Skill> EmptySkillList = new List<Skill>();
        public override List<Skill> Skills => unitComponent?.Skills ?? EmptySkillList;
        
        // Events
        public override event Action<IUnit> OnDeath;
        public override event Action<IUnit, IUnit> OnAttack;
        public override event Action<IUnit, Vector3> OnMove;
        public override event Action<string> OnAnimationEvent;
        
        // Public access to weapons for UI/debugging
        public IReadOnlyList<WeaponMount> Weapons => weapons;
        public int WeaponCount => weapons.Count;
        
        #region Tank Compatibility Properties
        // These properties provide backward compatibility with Tank.cs and TankInspectorEditor.cs
        // They assume weapon index 0 = Main Gun, index 1 = Machine Gun (for tanks)
        
        /// <summary>Get weapon by index (0 = primary/main gun, 1 = secondary/machine gun, etc.)</summary>
        public WeaponMount GetWeapon(int index) => index >= 0 && index < weapons.Count ? weapons[index] : null;
        
        /// <summary>Primary weapon (index 0) - typically Main Gun for tanks</summary>
        public WeaponMount PrimaryWeapon => GetWeapon(0);
        
        /// <summary>Secondary weapon (index 1) - typically Machine Gun for tanks</summary>
        public WeaponMount SecondaryWeapon => GetWeapon(1);
        
        // Main Gun stats (weapon index 0)
        public float MainGunDamage => PrimaryWeapon?.Damage ?? 0f;
        public float MainGunRange => PrimaryWeapon?.Range ?? 0f;
        public float MainGunCooldown => PrimaryWeapon?.Cooldown ?? 0f;
        public bool HasMainGun => PrimaryWeapon?.IsAvailable ?? false;
        
        // Machine Gun stats (weapon index 1)
        public float MachineGunDamage => SecondaryWeapon?.Damage ?? 0f;
        public float MachineGunRange => SecondaryWeapon?.Range ?? 0f;
        public float MachineGunCooldown => SecondaryWeapon?.Cooldown ?? 0f;
        public bool HasMachineGun => SecondaryWeapon?.IsAvailable ?? false;
        
        /// <summary>Get a formatted string describing all weapons</summary>
        public string GetWeaponsDescription()
        {
            if (weapons.Count == 0) return "No weapons configured";
            
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < weapons.Count; i++)
            {
                var w = weapons[i];
                if (i > 0) sb.AppendLine();
                sb.Append($"{w.WeaponName}: {w.Damage} dmg, {w.Range}m range, {w.Cooldown}s cooldown");
                if (!w.IsAvailable) sb.Append(" [NO TURRET]");
                if (!w.HasProjectile) sb.Append(" [NO PROJECTILE]");
            }
            return sb.ToString();
        }
        
        #endregion
        
        /// <summary>
        /// Get the maximum range across all available weapons
        /// </summary>
        private float GetMaxWeaponRange()
        {
            float maxRange = 0f;
            foreach (var weapon in weapons)
            {
                if (weapon.IsAvailable && weapon.Range > maxRange)
                {
                    maxRange = weapon.Range;
                }
            }
            return maxRange;
        }
        
        /// <summary>
        /// Get the best weapon for a given distance
        /// </summary>
        private WeaponMount GetBestWeaponForDistance(float distance)
        {
            WeaponMount bestWeapon = null;
            float bestScore = float.MinValue;
            
            foreach (var weapon in weapons)
            {
                if (!weapon.IsAvailable || !weapon.CanEngageAtDistance(distance))
                    continue;
                
                // Score based on selection mode
                float score = 0f;
                switch (selectionMode)
                {
                    case WeaponSelectionMode.BestForRange:
                        // Prefer weapons whose range closely matches the distance
                        score = -Mathf.Abs(weapon.Range - distance);
                        break;
                        
                    case WeaponSelectionMode.HighestDamage:
                        score = weapon.Damage;
                        break;
                        
                    case WeaponSelectionMode.FastestFire:
                        score = -weapon.Cooldown; // Lower cooldown = better
                        break;
                        
                    case WeaponSelectionMode.MostAccurate:
                        score = weapon.Accuracy;
                        break;
                        
                    case WeaponSelectionMode.ReadyFirst:
                        score = weapon.IsOnCooldown ? -weapon.CooldownRemaining : 1000f;
                        break;
                }
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestWeapon = weapon;
                }
            }
            
            return bestWeapon;
        }
        
        // Cached list for weapons in range to avoid GC allocation
        private List<WeaponMount> cachedWeaponsInRange = new List<WeaponMount>(4);
        
        /// <summary>
        /// Get all weapons that can engage at a given distance (fills cached list to avoid GC)
        /// </summary>
        private List<WeaponMount> GetWeaponsForDistance(float distance)
        {
            cachedWeaponsInRange.Clear();
            for (int i = 0; i < weapons.Count; i++)
            {
                var w = weapons[i];
                if (w.IsAvailable && w.CanEngageAtDistance(distance))
                {
                    cachedWeaponsInRange.Add(w);
                }
            }
            return cachedWeaponsInRange;
        }
        
        protected override void Awake()
        {
            Debug.Log($"{Name}: VehicleCombatUnit Awake called");
            
            base.Awake();
            
            // Get the Unit component
            unitComponent = GetComponent<Unit>();
            
            if (unitComponent == null)
            {
                Debug.LogError($"VehicleCombatUnit {gameObject.name} requires a Unit component!");
            }
            
            // Initialize all weapons
            foreach (var weapon in weapons)
            {
                weapon.Initialize();
            }
            
            Debug.Log($"{Name}: Initialized with {weapons.Count} weapon(s)");
        }
        
        protected override void HandleCombat()
        {
            if (currentTarget == null) return;
            
            base.HandleCombat();
            
            if (!isTargetInRange) return;
            
            float distance = Vector3.Distance(transform.position, currentTarget.Position);
            
            // Select best weapon for current distance
            WeaponMount bestWeapon = GetBestWeaponForDistance(distance);
            if (bestWeapon != null)
            {
                // Update current weapon index
                currentWeaponIndex = weapons.IndexOf(bestWeapon);
                
                // Rotate weapon towards target
                bestWeapon.RotateTowards(currentTarget.Position);
            }
        }
        
        public override void RotateGunTowardsTarget()
        {
            if (currentTarget == null || CurrentWeapon == null) return;
            CurrentWeapon.RotateTowards(currentTarget.Position);
        }
        
        protected override bool IsGunFacingCurrentTarget()
        {
            if (currentTarget == null || CurrentWeapon == null) return false;
            return CurrentWeapon.IsFacingTarget(currentTarget.Position);
        }
        
        public override bool TryAttack()
        {
            if (currentTarget == null) return false;
            
            float distance = Vector3.Distance(transform.position, currentTarget.Position);
            bool anyFired = false;
            
            if (allowMultipleWeaponsFiring)
            {
                // Fire all available weapons
                var weaponsInRange = GetWeaponsForDistance(distance);
                foreach (var weapon in weaponsInRange)
                {
                    if (weapon.CanFire() && weapon.IsFacingTarget(currentTarget.Position))
                    {
                        FireWeapon(weapon);
                        anyFired = true;
                    }
                }
            }
            else
            {
                // Fire only the best weapon
                var weapon = GetBestWeaponForDistance(distance);
                if (weapon != null && weapon.CanFire() && weapon.IsFacingTarget(currentTarget.Position))
                {
                    FireWeapon(weapon);
                    anyFired = true;
                }
            }
            
            if (anyFired)
            {
                lastAttackTime = Time.time;
            }
            
            return anyFired;
        }
        
        private void FireWeapon(WeaponMount weapon)
        {
            // Perform accuracy check
            HitResult hitResult = PerformAccuracyCheckForWeapon(weapon);
            float actualDamage = CalculateDamageForWeapon(weapon, hitResult);
            
            Debug.Log($"{Name}: Firing {weapon.WeaponName} - Accuracy: {weapon.Accuracy}%, HitResult: {hitResult}, Damage: {actualDamage:F1}");
            
            // WeaponMount.Fire() creates the projectile with proper speed from the weapon preset
            weapon.Fire(this, currentTarget, hitResult, actualDamage);
            
            // NOTE: We intentionally do NOT call NotifyFireSystemOfAttack() here because:
            // 1. WeaponMount.Fire() already creates the projectile with the correct speed
            // 2. LightweightFireSystem.ProcessAttack() would create a SECOND projectile with a different speed
            // This was causing the "two bullets fired at once" bug where one was fast and one was slower
        }
        
        public override bool CanAttack(IUnit target)
        {
            if (target == null || !IsValidTarget(target))
                return false;
            
            float distance = Vector3.Distance(transform.position, target.Position);
            
            // Check if any weapon can engage at this distance
            foreach (var weapon in weapons)
            {
                if (weapon.IsAvailable && weapon.CanEngageAtDistance(distance))
                    return true;
            }
            
            return false;
        }
        
        public override bool Attack(IUnit target)
        {
            if (!CanAttack(target)) return false;
            
            currentTarget = target;
            return TryAttack();
        }
        
        protected override void CreateProjectile(IUnit target)
        {
            // This is handled by individual WeaponMount.Fire() calls in TryAttack()
            // This method is required by CombatUnit but we use the WeaponMount system instead
        }
        
        #region Accuracy System
        
        private HitResult PerformAccuracyCheckForWeapon(WeaponMount weapon)
        {
            float accuracy = weapon.Accuracy;
            
            // Apply movement penalty
            if (IsMoving)
            {
                accuracy -= baseMovingAccuracyPenalty;
            }
            
            accuracy = Mathf.Clamp(accuracy, 0f, 100f);
            
            float roll = UnityEngine.Random.Range(0f, 100f);
            
            if (roll <= accuracy)
                return HitResult.FullHit;
            
            float partialRoll = UnityEngine.Random.Range(0f, 100f);
            if (partialRoll <= basePartialHitChance)
                return HitResult.PartialHit;
            
            return HitResult.Miss;
        }
        
        private float CalculateDamageForWeapon(WeaponMount weapon, HitResult hitResult)
        {
            float baseDamage = weapon.Damage;
            
            switch (hitResult)
            {
                case HitResult.FullHit:
                    return baseDamage;
                case HitResult.PartialHit:
                    return baseDamage * basePartialHitDamageMultiplier;
                case HitResult.Miss:
                    return 0f;
                default:
                    return baseDamage;
            }
        }
        
        public override float CalculateCurrentAccuracy()
        {
            if (CurrentWeapon == null) return 0f;
            
            float accuracy = CurrentWeapon.Accuracy;
            if (IsMoving)
            {
                accuracy -= baseMovingAccuracyPenalty;
            }
            return Mathf.Clamp(accuracy, 0f, 100f);
        }
        
        public override AccuracyInfo GetAccuracyInfo()
        {
            return new AccuracyInfo
            {
                BaseAccuracy = CurrentWeapon?.Accuracy ?? 0f,
                CurrentAccuracy = CalculateCurrentAccuracy(),
                IsMoving = IsMoving,
                MovementPenalty = IsMoving ? baseMovingAccuracyPenalty : 0f,
                WeaponType = CurrentWeapon?.WeaponName ?? "None"
            };
        }
        
        #endregion
        
        #region IUnit Interface Methods
        
        public override void Upgrade()
        {
            unitComponent?.Upgrade();
        }
        
        public override void AssignToTeam(Team team)
        {
            unitComponent?.AssignToTeam(team);
        }
        
        public override void TakeDamage(float amount)
        {
            unitComponent?.TakeDamage(amount);
        }
        
        public override void MoveTo(Vector3 destination)
        {
            unitComponent?.MoveTo(destination);
        }
        
        public override void UpdatePosition(Vector3 newPosition)
        {
            unitComponent?.UpdatePosition(newPosition);
        }
        
        public override void UseSkill(int skillIndex)
        {
            unitComponent?.UseSkill(skillIndex);
        }
        
        public override void PlayAnimation(string animationName)
        {
            unitComponent?.PlayAnimation(animationName);
        }
        
        #endregion
        
        #region Debug Visualization
        
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // Draw weapon ranges
            Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan };
            for (int i = 0; i < weapons.Count; i++)
            {
                var weapon = weapons[i];
                weapon.DrawGizmos(transform.position, colors[i % colors.Length]);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// How the vehicle selects which weapon to use
    /// </summary>
    public enum WeaponSelectionMode
    {
        BestForRange,   // Select weapon whose range best matches target distance
        HighestDamage,  // Always use highest damage weapon that can reach
        FastestFire,    // Use weapon with lowest cooldown
        MostAccurate,   // Use most accurate weapon
        ReadyFirst      // Use whichever weapon is off cooldown first
    }
}
