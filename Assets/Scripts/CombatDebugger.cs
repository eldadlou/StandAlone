using UnityEngine;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;
using MyGame.Game;
using MyGame.RuntimeSystems.Combat;
using System.Collections.Generic;

namespace MyGame.Game
{
    /// <summary>
    /// Comprehensive debug script to diagnose why tanks aren't attacking each other
    /// </summary>
    public class CombatDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showRealTimeStatus = true;
        [SerializeField] private float debugInterval = 2f;
        
        [Header("Test Controls")]
        [SerializeField] private KeyCode runDiagnosticsKey = KeyCode.F1;
        [SerializeField] private KeyCode forceDetectionKey = KeyCode.F2;
        [SerializeField] private KeyCode assignTeamsKey = KeyCode.F3;
        [SerializeField] private KeyCode testAttackKey = KeyCode.F4;
        
        private List<CombatUnit> combatUnits = new List<CombatUnit>();
        private List<Unit> allUnits = new List<Unit>();
        private float lastDebugTime;
        
        private void Start()
        {
            // Debug.Log("🔍 CombatDebugger: Starting combat diagnostics...");
            CollectUnits();
            RunComprehensiveDiagnostics();
        }
        
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(runDiagnosticsKey))
            {
                RunComprehensiveDiagnostics();
            }
            
            if (UnityEngine.Input.GetKeyDown(forceDetectionKey))
            {
                ForceDetectionTest();
            }
            
            if (UnityEngine.Input.GetKeyDown(assignTeamsKey))
            {
                AssignTeamsForTesting();
            }
            
            if (UnityEngine.Input.GetKeyDown(testAttackKey))
            {
                TestAttackSystem();
            }
            
            // Periodic debug updates
            if (Time.time - lastDebugTime > debugInterval)
            {
                if (showRealTimeStatus)
                {
                    UpdateRealTimeStatus();
                }
                lastDebugTime = Time.time;
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
            
            // Debug.Log($"🔍 CombatDebugger: Found {combatUnits.Count} combat units and {allUnits.Count} total units");
        }
        
        private void RunComprehensiveDiagnostics()
        {
            // Debug.Log("=== COMBAT SYSTEM COMPREHENSIVE DIAGNOSTICS ===");
            
            CollectUnits();
            
            // 1. Check system initialization
            CheckSystemInitialization();
            
            // 2. Check team assignments
            CheckTeamAssignments();
            
            // 3. Check layer assignments
            CheckLayerAssignments();
            
            // 4. Check detection system
            CheckDetectionSystem();
            
            // 5. Check combat state
            CheckCombatState();
            
            // 6. Check attack conditions
            CheckAttackConditions();
            
            // Debug.Log("=== END DIAGNOSTICS ===");
        }
        
        private void CheckSystemInitialization()
        {
            // Debug.Log("--- SYSTEM INITIALIZATION ---");
            
            // Check CentralizedDetectionManager
            var detectionManager = MyGame.Core.SystemInitializer.GetSystem<ICentralizedCombatDetection>();
            if (detectionManager != null)
            {
                // Debug.Log("✅ CentralizedDetectionManager is running");
            }
            else
            {
                // Debug.LogWarning("⚠️ CentralizedDetectionManager is NOT running - using fallback detection");
            }
            
            // Check LightweightFireSystem
            var fireSystem = MyGame.Core.SystemInitializer.GetSystem<MyGame.RuntimeSystems.Combat.ICombatFireCoordinator>();
            if (fireSystem != null)
            {
                // Debug.Log("✅ LightweightFireSystem is running");
            }
            else
            {
                // Debug.LogWarning("⚠️ LightweightFireSystem is NOT running");
            }
        }
        
        private void CheckTeamAssignments()
        {
            // Debug.Log("--- TEAM ASSIGNMENTS ---");
            
            foreach (var unit in allUnits)
            {
                Team team = unit.GetTeam();
                Player owner = unit.Owner;
                
                // Debug.Log($"Unit: {unit.name} | Team: {team} | Owner: {owner?.Name ?? "NULL"} | Owner Team: {owner?.Team ?? Team.None}");
                
                if (team == Team.None || owner == null)
                {
                    // Debug.LogError($"❌ CRITICAL: Unit {unit.name} has no team assignment!");
                }
            }
        }
        
        private void CheckLayerAssignments()
        {
            // Debug.Log("--- LAYER ASSIGNMENTS ---");
            
            foreach (var combatUnit in combatUnits)
            {
                int layer = combatUnit.gameObject.layer;
                Team team = combatUnit.GetTeam();
                
                // Debug.Log($"Combat Unit: {combatUnit.Name} | Layer: {layer} | Team: {team} | Enemy Mask: {combatUnit.EnemyLayerMask.value}");
                
                // Check if layer matches team
                int expectedLayer = team == Team.Player ? 6 : 7;
                if (layer != expectedLayer)
                {
                    // Debug.LogError($"❌ Layer mismatch: {combatUnit.Name} is on layer {layer} but should be on layer {expectedLayer} for team {team}");
                }
            }
        }
        
        private void CheckDetectionSystem()
        {
            // Debug.Log("--- DETECTION SYSTEM ---");
            
            foreach (var combatUnit in combatUnits)
            {
                // Debug.Log($"Testing detection for {combatUnit.Name}:");
                
                // Test Physics.OverlapSphere
                Collider[] colliders = Physics.OverlapSphere(combatUnit.Position, combatUnit.DetectionRadius, combatUnit.EnemyLayerMask);
                // Debug.Log($"  - Physics.OverlapSphere found {colliders.Length} colliders in range");
                
                foreach (var collider in colliders)
                {
                    // Use GetComponentInParent to find IUnit even if collider is on a child object
                    IUnit unit = collider.GetComponentInParent<IUnit>();
                    if (unit != null)
                    {
                        float distance = Vector3.Distance(combatUnit.Position, unit.Position);
                        bool isEnemy = combatUnit.Owner?.Team != unit.Owner?.Team;
                        bool isValid = combatUnit.IsValidTarget(unit);
                        
                        // Debug.Log($"    {unit.Name} at {distance:F1}m - Enemy: {isEnemy}, Valid: {isValid}");
                        
                        // if (!isValid)
                        // {
                        //     if (unit.Health <= 0)
                        //         // Debug.Log($"      Invalid: No health ({unit.Health})");
                        //     else if (combatUnit.Owner?.Team == unit.Owner?.Team)
                        //         // Debug.Log($"      Invalid: Same team ({unit.Owner?.Team})");
                        //     else if (unit == combatUnit)
                        //         // Debug.Log($"      Invalid: Targeting self");
                        //     else
                        //         // Debug.Log($"      Invalid: Unknown reason");
                        // }
                    }
                }
            }
        }
        
        private void CheckCombatState()
        {
            // Debug.Log("--- COMBAT STATE ---");
            
            foreach (var combatUnit in combatUnits)
            {
                // Debug.Log($"Combat Unit: {combatUnit.Name}");
                // Debug.Log($"  - In Combat: {combatUnit.IsInCombat}");
                // Debug.Log($"  - Current Target: {(combatUnit.CurrentTarget != null ? combatUnit.CurrentTarget.Name : "None")}");
                // Debug.Log($"  - Target In Range: {combatUnit.IsTargetInRange}");
                // Debug.Log($"  - Gun Facing Target: {combatUnit.IsGunFacingTarget}");
                
                if (combatUnit.CurrentTarget != null)
                {
                    float distance = Vector3.Distance(combatUnit.Position, combatUnit.CurrentTarget.Position);
                    // Debug.Log($"  - Distance to Target: {distance:F1}m");
                    // Debug.Log($"  - Attack Range: {combatUnit.AttackRange:F1}m");
                    // Debug.Log($"  - Attack Cooldown: {combatUnit.AttackCooldown:F1}s");
                }
            }
        }
        
        private void CheckAttackConditions()
        {
            // Debug.Log("--- ATTACK CONDITIONS ---");
            
            foreach (var combatUnit in combatUnits)
            {
                if (combatUnit.CurrentTarget != null)
                {
                    // Debug.Log($"Testing attack conditions for {combatUnit.Name} -> {combatUnit.CurrentTarget.Name}:");
                    
                    // Test CanAttack
                    bool canAttack = combatUnit.CanAttack(combatUnit.CurrentTarget);
                    // Debug.Log($"  - CanAttack: {canAttack}");
                    
                    // Test TryAttack
                    bool tryAttack = combatUnit.TryAttack();
                    // Debug.Log($"  - TryAttack: {tryAttack}");
                    
                    // Check cooldown
                    float timeSinceLastAttack = Time.time - combatUnit.LastAttackTime;
                    bool cooldownReady = timeSinceLastAttack >= combatUnit.AttackCooldown;
                    // Debug.Log($"  - Cooldown Ready: {cooldownReady} (Last attack: {timeSinceLastAttack:F1}s ago)");
                }
            }
        }
        
        private void AssignTeamsForTesting()
        {
            // Debug.Log("🔍 CombatDebugger: Assigning teams for testing...");
            
            Unit[] units = FindObjectsOfType<Unit>();
            
            for (int i = 0; i < units.Length; i++)
            {
                Team team = (i % 2 == 0) ? Team.Player : Team.AI;
                units[i].SetTeam(team);
                // Debug.Log($"Assigned {units[i].name} to {team} team");
            }
            
            // Debug.Log($"🔍 CombatDebugger: Teams assigned to {units.Length} units");
            
            // Refresh and run diagnostics
            CollectUnits();
            RunComprehensiveDiagnostics();
        }
        
        private void ForceDetectionTest()
        {
            // Debug.Log("🔍 CombatDebugger: Forcing detection test...");
            
            foreach (var combatUnit in combatUnits)
            {
                // Force update target detection
                combatUnit.SendMessage("UpdateTargetDetection", SendMessageOptions.DontRequireReceiver);
                // Debug.Log($"Forced detection update for {combatUnit.Name}");
            }
            
            // Run diagnostics after forced update
            RunComprehensiveDiagnostics();
        }
        
        private void TestAttackSystem()
        {
            // Debug.Log("🔍 CombatDebugger: Testing attack system...");
            
            foreach (var combatUnit in combatUnits)
            {
                if (combatUnit.CurrentTarget != null)
                {
                    // Debug.Log($"Testing attack for {combatUnit.Name} -> {combatUnit.CurrentTarget.Name}");
                    
                    // Force attack
                    bool result = combatUnit.TryAttack();
                    // Debug.Log($"Attack result: {result}");
                }
            }
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
            
            if (unitsWithTargets == 0 && allUnits.Count > 1)
            {
                // Debug.LogWarning($"🔍 CombatDebugger: No units have targets! Check team assignments and detection radius.");
            }
            
            if (unitsReadyToAttack == 0 && unitsWithTargets > 0)
            {
                // Debug.LogWarning($"🔍 CombatDebugger: {unitsWithTargets} units have targets but none are ready to attack!");
            }
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            GUILayout.Label("Combat Debugger", GUI.skin.box);
            
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
            
            if (GUILayout.Button("Assign Teams (F3)"))
            {
                AssignTeamsForTesting();
            }
            
            if (GUILayout.Button("Force Detection (F2)"))
            {
                ForceDetectionTest();
            }
            
            if (GUILayout.Button("Test Attack (F4)"))
            {
                TestAttackSystem();
            }
            
            GUILayout.EndArea();
        }
    }
}
