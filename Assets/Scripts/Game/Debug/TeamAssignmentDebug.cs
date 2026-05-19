using UnityEngine;
using MyGame.Core.Units;
using MyGame.Game;
using MyGame.Core;

namespace MyGame.Game
{
    /// <summary>
    /// Legacy debug spawner. Prefer <see cref="GameUnitSpawner"/> for match setup.
    /// </summary>
    public class TeamAssignmentDebug : MonoBehaviour
    {
        [Header("Team Settings")]
        [SerializeField] private Team playerTeam = Team.Player;
        [SerializeField] private Team aiTeam = Team.AI;
        
        [Header("Unit Lists")]
        [SerializeField] private UnitType[] playerUnitTypes = { UnitType.Tank, UnitType.Soldier };
        [SerializeField] private UnitType[] aiUnitTypes = { UnitType.Tank, UnitType.Soldier };
        
        [Header("Spawn Settings")]
        [SerializeField] private float teamSeparation = 50f; // Distance between teams
        [SerializeField] private float unitSpacing = 8f; // Distance between units in same team
        [SerializeField] private bool autoStartOnAwake = false;
        
        [Header("Spawn Center")]
        [SerializeField] private Terrain terrain;
        [SerializeField] private float terrainEdgeMargin = 25f;
        [SerializeField] private Transform spawnCenter; // Optional: XZ on terrain; empty = terrain center
        
        [Header("Tag Settings")]
        [SerializeField] private string playerUnitTag = "Player";
        [SerializeField] private string aiUnitTag = "AI";
        
        [Header("Manual Assignment")]
        [SerializeField] private GameObject[] playerUnits;
        [SerializeField] private GameObject[] aiUnits;
        
        public enum SpawnMode
        {
            CameraBased,
            NavMeshBased
        }
        
        [Header("Spawn Mode")]
        [SerializeField] private SpawnMode spawnMode = SpawnMode.CameraBased;
        
        private UnitPoolManager unitPoolManager;
        
        private void Start()
        {
            // Try to get the unit pool manager
            if (TryGetUnitPoolManager())
            {
                if (autoStartOnAwake)
                {
                    SpawnAllUnits();
                }
            }
            else
            {
                // If not ready, try again in a frame
                StartCoroutine(WaitForUnitPoolManager());
            }
        }
        
        private bool TryGetUnitPoolManager()
        {
            unitPoolManager = SystemInitializer.GetSystem<UnitPoolManager>();
            return unitPoolManager != null;
        }
        
        private System.Collections.IEnumerator WaitForUnitPoolManager()
        {
            Debug.Log("🎮 TeamAssignmentTest: Waiting for UnitPoolManager...");
            
            // Wait up to 5 seconds for the system to initialize
            float timeout = 5f;
            float elapsed = 0f;
            
            while (elapsed < timeout)
            {
                if (TryGetUnitPoolManager())
                {
                    Debug.Log("🎮 TeamAssignmentTest: UnitPoolManager found!");
                    if (autoStartOnAwake)
                    {
                        SpawnAllUnits();
                    }
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Debug.LogError("🎮 TeamAssignmentTest: UnitPoolManager not found after timeout!");
        }
        
        /// <summary>
        /// Spawn all units for both teams automatically
        /// </summary>
        [ContextMenu("Spawn All Units")]
        public void SpawnAllUnits()
        {
            Debug.Log("🎮 TeamAssignmentTest: Spawning all units...");
            
            if (unitPoolManager == null)
            {
                Debug.LogError("🎮 TeamAssignmentTest: Cannot spawn units - UnitPoolManager not found!");
                return;
            }
            
            Debug.Log($"🎮 TeamAssignmentTest: Player unit types: {string.Join(", ", playerUnitTypes)}");
            Debug.Log($"🎮 TeamAssignmentTest: AI unit types: {string.Join(", ", aiUnitTypes)}");
            
            // Clear any existing units
            ClearExistingUnits();
            
            SpawnPlacementUtility.ResolveTeamSpawnCenters(
                terrain,
                spawnCenter,
                teamSeparation,
                terrainEdgeMargin,
                out var playerCenter,
                out var aiCenter);

            Debug.Log($"🎮 TeamAssignmentTest: Player center: {playerCenter}, AI center: {aiCenter}");
            SpawnTeamUnits(playerUnitTypes, playerTeam, playerCenter);
            SpawnTeamUnits(aiUnitTypes, aiTeam, aiCenter);
            
            Debug.Log("🎮 TeamAssignmentTest: All units spawned successfully!");
        }
        
        /// <summary>
        /// Spawn units for a specific team at a given center position
        /// </summary>
        private void SpawnTeamUnits(UnitType[] unitTypes, Team team, Vector3 teamCenter)
        {
            Debug.Log($"🎮 TeamAssignmentTest: Spawning {unitTypes?.Length ?? 0} units for {team} team at center {teamCenter}");
            
            if (unitTypes == null || unitTypes.Length == 0)
            {
                Debug.LogWarning($"🎮 TeamAssignmentTest: No unit types specified for {team} team");
                return;
            }
            
            if (unitPoolManager == null)
            {
                Debug.LogError($"🎮 TeamAssignmentTest: UnitPoolManager is null when trying to spawn {team} team units!");
                return;
            }
            
            for (int i = 0; i < unitTypes.Length; i++)
            {
                // Calculate position in a line
                var spawnPosition = teamCenter + Vector3.forward * (i * unitSpacing);
                if (SpawnPlacementUtility.TryGetTerrain(terrain, out var terrainRef))
                    spawnPosition = SpawnPlacementUtility.ClampXZToTerrain(terrainRef, spawnPosition, terrainEdgeMargin);
                else
                    spawnPosition = SpawnPlacementUtility.ResolveGroundPosition(spawnPosition, null);

                Debug.Log($"🎮 TeamAssignmentTest: Attempting to spawn {unitTypes[i]} at {spawnPosition}");
                
                // Create unit using pool system (UnitPoolManager snaps to ground)
                Unit unit = unitPoolManager.CreateUnit(unitTypes[i], spawnPosition, Quaternion.identity, team);
                
                if (unit != null)
                {
                    Debug.Log($"🎮 TeamAssignmentTest: Successfully spawned {unitTypes[i]} for {team} team at {spawnPosition}");
                }
                else
                {
                    Debug.LogError($"🎮 TeamAssignmentTest: Failed to spawn {unitTypes[i]} for {team} team at {spawnPosition}");
                }
            }
        }
        
        /// <summary>
        /// Get the spawn center position
        /// </summary>
        private Vector3 GetSpawnCenter()
        {
            if (spawnCenter != null)
            {
                return spawnCenter.position;
            }
            
            // Use camera position as fallback
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                return mainCamera.transform.position + mainCamera.transform.forward * 20f;
            }
            
            // Last resort: use world origin
            return Vector3.zero;
        }
        
        /// <summary>
        /// Clear existing units from the scene
        /// </summary>
        private void ClearExistingUnits()
        {
            Unit[] existingUnits = FindObjectsOfType<Unit>();
            foreach (var unit in existingUnits)
            {
                if (unit != null)
                {
                    DestroyImmediate(unit.gameObject);
                }
            }
            Debug.Log($"🎮 TeamAssignmentTest: Cleared {existingUnits.Length} existing units");
        }
        
        /// <summary>
        /// Restart the game with new units
        /// </summary>
        [ContextMenu("Restart Game")]
        public void RestartGame()
        {
            Debug.Log("🔄 TeamAssignmentTest: Restarting game...");
            SpawnAllUnits();
        }
        
        /// <summary>
        /// Force spawn units (for debugging)
        /// </summary>
        [ContextMenu("Force Spawn Units")]
        public void ForceSpawnUnits()
        {
            Debug.Log("🎮 TeamAssignmentTest: Force spawning units...");
            
            // Try to get unit pool manager again
            if (!TryGetUnitPoolManager())
            {
                Debug.LogError("🎮 TeamAssignmentTest: Cannot force spawn - UnitPoolManager not found!");
                return;
            }
            
            SpawnAllUnits();
        }
        
        /// <summary>
        /// Auto-assign teams based on tags, names, or random assignment
        /// </summary>
        [ContextMenu("Auto Assign Teams")]
        public void AutoAssignTeams()
        {
            Debug.Log("TeamAssignmentTest: Auto-assigning teams...");
            
            // Find all units in the scene
            Unit[] allUnits = FindObjectsOfType<Unit>();
            
            foreach (var unit in allUnits)
            {
                // Check if unit has a tag that indicates team (only if tags exist)
                bool hasPlayerTag = false;
                bool hasAITag = false;
                
                // Only check tags if they are valid Unity tags
                if (IsValidUnityTag(playerUnitTag))
                {
                    hasPlayerTag = unit.gameObject.CompareTag(playerUnitTag);
                }
                
                if (IsValidUnityTag(aiUnitTag))
                {
                    hasAITag = unit.gameObject.CompareTag(aiUnitTag);
                }
                
                if (hasPlayerTag)
                {
                    unit.SetTeam(playerTeam);
                }
                else if (hasAITag)
                {
                    unit.SetTeam(aiTeam);
                }
                else
                {
                    // Default assignment based on name or position
                    if (unit.name.ToLower().Contains("player") || unit.name.ToLower().Contains("blue"))
                    {
                        unit.SetTeam(playerTeam);
                    }
                    else if (unit.name.ToLower().Contains("ai") || unit.name.ToLower().Contains("enemy") || unit.name.ToLower().Contains("red"))
                    {
                        unit.SetTeam(aiTeam);
                    }
                    else
                    {
                        // Random assignment for testing
                        Team randomTeam = Random.Range(0, 2) == 0 ? playerTeam : aiTeam;
                        unit.SetTeam(randomTeam);
                        Debug.Log($"TeamAssignmentTest: Randomly assigned {unit.name} to {randomTeam} team");
                    }
                }
            }
            
            Debug.Log($"TeamAssignmentTest: Auto-assignment complete. Found {allUnits.Length} units.");
        }
        
        /// <summary>
        /// Check if a tag is a valid Unity tag (built-in tags only)
        /// </summary>
        private bool IsValidUnityTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
                return false;
                
            // List of valid Unity tags
            string[] validTags = { 
                "Untagged", 
                "Respawn", 
                "Finish", 
                "EditorOnly", 
                "MainCamera", 
                "Player", 
                "GameController", 
                "AI",
            };
            
            return System.Array.Exists(validTags, tag => tag == tagName);
        }
        
        /// <summary>
        /// Assign a Unity tag to a GameObject based on team
        /// </summary>
        private void AssignUnityTag(GameObject gameObject, bool isPlayerTeam)
        {
            string tagToAssign = isPlayerTeam ? playerUnitTag : aiUnitTag;
            if (!string.IsNullOrEmpty(tagToAssign))
            {
                gameObject.tag = tagToAssign;
                Debug.Log($"🎮 TeamAssignmentTest: Assigned tag '{tagToAssign}' to {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"🎮 TeamAssignmentTest: No tag assigned for {gameObject.name} (isPlayerTeam: {isPlayerTeam})");
            }
        }
        
        [ContextMenu("Assign Teams Manually")]
        public void AssignTeamsManually()
        {
            Debug.Log("TeamAssignmentTest: Manually assigning teams...");
            
            // Assign player units
            foreach (var unitGO in playerUnits)
            {
                if (unitGO != null)
                {
                    Unit unit = unitGO.GetComponent<Unit>();
                    if (unit != null)
                    {
                        unit.SetTeam(playerTeam);
                    }
                    else
                    {
                        Debug.LogWarning($"TeamAssignmentTest: {unitGO.name} has no Unit component");
                    }
                }
            }
            
            // Assign AI units
            foreach (var unitGO in aiUnits)
            {
                if (unitGO != null)
                {
                    Unit unit = unitGO.GetComponent<Unit>();
                    if (unit != null)
                    {
                        unit.SetTeam(aiTeam);
                    }
                    else
                    {
                        Debug.LogWarning($"TeamAssignmentTest: {unitGO.name} has no Unit component");
                    }
                }
            }
            
            Debug.Log("TeamAssignmentTest: Manual assignment complete.");
        }
        
        [ContextMenu("Show All Unit Teams")]
        public void ShowAllUnitTeams()
        {
            Debug.Log("TeamAssignmentTest: Current team assignments:");
            
            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (var unit in allUnits)
            {
                Team team = unit.GetTeam();
                Debug.Log($"  {unit.name}: {team} team");
            }
        }
        
        [ContextMenu("Set All Units to Player Team")]
        public void SetAllToPlayerTeam()
        {
            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (var unit in allUnits)
            {
                unit.SetTeam(playerTeam);
            }
            Debug.Log($"TeamAssignmentTest: Set all {allUnits.Length} units to {playerTeam} team");
        }
        
        [ContextMenu("Set All Units to AI Team")]
        public void SetAllToAITeam()
        {
            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (var unit in allUnits)
            {
                unit.SetTeam(aiTeam);
            }
            Debug.Log($"TeamAssignmentTest: Set all {allUnits.Length} units to {aiTeam} team");
        }
        
        [ContextMenu("Assign Unity Tags to All Units")]
        public void AssignUnityTagsToAllUnits()
        {
            Debug.Log("TeamAssignmentTest: Assigning Unity tags to all units...");
            
            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (var unit in allUnits)
            {
                Team team = unit.GetTeam();
                bool isPlayerTeam = team == Team.Player;
                AssignUnityTag(unit.gameObject, isPlayerTeam);
            }
            
            Debug.Log($"TeamAssignmentTest: Unity tags assigned to {allUnits.Length} units");
        }
        
        [ContextMenu("Alternate Teams")]
        public void AlternateTeams()
        {
            Unit[] allUnits = FindObjectsOfType<Unit>();
            for (int i = 0; i < allUnits.Length; i++)
            {
                Team team = (i % 2 == 0) ? playerTeam : aiTeam;
                allUnits[i].SetTeam(team);
            }
            Debug.Log($"TeamAssignmentTest: Alternated teams for {allUnits.Length} units");
        }
        
        [ContextMenu("Test NavMesh Spawning")]
        public void TestNavMeshSpawning()
        {
            Debug.Log("🎮 TeamAssignmentTest: Testing NavMesh spawning...");
            
            // Temporarily switch to nav mesh mode
            SpawnMode originalMode = spawnMode;
            spawnMode = SpawnMode.NavMeshBased;
            
            // Get spawn center
            Vector3 center = GetSpawnCenter();
            
            // Spawn one unit for each team
            UnitType[] testPlayerTypes = { UnitType.Tank };
            UnitType[] testAITypes = { UnitType.Soldier };
            
            SpawnTeamUnits(testPlayerTypes, playerTeam, center + Vector3.left * teamSeparation);
            SpawnTeamUnits(testAITypes, aiTeam, center + Vector3.right * teamSeparation);
            
            // Restore original mode
            spawnMode = originalMode;
            
            Debug.Log("🎮 TeamAssignmentTest: NavMesh spawning test complete!");
        }
        
        [ContextMenu("Validate NavMesh")]
        public void ValidateNavMesh()
        {
            Debug.Log("🎮 TeamAssignmentTest: Validating NavMesh...");
            
            // Check if NavMesh is built
            if (!UnityEngine.AI.NavMesh.SamplePosition(Vector3.zero, out UnityEngine.AI.NavMeshHit hit, 100f, UnityEngine.AI.NavMesh.AllAreas))
            {
                Debug.LogError("🎮 TeamAssignmentTest: No NavMesh found! Please build the NavMesh in your scene.");
                return;
            }
            
            Debug.Log($"🎮 TeamAssignmentTest: NavMesh is valid. Found surface at {hit.position}");
            
            // Test a few random positions
            Vector3 testCenter = spawnCenter != null ? spawnCenter.position : Vector3.zero;
            for (int i = 0; i < 5; i++)
            {
                Vector3 testPos = testCenter + Random.insideUnitSphere * 20f; // Use 20f as test radius
                if (UnityEngine.AI.NavMesh.SamplePosition(testPos, out hit, 20f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    Debug.Log($"🎮 TeamAssignmentTest: Valid position {i + 1}: {hit.position}");
                }
                else
                {
                    Debug.LogWarning($"🎮 TeamAssignmentTest: Could not find valid position near {testPos}");
                }
            }
        }
    }
}

