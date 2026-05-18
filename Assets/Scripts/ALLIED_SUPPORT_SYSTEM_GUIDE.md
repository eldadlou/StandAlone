# Allied Support System Guide

## Overview

The **Allied Support System** is a unit AI coordination system that enables friendly units to assist each other during combat. When a unit is attacked, nearby allies within a configurable radius will automatically move to help and engage the attacker.

This creates more dynamic and realistic combat scenarios where units work together tactically.

## Features

- **Automatic Alert**: When a unit takes damage, nearby friendly units are automatically notified
- **Tactical Flanking**: Responding units position themselves to the sides of the attacked ally, not directly behind
- **Smart Targeting**: Allies will engage the attacker, prioritizing threats to their teammates
- **Distance Management**: Units maintain proper spacing and don't cluster too close together
- **Performance Optimized**: Uses SpatialGrid for efficient unit lookup, includes cooldowns to prevent spam

## How It Works

```
1. Unit A is attacked by Enemy X
2. AlliedSupportSystem.NotifyUnitAttacked() is called (from ProjectileBehavior)
3. System finds all friendly units within alertRadius
4. Nearby allies are assigned flanking positions to the sides of Unit A
5. Allies move to their positions and engage Enemy X
```

### Flanking Formation

```
              [Enemy X]
                  |
                  |
                  v
       [R2]   [Unit A]   [R1]     <- First responders flank at 45°
      [R4]              [R3]      <- Second pair flanks at 65°
     [R6]                [R5]     <- Third pair flanks at 85°
```

- Odd-numbered responders (R1, R3, R5) position on the **right** side
- Even-numbered responders (R2, R4, R6) position on the **left** side
- Each pair spreads further out to avoid clustering

## Setup

### Automatic Setup

The system is automatically created and registered by `SystemInitializer` when the game starts. No manual setup required.

### Manual Setup (Optional)

If you want to customize the system:

1. Create an empty GameObject in your scene
2. Add the `AlliedSupportSystem` component
3. Configure the settings in the Inspector

## Inspector Settings

### Alert Settings

| Setting | Default | Description |
|---------|---------|-------------|
| Alert Radius | 25m | Radius within which friendly units will be alerted |
| Max Responding Units | 5 | Maximum number of units that can respond to a single alert |
| Alert Cooldown | 2s | Cooldown between alerts for the same unit (prevents spam) |
| Min Damage To Alert | 1 | Minimum damage required to trigger an alert |

### Response Settings

| Setting | Default | Description |
|---------|---------|-------------|
| Engage Distance | 15m | Units within this distance will immediately engage the attacker |
| Move To Assist | true | If true, units will move towards the attacked ally |
| Min Distance From Ally | 6m | Minimum distance to keep from the attacked ally |
| Max Distance From Ally | 12m | Maximum distance from the attacked ally when positioning |
| Flanking Angle | 45° | Base angle for flanking positions (from attacker direction) |
| Unit Spacing | 4m | Spacing between assisting units |

### Debug Settings

| Setting | Default | Description |
|---------|---------|-------------|
| Enable Debug Logging | false | Log detailed information to console |
| Draw Debug Gizmos | true | Visualize alert radius and flanking positions in Scene view |

## Integration

### With ProjectileBehavior

The system is automatically notified when projectiles deal damage:

```csharp
// In ProjectileBehavior.cs
private void NotifyAlliedSupportSystem(float damageDealt)
{
    AlliedSupportSystem supportSystem = SystemInitializer.GetSystem<AlliedSupportSystem>();
    if (supportSystem != null)
    {
        supportSystem.NotifyUnitAttacked(target, attacker, damageDealt);
    }
}
```

### Manual Request for Support

You can manually request support from code:

```csharp
// Request nearby allies to help against a threat
AlliedSupportSystem supportSystem = SystemInitializer.GetSystem<AlliedSupportSystem>();
supportSystem?.RequestSupport(myUnit, threatUnit);
```

### With CentralizedDetectionManager

The Allied Support System works alongside the `CentralizedDetectionManager`:
- **CentralizedDetectionManager**: Handles target detection for individual units
- **AlliedSupportSystem**: Coordinates group responses when allies are attacked

## API Reference

### Public Methods

```csharp
/// <summary>
/// Notify the system that a unit was attacked. Alerts nearby friendly units.
/// </summary>
/// <param name="victim">The unit that was attacked</param>
/// <param name="attacker">The unit that performed the attack</param>
/// <param name="damage">Amount of damage dealt</param>
public void NotifyUnitAttacked(IUnit victim, IUnit attacker, float damage)

/// <summary>
/// Manually request support for a unit against a threat.
/// </summary>
/// <param name="requestingUnit">The unit requesting support</param>
/// <param name="threat">The threatening unit</param>
public void RequestSupport(IUnit requestingUnit, IUnit threat)

/// <summary>
/// Get the current alert radius.
/// </summary>
public float GetAlertRadius()

/// <summary>
/// Set the alert radius dynamically.
/// </summary>
public void SetAlertRadius(float radius)
```

## Performance Considerations

1. **SpatialGrid Integration**: Uses SpatialGrid for O(1) unit lookups instead of O(n) searches
2. **Alert Cooldown**: Prevents rapid repeated alerts from the same unit
3. **Max Responders**: Limits the number of units that respond to prevent overwhelming calculations
4. **Cleanup**: Automatically cleans up old cooldown entries to prevent memory leaks

## Debugging

### Enable Debug Logging

Set `enableDebugLogging = true` in the Inspector to see detailed logs:

```
AlliedSupportSystem: Tank_01 attacked by Enemy_Tank for 25.0 damage - alerting nearby allies
AlliedSupportSystem: 3 allies responding to help Tank_01
AlliedSupportSystem: Tank_02 moving to flank position to assist Tank_01 against Enemy_Tank
```

### Debug Gizmos

When `drawDebugGizmos = true` and the AlliedSupportSystem object is selected:
- **Green circle**: Alert radius around attacked unit
- **Red line**: Direction to attacker
- **Yellow sphere**: Attacked unit position
- **Blue circles**: Min/max distance zones
- **Cyan spheres**: Example flanking positions

## Customization

### Adjusting Flanking Behavior

For tighter formations:
```
minDistanceFromAlly: 4
maxDistanceFromAlly: 8
flankingAngle: 30
unitSpacing: 3
```

For wider spread formations:
```
minDistanceFromAlly: 8
maxDistanceFromAlly: 15
flankingAngle: 60
unitSpacing: 6
```

### Disabling Movement (Engage Only)

Set `moveToAssist = false` to only have units engage if they're already within engage distance.

## Troubleshooting

### Units Not Responding

1. Check that units are on the same team (`Owner.Team` matches)
2. Verify `alertRadius` is large enough
3. Ensure `minDamageToAlert` threshold is met
4. Check if cooldown is active for the victim unit

### Units Clustering Together

1. Increase `minDistanceFromAlly`
2. Increase `unitSpacing`
3. Increase `flankingAngle`

### Too Many Units Responding

1. Reduce `maxRespondingUnits`
2. Reduce `alertRadius`
3. Increase `alertCooldown`

## Related Systems

- `CentralizedDetectionManager` - Individual unit target detection
- `LightweightFireSystem` - Combat and projectile management
- `SpatialGrid` - Efficient spatial queries for unit lookup
- `ProjectileBehavior` - Triggers alerts when damage is dealt
