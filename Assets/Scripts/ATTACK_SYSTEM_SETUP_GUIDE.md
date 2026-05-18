# Attack System Setup Guide

## Overview
This guide explains how to properly set up the attack system with team assignments and testing.

## System Architecture

### 1. Interface Level (`IUnit`)
- **Attack Methods**: `CanAttack(IUnit target)`, `Attack(IUnit target)`
- **Combat Properties**: `AttackDamage`, `AttackRange`, `AttackCooldown`
- **Team Properties**: `Owner` (Player with Team assignment)

### 2. Implementation Level (`Unit`)
- **Team Assignment**: `AssignToTeam(Team team)` method
- **Combat Logic**: Delegates to `UnitData` for pure logic
- **Validation**: Range, team, cooldown checks

### 3. Data Level (`UnitData`)
- **Combat Stats**: Damage, range, cooldown per unit type
- **Team Validation**: No friendly fire logic
- **Damage Application**: Actual health reduction

## Setup Instructions

### Step 1: Scene Setup
1. **Add Required Components**:
   ```
   - GameManager (with TeamManager reference)
   - TeamManager
   - AIController
   - FireSystem
   - UnitSetupTest (for testing)
   ```

2. **Configure UnitSetupTest**:
   - Assign player unit prefab
   - Assign AI unit prefab
   - Set spawn positions
   - Enable `spawnUnitsOnStart` for automatic testing

### Step 2: Unit Prefab Setup
1. **Create Unit Prefabs**:
   - Tank, Soldier, Aircraft, etc.
   - Each must inherit from `Unit` class
   - Implement abstract methods (`GetInitialHealth()`, `GetInitialSpeed()`)

2. **Team Assignment**:
   ```csharp
   // After creating unit
   Unit unit = unitObject.GetComponent<Unit>();
   unit.AssignToTeam(Team.Player); // or Team.AI
   ```

### Step 3: Testing
1. **Automatic Testing**:
   - Enable `spawnUnitsOnStart` in UnitSetupTest
   - Units will spawn and be assigned to teams automatically

2. **Manual Testing**:
   - Press `P` to spawn player unit
   - Press `A` to spawn AI unit
   - Press `T` to test attacks

## Code Examples

### Creating Units with Team Assignment
```csharp
// Method 1: Using UnitSetupTest
public void SpawnPlayerUnit()
{
    GameObject unitObj = Instantiate(playerUnitPrefab, spawnPosition, Quaternion.identity);
    Unit unit = unitObj.GetComponent<Unit>();
    unit.AssignToTeam(Team.Player);
}

// Method 2: Manual creation
public void CreateUnit(UnitType type, Team team, Vector3 position)
{
    GameObject unitObj = Instantiate(GetUnitPrefab(type), position, Quaternion.identity);
    Unit unit = unitObj.GetComponent<Unit>();
    unit.AssignToTeam(team);
}
```

### Testing Attack System
```csharp
public void TestAttack(IUnit attacker, IUnit target)
{
    // Check if attack is possible
    bool canAttack = attacker.CanAttack(target);
    Debug.Log($"Can attack: {canAttack}");
    
    if (canAttack)
    {
        // Perform attack
        bool attackSuccess = attacker.Attack(target);
        Debug.Log($"Attack success: {attackSuccess}");
        
        if (attackSuccess)
        {
            Debug.Log($"Target health: {target.Health}");
        }
    }
}
```

### AI Integration
```csharp
// AI automatically uses the interface methods
private void MakeAIDecisions()
{
    foreach (var unit in aiUnits)
    {
        var nearbyEnemies = GetNearbyEnemyUnits(unit);
        
        if (nearbyEnemies.Count > 0)
        {
            var nearestEnemy = GetNearestUnit(unit, nearbyEnemies);
            bool attackSuccess = unit.Attack(nearestEnemy);
            // AI handles success/failure automatically
        }
    }
}
```

## Unit Attack Stats

| Unit Type | Damage | Range | Cooldown | Health | Speed |
|-----------|--------|-------|----------|--------|-------|
| **Tank** | 25 | 15m | 2s | 100 | 5 |
| **Soldier** | 15 | 10m | 1.5s | 50 | 8 |
| **Aircraft** | 30 | 20m | 3s | 75 | 15 |
| **Helicopter** | 20 | 18m | 2.5s | 60 | 12 |
| **Truck** | 5 | 5m | 4s | 30 | 10 |

## Common Issues and Solutions

### Issue 1: Units not attacking
**Cause**: Units not assigned to teams
**Solution**: Call `unit.AssignToTeam(Team.Player)` or `unit.AssignToTeam(Team.AI)`

### Issue 2: No damage dealt
**Cause**: UnitData not properly initialized
**Solution**: Ensure units inherit from `Unit` class and implement abstract methods

### Issue 3: AI not behaving
**Cause**: Units not in range or on cooldown
**Solution**: Check `CanAttack()` method for validation failures

### Issue 4: Friendly fire
**Cause**: Units on same team
**Solution**: Verify team assignment with `unit.Owner.Team`

## Debug Information

The system provides extensive logging:
- Unit creation and team assignment
- Attack attempts and validation results
- Damage dealt and health remaining
- AI decision making and targeting
- Range and cooldown validation

## Performance Considerations

- **Team Assignment**: Do once at unit creation
- **Attack Validation**: Only check when needed
- **Range Calculations**: Use Vector3.Distance efficiently
- **Cooldown Checks**: Use Time.time for accuracy

## Future Enhancements

1. **Unit Factory**: Centralized unit creation with team assignment
2. **Spawn Points**: Designated spawn areas for each team
3. **Unit Types**: More unit types with different stats
4. **Upgrade System**: Improve unit stats over time
5. **Formation System**: Group units for coordinated attacks 