# 🎯 Tank Organization Guide

## 📋 **Overview**

I've reorganized the tank system to eliminate duplicate properties and provide better visibility of tank stats in the inspector. Here's what changed:

## 🔧 **Changes Made**

### **1. UnitData Now Serializable**
- **File**: `Core/Units/UnitData.cs`
- **Change**: Added `[System.Serializable]` and `[SerializeField]` attributes
- **Result**: UnitData is now visible in the inspector

### **2. Tank Class Reorganized**
- **File**: `Core/Units/Tank.cs`
- **Removed**: Duplicate combat properties (damage, range, cooldown)
- **Kept**: Base stats (health, speed, armor)
- **Added**: Reference to weapon-specific stats from TankCombatUnit

### **3. TankCombatUnit Enhanced**
- **File**: `Core/Units/Combat/TankCombatUnit.cs`
- **Added**: Public properties to access weapon stats
- **Purpose**: Allow Tank class to display weapon information

### **4. Custom Inspector**
- **File**: `Editor/TankInspectorEditor.cs`
- **Features**: 
  - Real-time health display with color coding
  - Weapon stats display
  - Combat state information
  - Team assignment status

### **5. Health Display Component**
- **File**: `Presentation/HealthDisplay.cs`
- **Features**: 
  - Health bar above units in game world
  - Distance-based visibility
  - Color-coded health (green to red)

## 🏗️ **New Tank Structure**

### **Tank.cs (Base Stats)**
```csharp
[Header("Tank Base Stats")]
[SerializeField] private float tankHealth = 150f;
[SerializeField] private float tankSpeed = 5f;
[SerializeField] private string ownerName;

[Header("Tank Combat (Weapon-specific stats handled by TankCombatUnit)")]
[SerializeField] private bool showCombatInfo = true;

[Header("Tank Armor")]
[SerializeField] private float armorDamageReduction = 0.2f; // 20% damage reduction
```

### **TankCombatUnit.cs (Weapon Stats)**
```csharp
[Header("Tank Combat Settings")]
[SerializeField] private float mainGunDamage = 50f;
[SerializeField] private float mainGunRange = 20f;
[SerializeField] private float mainGunCooldown = 3f;
[SerializeField] private float machineGunDamage = 10f;
[SerializeField] private float machineGunRange = 8f;
[SerializeField] private float machineGunCooldown = 0.5f;

// Public properties for display
public float MainGunDamage => mainGunDamage;
public float MainGunRange => mainGunRange;
public float MainGunCooldown => mainGunCooldown;
public float MachineGunDamage => machineGunDamage;
public float MachineGunRange => machineGunRange;
public float MachineGunCooldown => machineGunCooldown;
```

### **UnitData.cs (Runtime Stats)**
```csharp
[System.Serializable]
public class UnitData
{
    [SerializeField] public float Health { get; set; }
    [SerializeField] public float Speed { get; set; }
    [SerializeField] public Player Owner { get; set; }
    [SerializeField] public UnitType Type { get; set; }
    // ... more properties
}
```

## 🎮 **How to Use**

### **Step 1: Setup Tank Prefab**
1. **Add Components**:
   - `Tank` (base stats)
   - `TankCombatUnit` (weapon stats)
   - `HealthDisplay` (optional - for in-game health display)

2. **Configure Stats**:
   - Set base health and speed in `Tank` component
   - Set weapon stats in `TankCombatUnit` component
   - Configure armor reduction in `Tank` component

### **Step 2: View Stats in Inspector**
The custom inspector will show:
- **Current Unit Data**: Real-time health, speed, team
- **Combat Information**: Weapon stats and current weapon selection
- **Combat State**: Whether in combat, current target, etc.

### **Step 3: In-Game Health Display**
1. **Create Health Bar Prefab**: UI Slider with background and fill
2. **Create Health Text Prefab**: UI Text element
3. **Assign to HealthDisplay**: Set the prefabs in the HealthDisplay component
4. **Configure**: Set offset, colors, and visibility distance

## 📊 **Inspector Display**

### **Current Unit Data Section**
```
Health: 120.5/150 (color-coded green to red)
Speed: 5.0
Team: Player
Owner: Player 1
```

### **Combat Information Section**
```
Main Gun:
  Damage: 50
  Range: 20m
  Cooldown: 3s

Machine Gun:
  Damage: 10
  Range: 8m
  Cooldown: 0.5s

Current Weapon:
  Using: Main Gun
  Damage: 50
  Range: 20m
  Cooldown: 3s
```

### **Combat State Section**
```
In Combat: True
Current Target: Enemy Tank
Target In Range: True
Gun Facing Target: True
Detection Radius: 15m
```

## 🎯 **Benefits**

### **1. No More Duplicate Properties**
- Base stats in `Tank`
- Weapon stats in `TankCombatUnit`
- Runtime stats in `UnitData`

### **2. Better Organization**
- Clear separation of concerns
- Easy to find and modify specific stats
- Weapon-specific system for different gun types

### **3. Inspector Visibility**
- See real-time health and stats
- Monitor combat state
- Debug team assignments

### **4. In-Game Health Display**
- Visual health bars above units
- Distance-based visibility
- Color-coded health status

## 🔧 **Configuration Examples**

### **Heavy Tank Configuration**
```csharp
// Tank.cs
tankHealth = 200f;
tankSpeed = 3f;
armorDamageReduction = 0.3f; // 30% damage reduction

// TankCombatUnit.cs
mainGunDamage = 75f;
mainGunRange = 25f;
mainGunCooldown = 4f;
machineGunDamage = 15f;
machineGunRange = 10f;
machineGunCooldown = 0.3f;
```

### **Light Tank Configuration**
```csharp
// Tank.cs
tankHealth = 100f;
tankSpeed = 8f;
armorDamageReduction = 0.1f; // 10% damage reduction

// TankCombatUnit.cs
mainGunDamage = 30f;
mainGunRange = 15f;
mainGunCooldown = 2f;
machineGunDamage = 8f;
machineGunRange = 6f;
machineGunCooldown = 0.2f;
```

## 🚀 **Next Steps**

1. **Test the new system**: Create tank prefabs with the new organization
2. **Configure health display**: Set up UI prefabs for in-game health bars
3. **Customize inspector**: Modify the custom editor if needed
4. **Extend to other units**: Apply similar organization to other unit types

---

**The new system provides better organization, visibility, and maintainability while eliminating property duplication!**
