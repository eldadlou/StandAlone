# Team System Setup Guide

## Overview
The team system implements a two-team gameplay system (Player vs AI) with comprehensive team management, victory conditions, and AI behavior.

## System Components

### 1. Enhanced Player Class (`Game/Player.cs`)
- **Team Enum**: `Team.Player` and `Team.AI`
- **Team Properties**: Color, AI difficulty, statistics
- **Resource Management**: Resources for future RTS features
- **Team Statistics**: Units created, destroyed, lost

### 2. TeamManager (`Game/TeamManager.cs`)
- **Team Collections**: Manages units per team
- **Victory Conditions**: Automatic win/lose detection
- **Team Statistics**: Tracks team performance
- **Event System**: Team-specific events

### 3. AIController (`Game/AIController.cs`)
- **AI Behavior**: Attack, patrol, decision making
- **Difficulty Levels**: 3 difficulty settings (1-3)
- **Configurable Parameters**: Attack range, patrol radius, decision interval

### 4. Enhanced GameManager (`Game/GameManager.cs`)
- **Team Integration**: Works with TeamManager
- **Team Events**: Handles team-specific unit events
- **Victory Handling**: Game end on team defeat/victory

## Setup Instructions

### Step 1: Scene Setup
1. **Add TeamManager to Scene**:
   - Create empty GameObject named "TeamManager"
   - Add `TeamManager` component
   - Configure team settings in inspector

2. **Add AIController to Scene**:
   - Create empty GameObject named "AIController"
   - Add `AIController` component
   - Configure AI settings in inspector

3. **Update GameManager**:
   - Ensure GameManager has reference to TeamManager
   - Teams will be auto-initialized

### Step 2: Unit Team Assignment
Units must be assigned to teams when created:

```csharp
// Example: Creating a player unit
var playerUnit = new Unit();
playerUnit.Owner = new Player("Player", Team.Player, false);

// Example: Creating an AI unit
var aiUnit = new Unit();
aiUnit.Owner = new Player("AI", Team.AI, true);
```

### Step 3: Team Events Integration
The system automatically handles team events:
- `OnTeamUnitCreated(Team team, IUnit unit)`
- `OnTeamUnitDestroyed(Team team, IUnit unit)`
- `OnTeamDefeated(Team team)`
- `OnVictoryCondition(Team winningTeam)`

## Configuration Options

### TeamManager Settings
- **enableVictoryConditions**: Enable/disable automatic victory detection
- **requiredUnitsToWin**: Number of units required to win (0 = destroy all)

### AIController Settings
- **decisionInterval**: How often AI makes decisions (seconds)
- **attackRange**: Range at which AI attacks enemies
- **patrolRadius**: Radius for patrol behavior
- **aiDifficulty**: AI difficulty level (1-3)

### AI Difficulty Levels
- **Level 1 (Easy)**: 3s decision interval, 8m attack range
- **Level 2 (Medium)**: 2s decision interval, 10m attack range
- **Level 3 (Hard)**: 1s decision interval, 12m attack range

## Victory Conditions

### Default Victory Condition
- **Win**: Destroy all enemy units
- **Lose**: All your units are destroyed

### Custom Victory Conditions
Override `CheckVictoryConditions()` in TeamManager for custom logic:
```csharp
private void CheckVictoryConditions()
{
    // Custom victory logic here
    if (teamUnits[Team.Player].Count <= requiredUnitsToWin)
    {
        TriggerVictoryCondition(Team.AI);
    }
}
```

## Team Statistics

### Player Statistics
- Units remaining
- Units created
- Units lost
- Resources
- Max resources

### AI Statistics
- Units remaining
- Difficulty level
- Decision interval
- Attack range

## Usage Examples

### Getting Team Units
```csharp
var playerUnits = TeamManager.Instance.GetTeamUnits(Team.Player);
var aiUnits = TeamManager.Instance.GetTeamUnits(Team.AI);
```

### Getting Team Statistics
```csharp
var playerStats = TeamManager.Instance.GetTeamStatistics(Team.Player);
var aiStats = AIController.Instance.GetAIStatistics();
```

### Setting AI Difficulty
```csharp
AIController.Instance.SetAIDifficulty(2); // Medium difficulty
```

### Team Events
```csharp
// Subscribe to team events
GameEvents.OnTeamDefeated += (team) => {
    Debug.Log($"{team} team defeated!");
};

GameEvents.OnVictoryCondition += (winningTeam) => {
    Debug.Log($"{winningTeam} team wins!");
};
```

## Visual Team Distinction

### Team Colors
- **Player Team**: Blue (`Color.blue`)
- **AI Team**: Red (`Color.red`)

### Unit Visuals
Units can be visually distinguished by team:
```csharp
// In UnitVisualCoordinator
public void SetTeamColor(Team team)
{
    var renderer = GetComponent<Renderer>();
    var teamColor = team == Team.Player ? Color.blue : Color.red;
    renderer.material.color = teamColor;
}
```

## Troubleshooting

### Common Issues
1. **Units not assigned to teams**: Ensure units have `Owner` property set
2. **AI not behaving**: Check AIController is active and has units
3. **Victory conditions not triggering**: Verify TeamManager is properly initialized

### Debug Information
The system provides extensive debug logging:
- Team unit creation/destruction
- AI decision making
- Victory condition checks
- Team statistics updates

## Future Enhancements

### Planned Features
1. **Multiple Teams**: Support for more than 2 teams
2. **Team Alliances**: Friendly fire settings
3. **Advanced AI**: More sophisticated AI behaviors
4. **Team Resources**: Resource gathering and management
5. **Team Buildings**: Base building and defense

### Extensibility
The system is designed to be easily extensible:
- Add new team types to `Team` enum
- Create custom victory conditions
- Implement team-specific unit types
- Add team-specific game mechanics 