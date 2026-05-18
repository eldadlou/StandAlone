# 🎯 Accuracy System Guide

## 📋 **Overview**

The accuracy system adds realistic combat mechanics where units can miss, partially hit, or fully hit their targets based on various factors like movement, weapon type, and base accuracy stats.

## 🎯 **Hit Results**

### **Three Possible Outcomes:**
1. **Full Hit** - Perfect accuracy, full damage
2. **Partial Hit** - Reduced accuracy, partial damage (50-60% of full damage)
3. **Miss** - Complete miss, no damage (projectile still flies but misses target)

## ⚙️ **Configuration**

### **TankCombatUnit Settings**
```
[Accuracy Settings]
- Main Gun Accuracy: 85% (base accuracy for main gun)
- Machine Gun Accuracy: 75% (base accuracy for machine gun)
- Moving Accuracy Penalty: 25% (penalty when tank is moving)
- Max Accuracy Penalty: 50% (maximum penalty cap)
- Partial Hit Damage Multiplier: 0.5 (50% damage for partial hits)
- Partial Hit Chance: 30% (chance of partial hit vs complete miss)
```

### **CombatUnit (Base) Settings**
```
[Base Accuracy Settings]
- Base Unit Accuracy: 80% (base accuracy for standard units)
- Base Moving Accuracy Penalty: 20% (penalty when unit is moving)
- Base Max Accuracy Penalty: 40% (maximum penalty cap)
- Base Partial Hit Damage Multiplier: 0.6 (60% damage for partial hits)
- Base Partial Hit Chance: 25% (chance of partial hit vs complete miss)
```

## 🔧 **How It Works**

### **1. Accuracy Calculation**
```csharp
Current Accuracy = Base Accuracy - Movement Penalty
```

**Example:**
- Tank with 85% base accuracy
- Moving (-25% penalty)
- **Final Accuracy: 60%**

### **2. Hit Determination**
```csharp
Random Roll (0-100) vs Current Accuracy
```

**Results:**
- **Roll ≤ Accuracy** → Full Hit
- **Roll > Accuracy** → Miss or Partial Hit (based on Partial Hit Chance)

### **3. Damage Calculation**
```csharp
Full Hit: Base Damage × 1.0
Partial Hit: Base Damage × Partial Hit Multiplier
Miss: 0 damage
```

## 🎮 **Visual Effects**

### **Projectile Behavior**
- **Full Hit**: Projectile flies directly to target
- **Partial Hit**: Projectile deviates slightly (1.2-1.5 units)
- **Miss**: Projectile deviates significantly (2.5-3.0 units)

### **Console Logging**
```
Tank1: Accuracy check - Accuracy: 60.0% (Base: 85.0%, Moving: True, Penalty: 25.0%, Weapon: Main Gun)
Tank1: Hit result: PartialHit, Damage: 25.0
Tank1 -> Tank2: PARTIAL HIT for 25.0 damage
```

## 📊 **Example Scenarios**

### **Scenario 1: Stationary Tank**
```
Base Accuracy: 85%
Movement Penalty: 0% (not moving)
Final Accuracy: 85%
Result: High chance of full hits
```

### **Scenario 2: Moving Tank**
```
Base Accuracy: 85%
Movement Penalty: 25% (moving)
Final Accuracy: 60%
Result: More misses and partial hits
```

### **Scenario 3: Machine Gun vs Main Gun**
```
Main Gun: 85% accuracy (stationary)
Machine Gun: 75% accuracy (stationary)
Result: Main gun more accurate but slower
```

## 🎯 **Tuning Guidelines**

### **For Realistic Combat:**
- **Tanks**: 80-90% base accuracy
- **Infantry**: 70-85% base accuracy
- **Artillery**: 60-75% base accuracy
- **Moving Penalty**: 20-30%
- **Partial Hit Chance**: 20-40%

### **For Arcade-Style Combat:**
- **All Units**: 90-95% base accuracy
- **Moving Penalty**: 10-15%
- **Partial Hit Chance**: 10-20%

### **For Hardcore Combat:**
- **Tanks**: 60-75% base accuracy
- **Moving Penalty**: 30-50%
- **Partial Hit Chance**: 40-60%

## 🔧 **Advanced Configuration**

### **Weapon-Specific Accuracy**
```csharp
// In TankCombatUnit
public float GetWeaponAccuracy()
{
    return useMainGun ? mainGunAccuracy : machineGunAccuracy;
}
```

### **Distance-Based Accuracy**
```csharp
public float CalculateDistanceAccuracy(float distance, float maxRange)
{
    float distanceFactor = 1f - (distance / maxRange);
    return baseAccuracy * distanceFactor;
}
```

### **Terrain-Based Accuracy**
```csharp
public float CalculateTerrainAccuracy()
{
    // Add terrain modifiers here
    return baseAccuracy;
}
```

## 🧪 **Testing**

### **Test Scenarios:**
1. **Stationary vs Moving** - Compare accuracy differences
2. **Different Weapons** - Test main gun vs machine gun
3. **Multiple Shots** - Verify hit distribution over time
4. **Edge Cases** - Test with 0% accuracy, 100% accuracy

### **Expected Results:**
- Moving units should miss more often
- Higher accuracy weapons should hit more consistently
- Partial hits should occur at reasonable frequency
- Console should show detailed accuracy information

## 🎮 **UI Integration**

### **Accuracy Display**
```csharp
public void UpdateAccuracyUI()
{
    var accuracyInfo = GetAccuracyInfo();
    accuracyText.text = $"Accuracy: {accuracyInfo.CurrentAccuracy:F0}%";
    
    if (accuracyInfo.IsMoving)
    {
        accuracyText.color = Color.yellow; // Warning color
    }
    else
    {
        accuracyText.color = Color.white; // Normal color
    }
}
```

### **Hit Result Display**
```csharp
public void ShowHitResult(HitResult result)
{
    string message = result switch
    {
        HitResult.FullHit => "FULL HIT!",
        HitResult.PartialHit => "Partial Hit",
        HitResult.Miss => "MISS!",
        _ => "Unknown"
    };
    
    // Display message to player
}
```

## 🚀 **Performance Considerations**

### **Optimization Tips:**
- Accuracy calculations are lightweight
- Random number generation is minimal
- No additional physics calculations
- Console logging can be disabled in production

### **Memory Usage:**
- Minimal additional memory overhead
- No persistent objects created
- Temporary calculations only

## ✅ **Benefits**

### **Gameplay:**
- ✅ **More realistic combat** - movement affects accuracy
- ✅ **Strategic depth** - positioning matters
- ✅ **Visual feedback** - players can see misses
- ✅ **Weapon variety** - different weapons have different accuracy

### **Technical:**
- ✅ **Configurable** - easy to tune for different game styles
- ✅ **Extensible** - can add more accuracy modifiers
- ✅ **Performance friendly** - minimal overhead
- ✅ **Debug friendly** - detailed logging for testing

The accuracy system adds depth and realism to combat while remaining easy to understand and configure!
