using UnityEngine;
using MyGame.Core.Units;
using MyGame.RuntimeSystems.Effects;

namespace MyGame.Game
{
    /// <summary>
    /// Test script to verify UnitParticleSystem is working
    /// </summary>
    public class UnitParticleSystemDebug : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestOnStart = false;
        
        [Header("Test Results")]
        [SerializeField] private bool particleSystemFound = false;
        [SerializeField] private bool particleSystemRegistered = false;
        
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
            TestUnitParticleSystem();
        }
        
        [ContextMenu("Test UnitParticleSystem")]
        public void TestUnitParticleSystem()
        {
            Debug.Log("🧪 Testing UnitParticleSystem...");
            
            // Test 1: Check if UnitParticleSystem exists in scene
            UnitParticleSystem particleSystem = FindObjectOfType<UnitParticleSystem>();
            if (particleSystem != null)
            {
                particleSystemFound = true;
                Debug.Log("✅ UnitParticleSystem found in scene");
            }
            else
            {
                particleSystemFound = false;
                Debug.LogError("❌ UnitParticleSystem not found in scene");
                return;
            }
            
            // Test 2: Check if UnitParticleSystem is registered with DependencyContainer
            var registeredSystem = MyGame.Core.SystemInitializer.GetSystem<UnitParticleSystem>();
            if (registeredSystem != null)
            {
                particleSystemRegistered = true;
                Debug.Log("✅ UnitParticleSystem is registered with DependencyContainer");
            }
            else
            {
                particleSystemRegistered = false;
                Debug.LogError("❌ UnitParticleSystem is NOT registered with DependencyContainer");
            }
            
            // Test 3: Check if UnitParticleSystem has prefabs assigned
            if (particleSystem.deathExplosionPrefab != null)
            {
                Debug.Log("✅ deathExplosionPrefab is assigned");
            }
            else
            {
                Debug.LogWarning("⚠️ deathExplosionPrefab is NOT assigned");
            }
            
            if (particleSystem.deathSmokePrefab != null)
            {
                Debug.Log("✅ deathSmokePrefab is assigned");
            }
            else
            {
                Debug.LogWarning("⚠️ deathSmokePrefab is NOT assigned");
            }
            
            // Test 4: Check if any units exist and have OnDeath events
            Unit[] allUnits = FindObjectsOfType<Unit>();
            Debug.Log($"🧪 Found {allUnits.Length} units in scene");
            
            foreach (var unit in allUnits)
            {
                if (unit != null)
                {
                    // int deathSubscribers = unit.OnDeath?.GetInvocationList().Length ?? 0;
                    // Debug.Log($"🧪 {unit.name}: OnDeath event has {deathSubscribers} subscribers");
                }
            }
            
            Debug.Log("🧪 UnitParticleSystem test complete");
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 250, 300, 150));
            GUILayout.Label("UnitParticleSystem Test", GUI.skin.box);
            
            if (GUILayout.Button("Test UnitParticleSystem"))
            {
                TestUnitParticleSystem();
            }
            
            GUILayout.Label($"ParticleSystem Found: {particleSystemFound}");
            GUILayout.Label($"ParticleSystem Registered: {particleSystemRegistered}");
            
            GUILayout.EndArea();
        }
    }
}
