using UnityEngine;
using System;
using System.Collections.Generic;
using MyGame.Core.Interfaces;
using MyGame.Core.Skills;
using MyGame.Game;
using MyGame.RuntimeSystems.Movement;
using MyGame.Presentation;
using MyGame.Core;
using MyGame.Core.Events;

namespace MyGame.Core.Units
{
    /// <summary>
    /// Pure MonoBehaviour wrapper that bridges pure logic (UnitData) with Unity-specific behavior
    /// No presentation concerns - all visuals handled by UnitVisualCoordinator
    /// </summary>
    public abstract class Unit : MonoBehaviour, IUnit
    {
        // Static registry to map UnitData to Unit components for efficient lookup
        private static readonly Dictionary<UnitData, Unit> unitDataRegistry = new Dictionary<UnitData, Unit>();
        
        // Pure logic data
        [Header("Unit Data (Read Only)")]
        [SerializeField] protected UnitData unitData;
        
        // Visual coordinator (optional - for presentation concerns)
        private UnitVisualCoordinator visualCoordinator;
        public UnitVisualCoordinator GetVisualCoordinator() => visualCoordinator;

        // IUnit interface implementation - delegates to UnitData
        public event Action<IUnit> OnDeath;
        public event Action<IUnit, IUnit> OnAttack;
        public event Action<IUnit, Vector3> OnMove;
        public event Action<string> OnAnimationEvent;

        // Properties delegate to UnitData
        public string Name { get; }
        public float Health => unitData?.Health ?? 0f;
        public float Speed => unitData?.Speed ?? 0f;
        public Player Owner => unitData?.Owner;
        public abstract UnitType Type { get; }
        public List<Skill> Skills => unitData?.Skills ?? new List<Skill>();
        public bool IsMoving => unitData?.IsMoving ?? false;
        public Vector3 Destination => unitData?.Destination ?? Vector3.zero;
        public Vector3 Position => transform.position;
        
        // Combat properties
        public float AttackDamage => unitData?.AttackDamage ?? 0f;
        public float AttackRange => unitData?.AttackRange ?? 0f;
        public float AttackCooldown => unitData?.AttackCooldown ?? 0f;
        public float LastAttackTime => unitData?.LastAttackTime ?? 0f;

        protected virtual void Awake()
        {
            // Debug.Log($"🚀🚀🚀 Unit.Awake() called for {gameObject.name} ({Type}) 🚀🚀🚀");
            
            // Initialize pure logic data with null owner (will be set later)
            unitData = new UnitData(Type, GetInitialHealth(), GetInitialSpeed(), null);
            
            // Register this unit in the static registry
            if (unitData != null)
            {
                unitDataRegistry[unitData] = this;
            }
            
            // Get visual coordinator (optional)
            visualCoordinator = GetComponent<UnitVisualCoordinator>();

            // Wire up events to bridge UnitData events to IUnit interface
            if (unitData != null)
            {
                unitData.OnDeath += (data) => {
                    OnDeath?.Invoke(this);
                    // Notify visual coordinator to hide destination marker on death
                    visualCoordinator?.OnUnitDeath();
                };
                unitData.OnAttack += (attacker, target) => {
                    // Find the Unit component that owns the target UnitData using the registry
                    Unit targetUnit = FindUnitByUnitData(target);
                    if (targetUnit != null)
                    {
                        OnAttack?.Invoke(this, targetUnit);
                    }
                    else
                    {
                        // Debug.LogWarning($"Unit {name}: Could not find Unit component for target UnitData");
                    }
                };
                unitData.OnMove += (unit, dest) => OnMove?.Invoke(this, dest);
                unitData.OnAnimationEvent += (eventName) => OnAnimationEvent?.Invoke(eventName);
            }
            
            // Notify systems about unit creation via events
            GameEvents.TriggerUnitCreated(this);
            
            // Debug log for unit registration
//            Debug.Log($"Unit {name} ({Type}) created and registered with GameManager");
        }
        
        private void OnEnable()
        {
            // Debug logging to verify event subscribers
            // Debug.Log($"Unit {name}: OnDeath event has {OnDeath?.GetInvocationList().Length ?? 0} subscribers");
            // Debug.Log($"Unit {name}: OnAttack event has {OnAttack?.GetInvocationList().Length ?? 0} subscribers");
            // Debug.Log($"Unit {name}: OnMove event has {OnMove?.GetInvocationList().Length ?? 0} subscribers");
        }
        
        /// <summary>
        /// Assign this unit to a team (should be called after creation)
        /// </summary>
        public virtual void AssignToTeam(Team team)
        {
            // Debug.Log($"🔧🔧🔧 AssignToTeam called for {name} with team: {team} 🔧🔧🔧");
            
            if (unitData != null)
            {
                Player teamPlayer = GetTeamPlayer(team);
                
                if (teamPlayer != null)
                {
                    unitData.Owner = teamPlayer;
                    // Debug.Log($"✅✅✅ Unit {name} assigned to {team} team (Owner: {teamPlayer.Name}, Team: {teamPlayer.Team}) ✅✅✅");
                }
                else
                {
                    // Debug.LogError($"❌ Unit {name}: Failed to assign to {team} team - GetTeamPlayer returned null");
                }
                
                // Notify visual coordinator to try initializing now that unit is properly set up
                visualCoordinator?.TryInitialize();
            }
            else
            {
                // Debug.LogError($"❌ Unit {name}: Cannot assign team - unitData is null");
            }
        }

        /// <summary>
        /// Manually set the team for debugging/testing purposes
        /// </summary>
        public virtual void SetTeam(Team team)
        {
            if (unitData != null)
            {
                Player teamPlayer = GetTeamPlayer(team);
                
                if (teamPlayer != null)
                {
                    unitData.Owner = teamPlayer;
                    
                    // Debug.Log($"Unit {name} manually set to {team} team (Owner: {teamPlayer.Name})");
                }
                else
                {
                    // Debug.LogError($"Unit {name}: Failed to set to {team} team - GameManager not initialized or Player not found");
                }
            }
            else
            {
                // Debug.LogError($"Unit {name}: Cannot set team - unitData is null");
            }
        }

        /// <summary>
        /// Get the current team assignment
        /// </summary>
        public virtual Team GetTeam()
        {
            return unitData?.Owner?.Team ?? Team.None;
        }

        // Abstract methods for unit-specific initialization
        protected abstract float GetInitialHealth();
        protected abstract float GetInitialSpeed();

        // Movement - pure logic, delegates to UnitData
        public virtual void MoveTo(Vector3 destination)
        {
            if (unitData != null)
            {
                unitData.SetMoving(destination);
                // Use dependency injection to get MovementSystem
                var movementSystem = MyGame.Core.SystemInitializer.GetSystem<MyGame.RuntimeSystems.Movement.MovementSystem>();
                movementSystem?.RegisterUnit(this);
            }
            
            // Notify visual coordinator (if present)
            visualCoordinator?.OnUnitMove(destination);
        }

        public virtual void UpdatePosition(Vector3 newPosition)
        {
            // Notify visual coordinator (if present)
            visualCoordinator?.OnPositionUpdate(newPosition);
        }

        // Combat - pure logic, delegates to UnitData
        public virtual void TakeDamage(float amount)
        {
            unitData?.TakeDamage(amount);
            
            // Notify visual coordinator (if present)
            visualCoordinator?.OnUnitDamaged(amount);
        }

        public virtual bool CanAttack(IUnit target)
        {
            if (target == null || unitData == null) 
            {
                // Debug.LogWarning($"Unit {name}: CanAttack failed - target or unitData is null");
                return false;
            }
            
            // Check if target is on different team (no friendly fire)
            if (Owner != null && target.Owner != null && Owner.Team == target.Owner.Team)
            {
                // Debug.LogWarning($"Unit {name}: CanAttack failed - friendly fire prevented (both on {Owner.Team} team)");
                return false;
            }
            
            // Note: Range check is handled by derived classes (e.g., VehicleCombatUnit)
            // to support weapon-specific ranges (main gun vs machine gun)
            
            // Check cooldown
            if (Time.time - unitData.LastAttackTime < unitData.AttackCooldown)
            {
                // Debug.LogWarning($"Unit {name}: CanAttack failed - cooldown not ready ({(Time.time - unitData.LastAttackTime):F1}s < {unitData.AttackCooldown:F1}s)");
                return false;
            }
            
            // Debug.Log($"Unit {name}: CanAttack passed all checks for {target.Name}");
            return true;
        }

        public virtual bool Attack(IUnit target)
        {
            // Debug.Log($"Unit {name}: Attack called for target {target?.Name}");
            
            if (!CanAttack(target))
            {
                // Debug.LogWarning($"Unit {name}: CanAttack returned false for {target?.Name}");
                return false;
            }
            
            if (unitData != null && target is Unit targetUnit)
            {
                // Debug.Log($"Unit {name}: Calling unitData.Attack for {targetUnit.name}");
                bool attackSuccess = unitData.Attack(targetUnit.unitData);
                
                if (attackSuccess)
                {
                    // Notify visual coordinator (if present)
                    visualCoordinator?.OnUnitAttack(target);
                    
                    // Debug.Log($"Unit {name} attacked {targetUnit.name} for {unitData.AttackDamage} damage");
                }
                else
                {
                    // Debug.LogWarning($"Unit {name}: unitData.Attack returned false for {targetUnit.name}");
                }
                
                return attackSuccess;
            }
            else
            {
                // Debug.LogError($"Unit {name}: unitData is null or target is not a Unit component");
            }
            
            return false;
        }

        // Skills - pure logic, delegates to UnitData
        public virtual void UseSkill(int skillIndex) 
        { 
            unitData?.UseSkill(skillIndex);
            
            // Notify visual coordinator (if present)
            visualCoordinator?.OnSkillUsed(skillIndex);
        }

        public virtual void Upgrade() 
        { 
            unitData?.Upgrade();
            
            // Notify visual coordinator (if present)
            visualCoordinator?.OnUnitUpgrade();
        }

        // Animation - pure logic, delegates to UnitData
        public virtual void PlayAnimation(string animationName)
        {
            unitData?.PlayAnimation(animationName);
        }

        // Selection - pure logic, notifies visual coordinator
        public virtual void SetSelected(bool selected)
        {
            // Notify visual coordinator (if present)
            visualCoordinator?.OnSelectionChanged(selected);
        }

        // Helper method for other units to access UnitData
        public UnitData GetUnitData() => unitData;
        
        /// <summary>
        /// Reset unit data when returning to pool
        /// </summary>
        public void ResetUnitData()
        {
            if (unitData != null)
            {
                unitData.Health = GetInitialHealth();
                unitData.IsMoving = false;
                unitData.Destination = Vector3.zero;
                unitData.LastAttackTime = 0f;
            }
        }
        
        // Helper method to find a Unit component by its UnitData
        private Unit FindUnitByUnitData(UnitData targetUnitData)
        {
            if (targetUnitData == null) return null;
            
            // Use the static registry for efficient lookup
            unitDataRegistry.TryGetValue(targetUnitData, out Unit targetUnit);
            return targetUnit;
        }
        
        // Cleanup when unit is destroyed
        protected virtual void OnDestroy()
        {
            // Remove from registry to prevent memory leaks
            if (unitData != null)
            {
                unitDataRegistry.Remove(unitData);
            }
        }
        
        // Static method to clear registry (useful for testing)
        public static void ClearRegistry()
        {
            unitDataRegistry.Clear();
        }

        /// <summary>
        /// Get the Player instance for a specific team
        /// </summary>
        private Player GetTeamPlayer(Team team)
        {
            // Debug.Log($"🔍🔍🔍 GetTeamPlayer called for team: {team} 🔍🔍🔍");
            
            // First try to get from GameManager
            if (GameManager.Instance != null)
            {
                // Debug.Log($"🔍 GameManager.Instance found: {GameManager.Instance != null}");
                Player teamPlayer = team == Team.Player ? 
                    GameManager.Instance.Player1 : 
                    GameManager.Instance.AI;
                
                // Debug.Log($"🔍 Team player from GameManager: {teamPlayer?.Name ?? "NULL"} (Team: {teamPlayer?.Team ?? Team.None})");
                
                if (teamPlayer != null)
                {
                    // Debug.Log($"✅ Returning team player: {teamPlayer.Name} (Team: {teamPlayer.Team})");
                    return teamPlayer;
                }
            }
            else
            {
                // Debug.LogWarning("GameManager.Instance is null");
            }
            
            // If GameManager is not available, create a temporary Player instance
            // Debug.LogWarning($"GameManager not available for team assignment. Creating temporary Player for {team} team.");
            Player tempPlayer = new Player(team == Team.Player ? "Player" : "AI", team, team == Team.AI);
            // Debug.Log($"Created temporary player: {tempPlayer.Name} (Team: {tempPlayer.Team})");
            return tempPlayer;
        }
    }
}
