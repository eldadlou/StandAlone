# 🔥 Fire System - How To Make It Work

## 🎯 **Overview**

This guide will walk you through setting up the complete fire system in Unity, from creating prefabs to testing combat functionality.

## 📋 **Prerequisites**

- Unity project with the fire system scripts
- Basic understanding of Unity prefabs and components
- Scene with terrain or ground for units to move on

## 🚀 **Step 1: Create Tank Prefabs**

### **1.1 Create Base Tank GameObject**
```
1. Right-click in Hierarchy → Create Empty
2. Rename to "Big Tank" (or "Small Tank", "Heavy Tank", etc.)
3. Add a basic 3D model or primitive (Cube, Capsule) as child
4. Position at (0, 0, 0)
```

### **1.2 Add Required Components**
```
Select the tank GameObject and add these components:

✅ TankCombatUnit (Script)
✅ Unit (Script) 
✅ Rigidbody (Physics)
✅ Collider (Box Collider or Capsule Collider)
✅ GunTurret (Script) - Add as child object
```

### **1.3 Configure TankCombatUnit Settings**
```
In the TankCombatUnit component inspector:

[Header("Tank Combat Settings")]
Main Gun Damage: 50
Main Gun Range: 20
Main Gun Cooldown: 3
Machine Gun Damage: 10
Machine Gun Range: 8
Machine Gun Cooldown: 0.5

[Header("Tank Components")]
Main Gun Turret: Assign main gun transform
Machine Gun Turret: Assign machine gun transform
```

### **1.4 Configure Unit Component**
```
In the Unit component inspector:
- Set appropriate health and speed values
- Assign team (Player or AI)
```

### **1.5 Configure GunTurret Component**
```
In the GunTurret component inspector:

[Header("Turret Settings")]
Rotation Speed: 90
Rotation Threshold: 5
Smooth Rotation: ✓
Limit Rotation: ✗

[Header("Rotation Limits")]
Min Rotation Angle: -180
Max Rotation Angle: 180
```

### **1.6 Create Gun Turret Children**
```
1. Create child GameObject named "MainGunTurret"
2. Create child GameObject named "MachineGunTurret"
3. Position them appropriately on the tank model
4. Assign them in TankCombatUnit inspector
```

### **1.7 Save as Prefab**
```
1. Drag the configured tank from Hierarchy to Project window
2. Delete from scene (keep prefab)
3. Repeat for different tank types with different names
```

## 🎮 **Step 2: Set Up the Scene**

### **2.1 Create Ground**
```
1. Create Plane or Terrain for units to move on
2. Ensure it has a Collider component
3. Set appropriate layer (e.g., "Ground")
```

### **2.2 Add LightweightFireSystem**
```
1. Create Empty GameObject named "FireSystem"
2. Add LightweightFireSystem component
3. Configure in inspector:

[Header("Global Combat Settings")]
Enable Projectiles: ✓
Projectile Prefab: (will create next)
Projectile Speed: 20

[Header("Combat Effects")]
Explosion Prefab: (optional)
Fire Sound: (optional)
Explosion Sound: (optional)
```

### **2.3 Create Projectile Prefab**
```
1. Create Sphere or other projectile model
2. Add ProjectileBehavior component
3. Configure settings:

[Header("Projectile Settings")]
Speed: 20
Lifetime: 10
Destroy On Hit: ✓
Hit Effect: (optional)

4. Save as prefab
5. Assign to LightweightFireSystem Projectile Prefab field
```

### **2.4 Set Up Layers**
```
1. Edit → Project Settings → Tags and Layers
2. Create layers:
   - "Player" (Layer 8)
   - "AI" (Layer 9)
   - "Ground" (Layer 10)

3. Assign layers to:
   - Player tanks: "Player" layer
   - AI tanks: "AI" layer
   - Ground: "Ground" layer
```

## 🔧 **Step 3: Configure Combat Settings**

### **3.1 Set Enemy Layer Masks**
```
In each TankCombatUnit inspector:

[Header("Combat Settings")]
Detection Radius: 15
Enemy Layer Mask: 
  - Player tanks: Set to "AI" layer
  - AI tanks: Set to "Player" layer
Gun Rotation Speed: 90
Rotation Threshold: 5
Target Update Interval: 0.5
```

### **3.2 Configure Detection**
```
The system will automatically:
- Detect enemies within detection radius
- Find closest enemy as target
- Update target every 0.5 seconds
- Switch targets when closer enemies appear
```

## 🎯 **Step 4: Spawn Units in Scene**

### **4.1 Spawn Player Tank**
```
1. Drag "Big Tank" prefab into scene
2. Position at (0, 0, 1)
3. In Unit component, set team to "Player"
4. Ensure it's on "Player" layer
```

### **4.2 Spawn AI Tank**
```
1. Drag "Small Tank" prefab into scene
2. Position at (10, 0, 1)
3. In Unit component, set team to "AI"
4. Ensure it's on "AI" layer
```

### **4.3 Verify Components**
```
Each tank should have:
✅ TankCombatUnit
✅ Unit
✅ Rigidbody
✅ Collider
✅ GunTurret (as child)
✅ Proper layer assignment
```

## 🧪 **Step 5: Test the System**

### **5.1 Basic Functionality Test**
```
1. Enter Play mode
2. Check Console for registration messages:
   "Registered combat unit: [Tank Name]"

3. Move tanks close to each other (within detection radius)
4. Watch for targeting messages:
   "Big Tank (Big Tank) targeting Small Tank (Small Tank)"
```

### **5.2 Combat Test**
```
1. Position tanks within attack range
2. Watch gun turrets rotate toward targets
3. Check for attack messages and projectile creation
4. Verify cooldown system (attacks every few seconds)
```

### **5.3 Debug Visualization**
```
1. Select tanks in scene view
2. Look for colored gizmos:
   - Yellow/Red sphere: Detection radius
   - Red sphere: Attack range
   - Green/Magenta line: Target line
   - Blue/Cyan spheres: Weapon ranges (tanks only)
```

## 🔍 **Step 6: Troubleshooting**

### **6.1 Common Issues & Solutions**

#### **Issue: "No GunTurret component found"**
```
Solution:
1. Ensure GunTurret component is added to tank
2. Check that GunTurret is child of tank GameObject
3. Verify component is enabled
```

#### **Issue: Units not detecting enemies**
```
Solution:
1. Check Enemy Layer Mask settings
2. Verify units are on correct layers
3. Ensure detection radius is large enough
4. Check that enemies have IUnit components
```

#### **Issue: Guns not rotating**
```
Solution:
1. Verify GunTurret component is working
2. Check rotation speed and threshold settings
3. Ensure targets are within detection range
4. Look for rotation gizmos in scene view
```

#### **Issue: No projectiles being created**
```
Solution:
1. Check LightweightFireSystem Projectile Prefab assignment
2. Verify Enable Projectiles is checked
3. Ensure units are within attack range
4. Check cooldown settings
```

### **6.2 Debug Commands**
```
Add these to your test script for debugging:

// Force target assignment
tankCombatUnit.SetTarget(enemyUnit);

// Check combat state
Debug.Log($"In Combat: {tankCombatUnit.IsInCombat}");
Debug.Log($"Target: {tankCombatUnit.CurrentTarget?.Name}");
Debug.Log($"In Range: {tankCombatUnit.IsTargetInRange}");
Debug.Log($"Gun Facing: {tankCombatUnit.IsGunFacingTarget}");

// Force attack
tankCombatUnit.TryAttack();
```

## ⚙️ **Step 7: Advanced Configuration**

### **7.1 Custom Tank Types**
```
Create new tank prefabs with different stats:

Heavy Tank:
- Main Gun Damage: 100
- Main Gun Range: 30
- Main Gun Cooldown: 5
- Machine Gun Damage: 15
- Machine Gun Range: 10
- Machine Gun Cooldown: 1

Light Tank:
- Main Gun Damage: 25
- Main Gun Range: 15
- Main Gun Cooldown: 2
- Machine Gun Damage: 5
- Machine Gun Range: 6
- Machine Gun Cooldown: 0.3
```

### **7.2 Performance Optimization**
```
For many units, adjust these settings:

1. Increase Target Update Interval (0.5 → 1.0 seconds)
2. Reduce Detection Radius (15 → 10)
3. Use spatial partitioning for large maps
4. Implement object pooling for projectiles
```

### **7.3 Custom Weapon Systems**
```
To add new weapon types:

1. Extend TankCombatUnit class
2. Add new weapon properties
3. Implement weapon selection logic
4. Add weapon-specific turret transforms
5. Override Attack methods for custom behavior
```

## 🎮 **Step 8: Integration with Game Systems**

### **8.1 Connect to Game Manager**
```
1. Ensure Unit components register with GameManager
2. Connect LightweightFireSystem events to game logic
3. Handle unit death and respawning
4. Manage team assignments dynamically
```

### **8.2 Add UI Elements**
```
1. Create health bars for units
2. Show current target information
3. Display weapon selection status
4. Add combat log for debugging
```

### **8.3 Save/Load System**
```
1. Save tank configurations and positions
2. Load saved combat scenarios
3. Persist unit stats and upgrades
4. Save/load team assignments
```

## ✅ **Verification Checklist**

### **Basic Setup**
- [ ] Tank prefabs created with all required components
- [ ] LightweightFireSystem added to scene
- [ ] Projectile prefab created and assigned
- [ ] Layers configured (Player, AI, Ground)
- [ ] Units spawned in scene with proper teams

### **Functionality Test**
- [ ] Units register with fire system on startup
- [ ] Target detection works (enemies found within radius)
- [ ] Gun turrets rotate toward targets
- [ ] Attacks execute when conditions are met
- [ ] Projectiles are created and move toward targets
- [ ] Cooldown system prevents spam attacks

### **Advanced Features**
- [ ] Weapon selection works (main gun vs machine gun)
- [ ] Fallback rotation works when GunTurret unavailable
- [ ] Debug gizmos display correctly
- [ ] Console logging shows clear vehicle identification
- [ ] Team-based targeting works correctly

## 🚀 **Quick Start Summary**

```
1. Create tank prefab with TankCombatUnit + Unit + GunTurret
2. Add LightweightFireSystem to scene
3. Create projectile prefab with ProjectileBehavior
4. Set up layers (Player, AI, Ground)
5. Spawn units with different teams
6. Test by moving units close together
7. Watch for automatic targeting and combat
```

## 🎯 **Expected Behavior**

Once set up correctly, you should see:
- **Automatic Detection**: Units find enemies within range
- **Smart Targeting**: Guns rotate smoothly toward targets
- **Weapon Selection**: Appropriate weapon chosen based on distance
- **Combat Execution**: Attacks fire when conditions are met
- **Visual Feedback**: Projectiles, gizmos, and debug information
- **Team Coordination**: Units only target enemy teams

The system is designed to work automatically - just set up the prefabs and spawn units, and they'll handle combat independently! 🎉

## 🔗 **Next Steps**

After basic setup works:
1. **Add more tank types** with different stats
2. **Implement unit upgrades** and progression
3. **Add special abilities** and skills
4. **Create mission scenarios** with multiple units
5. **Optimize performance** for large battles
6. **Add AI behavior** for autonomous units

The fire system provides a solid foundation for any tank combat game! 🚗💥
