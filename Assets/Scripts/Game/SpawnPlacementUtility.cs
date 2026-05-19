using UnityEngine;
using UnityEngine.AI;
using MyGame.Core.Units;

namespace MyGame.Game
{
    /// <summary>
    /// Resolves spawn positions from Terrain bounds/sampling, physics ground, and local NavMesh.
    /// </summary>
    public static class SpawnPlacementUtility
    {
        private const float RaycastStartHeight = 2000f;
        private const float RaycastMaxDistance = 4000f;
        private const float NavMeshSampleRadius = 8f;
        private const int UnitLayers = (1 << 6) | (1 << 7);

        public static bool TryGetTerrain(Terrain assignedTerrain, out Terrain terrain)
        {
            terrain = assignedTerrain;
            if (terrain != null)
                return true;

            terrain = Terrain.activeTerrain;
            if (terrain != null)
                return true;

            var terrains = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            if (terrains == null || terrains.Length == 0)
            {
                terrain = null;
                return false;
            }

            terrain = terrains[0];
            return true;
        }

        public static Bounds GetTerrainWorldBounds(Terrain terrain)
        {
            var data = terrain.terrainData;
            var worldSize = data.size;
            var worldMin = terrain.transform.position;
            var center = worldMin + worldSize * 0.5f;
            return new Bounds(center, worldSize);
        }

        public static bool IsInsideTerrainXZ(Terrain terrain, float worldX, float worldZ)
        {
            var bounds = GetTerrainWorldBounds(terrain);
            return worldX >= bounds.min.x && worldX <= bounds.max.x
                && worldZ >= bounds.min.z && worldZ <= bounds.max.z;
        }

        public static Vector3 SampleTerrainSurface(Terrain terrain, float worldX, float worldZ)
        {
            var worldPos = new Vector3(worldX, 0f, worldZ);
            var height = terrain.SampleHeight(worldPos) + terrain.transform.position.y;
            return new Vector3(worldX, height, worldZ);
        }

        /// <summary>
        /// Match center and team centers on the terrain (or snapped spawn center).
        /// </summary>
        public static void ResolveTeamSpawnCenters(
            Terrain assignedTerrain,
            Transform spawnCenter,
            float teamSeparation,
            float edgeMargin,
            out Vector3 playerCenter,
            out Vector3 aiCenter)
        {
            if (!TryGetTerrain(assignedTerrain, out var terrain))
            {
                var fallback = spawnCenter != null ? spawnCenter.position : Vector3.zero;
                fallback = ResolveGroundPosition(fallback, null);
                playerCenter = fallback + Vector3.left * teamSeparation * 0.5f;
                aiCenter = fallback + Vector3.right * teamSeparation * 0.5f;
                playerCenter = ResolveGroundPosition(playerCenter, null);
                aiCenter = ResolveGroundPosition(aiCenter, null);
                Debug.LogWarning(
                    "SpawnPlacement: No Terrain found. Assign Terrain on GameUnitSpawner or place Spawn Center on the map.");
                return;
            }

            var bounds = GetTerrainWorldBounds(terrain);
            var centerX = spawnCenter != null ? spawnCenter.position.x : bounds.center.x;
            var centerZ = spawnCenter != null ? spawnCenter.position.z : bounds.center.z;

            centerX = Mathf.Clamp(centerX, bounds.min.x + edgeMargin, bounds.max.x - edgeMargin);
            centerZ = Mathf.Clamp(centerZ, bounds.min.z + edgeMargin, bounds.max.z - edgeMargin);

            var matchCenter = SampleTerrainSurface(terrain, centerX, centerZ);
            matchCenter = ResolveGroundPosition(matchCenter, terrain);

            var halfSep = teamSeparation * 0.5f;
            var playerX = Mathf.Clamp(matchCenter.x - halfSep, bounds.min.x + edgeMargin, bounds.max.x - edgeMargin);
            var aiX = Mathf.Clamp(matchCenter.x + halfSep, bounds.min.x + edgeMargin, bounds.max.x - edgeMargin);

            playerCenter = ResolveGroundPosition(SampleTerrainSurface(terrain, playerX, matchCenter.z), terrain);
            aiCenter = ResolveGroundPosition(SampleTerrainSurface(terrain, aiX, matchCenter.z), terrain);
        }

        public static Vector3 ClampXZToTerrain(Terrain terrain, Vector3 worldPosition, float edgeMargin)
        {
            var bounds = GetTerrainWorldBounds(terrain);
            worldPosition.x = Mathf.Clamp(worldPosition.x, bounds.min.x + edgeMargin, bounds.max.x - edgeMargin);
            worldPosition.z = Mathf.Clamp(worldPosition.z, bounds.min.z + edgeMargin, bounds.max.z - edgeMargin);
            return SampleTerrainSurface(terrain, worldPosition.x, worldPosition.z);
        }

        /// <summary>
        /// World position on walkable ground for the given XZ.
        /// </summary>
        public static Vector3 ResolveGroundPosition(Vector3 worldPosition, Terrain assignedTerrain = null)
        {
            var x = worldPosition.x;
            var z = worldPosition.z;
            var groundY = worldPosition.y;

            if (TryGetTerrain(assignedTerrain, out var terrain)
                && IsInsideTerrainXZ(terrain, x, z))
            {
                groundY = SampleTerrainSurface(terrain, x, z).y;
            }
            else
            {
                var layerMask = Physics.DefaultRaycastLayers & ~UnitLayers;
                var rayOrigin = new Vector3(x, RaycastStartHeight, z);
                if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, RaycastMaxDistance, layerMask,
                        QueryTriggerInteraction.Ignore))
                {
                    groundY = hit.point.y;
                }
            }

            var probe = new Vector3(x, groundY, z);
            if (NavMesh.SamplePosition(probe, out var navHit, NavMeshSampleRadius, NavMesh.AllAreas))
                return navHit.position;

            return probe;
        }

        public static void PlaceUnitOnGround(Unit unit, Vector3 desiredPosition, Terrain assignedTerrain = null)
        {
            if (unit == null)
                return;

            var surface = ResolveGroundPosition(desiredPosition, assignedTerrain);
            var agent = unit.GetComponent<NavMeshAgent>();

            if (agent != null)
                agent.enabled = false;

            unit.transform.position = surface;
            AlignPivotToGround(unit.gameObject, surface.y);

            var finalPosition = unit.transform.position;

            if (agent != null)
            {
                agent.enabled = true;
                if (!agent.Warp(finalPosition)
                    && NavMesh.SamplePosition(finalPosition, out var navHit, NavMeshSampleRadius, NavMesh.AllAreas))
                {
                    agent.Warp(navHit.position);
                }
            }
        }

        private static void AlignPivotToGround(GameObject unitRoot, float groundY)
        {
            var pivotToBottom = GetPivotToBottomOffset(unitRoot);
            if (pivotToBottom <= 0.001f)
                return;

            var position = unitRoot.transform.position;
            position.y = groundY + pivotToBottom;
            unitRoot.transform.position = position;
        }

        private static float GetPivotToBottomOffset(GameObject unitRoot)
        {
            var renderers = unitRoot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return 0f;

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                if (renderers[i].enabled)
                    bounds.Encapsulate(renderers[i].bounds);
            }

            return unitRoot.transform.position.y - bounds.min.y;
        }
    }
}
