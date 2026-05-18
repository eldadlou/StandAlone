using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MyGame.Core.Interfaces;
using MyGame.Core.Units;
using MyGame.Core;

namespace MyGame.RuntimeSystems.Movement
{
    /// <summary>
    /// Professional movement system using NavMesh agents for terrain-aware movement
    /// </summary>
    public class MovementSystem : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float stoppingDistance = 0.5f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float agentSpeed = 5f;
        [SerializeField] private float agentAcceleration = 8f;
        [SerializeField] private float agentAngularSpeed = 120f;
        [SerializeField] private float baseOffset = 0.64f;
        [Header("Formation Settings")]
        [SerializeField] private float formationSpacing = 2f;
        [SerializeField] private float separationRadius = 1.5f;
        [SerializeField] private float separationStrength = 2f;
        [SerializeField] private  float bottomAgentRadius = 2.3f;
        private readonly Dictionary<IMovable, NavMeshAgent> unitAgents = new Dictionary<IMovable, NavMeshAgent>();
        private readonly List<IMovable> movingUnits = new List<IMovable>();

        private void Awake()
        {
            // Register with DependencyContainer
            DependencyContainer.Instance.Register(this);
        }

        public void RegisterUnit(IMovable unit)
        {
            if (!movingUnits.Contains(unit))
            {
                movingUnits.Add(unit);
                SetupNavMeshAgent(unit);
            }
        }

        private void SetupNavMeshAgent(IMovable unit)
        {
            if (unit is not MonoBehaviour unitMono)
                return;

            // Get or add NavMeshAgent
            var agent = unitMono.GetComponent<NavMeshAgent>();
            if (agent == null)
                agent = unitMono.gameObject.AddComponent<NavMeshAgent>();

            // Configure agent settings
            agent.speed = agentSpeed;
            agent.acceleration = agentAcceleration;
            agent.angularSpeed = agentAngularSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.height = 2f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = Random.Range(0, 100); // Random priority for better avoidance
            agent.baseOffset = baseOffset;
            agent.radius = bottomAgentRadius;
            unitAgents[unit] = agent;
        }

        private void Update()
        {
            UpdateMovement();
            ApplyFormationSeparation();
        }

        private void UpdateMovement()
        {
            for (int i = movingUnits.Count - 1; i >= 0; i--)
            {
                var unit = movingUnits[i];
                
                // Safety check: if unit is null or destroyed, remove it
                if (unit == null || (unit as MonoBehaviour) == null)
                {
                    movingUnits.RemoveAt(i);
                    continue;
                }
                
                // Safety check: if unit is inactive, remove it
                if ((unit as MonoBehaviour).gameObject.activeInHierarchy == false)
                {
                    UnregisterUnit(unit);
                    continue;
                }
                
                if (!unit.IsMoving)
                {
                    // Unit reached destination or stopped
                    if (unitAgents.TryGetValue(unit, out NavMeshAgent agent))
                    {
                        if (agent != null && agent.isActiveAndEnabled)
                        {
                            agent.ResetPath();
                        }
                    }
                    movingUnits.RemoveAt(i);
                    continue;
                }

                if (unitAgents.TryGetValue(unit, out NavMeshAgent unitAgent))
                {
                    // Safety check: if agent is null or inactive, skip this unit
                    if (unitAgent == null || !unitAgent.isActiveAndEnabled)
                    {
                        UnregisterUnit(unit);
                        continue;
                    }
                    
                    // Set destination for NavMesh agent
                    if (unitAgent.destination != unit.Destination)
                    {
                        try
                        {
                            unitAgent.SetDestination(unit.Destination);
                        }
                        catch (System.Exception e)
                        {
                            // Debug.LogWarning($"MovementSystem: Failed to set destination for {unit}: {e.Message}");
                            UnregisterUnit(unit);
                            continue;
                        }
                    }

                    // Update unit position from agent
                    if (unitAgent.hasPath)
                    {
                        Vector3 newPosition = unitAgent.nextPosition;
                        unit.UpdatePosition(newPosition);
                    }
                }
            }
        }

        private void ApplyFormationSeparation()
        {
            if (movingUnits.Count < 2) return;

            foreach (var unit in movingUnits)
            {
                if (unitAgents.TryGetValue(unit, out NavMeshAgent agent))
                {
                    Vector3 separation = Vector3.zero;
                    int neighbors = 0;

                    foreach (var other in movingUnits)
                    {
                        if (other == unit) continue;
                        
                        if (unitAgents.TryGetValue(other, out NavMeshAgent otherAgent))
                        {
                            float distance = Vector3.Distance(agent.transform.position, otherAgent.transform.position);
                            if (distance < separationRadius)
                            {
                                Vector3 awayFromOther = (agent.transform.position - otherAgent.transform.position).normalized;
                                separation += awayFromOther * (1f - distance / separationRadius);
                                neighbors++;
                            }
                        }
                    }

                    if (neighbors > 0)
                    {
                        separation /= neighbors;
                        // Apply separation as a velocity offset
                        agent.velocity += separation * separationStrength;
                    }
                }
            }
        }

        /// <summary>
        /// Move multiple units in formation to a destination
        /// </summary>
        public void MoveUnitsInFormation(List<IMovable> units, Vector3 destination)
        {
            if (units.Count == 0) return;

            // Calculate formation positions
            List<Vector3> formationPositions = CalculateFormationPositions(units.Count, destination);
            
            // Assign positions to units
            for (int i = 0; i < units.Count; i++)
            {
                if (i < formationPositions.Count)
                {
                    units[i].MoveTo(formationPositions[i]);
                }
            }
        }

        private List<Vector3> CalculateFormationPositions(int unitCount, Vector3 center)
        {
            List<Vector3> positions = new List<Vector3>();
            
            if (unitCount == 1)
            {
                positions.Add(center);
                return positions;
            }

            // Calculate grid formation
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
            float spacing = formationSpacing;
            
            for (int i = 0; i < unitCount; i++)
            {
                int row = i / gridSize;
                int col = i % gridSize;
                
                Vector3 offset = new Vector3(
                    (col - gridSize / 2f) * spacing,
                    0,
                    (row - gridSize / 2f) * spacing
                );
                
                Vector3 position = center + offset;
                
                // Ensure position is on NavMesh
                if (NavMesh.SamplePosition(position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                {
                    positions.Add(hit.position);
                }
                else
                {
                    positions.Add(position);
                }
            }
            
            return positions;
        }

        /// <summary>
        /// Clean up when unit is destroyed
        /// </summary>
        public void UnregisterUnit(IMovable unit)
        {
            movingUnits.Remove(unit);
            if (unitAgents.TryGetValue(unit, out NavMeshAgent agent))
            {
                unitAgents.Remove(unit);
                if (agent != null)
                    Destroy(agent);
            }
        }

        private void OnDestroy()
        {
            // Clean up all agents
            foreach (var agent in unitAgents.Values)
            {
                if (agent != null)
                    Destroy(agent);
            }
            unitAgents.Clear();
            movingUnits.Clear();
        }
    }
}
