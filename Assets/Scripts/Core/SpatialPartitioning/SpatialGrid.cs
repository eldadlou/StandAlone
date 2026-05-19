using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyGame.Core.Units;
using MyGame.Core.Events;
using MyGame.Core.Services;

namespace MyGame.Core.SpatialPartitioning
{
    /// <summary>
    /// Spatial grid for efficient unit queries using Unity's built-in Grid component
    /// Much more efficient than custom grid implementation
    /// </summary>
    public class SpatialGrid : MonoBehaviour, ISpatialUnitQuery
    {
        [Header("Grid Settings")]
        [SerializeField] private float cellSize = 10f;
        
        // Unity's built-in Grid component
        private Grid _unityGrid;
        private bool _initialized;
        private bool _registeredWithContainer;

        // Unit tracking using Unity's Grid coordinates
        private Dictionary<Vector3Int, List<IUnit>> _gridCells = new Dictionary<Vector3Int, List<IUnit>>();
        private Dictionary<IUnit, Vector3Int> _unitPositions = new Dictionary<IUnit, Vector3Int>();

        public float CellSize => cellSize;

        private void Awake() => EnsureInitialized();

        /// <summary>
        /// Idempotent setup for Grid + DI. Safe when Awake does not run (e.g. Edit Mode tests).
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            _unityGrid = GetComponent<Grid>();
            if (_unityGrid == null)
                _unityGrid = gameObject.AddComponent<Grid>();

            _unityGrid.cellSize = new Vector3(cellSize, 1f, cellSize);
            _unityGrid.cellGap = Vector3.zero;
            _unityGrid.cellSwizzle = GridLayout.CellSwizzle.XZY;

            if (!_registeredWithContainer)
            {
                var container = DependencyContainer.Instance;
                container.Register(this);
                container.RegisterAs<ISpatialUnitQuery>(this);
                _registeredWithContainer = true;
            }

            _initialized = true;
        }
        
        private void Start()
        {
            // Automatically register all existing units
            RegisterAllExistingUnits();
            
            // Subscribe to unit creation events to register new units
            GameEvents.OnUnitCreated += OnUnitCreated;
            
            // Subscribe to unit query event so SelectionRectangle can find all units
            GameEvents.OnGetAllUnits += GetAllUnits;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            GameEvents.OnUnitCreated -= OnUnitCreated;
            GameEvents.OnGetAllUnits -= GetAllUnits;
        }
        
        /// <summary>
        /// Called when a new unit is created - register it with the spatial grid
        /// </summary>
        private void OnUnitCreated(IUnit unit)
        {
            if (unit != null)
            {
                // Debug.Log($"SpatialGrid: Registering newly created unit {unit.Name} at position {unit.Position}");
                AddUnit(unit);
            }
        }
        
        private void Update()
        {
            // Update positions of all registered units
            UpdateAllUnitPositions();
        }
        
        /// <summary>
        /// Automatically register all existing units in the scene
        /// </summary>
        private void RegisterAllExistingUnits()
        {
            // Find all units with IUnit interface
            var allUnits = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb is IUnit)
                .Cast<IUnit>()
                .ToArray();
            
            // Debug.Log($"SpatialGrid: Found {allUnits.Length} units in scene");
            
            foreach (var unit in allUnits)
            {
                // Debug.Log($"SpatialGrid: Registering unit {unit.Name} at position {unit.Position}");
                AddUnit(unit);
            }
            
            // Debug.Log($"SpatialGrid: Auto-registered {allUnits.Length} existing units");
            // Debug.Log($"SpatialGrid: Grid now has {_unitPositions.Count} registered units and {_gridCells.Count} occupied cells");
        }
        
        /// <summary>
        /// Update positions of all registered units
        /// </summary>
        private void UpdateAllUnitPositions()
        {
            // Update positions of all registered units
            var unitsToUpdate = new List<IUnit>(_unitPositions.Keys);
            foreach (var unit in unitsToUpdate)
            {
                if (unit != null)
                {
                    UpdateUnitPosition(unit);
                }
            }
        }

        /// <summary>
        /// Add a unit to the spatial grid using Unity's Grid
        /// </summary>
        public void AddUnit(IUnit unit)
        {
            if (unit == null) 
            {
                // Debug.LogWarning("SpatialGrid: Cannot add null unit");
                return;
            }

            EnsureInitialized();

            // Use 2D grid coordinates (ignore Y for flat terrain)
            var worldPos = unit.Position;
            var flatPos = new Vector3(worldPos.x, 0, worldPos.z); // Set Y to 0 for flat terrain
            var gridPos = _unityGrid.WorldToCell(flatPos);
            gridPos = new Vector3Int(gridPos.x, 0, gridPos.z); // Remove Y coordinate
            
            if (!_gridCells.ContainsKey(gridPos))
                _gridCells[gridPos] = new List<IUnit>();
            
            _gridCells[gridPos].Add(unit);
            _unitPositions[unit] = gridPos;
            
            // Debug.Log($"SpatialGrid: Added unit {unit.Name} to grid cell {gridPos} at world position {unit.Position} (flat: {flatPos})");
        }

        /// <summary>
        /// Remove a unit from the spatial grid
        /// </summary>
        public void RemoveUnit(IUnit unit)
        {
            if (unit == null || !_unitPositions.ContainsKey(unit)) return;

            var gridPos = _unitPositions[unit];
            
            if (_gridCells.ContainsKey(gridPos))
                _gridCells[gridPos].Remove(unit);
            
            _unitPositions.Remove(unit);
        }

        /// <summary>
        /// Update unit position in the grid using Unity's Grid
        /// </summary>
        public void UpdateUnitPosition(IUnit unit)
        {
            if (unit == null) return;

            EnsureInitialized();

            // Use 2D grid coordinates (ignore Y for flat terrain)
            var worldPos = unit.Position;
            var flatPos = new Vector3(worldPos.x, 0, worldPos.z); // Set Y to 0 for flat terrain
            var newGridPos = _unityGrid.WorldToCell(flatPos);
            newGridPos = new Vector3Int(newGridPos.x, 0, newGridPos.z); // Remove Y coordinate
            
            if (_unitPositions.TryGetValue(unit, out var oldGridPos))
            {
                if (oldGridPos == newGridPos) return; // No change needed
                
                // Remove from old position
                if (_gridCells.ContainsKey(oldGridPos))
                    _gridCells[oldGridPos].Remove(unit);
            }
            
            // Add to new position
            if (!_gridCells.ContainsKey(newGridPos))
                _gridCells[newGridPos] = new List<IUnit>();
            
            _gridCells[newGridPos].Add(unit);
            _unitPositions[unit] = newGridPos;
        }

        /// <summary>
        /// Get units within a radius of a position using Unity's Grid
        /// Much more efficient than custom implementation
        /// </summary>
        public List<IUnit> GetUnitsInRadius(Vector3 position, float radius)
        {
            EnsureInitialized();

            var units = new List<IUnit>();
            
            // Use 2D grid coordinates (ignore Y for flat terrain)
            var flatPos = new Vector3(position.x, 0, position.z); // Set Y to 0 for flat terrain
            var centerGrid = _unityGrid.WorldToCell(flatPos);
            centerGrid = new Vector3Int(centerGrid.x, 0, centerGrid.z); // Remove Y coordinate
            var radiusInCells = Mathf.CeilToInt(radius / cellSize);
            
            // Debug.Log($"SpatialGrid.GetUnitsInRadius: Position={position}, FlatPos={flatPos}, Radius={radius}, CenterGrid={centerGrid}, RadiusInCells={radiusInCells}");
            // Debug.Log($"SpatialGrid: Total registered units: {_unitPositions.Count}, Occupied cells: {_gridCells.Count}");
            
            // Use Unity's Grid to get all cells in radius
            for (int x = -radiusInCells; x <= radiusInCells; x++)
            {
                for (int z = -radiusInCells; z <= radiusInCells; z++)
                {
                    var gridPos = centerGrid + new Vector3Int(x, 0, z);
                    
                    if (_gridCells.TryGetValue(gridPos, out var cellUnits))
                    {
                        // Debug.Log($"SpatialGrid: Found {cellUnits.Count} units in cell {gridPos}");
                        
                        foreach (var unit in cellUnits)
                        {
                            // Double-check distance using actual world positions
                            var distance = Vector3.Distance(position, unit.Position);
                            
                            if (distance <= radius)
                            {
                                units.Add(unit);
                                // Debug.Log($"SpatialGrid: Added unit {unit.Name} at distance {distance:F1}m");
                            }
                        }
                    }
                }
            }
            
            // Debug.Log($"SpatialGrid.GetUnitsInRadius: Returning {units.Count} units");
            return units;
        }

        /// <summary>
        /// Get units within a radius - NON-ALLOCATING version
        /// Clears and fills the provided list to avoid GC allocations
        /// </summary>
        public void GetUnitsInRadiusNonAlloc(Vector3 position, float radius, List<IUnit> resultList)
        {
            EnsureInitialized();

            resultList.Clear();
            
            // Use 2D grid coordinates (ignore Y for flat terrain)
            var flatPos = new Vector3(position.x, 0, position.z);
            var centerGrid = _unityGrid.WorldToCell(flatPos);
            centerGrid = new Vector3Int(centerGrid.x, 0, centerGrid.z);
            var radiusInCells = Mathf.CeilToInt(radius / cellSize);
            
            for (int x = -radiusInCells; x <= radiusInCells; x++)
            {
                for (int z = -radiusInCells; z <= radiusInCells; z++)
                {
                    var gridPos = centerGrid + new Vector3Int(x, 0, z);
                    
                    if (_gridCells.TryGetValue(gridPos, out var cellUnits))
                    {
                        for (int i = 0; i < cellUnits.Count; i++)
                        {
                            var unit = cellUnits[i];
                            var distance = Vector3.Distance(position, unit.Position);
                            
                            if (distance <= radius)
                            {
                                resultList.Add(unit);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get units in a specific grid cell using Unity's Grid
        /// </summary>
        public List<IUnit> GetUnitsInCell(Vector3Int gridPosition)
        {
            return _gridCells.TryGetValue(gridPosition, out var units) ? units : new List<IUnit>();
        }

        /// <summary>
        /// Get the grid position for a world position using Unity's Grid
        /// </summary>
        public Vector3Int WorldToGrid(Vector3 worldPosition)
        {
            EnsureInitialized();
            return _unityGrid.WorldToCell(worldPosition);
        }

        /// <summary>
        /// Get the world position for a grid position using Unity's Grid
        /// </summary>
        public Vector3 GridToWorld(Vector3Int gridPosition)
        {
            EnsureInitialized();
            return _unityGrid.CellToWorld(gridPosition);
        }

        /// <summary>
        /// Get the local position within a cell using Unity's Grid
        /// </summary>
        public Vector3 WorldToLocal(Vector3 worldPosition)
        {
            EnsureInitialized();
            return _unityGrid.WorldToLocal(worldPosition);
        }

        /// <summary>
        /// Get the number of registered units
        /// </summary>
        public int GetRegisteredUnitCount()
        {
            return _unitPositions.Count;
        }

        /// <summary>
        /// Get the number of occupied grid cells
        /// </summary>
        public int GetOccupiedCellCount()
        {
            return _gridCells.Count;
        }

        /// <summary>
        /// Clear all units from the grid
        /// </summary>
        public void ClearAllUnits()
        {
            _gridCells.Clear();
            _unitPositions.Clear();
        }

        /// <summary>
        /// Get all units in the grid
        /// </summary>
        public List<IUnit> GetAllUnits()
        {
            var allUnits = new List<IUnit>();
            foreach (var cellUnits in _gridCells.Values)
            {
                allUnits.AddRange(cellUnits);
            }
            return allUnits;
        }

        /// <summary>
        /// Get units in a rectangular area using Unity's Grid
        /// </summary>
        public List<IUnit> GetUnitsInArea(Vector3 center, Vector2 size)
        {
            EnsureInitialized();

            var units = new List<IUnit>();
            // Use 2D grid coordinates (ignore Y for flat terrain)
            var flatCenter = new Vector3(center.x, 0, center.z); // Set Y to 0 for flat terrain
            var centerGrid = _unityGrid.WorldToCell(flatCenter);
            centerGrid = new Vector3Int(centerGrid.x, 0, centerGrid.z); // Remove Y coordinate
            var sizeInCells = new Vector2Int(
                Mathf.CeilToInt(size.x / cellSize),
                Mathf.CeilToInt(size.y / cellSize)
            );
            
            for (int x = -sizeInCells.x; x <= sizeInCells.x; x++)
            {
                for (int z = -sizeInCells.y; z <= sizeInCells.y; z++)
                {
                    var gridPos = centerGrid + new Vector3Int(x, 0, z);
                    
                    if (_gridCells.TryGetValue(gridPos, out var cellUnits))
                    {
                        units.AddRange(cellUnits);
                    }
                }
            }
            
            return units;
        }

        /// <summary>
        /// Get the closest unit to a position within a radius
        /// </summary>
        public IUnit GetClosestUnit(Vector3 position, float radius)
        {
            var unitsInRange = GetUnitsInRadius(position, radius);
            
            IUnit closestUnit = null;
            float closestDistance = float.MaxValue;
            
            foreach (var unit in unitsInRange)
            {
                float distance = Vector3.Distance(position, unit.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestUnit = unit;
                }
            }
            
            return closestUnit;
        }

        /// <summary>
        /// Get units sorted by distance from a position
        /// </summary>
        public List<IUnit> GetUnitsSortedByDistance(Vector3 position, float radius)
        {
            var unitsInRange = GetUnitsInRadius(position, radius);
            
            // Sort by distance
            unitsInRange.Sort((a, b) => 
                Vector3.Distance(position, a.Position).CompareTo(Vector3.Distance(position, b.Position))
            );
            
            return unitsInRange;
        }

        // Visual debugging
        private void OnDrawGizmosSelected()
        {
            if (_unityGrid == null) return;
            
            // Draw grid cells with units
            foreach (var kvp in _gridCells)
            {
                var gridPos = kvp.Key;
                var units = kvp.Value;
                
                if (units.Count > 0)
                {
                    var worldPos = _unityGrid.CellToWorld(gridPos);
                    
                    // Color based on number of units in cell
                    float intensity = Mathf.Clamp01(units.Count / 5f);
                    Gizmos.color = new Color(intensity, 0, 1 - intensity, 0.3f);
                    
                    // Draw cell bounds
                    Gizmos.DrawWireCube(worldPos + _unityGrid.cellSize * 0.5f, _unityGrid.cellSize);
                    
                    // Draw unit count
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(worldPos + Vector3.up * 2f, units.Count.ToString());
                    #endif
                }
            }
        }

        /// <summary>
        /// Set the cell size and update the Unity Grid
        /// </summary>
        public void SetCellSize(float newCellSize)
        {
            cellSize = newCellSize;
            EnsureInitialized();
            _unityGrid.cellSize = new Vector3(cellSize, 1f, cellSize);
        }

        /// <summary>
        /// Get grid statistics for debugging
        /// </summary>
        public string GetGridStats()
        {
            return $"SpatialGrid Stats:\n" +
                   $"Total Units: {GetRegisteredUnitCount()}\n" +
                   $"Occupied Cells: {GetOccupiedCellCount()}\n" +
                   $"Cell Size: {cellSize}m\n" +
                   $"Unity Grid Active: {_unityGrid != null}";
        }
        
        /// <summary>
        /// Force re-registration of all units (useful for debugging)
        /// </summary>
        [ContextMenu("Re-register All Units")]
        public void ForceReregisterAllUnits()
        {
            ClearAllUnits();
            RegisterAllExistingUnits();
        }
        
        /// <summary>
        /// Manually register a unit (useful for debugging or direct registration)
        /// </summary>
        [ContextMenu("Register All Units Now")]
        public void RegisterAllUnitsNow()
        {
            // Find all units with IUnit interface
            var allUnits = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb is IUnit)
                .Cast<IUnit>()
                .ToArray();
            
            // Debug.Log($"SpatialGrid: Manual registration found {allUnits.Length} units in scene");
            
            foreach (var unit in allUnits)
            {
                if (unit != null && !_unitPositions.ContainsKey(unit))
                {
                    // Debug.Log($"SpatialGrid: Manually registering unit {unit.Name} at position {unit.Position}");
                    AddUnit(unit);
                }
            }
            
            // Debug.Log($"SpatialGrid: Manual registration complete - Grid now has {_unitPositions.Count} registered units and {_gridCells.Count} occupied cells");
        }
    }
}
