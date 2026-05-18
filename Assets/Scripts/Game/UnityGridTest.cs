using System;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Core;
using MyGame.Core.SpatialPartitioning;
using MyGame.Core.Units;
using Random = UnityEngine.Random;

namespace MyGame.Game
{
    /// <summary>
    /// Test script to verify Unity Grid-based SpatialGrid integration
    /// </summary>
    public class UnityGridTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private int testUnitCount = 10;
        [SerializeField] private float testRadius = 20f;
        
        [Header("References")]
        [SerializeField] private GameObject unitPrefab;
        
        private SpatialGrid spatialGrid;
        private IUnit[] testUnits;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(RunTestDelayed());
            }
        }
        
        private System.Collections.IEnumerator RunTestDelayed()
        {
            // Wait for systems to initialize
            yield return new WaitForSeconds(1f);
            RunUnityGridTest();
        }
        
        [ContextMenu("Run Unity Grid Test")]
        public void RunUnityGridTest()
        {
            Debug.Log("=== UNITY GRID SPATIALGRID TEST ===");
            
            // Get SpatialGrid from DependencyContainer, fallback to FindObjectOfType
            spatialGrid = DependencyContainer.Instance.TryResolve<SpatialGrid>();
            if (spatialGrid == null)
            {
                spatialGrid = FindObjectOfType<SpatialGrid>();
            }
            if (spatialGrid == null)
            {
                Debug.LogError("SpatialGrid not found in scene!");
                return;
            }
            
            Debug.Log($"Found SpatialGrid: {spatialGrid.name}");
            Debug.Log($"Grid Stats: {spatialGrid.GetGridStats()}");
            
            // Test Unity Grid APIs
            TestUnityGridAPIs();
            
            // Test unit registration
            TestUnitRegistration();
            
            // Test radius queries
            TestRadiusQueries();
            
            Debug.Log("=== UNITY GRID TEST COMPLETE ===");
        }
        
        private void TestUnityGridAPIs()
        {
            Debug.Log("--- Testing Unity Grid APIs ---");
            
            Vector3 testWorldPos = new Vector3(15f, 0f, 25f);
            
            // Test WorldToCell
            var gridPos = spatialGrid.WorldToGrid(testWorldPos);
            Debug.Log($"WorldToCell({testWorldPos}) = {gridPos}");
            
            // Test CellToWorld
            var backToWorld = spatialGrid.GridToWorld(gridPos);
            Debug.Log($"CellToWorld({gridPos}) = {backToWorld}");
            
            // Test WorldToLocal
            var localPos = spatialGrid.WorldToLocal(testWorldPos);
            Debug.Log($"WorldToLocal({testWorldPos}) = {localPos}");
            
            // Verify consistency
            float distance = Vector3.Distance(testWorldPos, backToWorld);
            Debug.Log($"Position consistency check: {distance:F3}m (should be < 1m)");
        }
        
        private void TestUnitRegistration()
        {
            Debug.Log("--- Testing Unit Registration ---");
            
            // Create test units
            testUnits = new IUnit[testUnitCount];
            
            for (int i = 0; i < testUnitCount; i++)
            {
                Vector3 position = new Vector3(
                    Random.Range(-50f, 50f),
                    0f,
                    Random.Range(-50f, 50f)
                );
                
                if (unitPrefab != null)
                {
                    var unitObj = Instantiate(unitPrefab, position, Quaternion.identity);
                    testUnits[i] = unitObj.GetComponent<IUnit>();
                }
                else
                {
                    // Create mock unit for testing
                    var mockUnit = CreateMockUnit(position, $"TestUnit_{i}");
                    testUnits[i] = mockUnit;
                }
                
                // Register with SpatialGrid
                spatialGrid.AddUnit(testUnits[i]);
                
                Debug.Log($"Created and registered unit {i} at {position}");
            }
            
            Debug.Log($"Registered {spatialGrid.GetRegisteredUnitCount()} units");
            Debug.Log($"Occupied cells: {spatialGrid.GetOccupiedCellCount()}");
        }
        
        private void TestRadiusQueries()
        {
            Debug.Log("--- Testing Radius Queries ---");
            
            Vector3 testCenter = Vector3.zero;
            
            // Test GetUnitsInRadius
            var unitsInRadius = spatialGrid.GetUnitsInRadius(testCenter, testRadius);
            Debug.Log($"Found {unitsInRadius.Count} units within {testRadius}m of {testCenter}");
            
            // Test GetClosestUnit
            var closestUnit = spatialGrid.GetClosestUnit(testCenter, testRadius);
            if (closestUnit != null)
            {
                float distance = Vector3.Distance(testCenter, closestUnit.Position);
                Debug.Log($"Closest unit: {closestUnit.Name} at {distance:F1}m");
            }
            
            // Test GetUnitsSortedByDistance
            var sortedUnits = spatialGrid.GetUnitsSortedByDistance(testCenter, testRadius);
            Debug.Log($"Sorted units by distance: {sortedUnits.Count} units");
            
            for (int i = 0; i < Mathf.Min(3, sortedUnits.Count); i++)
            {
                float distance = Vector3.Distance(testCenter, sortedUnits[i].Position);
                Debug.Log($"  {i + 1}. {sortedUnits[i].Name} at {distance:F1}m");
            }
        }
        
        private IUnit CreateMockUnit(Vector3 position, string name)
        {
            // Create a simple mock unit for testing
            var mockUnit = new MockUnit
            {
                Position = position,
                Name = name,
                Health = 100f,
                Owner = new MockPlayer { Team = Team.Player }
            };
            
            return mockUnit;
        }
        
        private void OnDrawGizmos()
        {
            if (spatialGrid != null)
            {
                // Draw test radius
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(Vector3.zero, testRadius);
                
                // Draw test units
                if (testUnits != null)
                {
                    Gizmos.color = Color.green;
                    foreach (var unit in testUnits)
                    {
                        if (unit != null)
                        {
                            Gizmos.DrawWireSphere(unit.Position, 1f);
                        }
                    }
                }
            }
        }
        
        // Mock classes for testing
        private class MockUnit : IUnit
        {
            public Vector3 Position { get; set; }
            public string Name { get; set; }
            public float Health { get; set; }
            public Player Owner { get; set; }
            
            // Implement other IUnit members with default values
            public UnitType Type => UnitType.Tank;
            public float Speed => 10f;
            public bool IsMoving => false;
            public Vector3 Destination => Position;
            public float AttackCooldown { get; }
            public float LastAttackTime => 0f;
            public List<MyGame.Core.Skills.Skill> Skills => new List<MyGame.Core.Skills.Skill>();
            
            public void AssignToTeam(Team team) { }
            public bool CanAttack(IUnit target)
            {
                throw new System.NotImplementedException();
            }

            public bool Attack(IUnit target)
            {
                throw new System.NotImplementedException();
            }

            public float AttackDamage { get; }
            public float AttackRange { get; }
            public void TakeDamage(float amount) { Health -= amount; }
            public void MoveTo(Vector3 destination) { }
            public void UpdatePosition(Vector3 newPosition) { Position = newPosition; }
            public void UseSkill(int skillIndex) { }
            public void PlayAnimation(string animationName) { }
            public void Upgrade() { }
            
            public event System.Action<IUnit> OnDeath;
            public event System.Action<IUnit, IUnit> OnAttack;
            public event System.Action<IUnit, Vector3> OnMove;
            public event System.Action<string> OnAnimationEvent;
        }
        
        private class MockPlayer : Player
        {
            public MockPlayer(string name, Team team, bool isAI = false) : base(name, team, isAI)
            {
                Team = team;
            }

            public MockPlayer() : base(String.Empty, Team.AI )
            {
                throw new System.NotImplementedException();
            }

            public Team Team { get; set; }
        }
    }
}
