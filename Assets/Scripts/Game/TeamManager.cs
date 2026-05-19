using System;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Events;

namespace MyGame.Game
{
    /// <summary>
    /// Manages team-based game logic, victory conditions, and team statistics
    /// </summary>
    public class TeamManager : MonoBehaviour
    {
        public static TeamManager Instance { get; private set; }
        
        [Header("Team Configuration")]
        public Player playerTeam;
        public Player aiTeam;
        
        [Header("Victory Conditions")]
        public bool enableVictoryConditions = true;
        public int requiredUnitsToWin = 0; // 0 = destroy all enemy units
        
        // Team unit collections
        private Dictionary<Team, List<IUnit>> teamUnits = new Dictionary<Team, List<IUnit>>();
        private Dictionary<Team, Player> teamPlayers = new Dictionary<Team, Player>();
        
        // Events
        public event Action<Team> OnTeamDefeated;
        public event Action<Team> OnVictoryCondition;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeTeams();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Subscribe to team events
            GameEvents.OnTeamUnitCreated += HandleTeamUnitCreated;
            GameEvents.OnTeamUnitDestroyed += HandleTeamUnitDestroyed;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            GameEvents.OnTeamUnitCreated -= HandleTeamUnitCreated;
            GameEvents.OnTeamUnitDestroyed -= HandleTeamUnitDestroyed;
        }
        
        private void InitializeTeams()
        {
            // Initialize team collections
            teamUnits[Team.Player] = new List<IUnit>();
            teamUnits[Team.AI] = new List<IUnit>();
            
            // Initialize team players if not already set
            if (playerTeam == null)
            {
                playerTeam = new Player("Player", Team.Player, false);
            }
            
            if (aiTeam == null)
            {
                aiTeam = new Player("AI", Team.AI, true);
            }
            
            teamPlayers[Team.Player] = playerTeam;
            teamPlayers[Team.AI] = aiTeam;
            
            Debug.Log($"TeamManager: Initialized teams - Player: {playerTeam.Name}, AI: {aiTeam.Name}");
        }
        
        private void HandleTeamUnitCreated(Team team, IUnit unit)
        {
            if (teamUnits.ContainsKey(team))
            {
                teamUnits[team].Add(unit);
                teamPlayers[team].UnitCreated();
                
                Debug.Log($"TeamManager: {team} team unit created - Total units: {teamUnits[team].Count}");
                
                // Check victory conditions
                CheckVictoryConditions();
            }
        }
        
        private void HandleTeamUnitDestroyed(Team team, IUnit unit)
        {
            if (teamUnits.ContainsKey(team))
            {
                teamUnits[team].Remove(unit);
                teamPlayers[team].UnitLost();
                
                Debug.Log($"TeamManager: {team} team unit destroyed - Remaining units: {teamUnits[team].Count}");
                
                // Check if team is defeated
                if (teamUnits[team].Count == 0)
                {
                    TriggerTeamDefeated(team);
                }
                
                // Check victory conditions
                CheckVictoryConditions();
            }
        }
        
        private void CheckVictoryConditions()
        {
            if (!enableVictoryConditions) return;
            
            // Check if any team has no units left
            if (teamUnits[Team.Player].Count == 0)
            {
                TriggerVictoryCondition(Team.AI);
            }
            else if (teamUnits[Team.AI].Count == 0)
            {
                TriggerVictoryCondition(Team.Player);
            }
        }
        
        private void TriggerTeamDefeated(Team team)
        {
            Debug.Log($"TeamManager: {team} team defeated!");
            OnTeamDefeated?.Invoke(team);
            GameEvents.TriggerTeamDefeated(team);
        }
        
        private void TriggerVictoryCondition(Team winningTeam)
        {
            Debug.Log($"TeamManager: Victory condition met! {winningTeam} team wins!");
            OnVictoryCondition?.Invoke(winningTeam);
            GameEvents.TriggerVictoryCondition(winningTeam);
        }
        
        /// <summary>
        /// Get all units for a specific team
        /// </summary>
        public List<IUnit> GetTeamUnits(Team team)
        {
            return teamUnits.ContainsKey(team) ? teamUnits[team] : new List<IUnit>();
        }
        
        /// <summary>
        /// Get the player object for a specific team
        /// </summary>
        public Player GetTeamPlayer(Team team)
        {
            return teamPlayers.ContainsKey(team) ? teamPlayers[team] : null;
        }
        
        /// <summary>
        /// Check if a team is defeated
        /// </summary>
        public bool IsTeamDefeated(Team team)
        {
            return teamUnits.ContainsKey(team) && teamUnits[team].Count == 0;
        }
        
        /// <summary>
        /// Get team statistics
        /// </summary>
        public TeamStatistics GetTeamStatistics(Team team)
        {
            var player = GetTeamPlayer(team);
            if (player == null) return new TeamStatistics();
            
            return new TeamStatistics
            {
                Team = team,
                UnitsRemaining = teamUnits.ContainsKey(team) ? teamUnits[team].Count : 0,
                UnitsCreated = player.UnitsCreated,
                UnitsLost = player.UnitsLost,
                Resources = player.Resources,
                MaxResources = player.MaxResources
            };
        }
        
        /// <summary>
        /// Get the opposing team
        /// </summary>
        public Team GetOpposingTeam(Team team)
        {
            return team == Team.Player ? Team.AI : Team.Player;
        }
    }
    
    /// <summary>
    /// Data structure for team statistics
    /// </summary>
    [System.Serializable]
    public struct TeamStatistics
    {
        public Team Team;
        public int UnitsRemaining;
        public int UnitsCreated;
        public int UnitsLost;
        public int Resources;
        public int MaxResources;
    }
} 