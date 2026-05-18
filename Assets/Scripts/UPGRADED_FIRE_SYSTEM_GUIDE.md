# 🔥 Upgraded FireSystem Guide

## 🎯 **New Features**

### **1. Gun Rotation System**
- **Automatic Targeting**: Guns automatically rotate to face enemies before shooting
- **Smooth Rotation**: Configurable rotation speed and smooth interpolation
- **Rotation Limits**: Optional angle limits for realistic turret movement
- **Visual Feedback**: Debug gizmos show rotation direction and limits

### **2. Enhanced Cooldown System**
- **Individual Cooldowns**: Each unit has its own attack cooldown
- **Configurable Timing**: Adjustable cooldown duration per unit type
- **Cooldown Validation**: Prevents spam attacks and ensures balanced combat

### **3. Detection Radius System**
- **Automatic Target Detection**: Units automatically find enemies within range
- **Nearest Target Priority**: Always targets the closest enemy
- **Team-Based Detection**: Only detects enemies from opposing teams
- **Visual Range Display**: Debug gizmos show detection radius

---

## 🛠️ **Setup Instructions**

### **Step 1: Configure FireSystem Component**

Add the `FireSystem` component to a GameObject in your scene and configure:

#### **Combat Settings**
```
✅ Enable Projectiles: true
🎯 Projectile Prefab: [Your projectile prefab]
⚡ Projectile Speed: 20
```

#### **Detection & Targeting**
```
🔍 Detection Radius: 15 (adjust based on unit type)
🎯 Enemy Layer Mask: -1 (all layers)
⏱️ Target Update Interval: 0.5 (seconds)
```

#### **Gun Rotation**
```
🔄 Gun Rotation Speed: 90 (degrees per second)
🎯 Rotation Threshold: 5 (degrees tolerance)
🔧 Gun Turret: [Assign turret transform]
✅ Use Gun Turret Component: true
```

#### **Attack Cooldown**
```
⏰ Attack Cooldown: 2 (seconds)
✅ Use Individual Cooldowns: true
```

### **Step 2: Add GunTurret Component to Units**

For each unit that should have rotating guns/turrets:

1. **Create Gun/Turret GameObject**:
   - Create a child GameObject under your unit
   - Name it "GunTurret" or "Weapon"
   - Position it where the gun should be

2. **Add GunTurret Component**:
   ```
   🎯 Rotation Speed: 90
   🎯 Rotation Threshold: 5
   ✅ Smooth Rotation: true
   ❌ Limit Rotation: false (or true with custom limits)
   ```

3. **Configure Rotation Limits** (optional):
   ```
   📐 Min Rotation Angle: -180
   📐 Max Rotation Angle: 180
   ```

### **Step 3: Subscribe Units to FireSystem**

In your unit setup code:

```csharp
// Get LightweightFireSystem reference
var fireSystem = SystemInitializer.GetSystem<LightweightFireSystem>();

// Note: Units automatically register with LightweightFireSystem via CombatUnit component
// No manual subscription needed - the system handles this automatically
```

---

## 🎮 **How It Works**

### **Target Detection Cycle**
1. **Scan for Enemies**: Every 0.5 seconds, scan for enemies within detection radius
2. **Find Nearest Target**: Select the closest enemy unit
3. **Validate Attack**: Check range, cooldown, and team restrictions
4. **Rotate Gun**: If not facing target, start gun rotation
5. **Fire**: Once gun is facing target and cooldown is ready, attack

### **Gun Rotation Process**
1. **Calculate Direction**: Determine direction to target
2. **Smooth Rotation**: Gradually rotate gun towards target
3. **Check Alignment**: Verify gun is facing target within threshold
4. **Fire When Ready**: Attack once properly aligned

### **Cooldown Management**
1. **Track Last Attack**: Record timestamp of last attack
2. **Check Cooldown**: Verify enough time has passed
3. **Prevent Spam**: Block attacks during cooldown period

---

## 📊 **Unit Configuration Examples**

### **Tank Configuration**
```
🔍 Detection Radius: 20
⚡ Gun Rotation Speed: 60
⏰ Attack Cooldown: 3
🎯 Attack Range: 15
```

### **Soldier Configuration**
```
🔍 Detection Radius: 12
⚡ Gun Rotation Speed: 120
⏰ Attack Cooldown: 1.5
🎯 Attack Range: 10
```

### **Aircraft Configuration**
```
🔍 Detection Radius: 25
⚡ Gun Rotation Speed: 180
⏰ Attack Cooldown: 2
🎯 Attack Range: 20
```

---

## 🔧 **Advanced Configuration**

### **Custom Gun Turret Setup**

For complex units with multiple weapons:

```csharp
// Get GunTurret component
var gunTurret = unit.GetComponentInChildren<GunTurret>();

// Configure rotation limits
gunTurret.SetRotationLimits(true, -90f, 90f);

// Set custom rotation speed
gunTurret.SetRotationSpeed(120f);

// Set rotation threshold
gunTurret.SetRotationThreshold(3f);
```

### **Dynamic Detection Radius**

```csharp
// Adjust detection radius based on unit upgrades
fireSystem.SetDetectionRadius(newRadius);

// Adjust rotation speed based on unit type
fireSystem.SetGunRotationSpeed(newSpeed);

// Adjust attack cooldown
fireSystem.SetAttackCooldown(newCooldown);
```

### **Target Information**

```csharp
// Get current target for a unit
IUnit currentTarget = fireSystem.GetCurrentTarget(unit);

// Check if unit is currently rotating
bool isRotating = fireSystem.IsRotating(unit);
```

---

## 🐛 **Debugging & Troubleshooting**

### **Enable Debug Visualization**
1. Select the FireSystem GameObject in the scene
2. Enable "Show Rotation Gizmos" in GunTurret components
3. View detection radius and target lines in Scene view

### **Common Issues**

#### **Guns Not Rotating**
- ✅ Check if GunTurret component is attached
- ✅ Verify gunTurret transform is assigned
- ✅ Ensure rotation speed > 0
- ✅ Check if target is within detection radius

#### **Units Not Attacking**
- ✅ Verify units are subscribed to FireSystem
- ✅ Check attack cooldown settings
- ✅ Ensure target is within attack range
- ✅ Verify team assignments are correct

#### **Performance Issues**
- ✅ Increase target update interval
- ✅ Reduce detection radius
- ✅ Limit number of units with gun turrets

---

## 🎯 **Testing Checklist**

### **Basic Functionality**
- [ ] Units detect enemies within range
- [ ] Guns rotate towards targets
- [ ] Attacks fire after gun alignment
- [ ] Cooldowns prevent spam attacks
- [ ] Team restrictions work correctly

### **Advanced Features**
- [ ] Rotation limits work (if enabled)
- [ ] Multiple units can target different enemies
- [ ] Units switch targets when current target dies
- [ ] Gun rotation is smooth and realistic
- [ ] Debug gizmos display correctly

### **Performance**
- [ ] No frame rate drops with many units
- [ ] Target detection updates at reasonable intervals
- [ ] Memory usage remains stable
- [ ] No memory leaks from subscriptions

---

## 🚀 **Next Steps**

### **Potential Enhancements**
1. **Different Weapon Types**: Support for multiple weapons per unit
2. **Aiming Accuracy**: Add accuracy modifiers and spread
3. **Cover System**: Units take cover and reduce detection
4. **Stealth Mechanics**: Some units are harder to detect
5. **Weapon Upgrades**: Improve rotation speed, range, or damage

### **Integration Ideas**
1. **AI Behavior**: Integrate with AI decision making
2. **Formation Combat**: Coordinate attacks in formations
3. **Terrain Effects**: Different detection ranges on different terrain
4. **Weather Effects**: Reduced detection in fog/rain
5. **Night/Day Cycle**: Different detection ranges based on time

---

## 📝 **Code Examples**

### **Manual Target Setting**
```csharp
// Force a unit to target a specific enemy
fireSystem.SetCurrentTarget(unit, target);
```

### **Custom Attack Logic**
```csharp
// Override default attack behavior
if (fireSystem.CanAttackTarget(unit, target))
{
    fireSystem.PerformAttack(unit, target);
}
```

### **Event Handling**
```csharp
// Listen for attack events
unit.OnAttack += (attacker, target) => {
    Debug.Log($"{attacker.name} attacked {target.name}");
};
```

This upgraded FireSystem provides a complete combat solution with realistic gun rotation, proper cooldown management, and intelligent target detection!
