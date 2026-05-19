using UnityEngine;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;
using MyGame.Game;

namespace MyGame.Game
{
    /// <summary>
    /// Simple debug script to test detection issues
    /// Attach this to any GameObject in the scene to run detection tests
    /// </summary>
    public class DetectionDebug : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private KeyCode testKey = KeyCode.F1;
        [SerializeField] private KeyCode assignTeamsKey = KeyCode.F2;
        
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(testKey))
            {
                RunDetectionTest();
            }
            
            if (UnityEngine.Input.GetKeyDown(assignTeamsKey))
            {
                AssignTeamsForTesting();
            }
        }
        
        private void RunDetectionTest()
        {
            Debug.Log("=== DETECTION DEBUG TEST ===");
            
            // Find all combat units
            CombatUnit[] combatUnits = FindObjectsOfType<CombatUnit>();
            Unit[] allUnits = FindObjectsOfType<Unit>();
            
            Debug.Log($"Found {combatUnits.Length} combat units and {allUnits.Length} total units");
            
            if (combatUnits.Length == 0)
            {
                Debug.LogError("No CombatUnit components found! Make sure vehicles have VehicleCombatUnit components.");
                return;
            }
            
            // Check team assignments
            Debug.Log("--- TEAM ASSIGNMENTS ---");
            foreach (var unit in allUnits)
            {
                Team team = unit.GetTeam();
                Debug.Log($"Unit: {unit.name} | Team: {team} | Owner: {unit.Owner?.Name ?? "NULL"}");
                
                if (team == Team.None)
                {
                    Debug.LogError($"Unit {unit.name} has no team assignment!");
                }
            }
            
            // Test detection for each combat unit
            Debug.Log("--- DETECTION TEST ---");
            foreach (var combatUnit in combatUnits)
            {
                Debug.Log($"Testing {combatUnit.Name}:");
                Debug.Log($"  Position: {combatUnit.Position}");
                Debug.Log($"  Team: {combatUnit.GetTeam()}");
                Debug.Log($"  Detection Radius: {combatUnit.DetectionRadius}m");
                Debug.Log($"  Layer Mask: {combatUnit.EnemyLayerMask.value}");
                
                // Test Physics.OverlapSphere
                Collider[] colliders = Physics.OverlapSphere(combatUnit.Position, combatUnit.DetectionRadius, combatUnit.EnemyLayerMask);
                Debug.Log($"  Found {colliders.Length} colliders in range");
                
                foreach (var collider in colliders)
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
                        }
                    }
                    else
                    {
                        Debug.Log($"    Collider without IUnit: {collider.name}");
                    }
                }
                
                Debug.Log("  ---");
            }
            
            Debug.Log("=== END DETECTION TEST ===");
        }
        
        private void AssignTeamsForTesting()
        {
            Debug.Log("=== ASSIGNING TEAMS ===");
            
            Unit[] allUnits = FindObjectsOfType<Unit>();
            
            for (int i = 0; i < allUnits.Length; i++)
            {
                Team team = (i % 2 == 0) ? Team.Player : Team.AI;
                allUnits[i].SetTeam(team);
                Debug.Log($"Assigned {allUnits[i].name} to {team} team");
            }
            
            Debug.Log("=== TEAMS ASSIGNED ===");
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label("Detection Debug Test", GUI.skin.box);
            
            if (GUILayout.Button($"Run Detection Test ({testKey})"))
            {
                RunDetectionTest();
            }
            
            if (GUILayout.Button($"Assign Teams ({assignTeamsKey})"))
            {
                AssignTeamsForTesting();
            }
            
            GUILayout.Label("Check Console for detailed output");
            GUILayout.EndArea();
        }
    }
}
