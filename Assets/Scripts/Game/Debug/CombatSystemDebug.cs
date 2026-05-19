using UnityEngine;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;
using MyGame.Game;
using System.Collections.Generic;
using MyGame.Presentation;
using MyGame.RuntimeSystems.Combat;

namespace MyGame.Game
{
    /// <summary>
    /// Comprehensive test script for debugging combat system issues
    /// Provides detailed information about unit states, team assignments, and combat conditions
    /// </summary>
    public class CombatSystemDebug : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showCombatStatus = true;
        [SerializeField] private bool showTeamAssignments = true;
        [SerializeField] private bool showAttackConditions = true;
        
        [Header("Test Controls")]
        [SerializeField] private KeyCode debugKey = KeyCode.F1;
        [SerializeField] private KeyCode forceAttackKey = KeyCode.F2;
        [SerializeField] private KeyCode assignTeamsKey = KeyCode.F3;
        
        private List<CombatUnit> combatUnits = new List<CombatUnit>();
        private List<Unit> allUnits = new List<Unit>();
        
        private void Start()
        {
            Debug.Log("CombatSystemTest: Starting combat system diagnostics...");
            CollectUnits();
        }
        
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(debugKey))
            {
                RunComprehensiveDiagnostics();
            }
            
            if (UnityEngine.Input.GetKeyDown(forceAttackKey))
            {
                ForceAttackTest();
            }
            
            if (UnityEngine.Input.GetKeyDown(assignTeamsKey))
            {
                AssignTeamsForTesting();
            }
            
            if (showCombatStatus)
            {
                UpdateCombatStatus();
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
            
            Debug.Log($"CombatSystemTest: Found {combatUnits.Count} combat units and {allUnits.Count} total units");
        }
        
        private void RunComprehensiveDiagnostics()
        {
            Debug.Log("=== COMBAT SYSTEM DIAGNOSTICS ===");
            
            // Check team assignments
            CheckTeamAssignments();
            
            // Check combat unit states
            CheckCombatUnitStates();
            
            // Check attack conditions
            CheckAttackConditions();
            
            // Check for conflicts
            CheckSystemConflicts();
            
            Debug.Log("=== END DIAGNOSTICS ===");
        }
        
        private void CheckTeamAssignments()
        {
            Debug.Log("--- TEAM ASSIGNMENTS ---");
            
            foreach (var unit in allUnits)
            {
                Team team = unit.GetTeam();
                Player owner = unit.Owner;
                
                Debug.Log($"Unit: {unit.name} | Team: {team} | Owner: {owner?.Name ?? "NULL"} | Owner Team: {owner?.Team ?? Team.None}");
                
                if (team == Team.None || owner == null)
                {
                    Debug.LogError($"CRITICAL: Unit {unit.name} has no team assignment!");
                }
            }
        }
        
        private void CheckCombatUnitStates()
        {
            Debug.Log("--- COMBAT UNIT STATES ---");
            
            foreach (var combatUnit in combatUnits)
            {
                Debug.Log($"CombatUnit: {combatUnit.Name}");
                Debug.Log($"  - IsInCombat: {combatUnit.IsInCombat}");
                Debug.Log($"  - CurrentTarget: {combatUnit.CurrentTarget?.Name ?? "NULL"}");
                Debug.Log($"  - IsTargetInRange: {combatUnit.IsTargetInRange}");
                Debug.Log($"  - IsGunFacingTarget: {combatUnit.IsGunFacingTarget}");
                Debug.Log($"  - Team: {combatUnit.GetTeam()}");
                Debug.Log($"  - AttackRange: {combatUnit.AttackRange}");
                Debug.Log($"  - AttackCooldown: {combatUnit.AttackCooldown}");
                Debug.Log($"  - LastAttackTime: {combatUnit.LastAttackTime}");
                Debug.Log($"  - Time since last attack: {Time.time - combatUnit.LastAttackTime:F1}s");
                
                if (combatUnit.CurrentTarget != null)
                {
                    float distance = Vector3.Distance(combatUnit.Position, combatUnit.CurrentTarget.Position);
                    Debug.Log($"  - Distance to target: {distance:F1}m");
                    Debug.Log($"  - Target team: {combatUnit.CurrentTarget.Owner?.Team ?? Team.None}");
                    Debug.Log($"  - Target health: {combatUnit.CurrentTarget.Health}");
                }
                
                Debug.Log("  ---");
            }
        }
        
        private void CheckAttackConditions()
        {
            Debug.Log("--- ATTACK CONDITIONS ---");
            
            foreach (var combatUnit in combatUnits)
            {
                if (combatUnit.CurrentTarget == null) continue;
                
                Debug.Log($"Testing attack conditions for {combatUnit.Name} -> {combatUnit.CurrentTarget.Name}:");
                
                // Check if target is valid
                bool targetValid = combatUnit.IsValidTarget(combatUnit.CurrentTarget);
                Debug.Log($"  - Target valid: {targetValid}");
                
                // Check range
                float distance = Vector3.Distance(combatUnit.Position, combatUnit.CurrentTarget.Position);
                bool inRange = distance <= combatUnit.AttackRange;
                Debug.Log($"  - In range: {inRange} ({distance:F1}m <= {combatUnit.AttackRange:F1}m)");
                
                // Check cooldown
                bool cooldownReady = (Time.time - combatUnit.LastAttackTime) >= combatUnit.AttackCooldown;
                Debug.Log($"  - Cooldown ready: {cooldownReady} ({(Time.time - combatUnit.LastAttackTime):F1}s >= {combatUnit.AttackCooldown:F1}s)");
                
                // Check gun facing
                bool gunFacing = combatUnit.IsGunFacingTarget;
                Debug.Log($"  - Gun facing target: {gunFacing}");
                
                // Check CanAttack
                bool canAttack = combatUnit.CanAttack(combatUnit.CurrentTarget);
                Debug.Log($"  - CanAttack returns: {canAttack}");
                
                Debug.Log("  ---");
            }
        }
        
        private void CheckSystemConflicts()
        {
            Debug.Log("--- SYSTEM CONFLICTS ---");
            
            // Check for multiple combat systems
            FireSystem[] fireSystems = FindObjectsOfType<FireSystem>();
            if (fireSystems.Length > 1)
            {
                Debug.LogWarning($"Found {fireSystems.Length} FireSystem instances - this may cause conflicts!");
            }
            
            // Check for units without proper components
            foreach (var unit in allUnits)
            {
                CombatUnit combatUnit = unit.GetComponent<CombatUnit>();
                if (combatUnit == null)
                {
                    Debug.LogWarning($"Unit {unit.name} has no CombatUnit component!");
                }
                
                GunTurret gunTurret = unit.GetComponentInChildren<GunTurret>();
                if (gunTurret == null)
                {
                    Debug.LogWarning($"Unit {unit.name} has no GunTurret component!");
                }
            }
            
            // Test detection specifically
            TestDetectionSystem();
        }
        
        private void TestDetectionSystem()
        {
            Debug.Log("--- DETECTION SYSTEM TEST ---");
            
            foreach (var combatUnit in combatUnits)
            {
                Debug.Log($"Testing detection for {combatUnit.Name}:");
                Debug.Log($"  - Detection Radius: {combatUnit.DetectionRadius}m");
                Debug.Log($"  - Enemy Layer Mask: {combatUnit.EnemyLayerMask.value}");
                Debug.Log($"  - Position: {combatUnit.Position}");
                Debug.Log($"  - Team: {combatUnit.GetTeam()}");
                
                // Test Physics.OverlapSphere directly
                Collider[] collidersInRange = Physics.OverlapSphere(combatUnit.Position, combatUnit.DetectionRadius, combatUnit.EnemyLayerMask);
                Debug.Log($"  - Physics.OverlapSphere found {collidersInRange.Length} colliders");
                
                foreach (var collider in collidersInRange)
                {
                    // Use GetComponentInParent to find IUnit even if collider is on a child object
                    IUnit unit = collider.GetComponentInParent<IUnit>();
                    if (unit != null)
                    {
                        float distance = Vector3.Distance(combatUnit.Position, unit.Position);
                        Debug.Log($"    - Found unit: {unit.Name} at {distance:F1}m, Team: {unit.Owner?.Team}");
                        
                        bool isValidTarget = combatUnit.IsValidTarget(unit);
                        Debug.Log($"    - IsValidTarget: {isValidTarget}");
                        
                        if (!isValidTarget)
                        {
                            // Check why it's not valid
                            if (unit.Health <= 0)
                                Debug.Log($"      - Reason: Unit has no health ({unit.Health})");
                            else if (combatUnit.Owner?.Team == unit.Owner?.Team)
                                Debug.Log($"      - Reason: Same team ({unit.Owner?.Team})");
                            else if (unit == combatUnit)
                                Debug.Log($"      - Reason: Targeting self");
                            else
                                Debug.Log($"      - Reason: Unknown validation failure");
                        }
                    }
                    else
                    {
                        Debug.Log($"    - Found collider without IUnit: {collider.name}");
                    }
                }
                
                Debug.Log("  ---");
            }
        }
        
        private void ForceAttackTest()
        {
            Debug.Log("=== FORCE ATTACK TEST ===");
            
            foreach (var combatUnit in combatUnits)
            {
                if (combatUnit.CurrentTarget != null)
                {
                    Debug.Log($"Forcing attack for {combatUnit.Name} on {combatUnit.CurrentTarget.Name}");
                    bool result = combatUnit.TryAttack();
                    Debug.Log($"Force attack result: {result}");
                }
            }
        }
        
        private void AssignTeamsForTesting()
        {
            Debug.Log("=== ASSIGNING TEAMS FOR TESTING ===");
            
            // Assign teams alternately
            for (int i = 0; i < allUnits.Count; i++)
            {
                Team team = (i % 2 == 0) ? Team.Player : Team.AI;
                allUnits[i].SetTeam(team);
                Debug.Log($"Assigned {allUnits[i].name} to {team} team");
            }
        }
        
        private void UpdateCombatStatus()
        {
            // This would update UI elements if needed
            // For now, just log occasionally
            if (Time.frameCount % 300 == 0) // Every 5 seconds
            {
                int unitsInCombat = 0;
                int unitsWithTargets = 0;
                
                foreach (var combatUnit in combatUnits)
                {
                    if (combatUnit.IsInCombat) unitsInCombat++;
                    if (combatUnit.CurrentTarget != null) unitsWithTargets++;
                }
                
                Debug.Log($"Combat Status: {unitsInCombat}/{combatUnits.Count} units in combat, {unitsWithTargets} with targets");
            }
        }
        
        private void OnGUI()
        {
            if (!showCombatStatus) return;
            
            GUILayout.BeginArea(new Rect(10, 250, 400, 300));
            GUILayout.Label("Combat System Debug", GUI.skin.box);
            
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
            
            if (GUILayout.Button("Force Attack Test (F2)"))
            {
                ForceAttackTest();
            }
            
            if (GUILayout.Button("Assign Teams (F3)"))
            {
                AssignTeamsForTesting();
            }
            
            GUILayout.EndArea();
        }
    }
}
