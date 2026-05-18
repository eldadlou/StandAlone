using System.Collections.Generic;
using MyGame.Core;
using UnityEngine;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;
using MyGame.Core.SpatialPartitioning;

namespace MyGame.RuntimeSystems.Combat
{
    /// <summary>
    /// Centralized detection manager that handles all enemy detection logic
    /// Eliminates individual unit updates and significantly improves performance
    /// </summary>
    public class CentralizedDetectionManager : MonoBehaviour, ICentralizedCombatDetection
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionUpdateInterval = 0.1f; // How often to run a batch
        [SerializeField] private bool enableDebugLogging = false;
        
        [Header("Performance Settings")]
        [SerializeField] private int maxUnitsPerBatch = 5; // Process fewer units per frame for smoother performance
        [SerializeField] private bool useSpatialGrid = true;
        
        // Core systems
        private SpatialGrid spatialGrid;
        private List<CombatUnit> registeredCombatUnits = new List<CombatUnit>();
        private Dictionary<CombatUnit, IUnit> currentTargets = new Dictionary<CombatUnit, IUnit>();
        private Dictionary<CombatUnit, float> lastTargetUpdateTimes = new Dictionary<CombatUnit, float>();
        
        // Performance optimization
        private float lastDetectionUpdateTime;
        private int currentBatchIndex = 0;
        private List<CombatUnit> unitsToProcess = new List<CombatUnit>();
        
        // GC OPTIMIZATION: Pre-allocated collections to avoid garbage every frame
        private List<IUnit> _reusableEnemyList = new List<IUnit>(32); // Reused for enemy detection
        private List<IUnit> _reusableSpatialQueryResult = new List<IUnit>(64); // Reused for spatial queries
        private Collider[] _reusableColliderArray = new Collider[64]; // Reused for Physics.OverlapSphere
        
        // Note: Caching was removed as it caused units to miss new enemies entering range
        // Fresh detection every cycle is more reliable for combat responsiveness
        
        private void Awake()
        {
            // Get SpatialGrid
            spatialGrid = GetComponent<SpatialGrid>( );
            if (spatialGrid == null && useSpatialGrid)
            {
                // Debug.LogWarning("CentralizedDetectionManager: SpatialGrid not found, falling back to Physics.OverlapSphere");
                useSpatialGrid = false;
            }
            
            var container = DependencyContainer.Instance;
            container.Register(this);
            container.RegisterAs<ICentralizedCombatDetection>(this);
            
            // Debug.Log($"CentralizedDetectionManager initialized - SpatialGrid: {useSpatialGrid}");
        }
        
        private void Update()
        {
            // Update detection periodically
            if (Time.time - lastDetectionUpdateTime >= detectionUpdateInterval)
            {
                UpdateDetectionBatch();
                lastDetectionUpdateTime = Time.time;
            }
        }
        
        /// <summary>
        /// Register a combat unit for centralized detection
        /// </summary>
        public void RegisterCombatUnit(CombatUnit combatUnit)
        {
            if (combatUnit == null || registeredCombatUnits.Contains(combatUnit)) return;
            
            registeredCombatUnits.Add(combatUnit);
            currentTargets[combatUnit] = null;
            // Set time in the PAST so unit gets processed immediately on the next detection cycle
            // This ensures newly registered units don't have to wait for the interval
            // We don't call UpdateUnitDetection immediately because other units might not be 
            // registered with SpatialGrid yet - the next cycle will handle it with 0 interval
            lastTargetUpdateTimes[combatUnit] = Time.time - detectionUpdateInterval - 1f;
            
            if (enableDebugLogging)
                Debug.Log($"CentralizedDetectionManager: Registered {combatUnit.Name}");
        }
        
        /// <summary>
        /// Unregister a combat unit
        /// </summary>
        public void UnregisterCombatUnit(CombatUnit combatUnit)
        {
            if (combatUnit == null) return;
            
            registeredCombatUnits.Remove(combatUnit);
            currentTargets.Remove(combatUnit);
            lastTargetUpdateTimes.Remove(combatUnit);
            
            if (enableDebugLogging)
                Debug.Log($"CentralizedDetectionManager: Unregistered {combatUnit.Name}");
        }
        
        /// <summary>
        /// Get current target for a combat unit
        /// </summary>
        public IUnit GetCurrentTarget(CombatUnit combatUnit)
        {
            return currentTargets.TryGetValue(combatUnit, out IUnit target) ? target : null;
        }
        
        /// <summary>
        /// Update detection for a batch of units to avoid frame spikes
        /// </summary>
        private void UpdateDetectionBatch()
        {
            if (registeredCombatUnits.Count == 0) return;
            
            // If we have fewer units than the batch size, process ALL units every cycle
            // This ensures small groups all get targets simultaneously
            if (registeredCombatUnits.Count <= maxUnitsPerBatch)
            {
                for (int i = 0; i < registeredCombatUnits.Count; i++)
                {
                    var combatUnit = registeredCombatUnits[i];
                    if (combatUnit != null)
                    {
                        UpdateUnitDetection(combatUnit);
                    }
                }
                currentBatchIndex = 0;
                return;
            }
            
            // For larger groups, use batching to avoid frame spikes
            int startIndex = currentBatchIndex;
            int endIndex = Mathf.Min(startIndex + maxUnitsPerBatch, registeredCombatUnits.Count);
            
            // Process current batch
            for (int i = startIndex; i < endIndex; i++)
            {
                var combatUnit = registeredCombatUnits[i];
                if (combatUnit != null)
                {
                    UpdateUnitDetection(combatUnit);
                }
            }
            
            // Move to next batch (wrap around)
            currentBatchIndex = endIndex >= registeredCombatUnits.Count ? 0 : endIndex;
            
            if (enableDebugLogging && Time.frameCount % 60 == 0)
            {
                Debug.Log($"CentralizedDetectionManager: Processed batch {startIndex}-{endIndex} of {registeredCombatUnits.Count} units");
            }
        }
        
        /// <summary>
        /// Update detection for a single unit
        /// PERFORMANCE OPTIMIZED: Uses sqrMagnitude, skips search when target is valid
        /// </summary>
        private void UpdateUnitDetection(CombatUnit combatUnit)
        {
            if (combatUnit == null) return;
            
            // Get current target from our tracking
            IUnit currentTarget = currentTargets.TryGetValue(combatUnit, out IUnit t) ? t : null;
            
            // OPTIMIZATION: Use squared distance to avoid sqrt
            float detectionRadiusSqr = combatUnit.DetectionRadius * combatUnit.DetectionRadius;
            Vector3 unitPos = combatUnit.Position;
            
            // Check if current target is still valid and in range
            bool currentTargetValid = false;
            if (currentTarget != null)
            {
                // Quick health check first (cheapest)
                if (currentTarget.Health > 0)
                {
                    // Use sqrMagnitude instead of Distance (no sqrt)
                    float distSqr = (unitPos - currentTarget.Position).sqrMagnitude;
                    currentTargetValid = distSqr <= detectionRadiusSqr;
                }
                
                if (!currentTargetValid)
                {
                    ClearTarget(combatUnit);
                    currentTarget = null;
                }
            }
            
            // OPTIMIZATION: Only search for new target if we don't have a valid one
            // This massively reduces work when units are already engaged
            if (currentTarget == null)
            {
                IUnit newTarget = FindBestTargetFast(combatUnit, unitPos, detectionRadiusSqr);
                if (newTarget != null)
                {
                    SetTarget(combatUnit, newTarget);
                }
            }
        }
        
        /// <summary>
        /// Find the best target for a combat unit - FAST VERSION
        /// Uses pre-calculated squared radius and position for efficiency
        /// </summary>
        private IUnit FindBestTargetFast(CombatUnit combatUnit, Vector3 unitPos, float detectionRadiusSqr)
        {
            if (combatUnit == null) return null;
            
            // Get enemies in range
            GetEnemiesInRangeOptimized(combatUnit);
            
            if (_reusableEnemyList.Count == 0) return null;
            
            // Find closest enemy using sqrMagnitude (no sqrt)
            IUnit closestEnemy = null;
            float closestDistSqr = float.MaxValue;
            
            for (int i = 0; i < _reusableEnemyList.Count; i++)
            {
                var enemy = _reusableEnemyList[i];
                float distSqr = (unitPos - enemy.Position).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closestEnemy = enemy;
                }
            }
            
            return closestEnemy;
        }
        
        /// <summary>
        /// Optimized method to get enemies in range
        /// GC OPTIMIZED: Reuses pre-allocated collections to avoid garbage
        /// </summary>
        private List<IUnit> GetEnemiesInRangeOptimized(CombatUnit combatUnit)
        {
            // Clear and reuse the enemy list - NO NEW ALLOCATIONS
            _reusableEnemyList.Clear();
            
            if (useSpatialGrid && spatialGrid != null)
            {
                // Use Unity Grid-based SpatialGrid with reusable list
                spatialGrid.GetUnitsInRadiusNonAlloc(combatUnit.Position, combatUnit.DetectionRadius, _reusableSpatialQueryResult);
                
                for (int i = 0; i < _reusableSpatialQueryResult.Count; i++)
                {
                    var unit = _reusableSpatialQueryResult[i];
                    if (unit == null) continue;
                    
                    // CRITICAL FIX: Check if this is the same unit by comparing positions
                    // We can't use unit != combatUnit because IUnit (Unit component) and CombatUnit
                    // are different components on the same GameObject - object reference won't match
                    if (!IsSameUnit(combatUnit, unit) && IsEnemy(combatUnit, unit))
                    {
                        _reusableEnemyList.Add(unit);
                    }
                }
            }
            else
            {
                // Fallback to Physics.OverlapSphereNonAlloc - NO GC ALLOCATION
                int hitCount = Physics.OverlapSphereNonAlloc(
                    combatUnit.Position, 
                    combatUnit.DetectionRadius, 
                    _reusableColliderArray, 
                    combatUnit.EnemyLayerMask);
                
                for (int i = 0; i < hitCount; i++)
                {
                    var collider = _reusableColliderArray[i];
                    if (collider == null) continue;
                    
                    // Use GetComponentInParent to find IUnit even if collider is on a child object
                    IUnit enemy = collider.GetComponentInParent<IUnit>();
                    if (enemy != null && !IsSameUnit(combatUnit, enemy) && IsEnemy(combatUnit, enemy))
                    {
                        _reusableEnemyList.Add(enemy);
                    }
                }
            }
            
            return _reusableEnemyList;
        }
        
        /// <summary>
        /// Check if an IUnit is the same unit as a CombatUnit
        /// OPTIMIZED: Simple gameObject comparison
        /// </summary>
        private bool IsSameUnit(CombatUnit combatUnit, IUnit unit)
        {
            // Fast path: compare gameObjects directly
            // Both CombatUnit and IUnit (Unit) are MonoBehaviours on the same GameObject
            return unit is MonoBehaviour mb && mb.gameObject == combatUnit.gameObject;
        }
        
        /// <summary>
        /// Check if a unit is an enemy - OPTIMIZED inline checks
        /// </summary>
        private bool IsEnemy(CombatUnit combatUnit, IUnit otherUnit)
        {
            // Combined null and health check
            if (otherUnit == null || otherUnit.Health <= 0) return false;
            
            // Team comparison (most common check)
            var ourOwner = combatUnit.Owner;
            var theirOwner = otherUnit.Owner;
            return ourOwner != null && theirOwner != null && ourOwner.Team != theirOwner.Team;
        }
        
        /// <summary>
        /// Set target for a combat unit
        /// </summary>
        private void SetTarget(CombatUnit combatUnit, IUnit target)
        {
            if (combatUnit == null || target == null) return;
            
            currentTargets[combatUnit] = target;
            
            // Notify the combat unit
            combatUnit.SetTarget(target);
            
           // if (enableDebugLogging)
                // Debug.Log($"CentralizedDetectionManager: {combatUnit.Name} targeting {target.Name}");
        }
        
        /// <summary>
        /// Clear target for a combat unit
        /// </summary>
        private void ClearTarget(CombatUnit combatUnit)
        {
            if (combatUnit == null) return;
            
            currentTargets[combatUnit] = null;
            
            // Notify the combat unit
            combatUnit.ClearTarget();
            
           // if (enableDebugLogging)
                // Debug.Log($"CentralizedDetectionManager: {combatUnit.Name} cleared target");
        }
        
        /// <summary>
        /// Force update detection for all units (for testing)
        /// </summary>
        [ContextMenu("Force Update All Detection")]
        public void ForceUpdateAllDetection()
        {
            foreach (var combatUnit in registeredCombatUnits)
            {
                if (combatUnit != null)
                {
                    UpdateUnitDetection(combatUnit);
                }
            }
            
            // Debug.Log($"CentralizedDetectionManager: Force updated detection for {registeredCombatUnits.Count} units");
        }
        
        /// <summary>
        /// Get performance statistics
        /// </summary>
        public string GetPerformanceStats()
        {
            int unitsWithTargets = 0;
            foreach (var target in currentTargets.Values)
            {
                if (target != null) unitsWithTargets++;
            }
            
            return $"CentralizedDetectionManager Stats:\n" +
                   $"Registered Units: {registeredCombatUnits.Count}\n" +
                   $"Units with Targets: {unitsWithTargets}\n" +
                   $"Update Interval: {detectionUpdateInterval}s\n" +
                   $"Batch Size: {maxUnitsPerBatch}\n" +
                   $"Using SpatialGrid: {useSpatialGrid}";
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!enableDebugLogging) return;
            
            // Draw detection ranges for registered units
            foreach (var combatUnit in registeredCombatUnits)
            {
                if (combatUnit != null)
                {
                    // Detection radius
                    Gizmos.color = currentTargets.TryGetValue(combatUnit, out IUnit target) && target != null ? Color.red : Color.yellow;
                    Gizmos.DrawWireSphere(combatUnit.Position, combatUnit.DetectionRadius);
                    
                    // Target line
                    if (target != null)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(combatUnit.Position, target.Position);
                    }
                }
            }
        }
    }
}
