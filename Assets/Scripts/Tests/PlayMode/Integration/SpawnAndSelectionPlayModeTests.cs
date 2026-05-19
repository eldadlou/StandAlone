using System.Collections;
using System.Reflection;
using MyGame.Core;
using MyGame.Core.Units;
using Team = MyGame.Core.Team;
using MyGame.Game;
using MyGame.Input;
using MyGame.Presentation;
using MyGame.RuntimeSystems.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyGame.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    [Category("Smoke")]
    public class SpawnAndSelectionPlayModeTests
    {
        private readonly System.Collections.Generic.List<GameObject> _created = new();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SystemInitializer.ClearAllSystems();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in _created)
            {
                if (go != null)
                    Object.Destroy(go);
            }

            _created.Clear();
            SystemInitializer.ClearAllSystems();
            yield return null;
        }

        [UnityTest]
        public IEnumerator SystemInitializer_RegistersSelectionAndCommandSystems()
        {
            var host = Track(new GameObject("Systems"));
            var initializer = host.AddComponent<SystemInitializer>();
            initializer.InitializeSystems();
            yield return null;

            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<SelectionManager>());
            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<InputHandler>());
            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<CommandSystem>());
            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<MovementSystem>());
        }

        [UnityTest]
        public IEnumerator GameUnitSpawner_WithTerrain_SpawnsUnitsOnSurface()
        {
            var terrain = CreateTerrain();
            Track(terrain.gameObject);
            var unitPrefab = CreateUnitPrefab();
            unitPrefab.SetActive(false);
            Track(unitPrefab);
            var spawnerGo = Track(new GameObject("Spawner"));
            var spawner = spawnerGo.AddComponent<GameUnitSpawner>();

            SetSpawnerFields(spawner, terrain, unitPrefab);
            spawner.SpawnMatchUnits();
            yield return null;

            var units = Object.FindObjectsByType<GenericUnit>(FindObjectsSortMode.None);
            Assert.GreaterOrEqual(units.Length, 1);

            var bounds = SpawnPlacementUtility.GetTerrainWorldBounds(terrain);
            foreach (var unit in units)
            {
                Assert.GreaterOrEqual(unit.transform.position.y, bounds.min.y - 1f);
                Assert.LessOrEqual(unit.transform.position.y, bounds.max.y + 5f);
            }
        }

        [UnityTest]
        public IEnumerator SelectUnit_ThenMoveTo_SetsUnitMoving()
        {
            var host = Track(new GameObject("Systems"));
            host.AddComponent<SystemInitializer>().InitializeSystems();
            yield return null;

            var unitGo = Track(CreateUnitPrefab());
            var unit = unitGo.GetComponent<GenericUnit>();
            yield return null;

            unit.GetUnitData().Owner = new Player("TestPlayer", Team.Player);
            unitGo.layer = 6;

            var selectionGo = Track(new GameObject("Selection"));
            var selection = selectionGo.AddComponent<SelectionManager>();
            selection.SelectUnit(unit);
            yield return null;

            Assert.AreEqual(1, selection.SelectedUnits.Count);

            unit.MoveTo(new Vector3(15f, 0f, 10f));
            yield return null;
            yield return new WaitForSeconds(0.1f);

            Assert.IsTrue(unit.IsMoving);
        }

        private GameObject Track(GameObject go)
        {
            _created.Add(go);
            return go;
        }

        private static Terrain CreateTerrain()
        {
            var data = new TerrainData
            {
                heightmapResolution = 33,
                size = new Vector3(120f, 30f, 120f)
            };
            var go = Terrain.CreateTerrainGameObject(data);
            go.name = "PlayModeTestTerrain";
            return go.GetComponent<Terrain>();
        }

        private static GameObject CreateUnitPrefab()
        {
            var go = new GameObject("TestUnitPrefab");
            go.AddComponent<CapsuleCollider>();
            go.AddComponent<GenericUnit>();
            return go;
        }

        private static void SetSpawnerFields(GameUnitSpawner spawner, Terrain terrain, GameObject unitPrefab)
        {
            var type = typeof(GameUnitSpawner);
            type.GetField("terrain", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(spawner, terrain);
            type.GetField("spawnOnStart", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(spawner, false);

            var entryType = type.GetNestedType("UnitSpawnEntry", BindingFlags.Public);
            var entry = System.Activator.CreateInstance(entryType);
            entryType.GetField("prefab")?.SetValue(entry, unitPrefab);

            var playerArray = System.Array.CreateInstance(entryType, 1);
            playerArray.SetValue(entry, 0);

            type.GetField("playerUnits", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(spawner, playerArray);
            type.GetField("aiUnits", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(spawner, System.Array.CreateInstance(entryType, 0));
        }
    }
}
