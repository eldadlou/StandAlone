using UnityEngine;
using System.Collections.Generic;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Game;
using MyGame.Core.Events;
using MyGame.RuntimeSystems.Movement;
using MyGame.Core.Units.Combat;

namespace MyGame.Core
{
    /// <summary>
    /// Manages unit creation and destruction using object pooling for better performance
    /// </summary>
    public class UnitPoolManager : MonoBehaviour
    {
        [Header("Unit Prefabs")]
        [SerializeField] private GameObject tankPrefab;
        [SerializeField] private GameObject soldierPrefab;
        [SerializeField] private GameObject aircraftPrefab;
        [SerializeField] private GameObject helicopterPrefab;
        [SerializeField] private GameObject truckPrefab;
        
        [Header("Pool Settings")]
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private float unitDestructionDelay = 2f; // Delay before returning to pool
        
        [Header("Death Settings")]
        [SerializeField] private bool useObjectPooling = true;
        [SerializeField] private bool destroyImmediately = false; // For testing
        
        // Object pools for each unit type
        private Dictionary<UnitType, ObjectPool<Unit>> unitPools = new Dictionary<UnitType, ObjectPool<Unit>>();
        private Dictionary<Unit, float> pendingDestruction = new Dictionary<Unit, float>();
        
        // Transform to organize pooled units
        private Transform poolParent;
        
        private void Awake()
        {
            // Create pool parent
            GameObject poolParentGO = new GameObject("UnitPools");
            poolParent = poolParentGO.transform;
            
            // Initialize pools
            InitializePools();
            
            // Register with dependency container
            DependencyContainer.Instance.Register(this);
            
            // Debug.Log("🏗️ UnitPoolManager initialized with object pooling");
        }
        
        private void InitializePools()
        {
            TryCreatePool(UnitType.Tank, tankPrefab);
            TryCreatePool(UnitType.Soldier, soldierPrefab);
            TryCreatePool(UnitType.Aircraft, aircraftPrefab);
            TryCreatePool(UnitType.Helicopter, helicopterPrefab);
            TryCreatePool(UnitType.Truck, truckPrefab);

            if (unitPools.Count == 0)
            {
                Debug.LogWarning(
                    "UnitPoolManager: No valid unit pools were created. Assign prefabs that include a Unit component (e.g. Tank) on the root or a child.",
                    this);
            }
        }

        private void TryCreatePool(UnitType type, GameObject prefab)
        {
            if (prefab == null)
                return;

            var unitComponent = prefab.GetComponent<Unit>() ?? prefab.GetComponentInChildren<Unit>(true);
            if (unitComponent == null)
            {
                var hasCombatOnly = prefab.GetComponent<CombatUnit>() != null
                    || prefab.GetComponentInChildren<CombatUnit>(true) != null;

                var hint = hasCombatOnly
                    ? " Has VehicleCombatUnit/CombatUnit but is missing Tank or GenericUnit on the prefab."
                    : " Add Tank or GenericUnit (+ VehicleCombatUnit) or clear this slot.";

                Debug.LogWarning(
                    $"UnitPoolManager: Prefab '{prefab.name}' ({type}) cannot be pooled.{hint}",
                    prefab);
                return;
            }

            unitPools[type] = new ObjectPool<Unit>(unitComponent, poolParent, initialPoolSize, maxPoolSize);
        }
        
        /// <summary>
        /// Create a unit using object pooling
        /// </summary>
        public Unit CreateUnit(UnitType type, Vector3 position, Quaternion rotation, Team team)
        {
            if (!unitPools.ContainsKey(type))
            {
                // Debug.LogError($"🏗️ No pool found for unit type: {type}");
                return null;
            }
            
            // Get unit from pool
            Unit unit = unitPools[type].Get();
            
            if (unit != null)
            {
                unit.transform.rotation = rotation;

                // Reset unit state before placement (colliders/renderers must be enabled)
                ResetUnit(unit);

                SpawnPlacementUtility.PlaceUnitOnGround(unit, position);
                
                // Assign team
                unit.AssignToTeam(team);
                
                // Set layer based on team
                SetUnitLayer(unit, team);
                
                // Subscribe to death event for cleanup
                unit.OnDeath += HandleUnitDeath;
                
                // Trigger unit created event for systems like SpatialGrid
                GameEvents.TriggerUnitCreated(unit);
                
                // Debug.Log($"🏗️ Created {type} unit at {position} for team {team}");
            }
            
            return unit;
        }
        
        /// <summary>
        /// Set the unit's layer based on its team
        /// Player units go to layer 6, AI units go to layer 7
        /// </summary>
        private void SetUnitLayer(Unit unit, Team team)
        {
            int layer = team == Team.Player ? 6 : 7; // Player = 6, AI = 7
            
            // Set layer for the main GameObject and all children
            SetGameObjectLayer(unit.gameObject, layer);
            
            // Debug.Log($"🏗️ Set {unit.name} to layer {layer} (Team: {team})");
        }
        
        /// <summary>
        /// Set the GameObject and all its children to the specified layer
        /// </summary>
        private void SetGameObjectLayer(GameObject obj, int layer)
        {
            obj.layer = layer;
            
            // Also set layer for all children
            Transform[] children = obj.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                child.gameObject.layer = layer;
            }
        }
        
        /// <summary>
        /// Reset unit to initial state when getting from pool
        /// </summary>
        private void ResetUnit(Unit unit)
        {
            // Reset unit data using the public method
            unit.ResetUnitData();
            
            // Reset visual components
            var visualCoordinator = unit.GetVisualCoordinator();
            if (visualCoordinator != null)
            {
                visualCoordinator.ResetVisuals();
            }
            
            // Enable all components
            unit.gameObject.SetActive(true);
            
            // Reset any other components that need resetting
            var collider = unit.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
        
        /// <summary>
        /// Handle unit death and schedule destruction
        /// </summary>
        private void HandleUnitDeath(IUnit unit)
        {
            if (unit is Unit unitComponent)
            {
                // Debug.Log($"💀 UnitPoolManager: Unit {unit.Name} died, scheduling destruction");
                
                if (destroyImmediately)
                {
                    // For testing - destroy immediately
                    DestroyUnit(unitComponent);
                }
                else if (useObjectPooling)
                {
                    // Schedule destruction with delay to allow particle effects
                    pendingDestruction[unitComponent] = Time.time + unitDestructionDelay;
                }
                else
                {
                    // Use Unity's Destroy
                    Destroy(unitComponent.gameObject, unitDestructionDelay);
                }
            }
        }
        
        /// <summary>
        /// Destroy unit and return to pool
        /// </summary>
        private void DestroyUnit(Unit unit)
        {
            if (unit == null) return;
            
            // Debug.Log($"🏗️ UnitPoolManager: Destroying unit {unit.Name}");
            
            // Unsubscribe from death event
            unit.OnDeath -= HandleUnitDeath;
            
            // Unregister from movement system
            var movementSystem = SystemInitializer.GetSystem<MovementSystem>();
            if (movementSystem != null)
            {
                movementSystem.UnregisterUnit(unit);
                // Debug.Log($"🏗️ UnitPoolManager: Unregistered {unit.Name} from movement system");
            }
            
            // Remove from pending destruction
            pendingDestruction.Remove(unit);
            
            // Return to appropriate pool
            if (unitPools.ContainsKey(unit.Type))
            {
                unitPools[unit.Type].Return(unit);
                // Debug.Log($"🏗️ Unit {unit.Name} returned to {unit.Type} pool");
            }
            else
            {
                // Debug.LogWarning($"🏗️ No pool found for {unit.Type}, destroying GameObject");
                Destroy(unit.gameObject);
            }
        }
        
        private void Update()
        {
            // Check for units that need to be destroyed
            if (useObjectPooling && pendingDestruction.Count > 0)
            {
                var unitsToDestroy = new List<Unit>();
                
                foreach (var kvp in pendingDestruction)
                {
                    if (Time.time >= kvp.Value)
                    {
                        unitsToDestroy.Add(kvp.Key);
                    }
                }
                
                foreach (var unit in unitsToDestroy)
                {
                    DestroyUnit(unit);
                }
            }
        }
        
        /// <summary>
        /// Get initial health for unit type
        /// </summary>
        private float GetInitialHealth(UnitType type)
        {
            var config = MyGame.Core.Configuration.GameConfig.Instance;
            
            switch (type)
            {
                case UnitType.Tank: return config.tankHealth;
                case UnitType.Soldier: return config.soldierHealth;
                case UnitType.Aircraft: return config.aircraftHealth;
                case UnitType.Helicopter: return config.helicopterHealth;
                case UnitType.Truck: return config.truckHealth;
                default: return config.defaultHealth;
            }
        }
        
        /// <summary>
        /// Get pool statistics
        /// </summary>
        public void LogPoolStats()
        {
            // Debug.Log("🏗️ Unit Pool Statistics:");
            foreach (var kvp in unitPools)
            {
                var pool = kvp.Value;
                // Debug.Log($"  {kvp.Key}: Active={pool.ActiveCount}, Pooled={pool.PooledCount}, Total={pool.TotalCount}");
            }
        }
        
        /// <summary>
        /// Clear all pools (useful for testing)
        /// </summary>
        public void ClearAllPools()
        {
            unitPools.Clear();
            pendingDestruction.Clear();
            // Debug.Log("🏗️ All unit pools cleared");
        }
        
        private void OnDestroy()
        {
            // Clean up any remaining units
            foreach (var unit in pendingDestruction.Keys)
            {
                if (unit != null)
                {
                    unit.OnDeath -= HandleUnitDeath;
                }
            }
            pendingDestruction.Clear();
        }
    }
}
