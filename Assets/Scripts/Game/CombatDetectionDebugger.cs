using UnityEngine;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;
using MyGame.Game;
using MyGame.RuntimeSystems.Combat;
using System.Collections.Generic;

namespace MyGame.Game
{
    /// <summary>
    /// Comprehensive debug script for diagnosing combat detection issues
    /// Provides detailed information about unit states, team assignments, and detection problems
    /// </summary>
    public class CombatDetectionDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showRealTimeStatus = true;
        [SerializeField] private bool logDetectionIssues = true;
        
        [Header("Test Controls")]
        [SerializeField] private KeyCode runDiagnosticsKey = KeyCode.F1;
        [SerializeField] private KeyCode assignTeamsKey = KeyCode.F2;
        [SerializeField] private KeyCode assignTagsKey = KeyCode.F3;
        [SerializeField] private KeyCode forceDetectionKey = KeyCode.F4;
        [SerializeField] private KeyCode testSystemsKey = KeyCode.F5;
        
        [Header("Detection Test Settings")]
        [SerializeField] private float testDetectionRadius = 25f; // Increased radius for testing
        [SerializeField] private bool testWithPhysicsOverlap = true;
        [SerializeField] private bool testWithSpatialGrid = true;
        
        private List<CombatUnit> combatUnits = new List<CombatUnit>();
        private List<Unit> allUnits = new List<Unit>();
        private float lastDiagnosticTime;
        private const float DIAGNOSTIC_INTERVAL = 5f; // Run diagnostics every 5 seconds
        
        private void Start()
        {
            Debug.Log("🔍 CombatDetectionDebugger: Starting combat detection diagnostics...");
            CollectUnits();
            RunInitialDiagnostics();
        }
        
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(runDiagnosticsKey))
            {
                RunComprehensiveDiagnostics();
            }
            
            if (UnityEngine.Input.GetKeyDown(assignTeamsKey))
            {
                AssignTeamsForTesting();
            }
            
            if (UnityEngine.Input.GetKeyDown(assignTagsKey))
            {
                AssignUnityTagsToAllUnits();
            }
            
            if (UnityEngine.Input.GetKeyDown(forceDetectionKey))
            {
                ForceDetectionTest();
            }
            
            if (UnityEngine.Input.GetKeyDown(testSystemsKey))
            {
                TestSystemInitialization();
            }
            
            // Run periodic diagnostics
            if (Time.time - lastDiagnosticTime > DIAGNOSTIC_INTERVAL)
            {
                if (showRealTimeStatus)
                {
                    UpdateRealTimeStatus();
                }
                lastDiagnosticTime = Time.time;
            }
        }
        
        private void CollectUnits()
        {
            combatUnits.Clear();
            allUnits.Clear();
            
            // Find all combat units
            CombatUnit[] foundCombatUnits = FindObjectsOfType<CombatUnit>();
            combatUnits.AddRange(foundCombatUnits);
            
            // Find all units
            Unit[] foundUnits = FindObjectsOfType<Unit>();
            allUnits.AddRange(foundUnits);
            
            Debug.Log($"🔍 CombatDetectionDebugger: Found {combatUnits.Count} combat units and {allUnits.Count} total units");
        }
        
        private void RunInitialDiagnostics()
        {
            Debug.Log("🔍 CombatDetectionDebugger: Running initial diagnostics...");
            TestSystemInitialization();
            CheckTeamAssignments();
            CheckCombatUnitSetup();
            CheckDetectionSystem();
        }
        
        private void RunComprehensiveDiagnostics()
        {
            Debug.Log("=== COMBAT DETECTION COMPREHENSIVE DIAGNOSTICS ===");
            
            CollectUnits(); // Refresh unit lists
            
            // Test system initialization first
            TestSystemInitialization();
            
            // Check team assignments
            CheckTeamAssignments();
            
            // Check combat unit setup
            CheckCombatUnitSetup();
            
            // Check detection system
            CheckDetectionSystem();
            
            // Check for common issues
            CheckCommonIssues();
            
            Debug.Log("=== END DIAGNOSTICS ===");
        }
        
        private void TestSystemInitialization()
        {
            Debug.Log("--- SYSTEM INITIALIZATION TEST ---");
            
            // Test SystemInitializer from DependencyContainer, fallback to FindObjectOfType
            var systemInitializer = DependencyContainer.Instance.TryResolve<SystemInitializer>();
            if (systemInitializer == null)
            {
                systemInitializer = FindObjectOfType<SystemInitializer>();
            }
            if (systemInitializer != null)
            {
                Debug.Log("✅ SystemInitializer found in scene");
            }
            else
            {
                Debug.LogError("❌ SystemInitializer NOT found in scene!");
            }
            
            // Test CentralizedDetectionManager
            var detectionManager = MyGame.Core.SystemInitializer.GetSystem<ICentralizedCombatDetection>();
            if (detectionManager != null)
            {
                Debug.Log("✅ CentralizedDetectionManager is initialized and available");
                Debug.Log($"   - Registered units: {detectionManager.GetType().GetField("registeredCombatUnits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(detectionManager) as List<CombatUnit> ?? new List<CombatUnit>()}");
            }
            else
            {
                Debug.LogWarning("⚠️ CentralizedDetectionManager is NOT initialized - using fallback detection");
            }
            
            // Test LightweightFireSystem
            var fireSystem = MyGame.Core.SystemInitializer.GetSystem<MyGame.RuntimeSystems.Combat.ICombatFireCoordinator>();
            if (fireSystem != null)
            {
                Debug.Log("✅ LightweightFireSystem is initialized and available");
            }
            else
            {
                Debug.LogWarning("⚠️ LightweightFireSystem is NOT initialized");
            }
            
            // Test UnitPoolManager
            var unitPoolManager = MyGame.Core.SystemInitializer.GetSystem<MyGame.Core.UnitPoolManager>();
            if (unitPoolManager != null)
            {
                Debug.Log("✅ UnitPoolManager is initialized and available");
            }
            else
            {
                Debug.LogWarning("⚠️ UnitPoolManager is NOT initialized");
            }
        }
        
        private void CheckTeamAssignments()
        {
            Debug.Log("--- TEAM ASSIGNMENTS ---");
            
            int unitsWithTeams = 0;
            int unitsWithoutTeams = 0;
            
            foreach (var unit in allUnits)
            {
                Team team = unit.GetTeam();
                Player owner = unit.Owner;
                
                Debug.Log($"Unit: {unit.name} | Team: {team} | Owner: {owner?.Name ?? "NULL"} | Owner Team: {owner?.Team ?? Team.None}");
                
                if (team == Team.None || owner == null)
                {
                    Debug.LogError($"CRITICAL: Unit {unit.name} has no team assignment!");
                    unitsWithoutTeams++;
                }
                else
                {
                    unitsWithTeams++;
                }
            }
            
            Debug.Log($"Team Assignment Summary: {unitsWithTeams} units with teams, {unitsWithoutTeams} units without teams");
        }
        
        private void CheckCombatUnitSetup()
        {
            Debug.Log("--- COMBAT UNIT SETUP ---");
            
            foreach (var combatUnit in combatUnits)
            {
                Debug.Log($"Combat Unit: {combatUnit.Name} ({combatUnit.gameObject.name})");
                Debug.Log($"  - Detection Radius: {combatUnit.DetectionRadius}m");
                Debug.Log($"  - Enemy Layer Mask: {combatUnit.EnemyLayerMask.value}");
                Debug.Log($"  - Position: {combatUnit.Position}");
                Debug.Log($"  - Team: {combatUnit.GetTeam()}");
                Debug.Log($"  - Current Target: {(combatUnit.CurrentTarget != null ? combatUnit.CurrentTarget.Name : "None")}");
                Debug.Log($"  - In Combat: {combatUnit.IsInCombat}");
                Debug.Log($"  - Target In Range: {combatUnit.IsTargetInRange}");
                Debug.Log($"  - Gun Facing Target: {combatUnit.IsGunFacingTarget}");
                
                // Check if unit has required components
                Unit unitComponent = combatUnit.GetComponent<Unit>();
                if (unitComponent == null)
                {
                    Debug.LogError($"  - MISSING: Unit component!");
                }
                
                Collider collider = combatUnit.GetComponent<Collider>();
                if (collider == null)
                {
                    Debug.LogError($"  - MISSING: Collider component!");
                }
                
                Debug.Log("  ---");
            }
        }
        
        private void CheckDetectionSystem()
        {
            Debug.Log("--- DETECTION SYSTEM TEST ---");
            
            foreach (var combatUnit in combatUnits)
            {
                Debug.Log($"Testing detection for {combatUnit.Name}:");
                
                // Test Physics.OverlapSphere
                if (testWithPhysicsOverlap)
                {
                    Collider[] collidersInRange = Physics.OverlapSphere(combatUnit.Position, testDetectionRadius, combatUnit.EnemyLayerMask);
                    Debug.Log($"  - Physics.OverlapSphere (radius {testDetectionRadius}m): Found {collidersInRange.Length} colliders");
                    
                    foreach (var collider in collidersInRange)
                    {
                        // Use GetComponentInParent to find IUnit even if collider is on a child object
                        IUnit unit = collider.GetComponentInParent<IUnit>();
                        if (unit != null)
                        {
                            float distance = Vector3.Distance(combatUnit.Position, unit.Position);
                            Debug.Log($"    Unit: {unit.Name} at {distance:F1}m, Team: {unit.Owner?.Team}");
                            
                            bool isValid = combatUnit.IsValidTarget(unit);
                            Debug.Log($"    Valid target: {isValid}");
                            
                            if (!isValid)
                            {
                                if (unit.Health <= 0)
                                    Debug.Log($"      Invalid: No health ({unit.Health})");
                                else if (combatUnit.Owner?.Team == unit.Owner?.Team)
                                    Debug.Log($"      Invalid: Same team ({unit.Owner?.Team})");
                                else if (unit == combatUnit)
                                    Debug.Log($"      Invalid: Targeting self");
                                else
                                    Debug.Log($"      Invalid: Unknown reason");
                            }
                        }
                        else
                        {
                            Debug.Log($"    Collider without IUnit: {collider.name}");
                        }
                    }
                }
                
                Debug.Log("  ---");
            }
        }
        
        private void CheckCommonIssues()
        {
            Debug.Log("--- COMMON ISSUES CHECK ---");
            
            // Check for units without teams
            int unitsWithoutTeams = 0;
            foreach (var unit in allUnits)
            {
                if (unit.GetTeam() == Team.None)
                {
                    unitsWithoutTeams++;
                }
            }
            
            if (unitsWithoutTeams > 0)
            {
                Debug.LogError($"ISSUE: {unitsWithoutTeams} units have no team assignment!");
                Debug.LogError("SOLUTION: Call AssignTeamsForTesting() or manually assign teams");
            }
            
            // Check for units without colliders
            int unitsWithoutColliders = 0;
            foreach (var unit in allUnits)
            {
                if (unit.GetComponent<Collider>() == null)
                {
                    unitsWithoutColliders++;
                }
            }
            
            if (unitsWithoutColliders > 0)
            {
                Debug.LogError($"ISSUE: {unitsWithoutColliders} units have no Collider component!");
                Debug.LogError("SOLUTION: Add Collider components to all units");
            }
            
            // Check for units too far apart
            if (allUnits.Count >= 2)
            {
                float maxDistance = 0f;
                Unit unit1 = null, unit2 = null;
                
                for (int i = 0; i < allUnits.Count; i++)
                {
                    for (int j = i + 1; j < allUnits.Count; j++)
                    {
                        float distance = Vector3.Distance(allUnits[i].Position, allUnits[j].Position);
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            unit1 = allUnits[i];
                            unit2 = allUnits[j];
                        }
                    }
                }
                
                Debug.Log($"Maximum distance between units: {maxDistance:F1}m ({unit1?.name} to {unit2?.name})");
                
                if (maxDistance > 50f)
                {
                    Debug.LogWarning($"WARNING: Units are very far apart ({maxDistance:F1}m). Consider moving them closer for testing.");
                }
            }
        }
        
        private void AssignTeamsForTesting()
        {
            Debug.Log("🔍 CombatDetectionDebugger: Assigning teams for testing...");
            
            Unit[] units = FindObjectsOfType<Unit>();
            
            for (int i = 0; i < units.Length; i++)
            {
                Team team = (i % 2 == 0) ? Team.Player : Team.AI;
                units[i].SetTeam(team);
                Debug.Log($"Assigned {units[i].name} to {team} team");
            }
            
            Debug.Log($"🔍 CombatDetectionDebugger: Teams assigned to {units.Length} units");
            
            // Refresh unit lists and run diagnostics
            CollectUnits();
            RunComprehensiveDiagnostics();
        }
        
        private void AssignUnityTagsToAllUnits()
        {
            Debug.Log("🔍 CombatDetectionDebugger: Assigning Unity tags to all units...");
            
            Unit[] units = FindObjectsOfType<Unit>();
            
            for (int i = 0; i < units.Length; i++)
            {
                Team team = units[i].GetTeam();
                if (team == Team.Player)
                {
                    units[i].gameObject.tag = "Player";
                    Debug.Log($"Assigned 'Player' tag to {units[i].name}");
                }
                else if (team == Team.AI)
                {
                    units[i].gameObject.tag = "AI";
                    Debug.Log($"Assigned 'AI' tag to {units[i].name}");
                }
                else
                {
                    Debug.LogWarning($"Cannot assign tag to {units[i].name} - no team assigned");
                }
            }
            
            Debug.Log($"🔍 CombatDetectionDebugger: Unity tags assigned to {units.Length} units");
        }
        
        private void ForceDetectionTest()
        {
            Debug.Log("🔍 CombatDetectionDebugger: Forcing detection test...");
            
            foreach (var combatUnit in combatUnits)
            {
                // Force update target detection
                combatUnit.SendMessage("UpdateTargetDetection", SendMessageOptions.DontRequireReceiver);
                
                Debug.Log($"Forced detection update for {combatUnit.Name}");
            }
            
            // Run diagnostics after forced update
            RunComprehensiveDiagnostics();
        }
        
        private void UpdateRealTimeStatus()
        {
            if (!showRealTimeStatus) return;
            
            int unitsInCombat = 0;
            int unitsWithTargets = 0;
            int unitsReadyToAttack = 0;
            
            foreach (var combatUnit in combatUnits)
            {
                if (combatUnit.IsInCombat) unitsInCombat++;
                if (combatUnit.CurrentTarget != null) unitsWithTargets++;
                if (combatUnit.IsGunFacingTarget && combatUnit.IsTargetInRange) unitsReadyToAttack++;
            }
            
            if (logDetectionIssues)
            {
                if (unitsWithTargets == 0 && allUnits.Count > 1)
                {
                    Debug.LogWarning($"🔍 CombatDetectionDebugger: No units have targets! Check team assignments and detection radius.");
                }
                
                if (unitsReadyToAttack == 0 && unitsWithTargets > 0)
                {
                    Debug.LogWarning($"🔍 CombatDetectionDebugger: {unitsWithTargets} units have targets but none are ready to attack!");
                }
            }
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 250));
            GUILayout.Label("Combat Detection Debugger", GUI.skin.box);
            
            GUILayout.Label($"Combat Units: {combatUnits.Count}");
            GUILayout.Label($"Total Units: {allUnits.Count}");
            
            int unitsInCombat = 0;
            int unitsWithTargets = 0;
            int unitsReadyToAttack = 0;
            
            foreach (var combatUnit in combatUnits)
            {
                if (combatUnit.IsInCombat) unitsInCombat++;
                if (combatUnit.CurrentTarget != null) unitsWithTargets++;
                if (combatUnit.IsGunFacingTarget && combatUnit.IsTargetInRange) unitsReadyToAttack++;
            }
            
            GUILayout.Label($"In Combat: {unitsInCombat}");
            GUILayout.Label($"With Targets: {unitsWithTargets}");
            GUILayout.Label($"Ready to Attack: {unitsReadyToAttack}");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Run Diagnostics (F1)"))
            {
                RunComprehensiveDiagnostics();
            }
            
            if (GUILayout.Button("Assign Teams (F2)"))
            {
                AssignTeamsForTesting();
            }
            
            if (GUILayout.Button("Assign Tags (F3)"))
            {
                AssignUnityTagsToAllUnits();
            }
            
            if (GUILayout.Button("Force Detection (F4)"))
            {
                ForceDetectionTest();
            }
            
            if (GUILayout.Button("Test Systems (F5)"))
            {
                TestSystemInitialization();
            }
            
            GUILayout.EndArea();
        }
    }
}

