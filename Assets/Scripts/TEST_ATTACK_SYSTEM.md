# Attack System Test Guide

## Overview
The attack system now includes proper damage application, range validation, cooldowns, and team-based restrictions.

## System Components

### 1. Enhanced UnitData (`Core/Units/UnitData.cs`)
- **Combat Properties**: AttackDamage, AttackRange, AttackCooldown
- **Team Validation**: No friendly fire
- **Cooldown System**: Prevents spam attacks
- **Damage Application**: Actual damage dealt to targets

### 2. Enhanced Unit Class (`Core/Units/Unit.cs`)
- **Range Validation**: Checks distance before attacking
- **Team Validation**: Prevents attacking allies
- **Attack Success**: Returns boolean for attack success

### 3. Enhanced FireSystem (`RuntimeSystems/Combat/FireSystem.cs`)
- **Visual Effects**: Projectile system
- **Attack Animation**: Fire animation triggers
- **Projectile Behavior**: Homing projectiles

### 4. Enhanced AIController (`Game/AIController.cs`)
- **Smart Targeting**: Uses CanAttack() for validation
- **Attack Feedback**: Logs success/failure

## Unit Attack Stats

| Unit Type | Damage | Range | Cooldown |
|-----------|--------|-------|----------|
| Tank | 25 | 15m | 2s |
| Soldier | 15 | 10m | 1.5s |
| Aircraft | 30 | 20m | 3s |
| Helicopter | 20 | 18m | 2.5s |
| Truck | 5 | 5m | 4s |

## Testing Steps

### Step 1: Setup Scene
1. **Add UnitSetupTest to Scene**:
   - Add `UnitSetupTest` component to a GameObject
   - Assign player and AI unit prefabs
   - Configure spawn positions

2. **Configure FireSystem**:
   - Add projectile prefab (optional)
   - Enable/disable projectiles
   - Set projectile speed

3. **Ensure Team Assignment**:
   - Units must be assigned to teams using `AssignToTeam()`
   - Player units: `unit.AssignToTeam(Team.Player)`
   - AI units: `unit.AssignToTeam(Team.AI)`

### Step 2: Test Basic Attacks
```csharp
// Test attack between units
var playerUnit = GetPlayerUnit();
var aiUnit = GetAIUnit();

// Should succeed if in range and not on cooldown
bool success = playerUnit.Attack(aiUnit);
Debug.Log($"Attack success: {success}");
```

**Using UnitSetupTest:**
- Press `T` key to test attacks
- Press `P` key to spawn player unit
- Press `A` key to spawn AI unit

### Step 3: Test Range Validation
```csharp
// Move units apart
playerUnit.MoveTo(Vector3.zero);
aiUnit.MoveTo(new Vector3(50, 0, 50));

// Should fail due to range
bool success = playerUnit.Attack(aiUnit);
Debug.Log($"Attack success: {success}"); // Should be false
```

### Step 4: Test Team Validation
```csharp
// Try to attack same team unit
var playerUnit1 = GetPlayerUnit();
var playerUnit2 = GetAnotherPlayerUnit();

// Should fail due to same team
bool success = playerUnit1.Attack(playerUnit2);
Debug.Log($"Attack success: {success}"); // Should be false
```

### Step 5: Test Cooldown System
```csharp
// Attack multiple times quickly
var playerUnit = GetPlayerUnit();
var aiUnit = GetAIUnit();

bool attack1 = playerUnit.Attack(aiUnit);
bool attack2 = playerUnit.Attack(aiUnit); // Should fail due to cooldown
bool attack3 = playerUnit.Attack(aiUnit); // Should fail due to cooldown

Debug.Log($"Attack 1: {attack1}, Attack 2: {attack2}, Attack 3: {attack3}");
```

### Step 6: Test AI Behavior
```csharp
// AI should automatically attack nearby enemies
// Check console for AI attack logs
// Verify AI respects cooldowns and ranges
```

## Expected Behavior

### ✅ Working Features
- **Damage Application**: Units take actual damage
- **Range Validation**: Can't attack beyond range
- **Team Validation**: No friendly fire
- **Cooldown System**: Prevents attack spam
- **AI Targeting**: AI attacks nearest valid target
- **Visual Effects**: Projectiles and animations

### ❌ Common Issues
1. **Units not attacking**: Check team assignment
2. **No damage dealt**: Verify UnitData initialization
3. **AI not attacking**: Check if units are in range
4. **Friendly fire**: Verify team assignment

## Debug Information

The system provides extensive logging:
- Attack attempts and results
- Damage dealt
- Range validation failures
- Cooldown checks
- AI decision making

## Performance Considerations

- **Cooldown Checks**: Use Time.time for efficiency
- **Range Calculations**: Only check when needed
- **Team Validation**: Cached team checks
- **Projectile Cleanup**: Automatic destruction

## Future Enhancements

1. **Different Attack Types**: Melee, ranged, area damage
2. **Armor System**: Damage reduction
3. **Critical Hits**: Random critical damage
4. **Status Effects**: Poison, stun, etc.
5. **Weapon Upgrades**: Damage/range improvements 