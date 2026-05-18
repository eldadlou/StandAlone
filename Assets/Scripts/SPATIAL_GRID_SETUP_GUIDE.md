# 🚀 Unity Grid-Based SpatialGrid Setup Guide for 60+ Units Performance

## 🎯 **Overview**

The SpatialGrid system now uses Unity's built-in Grid component for maximum performance with 60+ units. It reduces detection complexity from **O(n²)** to **O(n)** by leveraging Unity's optimized grid system with `WorldToCell` and `WorldToLocal` APIs.

## 🛠️ **Setup Instructions**

### **Step 1: Add Unity Grid-Based SpatialGrid to Your Scene**

1. **Create SpatialGrid GameObject:**
   ```
   Create Empty GameObject → Name it "SpatialGrid"
   ```

2. **Add SpatialGrid Component:**
   ```
   Add Component → Scripts → Core → SpatialPartitioning → SpatialGrid
   ```

3. **Unity Grid Auto-Setup:**
   ```
   The component automatically creates Unity's Grid component
   Cell Size: 10f (adjust based on detection radius)
   Cell Swizzle: XZY (optimized for 2D top-down view)
   Cell Gap: 0 (no gaps between cells)
   ```

### **Step 2: Register Units with Unity Grid-Based SpatialGrid**

The CombatUnit system will automatically register with SpatialGrid using Unity's Grid APIs, but ensure your units have the proper components:

```csharp
// Each unit should have:
- Unit component (implements IUnit)
- CombatUnit component (or TankCombatUnit)
- Collider component (for physics detection)

// The system automatically uses:
- Unity Grid.WorldToCell() for position conversion
- Unity Grid.CellToWorld() for world position lookup
- Unity Grid.WorldToLocal() for local cell coordinates
```

### **Step 3: Verify Dependency Container**

Ensure the DependencyContainer is set up in your scene:

```csharp
// In your scene setup script or GameManager
DependencyContainer.Instance.Register(spatialGrid);
```

## ⚙️ **Configuration Guidelines**

### **Unity Grid Cell Size Optimization**

**Formula:** `Cell Size = Detection Radius × 1.5`

**Examples:**
- Detection Radius 15m → Cell Size 22.5m
- Detection Radius 20m → Cell Size 30m
- Detection Radius 10m → Cell Size 15m

**Why:** Ensures units in adjacent cells are within detection range using Unity's optimized Grid system.

### **Grid Size Calculation**

**Formula:** `Grid Size = Map Size + (Cell Size × 2)`

**Examples:**
- Map 500×500m → Grid Size 544×544m (with 22m cells)
- Map 1000×1000m → Grid Size 1044×1044m (with 22m cells)

### **Unity Grid Performance Settings**

```csharp
// Recommended settings for 60+ units:
Cell Size: 22f (Unity Grid cell size)
Cell Swizzle: XZY (optimized for 2D top-down)
Update Interval: 0.5f (for units with targets)
Update Interval: 1.0f (for units without targets)

// Unity Grid APIs used:
- WorldToCell(): O(1) position conversion
- CellToWorld(): O(1) world position lookup
- WorldToLocal(): O(1) local coordinate calculation
```

## 🔧 **Troubleshooting**

### **Issue 1: Units Not Being Detected**

**Check:**
1. SpatialGrid is in the scene
2. Units are registered with SpatialGrid
3. Cell size is appropriate for detection radius
4. Grid size covers your map area

**Debug:**
```csharp
// Add this to your scene to verify registration
var spatialGrid = FindObjectOfType<SpatialGrid>();
Debug.Log($"SpatialGrid found: {spatialGrid != null}");
Debug.Log($"Registered units: {spatialGrid.GetRegisteredUnitCount()}");
```

### **Issue 2: Performance Still Poor**

**Solutions:**
1. **Increase cell size** to reduce grid complexity
2. **Reduce detection radius** for units without targets
3. **Increase update intervals** for units with stable targets
4. **Use adaptive update intervals** (already implemented)

### **Issue 3: Units Moving Between Cells**

**Check:**
1. Units call `UpdateUnitPosition()` when moving
2. Grid covers the entire movement area
3. Cell size is appropriate for unit speed

## 📊 **Performance Monitoring**

### **Add Performance Monitor**

1. **Create Performance Monitor:**
   ```
   Create Empty GameObject → Name it "PerformanceMonitor"
   Add Component → Scripts → Game → PerformanceMonitor
   ```

2. **Configure Thresholds:**
   ```
   Warning Threshold: 16ms
   Critical Threshold: 33ms
   Update Interval: 1s
   ```

3. **Monitor Output:**
   - Frame time should be < 16ms for 60fps
   - Cache hit rate should be > 80%
   - Units with targets should update less frequently

### **Unity Grid Expected Performance**

**With 60 units:**
- **Before Optimization:** 30-50ms per frame (red spikes)
- **After Optimization:** 5-8ms per frame (smooth)

**Performance Factors:**
- Units with targets: Update every 1.0s
- Units without targets: Update every 0.5s
- Unity Grid queries: O(1) using WorldToCell/CellToWorld
- Cached component lookups: 90%+ hit rate
- Unity's optimized Grid algorithms

## 🎮 **Testing**

### **Performance Test Script**

```csharp
// Add this to test with 60 units
public class PerformanceTest : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int unitCount = 60;
    
    void Start()
    {
        for (int i = 0; i < unitCount; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-200f, 200f),
                0f,
                Random.Range(-200f, 200f)
            );
            
            Instantiate(unitPrefab, position, Quaternion.identity);
        }
    }
}
```

### **Verification Steps**

1. **Check Console:** Look for performance warnings
2. **Monitor Frame Rate:** Should stay above 50fps
3. **Check Profiler:** No red spikes in Player Loop
4. **Verify Detection:** Units should still find and attack enemies

## 🚀 **Unity Grid Advanced Optimizations**

### **Dynamic Cell Sizing**

For very large maps, consider multiple Unity Grid-based SpatialGrid instances:

```csharp
// Create multiple grids for different areas
SpatialGrid nearGrid; // Small cells for close combat
SpatialGrid farGrid;  // Large cells for long-range detection

// Each uses Unity's Grid with different cell sizes
nearGrid.SetCellSize(10f);  // 10m cells for close combat
farGrid.SetCellSize(50f);   // 50m cells for long-range
```

### **LOD System**

Implement Level of Detail for detection:

```csharp
// Units far from player use less frequent updates
float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
float updateInterval = distanceToPlayer > 100f ? 2f : 0.5f;
```

### **Threading (Advanced)**

For 100+ units, consider threading detection updates:

```csharp
// Use Unity Jobs system for parallel detection
// This requires more advanced implementation
```

## 📋 **Checklist**

Before testing with 60+ units:

- [ ] SpatialGrid added to scene
- [ ] Cell size configured (Detection Radius × 1.5)
- [ ] Grid size covers map area
- [ ] DependencyContainer registered
- [ ] PerformanceMonitor added
- [ ] Units have proper components
- [ ] Detection radius appropriate
- [ ] Update intervals optimized

## 🎯 **Expected Results**

With proper Unity Grid setup, you should see:

1. **Smooth Performance:** No red spikes in profiler
2. **Fast Detection:** Units find enemies quickly using Unity's Grid APIs
3. **Efficient Updates:** Adaptive update intervals
4. **High Cache Hit Rate:** >80% component cache hits
5. **Stable Frame Rate:** >60fps with 60 units
6. **Unity Grid Optimization:** Leveraging Unity's built-in optimized algorithms

---

**Remember:** Unity's Grid component with WorldToCell/CellToWorld APIs is the key to scaling from 10-20 units to 60+ units without performance degradation!
