using UnityEngine;
using MyGame.Core.Units;
using MyGame.Presentation;
// using MyGame.Game;

namespace MyGame.Game
{
    /// <summary>
    /// Test script to demonstrate proper unit setup with team assignment
    /// </summary>
    public class UnitSetupTest : MonoBehaviour
    {
        [Header("Unit Setup")]
        public GameObject playerUnitPrefab;
        public GameObject aiUnitPrefab;
        
        [Header("Spawn Settings")]
        public Vector3 playerSpawnPosition = new Vector3(0, 0, 0);
        public Vector3 aiSpawnPosition = new Vector3(10, 0, 10);
        
        [Header("Test Controls")]
        public bool spawnUnitsOnStart = true;
        public KeyCode spawnPlayerUnitKey = KeyCode.P;
        public KeyCode spawnAIUnitKey = KeyCode.A;
        public KeyCode testAttackKey = KeyCode.T;
        public KeyCode testSelectionKey = KeyCode.S;
        public KeyCode testDestinationKey = KeyCode.D;
        
        private Unit playerUnit;
        private Unit aiUnit;
        private SelectionManager selectionManager;
        
        private void Start()
        {
            // Get selection manager
            selectionManager = FindFirstObjectByType<SelectionManager>();
            if (selectionManager == null)
            {
                Debug.LogWarning("SelectionManager not found in scene!");
            }
            
            if (spawnUnitsOnStart)
            {
                SpawnTestUnits();
            }
        }
        
        private void Update()
        {
            // Spawn player unit
            if (UnityEngine.Input.GetKeyDown(spawnPlayerUnitKey))
            {
                SpawnPlayerUnit();
            }
            
            // Spawn AI unit
            if (UnityEngine.Input.GetKeyDown(spawnAIUnitKey))
            {
                SpawnAIUnit();
            }
            
            // Test attack
            if (UnityEngine.Input.GetKeyDown(testAttackKey))
            {
                TestAttack();
            }
            
            // Test selection
            if (UnityEngine.Input.GetKeyDown(testSelectionKey))
            {
                TestSelection();
            }
            
            // Test destination marker
            if (UnityEngine.Input.GetKeyDown(testDestinationKey))
            {
                TestDestinationMarker();
            }
        }
        
        private void SpawnTestUnits()
        {
            SpawnPlayerUnit();
            SpawnAIUnit();
        }
        
        private void SpawnPlayerUnit()
        {
            if (playerUnitPrefab != null)
            {
                GameObject unitObj = Instantiate(playerUnitPrefab, playerSpawnPosition, Quaternion.identity);
                playerUnit = unitObj.GetComponent<Unit>();
                
                if (playerUnit != null)
                {
                    // Assign to player team
                    playerUnit.AssignToTeam(Team.Player);
                    Debug.Log($"Spawned player unit: {playerUnit.name}");
                    
                    // Check if visual components are present
                    CheckVisualComponents(playerUnit);
                }
            }
        }
        
        private void SpawnAIUnit()
        {
            if (aiUnitPrefab != null)
            {
                GameObject unitObj = Instantiate(aiUnitPrefab, aiSpawnPosition, Quaternion.identity);
                aiUnit = unitObj.GetComponent<Unit>();
                
                if (aiUnit != null)
                {
                    // Assign to AI team
                    aiUnit.AssignToTeam(Team.AI);
                    Debug.Log($"Spawned AI unit: {aiUnit.name}");
                    
                    // Check if visual components are present
                    CheckVisualComponents(aiUnit);
                }
            }
        }
        
        private void CheckVisualComponents(Unit unit)
        {
            UnitVisualCoordinator coordinator = unit.GetComponent<UnitVisualCoordinator>();
            UnitVisual visual = unit.GetComponent<UnitVisual>();
            
            Debug.Log($"Unit {unit.name} visual components:");
            Debug.Log($"  - UnitVisualCoordinator: {coordinator != null}");
            Debug.Log($"  - UnitVisual: {visual != null}");
            
            if (visual != null)
            {
                Debug.Log($"  - Selection Circle Prefab: {visual.selectionCirclePrefab != null}");
                Debug.Log($"  - Destination Marker Prefab: {visual.destinationMarkerPrefab != null}");
            }
        }
        
        private void TestSelection()
        {
            Debug.Log("=== Testing Selection System ===");
            
            if (selectionManager == null)
            {
                Debug.LogError("SelectionManager not found!");
                return;
            }
            
            if (playerUnit != null)
            {
                Debug.Log("Selecting player unit...");
                selectionManager.SelectUnit(playerUnit);
                
                // Test deselection after 2 seconds
                Invoke(nameof(TestDeselection), 2f);
            }
            else
            {
                Debug.LogWarning("Player unit not spawned!");
            }
        }
        
        private void TestDeselection()
        {
            Debug.Log("Deselecting all units...");
            selectionManager?.DeselectAll();
        }
        
        private void TestDestinationMarker()
        {
            Debug.Log("=== Testing Destination Marker ===");
            
            if (playerUnit != null)
            {
                Vector3 testDestination = playerUnit.transform.position + new Vector3(5, 0, 5);
                Debug.Log($"Moving player unit to {testDestination}");
                
                playerUnit.MoveTo(testDestination);
                
                // Hide destination marker after 3 seconds
                Invoke(nameof(HideDestinationMarker), 3f);
            }
            else
            {
                Debug.LogWarning("Player unit not spawned!");
            }
        }
        
        private void HideDestinationMarker()
        {
            if (playerUnit != null)
            {
                UnitVisual visual = playerUnit.GetComponent<UnitVisual>();
                if (visual != null)
                {
                    visual.HideDestinationMarker();
                    Debug.Log("Destination marker hidden");
                }
            }
        }
        
        private void TestAttack()
        {
            if (playerUnit != null && aiUnit != null)
            {
                Debug.Log("=== Testing Attack System ===");
                
                // Test if player can attack AI
                bool canAttack = playerUnit.CanAttack(aiUnit);
                Debug.Log($"Player can attack AI: {canAttack}");
                
                if (canAttack)
                {
                    bool attackSuccess = playerUnit.Attack(aiUnit);
                    Debug.Log($"Player attack success: {attackSuccess}");
                    
                    if (attackSuccess)
                    {
                        Debug.Log($"AI unit health after attack: {aiUnit.Health}");
                    }
                }
                
                // Test if AI can attack player
                bool aiCanAttack = aiUnit.CanAttack(playerUnit);
                Debug.Log($"AI can attack player: {aiCanAttack}");
                
                if (aiCanAttack)
                {
                    bool aiAttackSuccess = aiUnit.Attack(playerUnit);
                    Debug.Log($"AI attack success: {aiAttackSuccess}");
                    
                    if (aiAttackSuccess)
                    {
                        Debug.Log($"Player unit health after attack: {playerUnit.Health}");
                    }
                }
                
                // Test friendly fire (should fail)
                bool friendlyFire = playerUnit.CanAttack(playerUnit);
                Debug.Log($"Friendly fire test (should be false): {friendlyFire}");
                
                Debug.Log("=== Attack Test Complete ===");
            }
            else
            {
                Debug.LogWarning("Cannot test attack - units not spawned!");
            }
        }
        
        [ContextMenu("Spawn Test Units")]
        private void SpawnTestUnitsContext()
        {
            SpawnTestUnits();
        }
        
        [ContextMenu("Test Attack")]
        private void TestAttackContext()
        {
            TestAttack();
        }
        
        [ContextMenu("Test Selection")]
        private void TestSelectionContext()
        {
            TestSelection();
        }
        
        [ContextMenu("Test Destination Marker")]
        private void TestDestinationMarkerContext()
        {
            TestDestinationMarker();
        }
    }
} 