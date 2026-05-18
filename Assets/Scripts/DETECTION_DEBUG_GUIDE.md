# 🔍 Detection System Debug Guide

## 🚨 **Common Detection Issues & Solutions**

### **Issue 1: Tanks Not Recognizing Each Other**

**Possible Causes:**
1. **Missing Team Assignments** - Units have no team assigned
2. **Layer Mask Issues** - Units are on different layers than expected
3. **Missing Components** - Units lack required components
4. **Detection Radius Too Small** - Units are too far apart
5. **Collider Issues** - Units have no colliders or wrong collider types

### **Issue 2: AttackRange Property Error**

**Fixed:** The `AttackRange` property was not properly implemented in `CombatUnit.cs`. This has been fixed by:
- Making `AttackRange` abstract in `CombatUnit`
- Implementing it in `TankCombatUnit` to return weapon-specific ranges

## 🔧 **Debugging Steps**

### **Step 1: Use the Detection Debug Test**

1. **Attach the Debug Script:**
   - Add `DetectionDebugTest.cs` to any GameObject in your scene
   - This script will help identify detection issues

2. **Run the Test:**
   - Press `F1` to run detection diagnostics
   - Press `F2` to assign teams for testing
   - Check the Console for detailed output

### **Step 2: Check Team Assignments**

**Critical Issue:** Units must have teams assigned to be detected as enemies.

```csharp
// In your scene setup, ensure teams are assigned:
foreach (var unit in FindObjectsOfType<Unit>())
{
    // Assign alternating teams for testing
    Team team = (unitIndex % 2 == 0) ? Team.Player : Team.AI;
    unit.SetTeam(team);
}
```

### **Step 3: Verify Component Setup**

Each tank should have:
- ✅ `Unit` component (base unit functionality)
- ✅ `TankCombatUnit` component (combat behavior)
- ✅ `GunTurret` component(s) (visual rotation)
- ✅ `Collider` component (for detection)

### **Step 4: Check Layer Masks**

**Default Layer Mask:** `-1` (Everything)
**Custom Layer Mask:** Set specific layers for units

```csharp
// In CombatUnit inspector:
Enemy Layer Mask = -1  // Detects everything
// OR
Enemy Layer Mask = LayerMask.GetMask("Units")  // Detects only "Units" layer
```

### **Step 5: Verify Detection Radius**

**Default Detection Radius:** 15 meters
**Check Distance:** Ensure tanks are within detection range

```csharp
// In TankCombatUnit inspector:
Detection Radius = 15f  // Increase if tanks are too far apart
```

## 🐛 **Common Problems & Solutions**

### **Problem 1: "No CombatUnit components found"**
**Solution:** Ensure tanks have `TankCombatUnit` components attached

### **Problem 2: "Unit has no team assignment"**
**Solution:** Call `unit.SetTeam(Team.Player)` or `unit.SetTeam(Team.AI)` on all units

### **Problem 3: "Found 0 colliders in range"**
**Solutions:**
- Check if units have Collider components
- Verify Layer Mask settings
- Ensure units are within detection radius
- Check if units are on the correct layers

### **Problem 4: "Same team" validation failure**
**Solution:** Ensure units are assigned to different teams

### **Problem 5: "No health" validation failure**
**Solution:** Check if units have proper health values

## 🔍 **Debug Output Interpretation**

### **Good Detection Output:**
```
Testing Tank1:
  Position: (10, 0, 5)
  Team: Player
  Detection Radius: 15m
  Layer Mask: -1
  Found 2 colliders in range
    Unit: Tank2 at 8.5m, Team: AI
    Valid target: True
```

### **Bad Detection Output:**
```
Testing Tank1:
  Position: (10, 0, 5)
  Team: None  ← PROBLEM: No team assigned
  Detection Radius: 15m
  Layer Mask: -1
  Found 0 colliders in range  ← PROBLEM: No units detected
```

## 🛠️ **Quick Fixes**

### **Fix 1: Assign Teams**
```csharp
// Add this to your scene setup script
void AssignTeams()
{
    Unit[] units = FindObjectsOfType<Unit>();
    for (int i = 0; i < units.Length; i++)
    {
        Team team = (i % 2 == 0) ? Team.Player : Team.AI;
        units[i].SetTeam(team);
    }
}
```

### **Fix 2: Increase Detection Radius**
```csharp
// In TankCombatUnit inspector, increase detection radius
Detection Radius = 25f  // Instead of 15f
```

### **Fix 3: Check Layer Setup**
```csharp
// Ensure all units are on the same layer or adjust layer mask
// In CombatUnit inspector:
Enemy Layer Mask = -1  // Detects all layers
```

## 📋 **Checklist**

Before testing detection:
- [ ] All units have `Unit` components
- [ ] All tanks have `TankCombatUnit` components  
- [ ] All units have `Collider` components
- [ ] All units have teams assigned (Player or AI)
- [ ] Units are within detection radius of each other
- [ ] Layer masks are set correctly
- [ ] No compilation errors in console

## 🎯 **Testing Steps**

1. **Setup Scene:**
   - Place 2+ tanks in the scene
   - Ensure they have all required components
   - Position them within 15 meters of each other

2. **Assign Teams:**
   - Use the debug script or manually assign teams
   - Make sure tanks are on different teams

3. **Run Detection Test:**
   - Press F1 to run the detection debug test
   - Check console output for issues

4. **Verify Combat:**
   - Tanks should start targeting each other
   - Guns should rotate towards targets
   - Attacks should occur when conditions are met

## 🔧 **If Still Not Working**

1. **Check Console Errors:** Look for compilation or runtime errors
2. **Verify Component Hierarchy:** Ensure components are properly attached
3. **Test with Debug Script:** Use the provided debug script for detailed diagnostics
4. **Check Unity Inspector:** Verify all settings in the inspector
5. **Restart Unity:** Sometimes Unity needs a restart to recognize changes

---

**Remember:** The most common issue is missing team assignments. Always ensure units have teams assigned before testing detection!
