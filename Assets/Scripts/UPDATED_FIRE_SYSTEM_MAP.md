# 🔥 Updated Fire System Map - Complete Architecture

## 🏗️ **System Overview**

The fire system has been completely refactored from a centralized approach to a **component-based, distributed architecture** where each unit manages its own combat logic while coordinating through a lightweight global system.

## 📁 **File Structure & Components**

```
Fire System Components
├── Core/Units/Combat/
│   ├── CombatUnit.cs              ← Base abstract class for all combat units
│   └── TankCombatUnit.cs          ← Concrete tank implementation
├── RuntimeSystems/Combat/
│   ├── LightweightFireSystem.cs   ← Global coordination & effects
│   └── ProjectileBehavior.cs      ← Projectile movement & damage
├── Presentation/
│   └── GunTurret.cs               ← Visual turret rotation
└── Core/Units/
    └── Unit.cs                    ← Base unit functionality
```

## 🔄 **Component Architecture**

### **1. CombatUnit (Base Abstract Class)**
**Location**: `Core/Units/Combat/CombatUnit.cs`
**Purpose**: Base class for all combat-capable units

#### **Key Features:**
- **Target Detection**: Physics-based enemy finding with configurable radius
- **Combat State Management**: Tracks combat status, targets, and ranges
- **Gun Rotation**: Coordinates with GunTurret component
- **Cooldown Management**: Unified attack cooldown system
- **Abstract Interface**: Defines contract for derived classes

#### **Combat Settings:**
```csharp
[Header("Combat Settings")]
[SerializeField] protected float detectionRadius = 15f;        // Enemy detection range
[SerializeField] protected LayerMask enemyLayerMask = -1;      // What layers to detect
[SerializeField] protected float gunRotationSpeed = 90f;       // Degrees per second
[SerializeField] protected float rotationThreshold = 5f;       // Aiming tolerance
[SerializeField] protected float targetUpdateInterval = 0.5f;  // Detection frequency
```

#### **Combat State:**
```csharp
[Header("Combat State")]
[SerializeField] protected bool isInCombat = false;            // Currently in combat
[SerializeField] protected IUnit currentTarget;                // Current enemy target
[SerializeField] protected bool isTargetInRange = false;       // Target within attack range
private bool isGunFacingTarget;                                // Gun aimed at target
```

#### **Abstract Properties (Must Implement):**
```csharp
public abstract float AttackDamage { get; }                    // Weapon damage
public abstract float AttackRange { get; }                     // Weapon range
public abstract float AttackCooldown { get; }                  // Attack frequency
public abstract float Health { get; }                          // Current health
public abstract Vector3 Position { get; }                      // World position
public abstract string Name { get; }                           // Vehicle type name
public abstract bool CanAttack(IUnit target);                  // Can attack target?
public abstract bool Attack(IUnit target);                     // Perform attack
```

#### **Combat Flow:**
```
Update() → UpdateCombat() → UpdateTargetDetection() → SetTarget() → HandleCombat()
                                                                     ↓
                                                              RotateGunTowardsTarget()
                                                                     ↓
                                                              IsGunFacingCurrentTarget()
                                                                     ↓
                                                              TryAttack() → Attack()
```

### **2. TankCombatUnit (Concrete Implementation)**
**Location**: `Core/Units/Combat/TankCombatUnit.cs`
**Purpose**: Tank-specific combat behavior with dual weapon systems

#### **Tank Features:**
- **Dual Weapon System**: Main gun (long range) + Machine gun (short range)
- **Smart Weapon Selection**: Automatically chooses weapon based on target distance
- **Fallback Rotation**: Manual rotation if GunTurret component unavailable
- **Component Delegation**: Delegates base functionality to Unit component

#### **Weapon Configuration:**
```csharp
[Header("Tank Combat Settings")]
[SerializeField] private float mainGunDamage = 50f;           // Heavy damage
[SerializeField] private float mainGunRange = 20f;            // Long range
[SerializeField] private float mainGunCooldown = 3f;          // Slow fire rate
[SerializeField] private float machineGunDamage = 10f;        // Light damage
[SerializeField] private float machineGunRange = 8f;          // Short range
[SerializeField] private float machineGunCooldown = 0.5f;     // Fast fire rate
```

#### **Weapon Selection Logic:**
```csharp
// Choose weapon based on range
if (distanceToTarget <= machineGunRange)
{
    useMainGun = false;  // Use machine gun for close targets
}
else if (distanceToTarget <= mainGunRange)
{
    useMainGun = true;   // Use main gun for medium range
}
```

#### **Gun Turret Management:**
```csharp
[Header("Tank Components")]
[SerializeField] private Transform mainGunTurret;             // Main gun transform
[SerializeField] private Transform machineGunTurret;          // Machine gun transform
```

### **3. LightweightFireSystem (Global Coordinator)**
**Location**: `RuntimeSystems/Combat/LightweightFireSystem.cs`
**Purpose**: Handles global combat effects and coordination

#### **Responsibilities:**
- **Effect Management**: Projectiles, explosions, sounds
- **Unit Registration**: Tracks all combat units in scene
- **Global Cooldowns**: Prevents spam attacks
- **Team Coordination**: Enemy finding utilities

#### **Global Settings:**
```csharp
[Header("Global Combat Settings")]
[SerializeField] private bool enableProjectiles = true;       // Toggle projectiles
[SerializeField] private GameObject projectilePrefab;         // Projectile template
[SerializeField] private float projectileSpeed = 20f;         // Projectile velocity

[Header("Combat Effects")]
[SerializeField] private GameObject explosionPrefab;          // Explosion effect
[SerializeField] private AudioClip fireSound;                 // Firing sound
[SerializeField] private AudioClip explosionSound;            // Explosion sound
```

#### **Unit Management:**
```csharp
// Tracking
private List<ICombatUnit> registeredCombatUnits = new List<ICombatUnit>();
private Dictionary<IUnit, float> globalCooldowns = new Dictionary<IUnit, float>();

// Events
public System.Action<IUnit, IUnit> OnUnitAttack;             // Attack notification
public System.Action<IUnit> OnUnitDeath;                     // Death notification
```

#### **Attack Processing:**
```csharp
public bool ProcessAttack(IUnit attacker, IUnit target)
{
    // Check global cooldown
    // Create visual/audio effects
    // Set global cooldown
    // Notify listeners
    return true;
}
```

### **4. GunTurret (Visual Component)**
**Location**: `Presentation/GunTurret.cs`
**Purpose**: Handles visual rotation of gun turrets

#### **Turret Features:**
- **Smooth Rotation**: Configurable rotation speed and smoothing
- **Rotation Limits**: Optional angle constraints
- **Visual Feedback**: Gizmos for debugging
- **Performance**: Efficient rotation calculations

#### **Configuration:**
```csharp
[Header("Turret Settings")]
public float rotationSpeed = 90f;                             // Degrees per second
public float rotationThreshold = 5f;                          // Aiming tolerance
public bool smoothRotation = true;                            // Smooth vs instant
public bool limitRotation = false;                            // Enable angle limits

[Header("Rotation Limits")]
public float minRotationAngle = -180f;                        // Minimum angle
public float maxRotationAngle = 180f;                         // Maximum angle
```

#### **Core Methods:**
```csharp
public void SetTarget(Vector3 targetPosition, bool immediate = false)
public void StopRotation()
public bool IsFacingTarget(Vector3 targetPosition)
public float GetRotationProgress()
```

### **5. ProjectileBehavior (Projectile Logic)**
**Location**: `RuntimeSystems/Combat/ProjectileBehavior.cs`
**Purpose**: Manages projectile movement, collision, and damage

#### **Projectile Features:**
- **Target Tracking**: Follows moving targets
- **Lifetime Management**: Auto-destroy after time/distance
- **Hit Detection**: Collision and proximity-based hits
- **Damage Application**: Delegates to target's damage system

#### **Configuration:**
```csharp
[Header("Projectile Settings")]
[SerializeField] private float speed = 20f;                   // Movement speed
[SerializeField] private float lifetime = 10f;                // Time before destroy
[SerializeField] private bool destroyOnHit = true;            // Destroy on impact
[SerializeField] private GameObject hitEffect;                 // Impact effect
```

#### **Core Methods:**
```csharp
public void Initialize(IUnit attacker, IUnit target, float projectileSpeed)
private void MoveTowardsTarget()
private void OnHitTarget()
private void ApplyDamage()
```

## 🔄 **Data Flow & Communication**

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
        │
        ▼
┌─────────────────┐
│Lightweight      │ ← Registers unit for global coordination
│FireSystem       │
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

## 🔧 **Key Features & Capabilities**

### **1. Vehicle Identification System**
```csharp
// Each unit has a descriptive name based on prefab
public abstract string Name { get; }  // "Big Tank", "Small Tank", etc.

// Debug output shows clear vehicle identification
Debug.Log($"{Name} ({gameObject.name}) targeting {target.Name} ({target.Name})");
// Output: "Big Tank (Big Tank) targeting Small Tank (Small Tank)"
```

### **2. Dual Weapon System (Tanks)**
- **Main Gun**: High damage, long range, slow fire rate
- **Machine Gun**: Low damage, short range, fast fire rate
- **Smart Selection**: Automatically chooses weapon based on target distance
- **Fallback Support**: Manual rotation if GunTurret component unavailable

### **3. Unified Cooldown Management**
- **Base Class Cooldown**: Single `lastAttackTime` field in CombatUnit
- **No Duplication**: Removed local cooldown tracking in TankCombatUnit
- **Consistent Behavior**: All units use same cooldown system

### **4. Robust GunTurret Integration**
- **Primary Method**: Uses GunTurret component when available
- **Fallback Method**: Manual rotation when GunTurret not present
- **Configurable**: Rotation speed, thresholds, and limits

### **5. Global Effect Coordination**
- **Projectile Management**: Creates and tracks projectiles
- **Audio/Visual Effects**: Handles explosions, sounds, and particles
- **Team Coordination**: Finds enemies, manages global cooldowns

## 📊 **Performance Characteristics**

### **Update Frequency:**
- **CombatUnit**: Every frame (Update)
- **Target Detection**: Every 0.5 seconds (configurable)
- **Gun Rotation**: Every frame when rotating
- **Projectile Movement**: Every frame until hit

### **Memory Usage:**
- **Unit Registration**: List of ICombatUnit references
- **Cooldown Tracking**: Dictionary<IUnit, float>
- **Projectile Pooling**: Not implemented (potential optimization)

### **CPU Usage:**
- **Physics Queries**: OverlapSphere every 0.5s per unit
- **Rotation Calculations**: Quaternion operations per frame
- **Distance Calculations**: Vector3.Distance per target check

## 🎯 **System Benefits**

### **1. Separation of Concerns**
- **CombatUnit**: Handles combat logic and state
- **TankCombatUnit**: Manages tank-specific behavior
- **GunTurret**: Handles visual rotation
- **LightweightFireSystem**: Coordinates global effects

### **2. Extensibility**
- **Easy to Add**: New vehicle types (APC, Artillery, etc.)
- **Modular Design**: Components can be mixed and matched
- **Interface-Based**: Clear contracts between components

### **3. Performance**
- **Distributed Updates**: No central bottleneck
- **Efficient Detection**: Configurable update intervals
- **Optimized Rotation**: Smooth, frame-rate independent

### **4. Maintainability**
- **Clear Boundaries**: Each component has single responsibility
- **Consistent Patterns**: Similar structure across all combat units
- **Debug Support**: Comprehensive logging and gizmos

## 🚀 **Current Status & Ready Features**

✅ **Combat System**: Fully functional with target detection, rotation, and firing
✅ **Vehicle Naming**: Clear identification of different tank types
✅ **Dual Weapons**: Smart weapon selection based on range
✅ **Gun Rotation**: Smooth turret rotation with fallback support
✅ **Projectile System**: Complete projectile movement and damage
✅ **Global Coordination**: Effects, sounds, and team management
✅ **Debug Support**: Visual gizmos and comprehensive logging
✅ **Interface Compliance**: All required interfaces properly implemented

## 🔮 **Future Enhancement Opportunities**

### **1. Object Pooling**
```csharp
// Implement projectile pooling for better performance
public class ProjectilePool : MonoBehaviour
{
    private Queue<GameObject> projectilePool;
    // Reuse projectiles instead of creating new ones
}
```

### **2. Spatial Partitioning**
```csharp
// Use spatial grid for enemy detection
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

The fire system is now a **robust, extensible, and performant** combat solution that provides clear vehicle identification, smart weapon selection, and smooth visual feedback! 🎉
