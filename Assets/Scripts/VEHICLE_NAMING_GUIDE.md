# 🚗 Vehicle Naming System Guide

## 🎯 **Overview**

The fire system now includes a `Name` property that represents the specific vehicle type, separate from the owner (Player/AI). This allows you to have different tank variants with descriptive names.

## 🔧 **How It Works**

### **1. Name Property Implementation**

```csharp
// In CombatUnit (Base Class)
public abstract string Name { get; }  // Vehicle type name (e.g., "Big Tank", "Small Tank")

// In TankCombatUnit (Implementation)
public override string Name => gameObject.name;  // Returns the prefab name
```

### **2. Prefab Naming Convention**

Name your prefabs descriptively to get meaningful vehicle names:

```
Prefab Names → Display Names
├── "Big Tank" → "Big Tank"
├── "Small Tank" → "Small Tank"  
├── "Heavy Tank" → "Heavy Tank"
├── "Light Tank" → "Light Tank"
└── "Artillery Tank" → "Artillery Tank"
```

### **3. Usage Examples**

```csharp
// Getting vehicle information
ICombatUnit tank = GetComponent<TankCombatUnit>();
string vehicleName = tank.Name;        // "Big Tank"
string prefabName = gameObject.name;   // "Big Tank"
Player owner = tank.Owner;             // Player1 or AI
Team team = owner.Team;                // Player or AI

// Debug output will show:
// "Big Tank (Big Tank) targeting Small Tank (Small Tank)"
```

## 🎮 **Prefab Setup Examples**

### **Example 1: Big Tank Prefab**
```
GameObject: "Big Tank"
├── TankCombatUnit
│   ├── mainGunDamage: 75f
│   ├── mainGunRange: 25f
│   └── mainGunCooldown: 4f
├── Unit (Base Unit Component)
└── GunTurret
```

### **Example 2: Small Tank Prefab**
```
GameObject: "Small Tank"
├── TankCombatUnit
│   ├── mainGunDamage: 35f
│   ├── mainGunRange: 15f
│   └── mainGunCooldown: 2f
├── Unit (Base Unit Component)
└── GunTurret
```

### **Example 3: Heavy Tank Prefab**
```
GameObject: "Heavy Tank"
├── TankCombatUnit
│   ├── mainGunDamage: 100f
│   ├── mainGunRange: 30f
│   └── mainGunCooldown: 5f
├── Unit (Base Unit Component)
└── GunTurret
```

## 🔄 **System Benefits**

### **1. Clear Identification**
- **Before**: "Unit targeting Unit" (confusing)
- **After**: "Big Tank targeting Small Tank" (clear)

### **2. Easy Debugging**
- Know exactly which vehicle types are fighting
- Track performance by vehicle type
- Identify balance issues

### **3. Future Extensibility**
- Easy to add new vehicle types
- Can implement vehicle-specific logic
- Support for different weapon loadouts

## 📝 **Implementation Details**

### **CombatUnit Base Class**
```csharp
public abstract class CombatUnit : MonoBehaviour, ICombatUnit
{
    // Vehicle identification
    public abstract string Name { get; }  // Vehicle type name
    
    // Debug logging now shows both names
    Debug.Log($"{Name} ({gameObject.name}) targeting {target.Name} ({target.name})");
}
```

### **TankCombatUnit Implementation**
```csharp
public class TankCombatUnit : CombatUnit
{
    // Returns the prefab name as the vehicle type
    public override string Name => gameObject.name;
    
    // Can be overridden for custom naming logic
    // public override string Name => GetCustomVehicleName();
}
```

### **Custom Naming (Optional)**
```csharp
// If you want custom names instead of prefab names
public override string Name => GetCustomVehicleName();

private string GetCustomVehicleName()
{
    // Custom logic based on stats, appearance, etc.
    if (mainGunDamage > 80f) return "Heavy Tank";
    if (mainGunDamage > 50f) return "Medium Tank";
    return "Light Tank";
}
```

## 🎯 **Current Status**

✅ **Implemented**: Basic Name property returning prefab name
✅ **Integrated**: Debug logging shows vehicle types
✅ **Ready**: System can handle multiple tank variants

## 🚀 **Next Steps**

1. **Create Prefabs**: Make different tank prefabs with descriptive names
2. **Test Naming**: Verify debug logs show proper vehicle identification
3. **Extend System**: Add more vehicle types (APC, Artillery, etc.)
4. **Custom Logic**: Implement vehicle-specific behaviors if needed

## 📊 **Example Debug Output**

```
Big Tank (Big Tank) targeting Small Tank (Small Tank)
Projectile hit Small Tank (Player) at (10, 0, 5)
Heavy Tank (Heavy Tank) targeting Big Tank (Big Tank)
```

This system now provides clear, meaningful identification of different vehicle types in your combat system! 🎉
