# 🎯 Projectile Implementation Guide (Updated)

## 🚀 **Quick Start: Create Basic Projectile**

### **Step 1: Create Projectile Prefab**

1. **In Unity Editor:**
   ```
   Right-click in Hierarchy → Create Empty → Name it "Projectile"
   ```

2. **Add Visual Component:**
   ```
   Right-click on Projectile → 3D Object → Sphere
   Delete the original Empty GameObject
   Rename Sphere to "Projectile"
   ```

3. **Configure Sphere:**
   ```
   Scale: (0.2, 0.2, 0.2) - Small bullet size
   Position: (0, 0, 0)
   ```

4. **Add Required Components:**
   ```
   Add Component → Physics → Sphere Collider
   - Check "Is Trigger"
   - Radius: 0.1
   
   Add Component → Scripts → ProjectileBehavior
   ```

5. **Create Material:**
   ```
   Right-click in Project → Create → Material
   Name: "ProjectileMaterial"
   Color: Bright yellow/orange
   Shader: Standard
   ```

6. **Apply Material:**
   ```
   Drag ProjectileMaterial onto the Sphere
   ```

7. **Save as Prefab:**
   ```
   Drag Projectile from Hierarchy to Project window
   Delete from scene (keep prefab)
   ```

### **Step 2: Assign Projectiles to Units**

#### **For Tank Units (TankCombatUnit):**
```
1. Select your Tank GameObject in the scene
2. In the TankCombatUnit component inspector:
   
   [Projectile Settings]
   - Main Gun Projectile Prefab: Drag your tank shell prefab
   - Machine Gun Projectile Prefab: Drag your bullet prefab
   - Main Gun Projectile Speed: 15
   - Machine Gun Projectile Speed: 30
```

#### **For Other Combat Units (CombatUnit):**
```
1. Select your unit GameObject in the scene
2. In the CombatUnit component inspector:
   
   [Projectile Settings]
   - Projectile Prefab: Drag your projectile prefab
   - Projectile Speed: 20
```

### **Step 3: Test the System**

1. **Use TeamAssignmentTest:**
   ```
   - Add TeamAssignmentTest component to any GameObject
   - Check "Start Game On Start"
   - Press Play
   - Units should spawn and start fighting
   - You should see projectiles flying between units
   ```

## 🔧 **Advanced Configuration**

### **Step 4: Create Different Projectile Types**

#### **Tank Shell (Large Projectile)**
```
1. Duplicate Projectile prefab → "TankShell"
2. Scale: (0.5, 0.5, 0.5) - Larger
3. Material: Dark gray metallic
4. ProjectileBehavior settings:
   - Speed: 15 (slower)
   - Lifetime: 15 (longer range)
```

#### **Machine Gun Bullet (Small Projectile)**
```
1. Duplicate Projectile prefab → "MachineGunBullet"
2. Scale: (0.1, 0.1, 0.1) - Smaller
3. Material: Bright yellow
4. ProjectileBehavior settings:
   - Speed: 30 (faster)
   - Lifetime: 8 (shorter range)
```

#### **Soldier Bullet (Very Small)**
```
1. Duplicate Projectile prefab → "SoldierBullet"
2. Scale: (0.05, 0.05, 0.05) - Very small
3. Material: Bright orange
4. ProjectileBehavior settings:
   - Speed: 25
   - Lifetime: 6
```

### **Step 5: Add Visual Effects**

#### **Trail Effect**
```
1. Select Projectile prefab
2. Add Component → Effects → Trail Renderer
3. Configure:
   - Material: Create new material with "Particles/Additive" shader
   - Color: Bright yellow/orange
   - Width: Start 0.1, End 0.01
   - Time: 0.5
```

#### **Muzzle Flash**
```
1. Create Empty GameObject → "MuzzleFlash"
2. Add Particle System component
3. Configure for bright flash effect
4. Save as prefab
5. Assign to LightweightFireSystem "Fire Effect" field
```

## 🎮 **Testing & Debugging**

### **Visual Debugging**
```
1. Enable Gizmos in Scene view
2. ProjectileBehavior draws:
   - Red line: Projectile path
   - Yellow ray: Movement direction
   - Green sphere: Hit detection range
```

### **Console Debugging**
```
1. Check Console for:
   - "Created projectile at [position]"
   - "Projectile hit [target] for [damage] damage"
   - "No projectile prefab assigned!" (if missing)
   - "Projectile prefab missing ProjectileBehavior component!"
```

### **Common Issues & Solutions**

#### **Issue: No Projectiles Visible**
```
Solution:
1. Check projectile prefab is assigned in unit inspector
2. Verify projectile has MeshRenderer and material
3. Ensure projectile scale is not too small
4. Check projectile has ProjectileBehavior component
```

#### **Issue: Projectiles Not Moving**
```
Solution:
1. Check ProjectileBehavior.Initialize() is called
2. Verify target is not null
3. Check projectile speed > 0
4. Ensure target has valid Position
```

#### **Issue: No Damage Applied**
```
Solution:
1. Check target has Unit component
2. Verify Unit.TakeDamage() method exists
3. Check team assignments (no friendly fire)
4. Ensure target is not already destroyed
```

#### **Issue: Wrong Projectile Type Used**
```
Solution:
1. For tanks: Check both main gun and machine gun prefabs are assigned
2. Verify distance-based weapon selection is working
3. Check projectile speed settings match weapon type
```

## 📊 **Performance Optimization**

### **Object Pooling for Projectiles**
```csharp
// Create ProjectilePool component
public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int poolSize = 20;
    
    private Queue<GameObject> projectilePool;
    
    private void Start()
    {
        projectilePool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab);
            projectile.SetActive(false);
            projectilePool.Enqueue(projectile);
        }
    }
    
    public GameObject GetProjectile()
    {
        if (projectilePool.Count > 0)
        {
            GameObject projectile = projectilePool.Dequeue();
            projectile.SetActive(true);
            return projectile;
        }
        return Instantiate(projectilePrefab);
    }
    
    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
        projectilePool.Enqueue(projectile);
    }
}
```

### **Modify ProjectileBehavior**
```csharp
// Add to ProjectileBehavior
private void DestroyProjectile()
{
    // Try to return to pool first
    ProjectilePool pool = FindObjectOfType<ProjectilePool>();
    if (pool != null)
    {
        pool.ReturnProjectile(gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
}
```

## 🎯 **Final Checklist**

### **Before Testing:**
- [ ] Projectile prefab created with visual mesh
- [ ] ProjectileBehavior component added
- [ ] Collider component added (IsTrigger = true)
- [ ] Material applied to projectile
- [ ] Projectile prefab assigned to unit in inspector
- [ ] Tank units have both main gun and machine gun projectiles assigned
- [ ] Tank units spawned with proper team assignments

### **After Testing:**
- [ ] Projectiles visible during combat
- [ ] Projectiles move towards targets
- [ ] Damage applied when projectiles hit
- [ ] Projectiles destroyed after hitting
- [ ] Tanks use correct projectile type based on distance
- [ ] No console errors
- [ ] Performance acceptable (no lag)

### **Optional Enhancements:**
- [ ] Different projectile types for different weapons
- [ ] Trail effects on projectiles
- [ ] Muzzle flash effects
- [ ] Impact/explosion effects
- [ ] Sound effects for firing and impacts
- [ ] Object pooling for better performance

## 🚀 **Next Steps After Implementation**

1. **Test with multiple units** - Ensure system scales
2. **Add different projectile types** - Tank shells vs bullets vs soldier rounds
3. **Enhance visual effects** - Trails, explosions, particles
4. **Add sound effects** - Firing, impacts, explosions
5. **Optimize performance** - Object pooling, LOD
6. **Add projectile physics** - Gravity, wind, ricochet

## 🔄 **System Changes Summary**

### **What Changed:**
- ❌ **Removed**: Global projectile prefab from LightweightFireSystem
- ✅ **Added**: Individual projectile prefabs for each unit
- ✅ **Added**: Manual assignment in inspector
- ✅ **Added**: Tank-specific dual projectile system (main gun + machine gun)
- ✅ **Added**: Distance-based weapon selection for tanks

### **Benefits:**
- 🎯 **More Control**: Each unit can have unique projectiles
- 🔧 **Easier Setup**: No need to configure global systems
- 🎮 **Better Visuals**: Different projectiles for different weapons
- 📊 **Better Performance**: Units create their own projectiles directly

The basic projectile system should now be working! You should see visual projectiles flying between units during combat, with proper damage application when they hit targets. Each unit will use its own assigned projectile prefab.
