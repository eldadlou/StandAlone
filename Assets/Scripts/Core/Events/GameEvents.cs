using System;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Core.Units;
using Team = MyGame.Core.Team;

namespace MyGame.Core.Events
{
    /// <summary>
    /// Centralized event system for decoupling game systems
    /// </summary>
    public static class GameEvents
    {
        // Unit Events
        public static event Action<IUnit> OnUnitCreated;
        public static event Action<IUnit> OnUnitDestroyed;
        public static event Action<IUnit> OnUnitSelected;
        public static event Action<IUnit> OnUnitDeselected;
        public static event Action<IUnit, Vector3> OnUnitMoveCommand;
        public static event Action<IUnit, IUnit> OnUnitAttackCommand;
        public static event Action<IUnit, IUnit> OnUnitAttack;
        
        // Selection Events
        public static event Action<Vector2> OnSelectionStart;
        public static event Action<Vector2> OnSelectionEnd;
        public static event Action<Rect> OnMultiSelection;
        public static event Action OnSelectionClear;
        
        // Input Events
        public static event Action<Vector2> OnLeftClick;
        public static event Action<Vector2> OnRightClick;
        public static event Action<Vector2> OnMouseDrag;
        
        // System Events
        public static event Action OnGameStart;
        public static event Action OnGameEnd;
        public static event Action OnSystemsInitialized;
        
        // Team Events
        public static event Action<Team, IUnit> OnTeamUnitCreated;
        public static event Action<Team, IUnit> OnTeamUnitDestroyed;
        public static event Action<Team> OnTeamDefeated;
        public static event Action<Team> OnVictoryCondition;
        
        // Unit Query Events
        public static event Func<List<IUnit>> OnGetAllUnits;
        
        // Explosion and Destructible Events
        public static event Action<Vector3, float, float, MyGame.Core.Interfaces.IDestructible> OnExplosion; // position, radius, damage, source
        public static event Action<MyGame.Core.Interfaces.IDestructible> OnDestructibleCreated;
        public static event Action<MyGame.Core.Interfaces.IDestructible> OnDestructibleDestroyed;
        public static event Action<MyGame.Core.Interfaces.IDestructible, float, MyGame.Core.Interfaces.IDestructible> OnObjectDamaged; // target, damage, source
        public static event Action<MyGame.Core.Interfaces.IDestructible, Vector3, float, float> OnObjectExploded; // source, position, radius, damage
        
        // Unit Creation/Destruction
        public static void TriggerUnitCreated(IUnit unit)
        {
            // Debug.Log($"GameEvents: TriggerUnitCreated called for {unit?.GetType().Name} - Event subscribers: {OnUnitCreated?.GetInvocationList().Length ?? 0}");
            OnUnitCreated?.Invoke(unit);
        }
        
        public static void TriggerUnitDestroyed(IUnit unit)
        {
            OnUnitDestroyed?.Invoke(unit);
        }
        
        // Selection Events
        public static void TriggerUnitSelected(IUnit unit)
        {
            OnUnitSelected?.Invoke(unit);
        }
        
        public static void TriggerUnitDeselected(IUnit unit)
        {
            OnUnitDeselected?.Invoke(unit);
        }
        
        public static void TriggerSelectionStart(Vector2 position)
        {
            OnSelectionStart?.Invoke(position);
        }
        
        public static void TriggerSelectionEnd(Vector2 position)
        {
            OnSelectionEnd?.Invoke(position);
        }
        
        public static void TriggerMultiSelection(Rect selectionRect)
        {
            OnMultiSelection?.Invoke(selectionRect);
        }
        
        public static void TriggerSelectionClear()
        {
            OnSelectionClear?.Invoke();
        }
        
        // Input Events
        public static void TriggerLeftClick(Vector2 position)
        {
            OnLeftClick?.Invoke(position);
        }
        
        public static void TriggerRightClick(Vector2 position)
        {
            OnRightClick?.Invoke(position);
        }
        
        public static void TriggerMouseDrag(Vector2 position)
        {
            OnMouseDrag?.Invoke(position);
        }
        
        // Command Events
        public static void TriggerMoveCommand(IUnit unit, Vector3 destination)
        {
            OnUnitMoveCommand?.Invoke(unit, destination);
        }
        
        public static void TriggerAttackCommand(IUnit attacker, IUnit target)
        {
            OnUnitAttackCommand?.Invoke(attacker, target);
        }
        
        public static void TriggerUnitAttack(IUnit attacker, IUnit target)
        {
            // Debug.Log($"GameEvents: TriggerUnitAttack called - {attacker?.Name} attacked {target?.Name}");
            OnUnitAttack?.Invoke(attacker, target);
        }
        
        // System Events
        public static void TriggerGameStart()
        {
            OnGameStart?.Invoke();
        }
        
        public static void TriggerGameEnd()
        {
            OnGameEnd?.Invoke();
        }
        
        public static void TriggerSystemsInitialized()
        {
            OnSystemsInitialized?.Invoke();
        }
        
        // Team Events
        public static void TriggerTeamUnitCreated(Team team, IUnit unit)
        {
            OnTeamUnitCreated?.Invoke(team, unit);
        }
        
        public static void TriggerTeamUnitDestroyed(Team team, IUnit unit)
        {
            OnTeamUnitDestroyed?.Invoke(team, unit);
        }
        
        public static void TriggerTeamDefeated(Team team)
        {
            OnTeamDefeated?.Invoke(team);
        }
        
        public static void TriggerVictoryCondition(Team winningTeam)
        {
            OnVictoryCondition?.Invoke(winningTeam);
        }
        
        // Unit Query Methods
        public static List<IUnit> GetAllUnits()
        {
            return OnGetAllUnits?.Invoke() ?? new List<IUnit>();
        }
        
        // Explosion Events
        public static void TriggerExplosion(Vector3 position, float radius, float damage, MyGame.Core.Interfaces.IDestructible source = null)
        {
            // Debug.Log($"💥 GameEvents: TriggerExplosion called at {position} (radius: {radius}, damage: {damage}) - Event subscribers: {OnExplosion?.GetInvocationList().Length ?? 0}");
            OnExplosion?.Invoke(position, radius, damage, source);
        }
        
        public static void TriggerDestructibleCreated(MyGame.Core.Interfaces.IDestructible destructible)
        {
            OnDestructibleCreated?.Invoke(destructible);
        }
        
        public static void TriggerDestructibleDestroyed(MyGame.Core.Interfaces.IDestructible destructible)
        {
            OnDestructibleDestroyed?.Invoke(destructible);
        }
        
        public static void TriggerObjectDamaged(MyGame.Core.Interfaces.IDestructible target, float damage, MyGame.Core.Interfaces.IDestructible source = null)
        {
            OnObjectDamaged?.Invoke(target, damage, source);
        }
        
        public static void TriggerObjectExploded(MyGame.Core.Interfaces.IDestructible source, Vector3 position, float radius, float damage)
        {
            // Debug.Log($"💥 GameEvents: TriggerObjectExploded called for {source?.GetType().Name} at {position} (radius: {radius}, damage: {damage}) - Event subscribers: {OnObjectExploded?.GetInvocationList().Length ?? 0}");
            OnObjectExploded?.Invoke(source, position, radius, damage);
        }
        
        // Cleanup method to prevent memory leaks
        public static void ClearAllEvents()
        {
            OnUnitCreated = null;
            OnUnitDestroyed = null;
            OnUnitSelected = null;
            OnUnitDeselected = null;
            OnUnitMoveCommand = null;
            OnUnitAttackCommand = null;
            OnUnitAttack = null;
            OnSelectionStart = null;
            OnSelectionEnd = null;
            OnMultiSelection = null;
            OnSelectionClear = null;
            OnLeftClick = null;
            OnRightClick = null;
            OnMouseDrag = null;
            OnGameStart = null;
            OnGameEnd = null;
            OnSystemsInitialized = null;
            OnTeamUnitCreated = null;
            OnTeamUnitDestroyed = null;
            OnTeamDefeated = null;
            OnVictoryCondition = null;
            OnGetAllUnits = null;
            OnExplosion = null;
            OnDestructibleCreated = null;
            OnDestructibleDestroyed = null;
            OnObjectDamaged = null;
            OnObjectExploded = null;
        }
    }
} 