using UnityEngine;
using MyGame.Core.Units;

namespace MyGame.Game
{
    /// <summary>
    /// Test script to verify OnDeath event system
    /// </summary>
    public class OnDeathTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestOnStart = false;
        [SerializeField] private float testDamage = 1000f;
        
        [Header("Test Results")]
        [SerializeField] private int unitsTested = 0;
        [SerializeField] private int deathEventsReceived = 0;
        
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
            RunOnDeathTest();
        }
        
        [ContextMenu("Run OnDeath Test")]
        public void RunOnDeathTest()
        {
            Debug.Log("🧪 Starting OnDeath event system test...");
            
            // Find all units in the scene
            Unit[] allUnits = FindObjectsOfType<Unit>();
            Debug.Log($"🧪 Found {allUnits.Length} units to test");
            
            unitsTested = 0;
            deathEventsReceived = 0;
            
            foreach (var unit in allUnits)
            {
                TestUnitDeath(unit);
            }
            
            Debug.Log($"🧪 Test complete: {unitsTested} units tested, {deathEventsReceived} death events received");
        }
        
        private void TestUnitDeath(Unit unit)
        {
            if (unit == null) return;
            
            Debug.Log($"🧪 Testing unit: {unit.name} (Health: {unit.Health})");
            
            // Subscribe to death event
            unit.OnDeath += (deadUnit) => {
                deathEventsReceived++;
                Debug.Log($"💀 DEATH EVENT RECEIVED for {deadUnit.Name}! Event #{deathEventsReceived}");
            };
            
            // Check current subscribers
            // int subscriberCount = unit.OnDeath?.GetInvocationList().Length ?? 0;
            // Debug.Log($"🧪 {unit.name}: OnDeath event has {subscriberCount} subscribers");
            
            // Apply lethal damage
            Debug.Log($"🧪 Applying {testDamage} damage to {unit.name}");
            unit.TakeDamage(testDamage);
            
            unitsTested++;
            
            // Wait a frame to let events process
            StartCoroutine(CheckHealthAfterFrame(unit));
        }
        
        private System.Collections.IEnumerator CheckHealthAfterFrame(Unit unit)
        {
            yield return null; // Wait one frame
            
            Debug.Log($"🧪 {unit.name}: Health after damage: {unit.Health}");
            
            if (unit.Health <= 0)
            {
                Debug.Log($"✅ {unit.name}: Successfully killed (Health: {unit.Health})");
            }
            else
            {
                Debug.LogWarning($"⚠️ {unit.name}: Still alive after lethal damage (Health: {unit.Health})");
            }
        }
        
        [ContextMenu("Reset All Units")]
        public void ResetAllUnits()
        {
            Debug.Log("🔄 Resetting all units...");
            
            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (var unit in allUnits)
            {
                if (unit != null)
                {
                    // Reset health by reassigning team (this recreates UnitData)
                    unit.AssignToTeam(unit.Owner?.Team ?? Team.Player);
                    Debug.Log($"🔄 Reset {unit.name}");
                }
            }
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("OnDeath Event System Test", GUI.skin.box);
            
            if (GUILayout.Button("Run Test"))
            {
                RunOnDeathTest();
            }
            
            if (GUILayout.Button("Reset Units"))
            {
                ResetAllUnits();
            }
            
            GUILayout.Label($"Units Tested: {unitsTested}");
            GUILayout.Label($"Death Events: {deathEventsReceived}");
            
            GUILayout.EndArea();
        }
    }
}
