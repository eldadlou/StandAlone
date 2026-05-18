using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;

namespace MyGame.Core.Performance
{
    /// <summary>
    /// Performance monitoring system for tracking and optimizing game performance
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        [SerializeField] private bool enableMonitoring = true;
        [SerializeField] private float updateInterval = 1f;
        [SerializeField] private bool logToConsole = false;
        
        private readonly Dictionary<string, PerformanceMetric> _metrics = new Dictionary<string, PerformanceMetric>();
        private float _lastUpdateTime;
        
        // Performance thresholds
        private const float WARNING_THRESHOLD = 16.67f; // 60 FPS
        private const float CRITICAL_THRESHOLD = 33.33f; // 30 FPS
        
        private void Start()
        {
            if (enableMonitoring)
            {
                InitializeMetrics();
                DependencyContainer.Instance.Register(this);
            }
        }
        
        private void Update()
        {
            if (!enableMonitoring) return;
            
            //UpdateMetrics();
            
            if (Time.time - _lastUpdateTime >= updateInterval)
            {
                _lastUpdateTime = Time.time;
                AnalyzePerformance();
            }
        }
        
        private void InitializeMetrics()
        {
            AddMetric("FrameTime", "Frame Time (ms)");
            AddMetric("FPS", "Frames Per Second");
            AddMetric("MemoryUsage", "Memory Usage (MB)");
            AddMetric("ActiveUnits", "Active Units");
            AddMetric("DrawCalls", "Draw Calls");
            AddMetric("Triangles", "Triangles");
            AddMetric("Vertices", "Vertices");
        }
        
        public void AddMetric(string key, string displayName)
        {
            if (!_metrics.ContainsKey(key))
            {
                _metrics[key] = new PerformanceMetric(displayName);
            }
        }
        
        public void UpdateMetric(string key, float value)
        {
            if (_metrics.TryGetValue(key, out var metric))
            {
                metric.UpdateValue(value);
            }
        }
        
        private void UpdateMetrics()
        {
            // Frame time and FPS
            float frameTime = Time.unscaledDeltaTime * 1000f;
            float fps = 1f / Time.unscaledDeltaTime;
            
            UpdateMetric("FrameTime", frameTime);
            UpdateMetric("FPS", fps);
            
            // Memory usage
            long memoryUsage = System.GC.GetTotalMemory(false) / (1024 * 1024);
            UpdateMetric("MemoryUsage", memoryUsage);
            
            // Unity rendering stats
            //var lastFrame = ProfilerDriver.lastFrameIndex;
            
           // UpdateMetric("DrawCalls", UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(UnityEngine.Profiling.ProfilerArea.Physics.));
            //UpdateMetric("Triangles", UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(UnityEngine.Profiling.ProfilerArea.GPU));
            //UpdateMetric("Vertices", UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(UnityEngine.Profiling.ProfilerArea.GPU));
            
            // Active units count
            // var gameManager = DependencyContainer.Instance.TryResolve<MyGame.Game.GameManager>();
            // if (gameManager != null)
            // {
            //     UpdateMetric("ActiveUnits", gameManager.AllUnits.Count);
            // }
        }
        
        private void AnalyzePerformance()
        {
            var frameTimeMetric = _metrics["FrameTime"];
            var fpsMetric = _metrics["FPS"];
            
            if (frameTimeMetric.CurrentValue > CRITICAL_THRESHOLD)
            {
                LogPerformanceWarning($"Critical performance issue: {frameTimeMetric.CurrentValue:F1}ms frame time ({fpsMetric.CurrentValue:F0} FPS)");
                SuggestOptimizations();
            }
            else if (frameTimeMetric.CurrentValue > WARNING_THRESHOLD)
            {
                LogPerformanceWarning($"Performance warning: {frameTimeMetric.CurrentValue:F1}ms frame time ({fpsMetric.CurrentValue:F0} FPS)");
            }
            
            if (logToConsole)
            {
                LogPerformanceReport();
            }
        }
        
        private void SuggestOptimizations()
        {
            var suggestions = new List<string>();
            
            if (_metrics.TryGetValue("ActiveUnits", out var unitsMetric) && unitsMetric.CurrentValue > 500)
            {
                suggestions.Add("Consider reducing unit count or implementing LOD system");
            }
            
            if (_metrics.TryGetValue("MemoryUsage", out var memoryMetric) && memoryMetric.CurrentValue > 1000)
            {
                suggestions.Add("High memory usage detected - check for memory leaks");
            }
            
            if (_metrics.TryGetValue("DrawCalls", out var drawCallsMetric) && drawCallsMetric.CurrentValue > 1000)
            {
                suggestions.Add("High draw call count - consider batching or LOD");
            }
            
            if (suggestions.Count > 0)
            {
                Debug.LogWarning("Performance optimization suggestions:");
                foreach (var suggestion in suggestions)
                {
                    Debug.LogWarning($"- {suggestion}");
                }
            }
        }
        
        private void LogPerformanceWarning(string message)
        {
            Debug.LogWarning($"[Performance] {message}");
        }
        
        private void LogPerformanceReport()
        {
            Debug.Log("=== Performance Report ===");
            foreach (var metric in _metrics.Values)
            {
                Debug.Log($"{metric.DisplayName}: {metric.CurrentValue:F2} (Avg: {metric.AverageValue:F2})");
            }
            Debug.Log("=========================");
        }
        
        public PerformanceReport GetPerformanceReport()
        {
            var report = new PerformanceReport();
            
            foreach (var kvp in _metrics)
            {
                report.Metrics[kvp.Key] = new MetricData
                {
                    CurrentValue = kvp.Value.CurrentValue,
                    AverageValue = kvp.Value.AverageValue,
                    MinValue = kvp.Value.MinValue,
                    MaxValue = kvp.Value.MaxValue
                };
            }
            
            return report;
        }
        
        public void ResetMetrics()
        {
            foreach (var metric in _metrics.Values)
            {
                metric.Reset();
            }
        }
    }
    
    /// <summary>
    /// Individual performance metric
    /// </summary>
    public class PerformanceMetric
    {
        public string DisplayName { get; }
        public float CurrentValue { get; private set; }
        public float AverageValue { get; private set; }
        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }
        
        private readonly List<float> _values = new List<float>();
        private const int MAX_SAMPLES = 60; // 1 second at 60 FPS
        
        public PerformanceMetric(string displayName)
        {
            DisplayName = displayName;
            Reset();
        }
        
        public void UpdateValue(float value)
        {
            CurrentValue = value;
            
            _values.Add(value);
            if (_values.Count > MAX_SAMPLES)
            {
                _values.RemoveAt(0);
            }
            
            // Update statistics
            float sum = 0f;
            MinValue = float.MaxValue;
            MaxValue = float.MinValue;
            
            foreach (float val in _values)
            {
                sum += val;
                MinValue = Mathf.Min(MinValue, val);
                MaxValue = Mathf.Max(MaxValue, val);
            }
            
            AverageValue = sum / _values.Count;
        }
        
        public void Reset()
        {
            CurrentValue = 0f;
            AverageValue = 0f;
            MinValue = 0f;
            MaxValue = 0f;
            _values.Clear();
        }
    }
    
    /// <summary>
    /// Performance report data structure
    /// </summary>
    [System.Serializable]
    public class PerformanceReport
    {
        public Dictionary<string, MetricData> Metrics = new Dictionary<string, MetricData>();
    }
    
    [System.Serializable]
    public struct MetricData
    {
        public float CurrentValue;
        public float AverageValue;
        public float MinValue;
        public float MaxValue;
    }
}
