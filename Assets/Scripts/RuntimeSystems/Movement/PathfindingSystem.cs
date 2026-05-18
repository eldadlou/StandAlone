using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using MyGame.Core;

namespace MyGame.RuntimeSystems.Movement
{
    /// <summary>
    /// Advanced pathfinding system that works with NavMesh for terrain-aware pathfinding
    /// </summary>
    public class PathfindingSystem : MonoBehaviour, INavigationMeshValidation
    {
        [Header("Pathfinding Settings")]
        [SerializeField] private float pathfindingTimeout = 5f;
        [SerializeField] private int maxPathLength = 100;
        [SerializeField] private float pathSmoothing = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugPaths = false;
        [SerializeField] private Color pathColor = Color.green;
        [SerializeField] private float pathLineWidth = 0.1f;

        private void Awake()
        {
            var container = DependencyContainer.Instance;
            container.Register(this);
            container.RegisterAs<INavigationMeshValidation>(this);
        }

        /// <summary>
        /// Find a path from start to end using NavMesh
        /// </summary>
        public List<Vector3> FindPath(Vector3 start, Vector3 end)
        {
            NavMeshPath navPath = new NavMeshPath();
            
            // Try to calculate path
            if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, navPath))
            {
                List<Vector3> path = new List<Vector3>(navPath.corners);
                
                // Smooth the path if requested
                if (pathSmoothing > 0)
                {
                    path = SmoothPath(path);
                }
                
                return path;
            }
            
            Debug.LogWarning($"No path found from {start} to {end}");
            return new List<Vector3>();
        }

        /// <summary>
        /// Find path with specific area mask
        /// </summary>
        public List<Vector3> FindPath(Vector3 start, Vector3 end, int areaMask)
        {
            NavMeshPath navPath = new NavMeshPath();
            
            if (NavMesh.CalculatePath(start, end, areaMask, navPath))
            {
                List<Vector3> path = new List<Vector3>(navPath.corners);
                
                if (pathSmoothing > 0)
                {
                    path = SmoothPath(path);
                }
                
                return path;
            }
            
            return new List<Vector3>();
        }

        /// <summary>
        /// Check if a position is reachable on NavMesh
        /// </summary>
        public bool IsPositionReachable(Vector3 position, float maxDistance = 10f)
        {
            return NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas);
        }

        /// <summary>
        /// Get the nearest valid position on NavMesh
        /// </summary>
        public Vector3 GetNearestValidPosition(Vector3 position, float maxDistance = 10f)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }
            
            return position; // Fallback to original position
        }

        /// <summary>
        /// Find path avoiding obstacles
        /// </summary>
        public List<Vector3> FindPathAvoidingObstacles(Vector3 start, Vector3 end, List<Vector3> obstacles)
        {
            // This is a simplified version - in a real implementation you might use
            // more sophisticated obstacle avoidance algorithms
            
            List<Vector3> path = FindPath(start, end);
            
            if (path.Count > 0 && obstacles.Count > 0)
            {
                path = AvoidObstacles(path, obstacles);
            }
            
            return path;
        }

        /// <summary>
        /// Smooth a path using interpolation
        /// </summary>
        private List<Vector3> SmoothPath(List<Vector3> originalPath)
        {
            if (originalPath.Count < 3) return originalPath;
            
            List<Vector3> smoothedPath = new List<Vector3>();
            smoothedPath.Add(originalPath[0]);
            
            for (int i = 1; i < originalPath.Count - 1; i++)
            {
                Vector3 prev = originalPath[i - 1];
                Vector3 current = originalPath[i];
                Vector3 next = originalPath[i + 1];
                
                // Interpolate between points
                Vector3 smoothed = Vector3.Lerp(current, (prev + next) * 0.5f, pathSmoothing);
                
                // Ensure the smoothed point is on NavMesh
                if (NavMesh.SamplePosition(smoothed, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    smoothedPath.Add(hit.position);
                }
                else
                {
                    smoothedPath.Add(current);
                }
            }
            
            smoothedPath.Add(originalPath[originalPath.Count - 1]);
            return smoothedPath;
        }

        /// <summary>
        /// Simple obstacle avoidance by pushing path points away from obstacles
        /// </summary>
        private List<Vector3> AvoidObstacles(List<Vector3> path, List<Vector3> obstacles)
        {
            List<Vector3> avoidedPath = new List<Vector3>();
            
            foreach (Vector3 pathPoint in path)
            {
                Vector3 adjustedPoint = pathPoint;
                
                foreach (Vector3 obstacle in obstacles)
                {
                    float distance = Vector3.Distance(pathPoint, obstacle);
                    if (distance < 2f) // Avoidance radius
                    {
                        Vector3 awayFromObstacle = (pathPoint - obstacle).normalized;
                        adjustedPoint += awayFromObstacle * (2f - distance) * 0.5f;
                    }
                }
                
                // Ensure adjusted point is still on NavMesh
                if (NavMesh.SamplePosition(adjustedPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    avoidedPath.Add(hit.position);
                }
                else
                {
                    avoidedPath.Add(pathPoint);
                }
            }
            
            return avoidedPath;
        }

        /// <summary>
        /// Get path distance (useful for AI decision making)
        /// </summary>
        public float GetPathDistance(Vector3 start, Vector3 end)
        {
            List<Vector3> path = FindPath(start, end);
            if (path.Count < 2) return Vector3.Distance(start, end);
            
            float distance = 0f;
            for (int i = 1; i < path.Count; i++)
            {
                distance += Vector3.Distance(path[i - 1], path[i]);
            }
            
            return distance;
        }

        /// <summary>
        /// Check if there's a direct line of sight between two points
        /// </summary>
        public bool HasLineOfSight(Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            
            // Raycast to check for obstacles
            if (Physics.Raycast(start, direction, out RaycastHit hit, distance))
            {
                return false; // Obstacle in the way
            }
            
            return true;
        }

        private void OnDrawGizmos()
        {
            if (!showDebugPaths) return;
            
            // Draw NavMesh boundaries (simplified)
            Gizmos.color = pathColor;
            
            // This is a simplified debug visualization
            // In a real implementation, you might want to draw actual paths
            // or NavMesh boundaries
        }
    }
}
