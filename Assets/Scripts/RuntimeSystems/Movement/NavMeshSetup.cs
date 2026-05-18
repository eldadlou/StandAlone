using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace MyGame.RuntimeSystems.Movement
{
    /// <summary>
    /// Helper script for NavMesh setup and management
    /// </summary>
    public class NavMeshSetup : MonoBehaviour
    {
        [Header("NavMesh Settings")]
        [SerializeField] private bool autoBakeOnStart = false;
        [SerializeField] private bool showNavMeshDebug = false;
        
        [Header("Baking Settings")]
        [SerializeField] private float agentRadius = 0.5f;
        [SerializeField] private float agentHeight = 2f;
        [SerializeField] private float maxSlope = 45f;
        [SerializeField] private float stepHeight = 0.4f;
        [SerializeField] private float dropHeight = 2f;
        [SerializeField] private float jumpDistance = 2f;
        [SerializeField] private NavMeshSurface surface;
        private void Start()
        {
            if (autoBakeOnStart)
            {
                BakeNavMesh();
            }
        }

        /// <summary>
        /// Bake NavMesh for the current scene
        /// </summary>
        [ContextMenu("Bake NavMesh")]
        public void BakeNavMesh()
        {
            Debug.Log("Baking NavMesh...");
            
            // Create NavMesh surface
            surface = gameObject.GetComponent<NavMeshSurface>();
            if (surface == null)
            {
                surface = gameObject.AddComponent<NavMeshSurface>();
            }

            // Configure surface settings
            surface.agentTypeID = 0; // Default agent type
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.defaultArea = 0; // Walkable area
            
            // Set agent parameters
            surface.voxelSize = agentRadius;
            surface.tileSize = 1; // TODO :NEEED TO CHECK THIS -the surface dont have THose api's making compile error
            // surface.agentRadius = agentRadius; // 
            // surface.agentHeight = agentHeight;
            // surface.stepHeight = maxSlope;
            // surface.stepHeight = stepHeight;
            // surface.dropHeight = dropHeight;
            // surface.jumpDistance = jumpDistance;

            // Build NavMesh
            surface.BuildNavMesh();
            
            Debug.Log("NavMesh baking completed!");
        }

        /// <summary>
        /// Clear existing NavMesh
        /// </summary>
        [ContextMenu("Clear NavMesh")]
        public void ClearNavMesh()
        {
            NavMesh.RemoveAllNavMeshData();
            Debug.Log("NavMesh cleared!");
        }

        /// <summary>
        /// Check if NavMesh is properly set up
        /// </summary>
        public bool IsNavMeshValid()
        {
            return NavMesh.CalculateTriangulation().vertices.Length > 0;
        }

        /// <summary>
        /// Get NavMesh statistics
        /// </summary>
        public void LogNavMeshStats()
        {
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            
            Debug.Log($"NavMesh Statistics:");
            Debug.Log($"- Vertices: {triangulation.vertices.Length}");
            Debug.Log($"- Indices: {triangulation.indices.Length}");
            Debug.Log($"- Areas: {triangulation.areas.Length}");
            Debug.Log($"- Valid: {IsNavMeshValid()}");
        }

        /// <summary>
        /// Validate that all units can navigate properly
        /// </summary>
        public void ValidateUnitNavigation()
        {
            var units = FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
            
            Debug.Log($"Found {units.Length} NavMesh agents in scene");
            
            foreach (var agent in units)
            {
                if (!agent.isOnNavMesh)
                {
                    Debug.LogWarning($"Agent {agent.name} is not on NavMesh!");
                }
                else
                {
                    Debug.Log($"Agent {agent.name} is properly positioned on NavMesh");
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!showNavMeshDebug) return;
            
            // Draw NavMesh boundaries
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            
            Gizmos.color = Color.green;
            
            for (int i = 0; i < triangulation.indices.Length; i += 3)
            {
                Vector3 v1 = triangulation.vertices[triangulation.indices[i]];
                Vector3 v2 = triangulation.vertices[triangulation.indices[i + 1]];
                Vector3 v3 = triangulation.vertices[triangulation.indices[i + 2]];
                
                Gizmos.DrawLine(v1, v2);
                Gizmos.DrawLine(v2, v3);
                Gizmos.DrawLine(v3, v1);
            }
        }

        /// <summary>
        /// Setup instructions for the user
        /// </summary>
        [ContextMenu("Show Setup Instructions")]
        public void ShowSetupInstructions()
        {
            Debug.Log("=== NavMesh Setup Instructions ===");
            Debug.Log("1. Ensure your terrain/ground objects have Colliders");
            Debug.Log("2. Mark walkable areas with 'Navigation Static' in Object Inspector");
            Debug.Log("3. Run 'Bake NavMesh' from this component's context menu");
            Debug.Log("4. Verify NavMesh is valid using 'Log NavMesh Stats'");
            Debug.Log("5. Test unit movement to ensure everything works");
            Debug.Log("==================================");
        }
    }
} 