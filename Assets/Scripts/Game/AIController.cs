using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Core.Units;
using MyGame.Core.Events;

namespace MyGame.Game
{
    /// <summary>
    /// Controls AI team behavior and decision making
    /// </summary>
    public class AIController : MonoBehaviour
    {
        [Header("AI Settings")]
        public float decisionInterval = 2f; // How often AI makes decisions
        public float attackRange = 10f; // Range at which AI will attack
        public float patrolRadius = 20f; // Radius for patrol behavior
        
        [Header("AI Difficulty")]
        public int aiDifficulty = 1; // 1-3, affects AI behavior complexity
        
        private Player aiPlayer;
        private List<IUnit> aiUnits = new List<IUnit>();
        private Dictionary<IUnit, Vector3> unitPatrolPoints = new Dictionary<IUnit, Vector3>();
        
        private void Start()
        {
            // Subscribe to team events
            GameEvents.OnTeamUnitCreated += HandleTeamUnitCreated;
            GameEvents.OnTeamUnitDestroyed += HandleTeamUnitDestroyed;
            
            // Get AI player reference
            aiPlayer = TeamManager.Instance?.GetTeamPlayer(Team.AI);
            
            if (aiPlayer != null)
            {
                aiPlayer.AIDifficulty = aiDifficulty;
            }
            
            // Start AI decision making
            StartCoroutine(AIDecisionLoop());
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            GameEvents.OnTeamUnitCreated -= HandleTeamUnitCreated;
            GameEvents.OnTeamUnitDestroyed -= HandleTeamUnitDestroyed;
        }
        
        private void HandleTeamUnitCreated(Team team, IUnit unit)
        {
            if (team == Team.AI)
            {
                aiUnits.Add(unit);
                Debug.Log($"AIController: AI unit created - Total AI units: {aiUnits.Count}");
            }
        }
        
        private void HandleTeamUnitDestroyed(Team team, IUnit unit)
        {
            if (team == Team.AI)
            {
                aiUnits.Remove(unit);
                unitPatrolPoints.Remove(unit);
                Debug.Log($"AIController: AI unit destroyed - Remaining AI units: {aiUnits.Count}");
            }
        }
        
        private IEnumerator AIDecisionLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(decisionInterval);
                
                if (aiUnits.Count > 0)
                {
                    MakeAIDecisions();
                }
            }
        }
        
        private void MakeAIDecisions()
        {
            foreach (var unit in aiUnits)
            {
                if (unit == null) continue;
                
                // Get nearby enemy units
                var nearbyEnemies = GetNearbyEnemyUnits(unit);
                
                if (nearbyEnemies.Count > 0)
                {
                    // Attack nearest enemy
                    var nearestEnemy = GetNearestUnit(unit, nearbyEnemies);
                    if (nearestEnemy != null)
                    {
                        bool attackSuccess = unit.Attack(nearestEnemy);
                        if (attackSuccess)
                        {
                            Debug.Log($"AIController: AI unit successfully attacked enemy");
                        }
                        else
                        {
                            Debug.Log($"AIController: AI unit failed to attack enemy (cooldown/range)");
                        }
                    }
                }
                else
                {
                    // Patrol behavior
                    PatrolBehavior(unit);
                }
            }
        }
        
        private List<IUnit> GetNearbyEnemyUnits(IUnit aiUnit)
        {
            var nearbyEnemies = new List<IUnit>();
            var playerUnits = TeamManager.Instance?.GetTeamUnits(Team.Player) ?? new List<IUnit>();
            
            foreach (var enemyUnit in playerUnits)
            {
                if (enemyUnit == null) continue;
                
                // Check if AI unit can attack this enemy
                if (aiUnit.CanAttack(enemyUnit))
                {
                    nearbyEnemies.Add(enemyUnit);
                }
            }
            
            return nearbyEnemies;
        }
        
        private IUnit GetNearestUnit(IUnit referenceUnit, List<IUnit> units)
        {
            IUnit nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var unit in units)
            {
                if (unit == null) continue;
                
                float distance = Vector3.Distance(referenceUnit.Position, unit.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = unit;
                }
            }
            
            return nearest;
        }
        
        private void PatrolBehavior(IUnit unit)
        {
            // If unit doesn't have a patrol point, assign one
            if (!unitPatrolPoints.ContainsKey(unit))
            {
                Vector3 patrolPoint = GetRandomPatrolPoint(unit.Position);
                unitPatrolPoints[unit] = patrolPoint;
            }
            
            // Move towards patrol point
            Vector3 currentPatrolPoint = unitPatrolPoints[unit];
            float distanceToPatrolPoint = Vector3.Distance(unit.Position, currentPatrolPoint);
            
            if (distanceToPatrolPoint < 2f)
            {
                // Reached patrol point, get new one
                Vector3 newPatrolPoint = GetRandomPatrolPoint(unit.Position);
                unitPatrolPoints[unit] = newPatrolPoint;
            }
            else
            {
                // Move towards current patrol point
                unit.MoveTo(currentPatrolPoint);
            }
        }
        
        private Vector3 GetRandomPatrolPoint(Vector3 center)
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 patrolPoint = center + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // Ensure patrol point is within reasonable bounds
            patrolPoint.y = center.y; // Keep same height
            
            return patrolPoint;
        }
        
        /// <summary>
        /// Set AI difficulty level
        /// </summary>
        public void SetAIDifficulty(int difficulty)
        {
            aiDifficulty = Mathf.Clamp(difficulty, 1, 3);
            
            if (aiPlayer != null)
            {
                aiPlayer.AIDifficulty = aiDifficulty;
            }
            
            // Adjust AI behavior based on difficulty
            switch (aiDifficulty)
            {
                case 1: // Easy
                    decisionInterval = 3f;
                    attackRange = 8f;
                    break;
                case 2: // Medium
                    decisionInterval = 2f;
                    attackRange = 10f;
                    break;
                case 3: // Hard
                    decisionInterval = 1f;
                    attackRange = 12f;
                    break;
            }
        }
        
        /// <summary>
        /// Get current AI statistics
        /// </summary>
        public AITeamStatistics GetAIStatistics()
        {
            return new AITeamStatistics
            {
                UnitsRemaining = aiUnits.Count,
                Difficulty = aiDifficulty,
                DecisionInterval = decisionInterval,
                AttackRange = attackRange
            };
        }
    }
    
    /// <summary>
    /// Data structure for AI team statistics
    /// </summary>
    [System.Serializable]
    public struct AITeamStatistics
    {
        public int UnitsRemaining;
        public int Difficulty;
        public float DecisionInterval;
        public float AttackRange;
    }
} 