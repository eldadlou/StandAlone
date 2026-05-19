using MyGame.Core;
using MyGame.Game;
using NUnit.Framework;
using UnityEngine;

namespace MyGame.Tests.Game
{
    [TestFixture]
    [Category("Game")]
    [Category("Spawn")]
    public class SpawnPlacementUtilityTests
    {
        private Terrain _terrain;
        private GameObject _terrainObject;

        [SetUp]
        public void SetUp()
        {
            _terrain = TestTerrainFactory.CreateFlatTerrain(Vector3.zero, new Vector3(200f, 50f, 200f));
            _terrainObject = _terrain.gameObject;
        }

        [TearDown]
        public void TearDown()
        {
            if (_terrainObject != null)
                Object.DestroyImmediate(_terrainObject);
        }

        [Test]
        public void SampleTerrainSurface_ReturnsHeightAtTerrainBase()
        {
            var surface = SpawnPlacementUtility.SampleTerrainSurface(_terrain, 50f, 50f);

            Assert.AreEqual(50f, surface.x, 0.01f);
            Assert.AreEqual(50f, surface.z, 0.01f);
            Assert.AreEqual(_terrain.transform.position.y, surface.y, 0.1f);
        }

        [Test]
        public void GetTerrainWorldBounds_MatchesTerrainSize()
        {
            var bounds = SpawnPlacementUtility.GetTerrainWorldBounds(_terrain);

            Assert.AreEqual(200f, bounds.size.x, 0.01f);
            Assert.AreEqual(200f, bounds.size.z, 0.01f);
        }

        [Test]
        public void IsInsideTerrainXZ_InsideBounds_ReturnsTrue()
        {
            Assert.IsTrue(SpawnPlacementUtility.IsInsideTerrainXZ(_terrain, 100f, 100f));
        }

        [Test]
        public void IsInsideTerrainXZ_OutsideBounds_ReturnsFalse()
        {
            Assert.IsFalse(SpawnPlacementUtility.IsInsideTerrainXZ(_terrain, -10f, 100f));
        }

        [Test]
        public void ResolveTeamSpawnCenters_SeparatesTeamsAlongX()
        {
            SpawnPlacementUtility.ResolveTeamSpawnCenters(
                _terrain,
                null,
                teamSeparation: 40f,
                edgeMargin: 10f,
                out var playerCenter,
                out var aiCenter);

            Assert.Greater(aiCenter.x, playerCenter.x);
            Assert.AreEqual(playerCenter.z, aiCenter.z, 0.5f);
            Assert.IsTrue(SpawnPlacementUtility.IsInsideTerrainXZ(_terrain, playerCenter.x, playerCenter.z));
            Assert.IsTrue(SpawnPlacementUtility.IsInsideTerrainXZ(_terrain, aiCenter.x, aiCenter.z));
        }

        [Test]
        public void ClampXZToTerrain_WhenOutside_ClampedInside()
        {
            var clamped = SpawnPlacementUtility.ClampXZToTerrain(_terrain, new Vector3(-50f, 0f, 300f), 10f);

            var bounds = SpawnPlacementUtility.GetTerrainWorldBounds(_terrain);
            Assert.GreaterOrEqual(clamped.x, bounds.min.x + 10f);
            Assert.LessOrEqual(clamped.z, bounds.max.z - 10f);
        }
    }
}
