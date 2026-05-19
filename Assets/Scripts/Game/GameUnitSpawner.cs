using System;
using System.Collections;
using UnityEngine;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Events;

namespace MyGame.Game
{
    /// <summary>
    /// Production match setup: choose which unit prefabs spawn for Player and AI.
    /// Uses object pools when available; otherwise instantiates prefabs directly.
    /// </summary>
    public class GameUnitSpawner : MonoBehaviour
    {
        [Serializable]
        public class UnitSpawnEntry
        {
            [Tooltip("Prefab with Tank, Truck, or GenericUnit (+ VehicleCombatUnit recommended)")]
            public GameObject prefab;

            [Tooltip("Offset from team spawn center")]
            public Vector3 localOffset;
        }

        [Header("Player team")]
        [SerializeField] private UnitSpawnEntry[] playerUnits = Array.Empty<UnitSpawnEntry>();

        [Header("AI team")]
        [SerializeField] private UnitSpawnEntry[] aiUnits = Array.Empty<UnitSpawnEntry>();

        [Header("Terrain")]
        [Tooltip("Leave empty to use Terrain.activeTerrain or the first Terrain in the scene.")]
        [SerializeField] private Terrain terrain;

        [Tooltip("Keep spawn points this far from the terrain edge (world units).")]
        [SerializeField] private float terrainEdgeMargin = 25f;

        [Header("Layout")]
        [SerializeField] private Transform spawnCenter;
        [SerializeField] private float teamSeparation = 50f;
        [SerializeField] private float unitSpacing = 8f;

        [Header("Timing")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private float spawnDelaySeconds = 0.1f;

        private void Start()
        {
            if (spawnOnStart)
                StartCoroutine(SpawnWhenReady());
        }

        private IEnumerator SpawnWhenReady()
        {
            if (spawnDelaySeconds > 0f)
                yield return new WaitForSeconds(spawnDelaySeconds);
            else
                yield return null;

            SpawnMatchUnits();
        }

        [ContextMenu("Spawn Match Units")]
        public void SpawnMatchUnits()
        {
            ClearExistingUnits();

            SpawnPlacementUtility.ResolveTeamSpawnCenters(
                terrain,
                spawnCenter,
                teamSeparation,
                terrainEdgeMargin,
                out var playerCenter,
                out var aiCenter);

            var playerCount = SpawnTeam(playerUnits, Team.Player, playerCenter);
            var aiCount = SpawnTeam(aiUnits, Team.AI, aiCenter);

            if (playerCount + aiCount == 0)
            {
                Debug.LogError(
                    "GameUnitSpawner: No units spawned. Assign prefabs with Tank/GenericUnit on Player/AI lists, " +
                    "or fix UnitPoolManager prefab slots.",
                    this);
            }
            else
            {
                Debug.Log(
                    $"GameUnitSpawner: Spawned {playerCount} player + {aiCount} AI units. " +
                    $"Player @ {playerCenter}, AI @ {aiCenter}",
                    this);
            }
        }

        [ContextMenu("Snap Spawn Center To Terrain Center")]
        private void SnapSpawnCenterToTerrain()
        {
            if (!SpawnPlacementUtility.TryGetTerrain(terrain, out var t))
            {
                Debug.LogWarning("GameUnitSpawner: No Terrain in scene.", this);
                return;
            }

            var bounds = SpawnPlacementUtility.GetTerrainWorldBounds(t);
            var center = SpawnPlacementUtility.SampleTerrainSurface(t, bounds.center.x, bounds.center.z);

            if (spawnCenter == null)
            {
                var go = new GameObject("SpawnCenter");
                go.transform.SetParent(transform);
                spawnCenter = go.transform;
            }

            spawnCenter.position = center;
            Debug.Log($"GameUnitSpawner: Spawn center set to terrain center {center}", this);
        }

        private int SpawnTeam(UnitSpawnEntry[] entries, Team team, Vector3 teamCenter)
        {
            if (entries == null || entries.Length == 0)
                return 0;

            var pool = DependencyContainer.Instance.TryResolve<UnitPoolManager>();
            var spawned = 0;
            SpawnPlacementUtility.TryGetTerrain(terrain, out var terrainRef);

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry?.prefab == null)
                    continue;

                var template = entry.prefab.GetComponent<Unit>() ?? entry.prefab.GetComponentInChildren<Unit>(true);
                if (template == null)
                {
                    Debug.LogError(
                        $"GameUnitSpawner: Prefab '{entry.prefab.name}' has no Unit/Tank/Truck/GenericUnit. Skipped.",
                        entry.prefab);
                    continue;
                }

                var position = teamCenter + entry.localOffset + Vector3.forward * (i * unitSpacing);
                if (terrainRef != null)
                    position = SpawnPlacementUtility.ClampXZToTerrain(terrainRef, position, terrainEdgeMargin);
                else
                    position = SpawnPlacementUtility.ResolveGroundPosition(position, null);

                var unit = TrySpawnFromPool(pool, template.Type, position, team);
                if (unit == null)
                    unit = SpawnFromPrefab(entry.prefab, position, team);

                if (unit != null)
                {
                    SpawnPlacementUtility.PlaceUnitOnGround(unit, position, terrainRef);
                    spawned++;
                }
            }

            return spawned;
        }

        private static Unit TrySpawnFromPool(UnitPoolManager pool, UnitType type, Vector3 position, Team team)
        {
            if (pool == null)
                return null;

            return pool.CreateUnit(type, position, Quaternion.identity, team);
        }

        private static Unit SpawnFromPrefab(GameObject prefab, Vector3 position, Team team)
        {
            var instance = Instantiate(prefab, position, Quaternion.identity);
            var unit = instance.GetComponent<Unit>() ?? instance.GetComponentInChildren<Unit>(true);
            if (unit == null)
            {
                Debug.LogError($"GameUnitSpawner: Instantiated '{prefab.name}' but no Unit component found.", instance);
                Destroy(instance);
                return null;
            }

            unit.AssignToTeam(team);
            GameEvents.TriggerUnitCreated(unit);
            return unit;
        }

        private static void ClearExistingUnits()
        {
            var units = UnityEngine.Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (var unit in units)
            {
                if (unit != null)
                    Destroy(unit.gameObject);
            }
        }
    }
}
