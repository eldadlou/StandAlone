using UnityEngine;
using System.Collections.Generic;
using MyGame.Core.Units.Combat;

namespace MyGame.Game
{
    /// <summary>
    /// Monitors performance of the detection system and provides optimization insights
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Performance Monitoring")]
        [SerializeField] private bool enableMonitoring = true;
        [SerializeField] private float updateInterval = 1f; // Update stats every second
        
        [Header("Performance Thresholds")]
        [SerializeField] private float warningThreshold = 16f; // ms per frame
        [SerializeField] private float criticalThreshold = 33f; // ms per frame
        
        // Performance stats
        private float lastUpdateTime;
        private float frameTimeSum;
        private int frameCount;
        private float averageFrameTime;
        private float maxFrameTime;
        private float minFrameTime = float.MaxValue;
        
        // Detection system stats
        private int totalCombatUnits;
        private int unitsWithTargets;
        private int unitsSearching;
        private float detectionUpdateTime;
        
        // Cache stats
        private int cacheHits;
        private int cacheMisses;
        private float cacheHitRate;
        
        private void Update()
        {
            if (!enableMonitoring) return;
            
            // Track frame time
            float currentFrameTime = Time.unscaledDeltaTime * 1000f; // Convert to milliseconds
            frameTimeSum += currentFrameTime;
            frameCount++;
            
            if (currentFrameTime > maxFrameTime) maxFrameTime = currentFrameTime;
            if (currentFrameTime < minFrameTime) minFrameTime = currentFrameTime;
            
            // Update stats periodically
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdatePerformanceStats();
                lastUpdateTime = Time.time;
            }
        }
        
        private void UpdatePerformanceStats()
        {
            // Calculate frame time stats
            averageFrameTime = frameTimeSum / frameCount;
            
            // Reset counters
            frameTimeSum = 0f;
            frameCount = 0;
            
            // Get detection system stats
            UpdateDetectionStats();
            
            // Log performance info
            LogPerformanceInfo();
            
            // Check for performance issues
            CheckPerformanceIssues();
        }
        
        private void UpdateDetectionStats()
        {
            var combatUnits = FindObjectsOfType<CombatUnit>();
            totalCombatUnits = combatUnits.Length;
            unitsWithTargets = 0;
            unitsSearching = 0;
            
            foreach (var unit in combatUnits)
            {
                if (unit.IsInCombat && unit.CurrentTarget != null)
                {
                    unitsWithTargets++;
                }
                else if (unit.IsInCombat)
                {
                    unitsSearching++;
                }
            }
        }
        
        private void LogPerformanceInfo()
        {
            string performanceColor = GetPerformanceColor(averageFrameTime);
            
            Debug.Log($"[PERFORMANCE] Frame Time: {performanceColor}{averageFrameTime:F1}ms{ColorUtility.ToHtmlStringRGB(Color.white)} " +
                     $"(Min: {minFrameTime:F1}ms, Max: {maxFrameTime:F1}ms)\n" +
                     $"[DETECTION] Units: {totalCombatUnits}, With Targets: {unitsWithTargets}, Searching: {unitsSearching}\n" +
                     $"[CACHE] Hit Rate: {cacheHitRate:F1}% ({cacheHits}/{cacheHits + cacheMisses})");
        }
        
        private string GetPerformanceColor(float frameTime)
        {
            if (frameTime <= warningThreshold) return ColorUtility.ToHtmlStringRGB(Color.green);
            if (frameTime <= criticalThreshold) return ColorUtility.ToHtmlStringRGB(Color.yellow);
            return ColorUtility.ToHtmlStringRGB(Color.red);
        }
        
        private void CheckPerformanceIssues()
        {
            if (averageFrameTime > criticalThreshold)
            {
                Debug.LogWarning($"[PERFORMANCE CRITICAL] Average frame time {averageFrameTime:F1}ms exceeds critical threshold {criticalThreshold}ms!");
                SuggestOptimizations();
            }
            else if (averageFrameTime > warningThreshold)
            {
                Debug.LogWarning($"[PERFORMANCE WARNING] Average frame time {averageFrameTime:F1}ms exceeds warning threshold {warningThreshold}ms");
            }
        }
        
        private void SuggestOptimizations()
        {
            Debug.Log("[OPTIMIZATION SUGGESTIONS]");
            
            if (totalCombatUnits > 50)
            {
                Debug.Log("- Consider reducing detection radius for units without targets");
                Debug.Log("- Increase target update intervals for units with stable targets");
                Debug.Log("- Use SpatialGrid system if not already enabled");
            }
            
            if (cacheHitRate < 50f)
            {
                Debug.Log("- Cache hit rate is low, consider increasing cache duration");
            }
            
            if (unitsSearching > totalCombatUnits * 0.8f)
            {
                Debug.Log("- Many units are searching for targets, consider reducing search frequency");
            }
        }
        
        // Public methods for external access
        public void SetCacheStats(int hits, int misses)
        {
            cacheHits = hits;
            cacheMisses = misses;
            cacheHitRate = (hits + misses) > 0 ? (float)hits / (hits + misses) * 100f : 0f;
        }
        
        public float GetAverageFrameTime() => averageFrameTime;
        public int GetTotalCombatUnits() => totalCombatUnits;
        public float GetCacheHitRate() => cacheHitRate;
        
        private void OnGUI()
        {
            if (!enableMonitoring) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Performance Monitor", GUI.skin.box);
            GUILayout.Label($"Frame Time: {averageFrameTime:F1}ms");
            GUILayout.Label($"Combat Units: {totalCombatUnits}");
            GUILayout.Label($"With Targets: {unitsWithTargets}");
            GUILayout.Label($"Searching: {unitsSearching}");
            GUILayout.Label($"Cache Hit Rate: {cacheHitRate:F1}%");
            GUILayout.EndArea();
        }
    }
}
