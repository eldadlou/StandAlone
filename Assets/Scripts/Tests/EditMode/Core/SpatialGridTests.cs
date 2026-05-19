using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using MyGame.Core;
using MyGame.Core.Events;
using MyGame.Core.SpatialPartitioning;
using MyGame.Core.Skills;
using MyGame.Core.Units;
using MyGame.Game;

namespace MyGame.Tests.Core
{
    [TestFixture]
    [Category("Core")]
    public class SpatialGridTests
    {
        private GameObject _gridHost;
        private SpatialGrid _spatialGrid;

        [SetUp]
        public void SetUp()
        {
            DependencyContainer.Instance.Clear();
            GameEvents.ClearAllEvents();

            _gridHost = new GameObject("SpatialGridTests");
            _spatialGrid = _gridHost.AddComponent<SpatialGrid>();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAllEvents();
            DependencyContainer.Instance.Clear();

            if (_gridHost != null)
                Object.DestroyImmediate(_gridHost);
        }

        [Test]
        public void AddUnit_WhenUnitAdded_IsIncludedInRadiusQuery()
        {
            var unit = CreateUnit(new Vector3(5f, 0f, 5f), "Alpha");
            _spatialGrid.AddUnit(unit);

            var results = _spatialGrid.GetUnitsInRadius(new Vector3(5f, 0f, 5f), 2f);

            Assert.Contains(unit, results);
        }

        [Test]
        public void RemoveUnit_WhenUnitRemoved_IsExcludedFromRadiusQuery()
        {
            var unit = CreateUnit(new Vector3(0f, 0f, 0f), "Beta");
            _spatialGrid.AddUnit(unit);
            _spatialGrid.RemoveUnit(unit);

            var results = _spatialGrid.GetUnitsInRadius(Vector3.zero, 10f);

            Assert.IsFalse(results.Contains(unit));
        }

        [Test]
        public void GetClosestUnit_WhenMultipleUnits_ReturnsNearest()
        {
            var near = CreateUnit(new Vector3(1f, 0f, 0f), "Near");
            var far = CreateUnit(new Vector3(20f, 0f, 0f), "Far");
            _spatialGrid.AddUnit(near);
            _spatialGrid.AddUnit(far);

            var closest = _spatialGrid.GetClosestUnit(Vector3.zero, 25f);

            Assert.AreSame(near, closest);
        }

        [Test]
        public void WorldToGrid_ThenGridToWorld_IsConsistentWithinCell()
        {
            var world = new Vector3(12f, 0f, 18f);
            var grid = _spatialGrid.WorldToGrid(world);
            var back = _spatialGrid.GridToWorld(grid);

            Assert.Less(Vector3.Distance(new Vector3(world.x, 0f, world.z), new Vector3(back.x, 0f, back.z)), 15f);
        }

        private static TestGridUnit CreateUnit(Vector3 position, string name)
        {
            return new TestGridUnit
            {
                Position = position,
                Name = name,
                Owner = new Player("Test", Team.Player)
            };
        }

        private sealed class TestGridUnit : IUnit
        {
            public string Name { get; set; }
            public float Health { get; set; } = 100f;
            public Vector3 Position { get; set; }
            public UnitType Type => UnitType.Tank;
            public Player Owner { get; set; }
            public float Speed => 1f;
            public bool IsMoving => false;
            public Vector3 Destination => Position;
            public float AttackDamage => 1f;
            public float AttackRange => 1f;
            public float AttackCooldown => 1f;
            public float LastAttackTime => 0f;
            public List<Skill> Skills { get; } = new();

            public event System.Action<IUnit> OnDeath;
            public event System.Action<IUnit, IUnit> OnAttack;
            public event System.Action<IUnit, Vector3> OnMove;
            public event System.Action<string> OnAnimationEvent;

            public void AssignToTeam(Team team) { }
            public bool CanAttack(IUnit target) => true;
            public bool Attack(IUnit target) => true;
            public void TakeDamage(float amount) => Health -= amount;
            public void MoveTo(Vector3 destination) => Position = destination;
            public void UpdatePosition(Vector3 newPosition) => Position = newPosition;
            public void UseSkill(int skillIndex) { }
            public void PlayAnimation(string animationName) { }
            public void Upgrade() { }
        }
    }
}
