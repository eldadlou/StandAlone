using UnityEngine;
using MyGame.Core.Units;
using MyGame.RuntimeSystems.Combat;
using MyGame.Presentation;
using MyGame.Core;
using MyGame.Core.Events;

namespace MyGame.Game
{
    /// <summary>
    /// Test script to demonstrate the upgraded FireSystem functionality
    /// </summary>
    public class FireSystemDebug : MonoBehaviour
    {
        [Header("Test Settings")]
        public GameObject playerUnitPrefab;
        public GameObject aiUnitPrefab;
        public GameObject projectilePrefab;
        
        [Header("Spawn Positions")]
        public Vector3 playerSpawnPosition = new Vector3(0, 0, 0);
        public Vector3 aiSpawnPosition = new Vector3(10, 0, 0);
        
        [Header("FireSystem Configuration")]
        public float detectionRadius = 15f;
        public float gunRotationSpeed = 90f;
        public float attackCooldown = 2f;
        
        private FireSystem fireSystem;
        private IUnit playerUnit;
        private IUnit aiUnit;
        
        private void Start()
        {
            SetupFireSystem();
            SpawnTestUnits();
            SubscribeUnits();
            
            // Debug.Log("FireSystem Test initialized! Units should automatically detect and attack each other.");
        }
        
        private void SetupFireSystem()
        {
            // Get or create FireSystem
            fireSystem = DependencyContainer.Instance.TryResolve<FireSystem>();
            if (fireSystem == null)
            {
                GameObject fireSystemGO = new GameObject("FireSystem");
                fireSystem = fireSystemGO.AddComponent<FireSystem>();
                DependencyContainer.Instance.Register(fireSystem);
            }
            
            // Configure FireSystem
            fireSystem.detectionRadius = detectionRadius;
            fireSystem.gunRotationSpeed = gunRotationSpeed;
            fireSystem.attackCooldown = attackCooldown;
            fireSystem.projectilePrefab = projectilePrefab;
            fireSystem.enableProjectiles = true;
            fireSystem.useGunTurretComponent = true;
            
            // Debug.Log($"FireSystem configured - Detection: {detectionRadius}m, Rotation: {gunRotationSpeed}°/s, Cooldown: {attackCooldown}s");
        }
        
        private void SpawnTestUnits()
        {
            // Spawn player unit
            if (playerUnitPrefab != null)
            {
                GameObject playerGO = Instantiate(playerUnitPrefab, playerSpawnPosition, Quaternion.identity);
                playerUnit = playerGO.GetComponent<IUnit>();
                
                if (playerUnit != null)
                {
                    playerUnit.AssignToTeam(Team.Player);
                    SetupUnitGunTurret(playerGO, "Player Gun");
                    GameEvents.TriggerUnitCreated(playerUnit);
                    // Debug.Log($"Player unit spawned at {playerSpawnPosition}");
                }
            }
            
            // Spawn AI unit
            if (aiUnitPrefab != null)
            {
                GameObject aiGO = Instantiate(aiUnitPrefab, aiSpawnPosition, Quaternion.identity);
                aiUnit = aiGO.GetComponent<IUnit>();
                
                if (aiUnit != null)
                {
                    aiUnit.AssignToTeam(Team.AI);
                    SetupUnitGunTurret(aiGO, "AI Gun");
                    GameEvents.TriggerUnitCreated(aiUnit);
                    // Debug.Log($"AI unit spawned at {aiSpawnPosition}");
                }
            }
        }
        
        private void SetupUnitGunTurret(GameObject unitGO, string gunName)
        {
            // Create gun turret child object
            GameObject gunTurretGO = new GameObject(gunName);
            gunTurretGO.transform.SetParent(unitGO.transform);
            gunTurretGO.transform.localPosition = new Vector3(0, 1, 0); // Position above unit
            
            // Add GunTurret component
            GunTurret gunTurret = gunTurretGO.AddComponent<GunTurret>();
            gunTurret.rotationSpeed = gunRotationSpeed;
            gunTurret.rotationThreshold = 5f;
            gunTurret.smoothRotation = true;
            gunTurret.showRotationGizmos = true;
            
            // Debug.Log($"Gun turret setup for {unitGO.name}");
        }
        
        private void SubscribeUnits()
        {
            if (fireSystem != null)
            {
                if (playerUnit != null)
                {
                    fireSystem.SubscribeToUnit(playerUnit);
                    // Debug.Log("Player unit subscribed to FireSystem");
                }
                
                if (aiUnit != null)
                {
                    fireSystem.SubscribeToUnit(aiUnit);
                    // Debug.Log("AI unit subscribed to FireSystem");
                }
            }
        }
        
        private void Update()
        {
            // Display debug information
            if (UnityEngine.Input.GetKeyDown(KeyCode.F1))
            {
                DisplayDebugInfo();
            }
            
            // Manual attack test
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                TestManualAttack();
            }
            
            // Move units apart
            if (UnityEngine.Input.GetKeyDown(KeyCode.M))
            {
                MoveUnitsApart();
            }
            
            // Move units together
            if (UnityEngine.Input.GetKeyDown(KeyCode.N))
            {
                MoveUnitsTogether();
            }
        }
        
        private void DisplayDebugInfo()
        {
            // Debug.Log("=== FireSystem Debug Info ===");
            
            if (playerUnit != null)
            {
                IUnit playerTarget = fireSystem.GetCurrentTarget(playerUnit);
                bool playerRotating = fireSystem.IsRotating(playerUnit);
                
                // Debug.Log($"Player Unit:");
                // Debug.Log($"  Position: {playerUnit.Position}");
                // Debug.Log($"  Health: {playerUnit.Health}");
                // Debug.Log($"  Current Target: {(playerTarget != null ? playerTarget.Owner : "None")}");
                // Debug.Log($"  Is Rotating: {playerRotating}");
            }
            
            if (aiUnit != null)
            {
                IUnit aiTarget = fireSystem.GetCurrentTarget(aiUnit);
                bool aiRotating = fireSystem.IsRotating(aiUnit);
                
                // Debug.Log($"AI Unit:");
                // Debug.Log($"  Position: {aiUnit.Position}");
                // Debug.Log($"  Health: {aiUnit.Health}");
                // Debug.Log($"  Current Target: {(aiTarget != null ? aiTarget.Owner : "None")}");
                // Debug.Log($"  Is Rotating: {aiRotating}");
            }
            
            float distance = Vector3.Distance(playerUnit.Position, aiUnit.Position);
            // Debug.Log($"Distance between units: {distance:F2}m");
            // Debug.Log($"Detection radius: {detectionRadius}m");
            // Debug.Log($"Attack range: {playerUnit.AttackRange}m");
        }
        
        private void TestManualAttack()
        {
            if (playerUnit != null && aiUnit != null)
            {
                // Debug.Log("Testing manual attack...");
                bool success = playerUnit.Attack(aiUnit);
                // Debug.Log($"Manual attack result: {success}");
            }
        }
        
        private void MoveUnitsApart()
        {
            if (playerUnit != null && aiUnit != null)
            {
                playerUnit.MoveTo(new Vector3(-5, 0, 0));
                aiUnit.MoveTo(new Vector3(15, 0, 0));
                // Debug.Log("Units moved apart - should stop detecting each other");
            }
        }
        
        private void MoveUnitsTogether()
        {
            if (playerUnit != null && aiUnit != null)
            {
                playerUnit.MoveTo(new Vector3(0, 0, 0));
                aiUnit.MoveTo(new Vector3(8, 0, 0));
                // Debug.Log("Units moved together - should start detecting each other");
            }
        }
        
        private void OnDrawGizmos()
        {
            // Draw spawn positions
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerSpawnPosition, 1f);
            Gizmos.DrawWireSphere(aiSpawnPosition, 1f);
            
            // Draw detection radius around units
            if (Application.isPlaying)
            {
                if (playerUnit != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(playerUnit.Position, detectionRadius);
                }
                
                if (aiUnit != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(aiUnit.Position, detectionRadius);
                }
            }
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("FireSystem Test Controls", GUI.skin.box);
            GUILayout.Label("F1 - Display Debug Info");
            GUILayout.Label("Space - Test Manual Attack");
            GUILayout.Label("M - Move Units Apart");
            GUILayout.Label("N - Move Units Together");
            GUILayout.EndArea();
        }
    }
}
