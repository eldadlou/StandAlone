# 🔄 Fire System Component Flow Chart

## 🏗️ **System Architecture Overview**

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              GAME SCENE                                         │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐            │
│  │   Player Tank   │    │    AI Tank      │    │   Other Units   │            │
│  │   Prefab        │    │   Prefab        │    │   Prefabs       │            │
│  └─────────────────┘    └─────────────────┘    └─────────────────┘            │
│           │                       │                       │                    │
│           ▼                       ▼                       ▼                    │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                    UNIT COMPONENT LAYER                                │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                    │    │
│  │  │   Unit      │  │   Unit      │  │   Unit      │                    │    │
│  │  │ (Base)      │  │ (Base)      │  │ (Base)      │                    │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘                    │    │
│  │           │               │               │                            │    │
│  │           ▼               ▼               ▼                            │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                    │    │
│  │  │TankCombat   │  │TankCombat   │  │OtherCombat  │                    │    │
│  │  │  Unit       │  │  Unit       │  │   Unit      │                    │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘                    │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│           │                       │                       │                    │
│           ▼                       ▼                       ▼                    │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                    COMBAT LOGIC LAYER                                  │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                    │    │
│  │  │   Gun       │  │   Gun       │  │   Gun       │                    │    │
│  │  │  Turret     │  │  Turret     │  │  Turret     │                    │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘                    │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│           │                       │                       │                    │
│           ▼                       ▼                       ▼                    │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                    GLOBAL COORDINATION LAYER                            │    │
│  │                    LightweightFireSystem                                │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                    │    │
│  │  │ Projectile  │  │ Explosion   │  │   Audio     │                    │    │
│  │  │  System     │  │   System    │  │   System    │                    │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘                    │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## 🔄 **Data Flow & Component Interaction**

### **1. Unit Initialization Flow**
```
Unit Prefab Spawn
        │
        ▼
┌─────────────────┐
│   Unit.Awake()  │ ← Creates UnitData, registers with GameManager
└─────────────────┘
        │
        ▼
┌─────────────────┐
│TankCombatUnit   │ ← Extends CombatUnit, implements ICombatUnit
│   .Awake()      │ ← Gets Unit component, finds GunTurret children
└─────────────────┘
        │
        ▼
┌─────────────────┐
│  GunTurret      │ ← Handles visual rotation of gun models
│  Components     │ ← One per gun (main gun, machine gun)
└─────────────────┘
```

### **2. Combat Detection Flow**
```
Update() Loop (Every Frame)
        │
        ▼
┌─────────────────┐
│CombatUnit       │ ← Base class handles common combat logic
│.UpdateCombat()  │
└─────────────────┘
        │
        ▼
┌─────────────────┐
│Target Detection │ ← Physics.OverlapSphere every 0.5 seconds
│(Detection       │ ← Finds enemies within detection radius
│ Radius)         │
└─────────────────┘
        │
        ▼
┌─────────────────┐
│Set Target       │ ← Closest enemy becomes current target
│(if new enemy)   │ ← Triggers combat state changes
└─────────────────┘
```

### **3. Combat Execution Flow**
```
Target Acquired
        │
        ▼
┌─────────────────┐
│Range Check      │ ← Is target within AttackRange?
└─────────────────┘
        │
        ▼
┌─────────────────┐
│Gun Rotation     │ ← Rotate gun towards target
│(GunTurret)     │ ← Uses GunTurret.SetTarget()
└─────────────────┘
        │
        ▼
┌─────────────────┐
│Facing Check     │ ← Is gun facing target within threshold?
└─────────────────┘
        │
        ▼
┌─────────────────┐
│Cooldown Check   │ ← Has enough time passed since last attack?
└─────────────────┘
        │
        ▼
┌─────────────────┐
│Attack Execution │ ← Perform attack, create projectiles
└─────────────────┘
```

### **4. Projectile & Effects Flow**
```
Attack Executed
        │
        ▼
┌─────────────────┐
│Lightweight      │ ← Global system handles effects
│FireSystem       │
└─────────────────┘
        │
        ▼
┌─────────────────┐
│Create Projectile│ ← Instantiate projectile prefab
│(Projectile      │ ← Add ProjectileBehavior component
│ Behavior)       │
└─────────────────┘
        │
        ▼
┌─────────────────┐
│Projectile       │ ← Moves towards target
│Movement         │ ← Updates position every frame
└─────────────────┘
        │
        ▼
┌─────────────────┐
│Hit Detection    │ ← Collision or distance-based hit
└─────────────────┘
        │
        ▼
┌─────────────────┐
│Damage & Effects │ ← Apply damage, create explosion, play sound
└─────────────────┘
```

## 🔧 **Component Responsibilities**

### **CombatUnit (Base Class)**
- **Target Detection**: Physics-based enemy finding
- **Combat State Management**: In combat, target tracking
- **Range Checking**: Distance validation
- **Abstract Interface**: Defines combat contract

### **TankCombatUnit (Derived Class)**
- **Weapon Selection**: Main gun vs machine gun based on range
- **Dual Turret Management**: Handles main and machine gun turrets
- **Tank-Specific Logic**: Armor, weapon switching, etc.
- **Component Delegation**: Delegates to underlying Unit component

### **GunTurret (Visual Component)**
- **Rotation Logic**: Smooth turret rotation towards target
- **Rotation Limits**: Configurable angle constraints
- **Visual Feedback**: Gizmos for debugging
- **Performance**: Efficient rotation calculations

### **LightweightFireSystem (Global Coordinator)**
- **Effect Management**: Projectiles, explosions, sounds
- **Unit Registration**: Tracks all combat units in scene
- **Global Cooldowns**: Prevents spam attacks
- **Team Coordination**: Enemy finding utilities

### **ProjectileBehavior (Projectile Logic)**
- **Movement**: Follows target with configurable speed
- **Lifetime Management**: Auto-destroy after time/distance
- **Hit Detection**: Collision and proximity-based hits
- **Damage Application**: Delegates to target's damage system

## 📊 **Performance Characteristics**

### **Update Frequency**
- **CombatUnit**: Every frame (Update)
- **Target Detection**: Every 0.5 seconds (configurable)
- **Gun Rotation**: Every frame when rotating
- **Projectile Movement**: Every frame until hit

### **Memory Usage**
- **Unit Registration**: List of ICombatUnit references
- **Cooldown Tracking**: Dictionary<IUnit, float>
- **Projectile Pooling**: Not implemented (potential optimization)

### **CPU Usage**
- **Physics Queries**: OverlapSphere every 0.5s per unit
- **Rotation Calculations**: Quaternion operations per frame
- **Distance Calculations**: Vector3.Distance per target check

## 🎯 **Optimization Opportunities**

### **1. Object Pooling**
```csharp
// Implement projectile pooling instead of Instantiate/Destroy
public class ProjectilePool : MonoBehaviour
{
    private Queue<GameObject> projectilePool;
    // Reuse projectiles instead of creating new ones
}
```

### **2. Spatial Partitioning**
```csharp
// Use spatial grid for enemy detection instead of OverlapSphere
public class CombatGrid : MonoBehaviour
{
    private Dictionary<Vector2Int, List<IUnit>> grid;
    // Only check nearby grid cells for enemies
}
```

### **3. Event-Driven Updates**
```csharp
// Only update combat when relevant events occur
public event Action OnEnemyInRange;  // Instead of constant checking
public event Action OnTargetLost;    // Trigger updates only when needed
```

## 🔄 **Component Communication Summary**

```
Unit ←→ TankCombatUnit ←→ CombatUnit ←→ GunTurret
  │           │              │            │
  │           ▼              ▼            ▼
  │    ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
  │    │   Unit      │ │  Combat     │ │   Gun       │
  │    │ Component   │ │  Logic      │ │  Rotation   │
  │    └─────────────┘ └─────────────┘ └─────────────┘
  │           │              │            │
  └───────────┼──────────────┼────────────┘
              ▼              ▼
    ┌─────────────────────────────────┐
    │      LightweightFireSystem      │
    │     (Global Coordination)       │
    └─────────────────────────────────┘
              │
              ▼
    ┌─────────────────────────────────┐
    │      ProjectileBehavior         │
    │     (Projectile Management)     │
    └─────────────────────────────────┘
```

This architecture provides:
- **Separation of Concerns**: Each component has a single responsibility
- **Extensibility**: Easy to add new unit types and weapons
- **Performance**: Distributed updates, no central bottleneck
- **Maintainability**: Clear component boundaries and communication
