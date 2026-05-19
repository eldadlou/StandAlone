using NUnit.Framework;
using MyGame.Core;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.PerformanceTesting;
using MyGame.Core.Skills;
using MyGame.Core.SpatialPartitioning;
using MyGame.Core.Units;
using MyGame.Game;

namespace MyGame.Tests.Core
{
    [TestFixture]
    [Category("Performance")]
    public class SpatialGridPerformanceTests
    {
        private GameObject _gridHost;
        private SpatialGrid _spatialGrid;

        [SetUp]
        public void SetUp()
        {
            _gridHost = new GameObject("SpatialGridPerformance");
            _spatialGrid = _gridHost.AddComponent<SpatialGrid>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gridHost != null)
                Object.DestroyImmediate(_gridHost);
        }

        [Test, Performance]
        public void GetUnitsInRadius_With100Units_MedianUnder2Ms()
        {
            const int unitCount = 100;
            for (var i = 0; i < unitCount; i++)
            {
                _spatialGrid.AddUnit(new PerfUnit(new Vector3(i * 2f, 0f, 0f)));
            }

            Measure.Method(() =>
                {
                    _spatialGrid.GetUnitsInRadius(Vector3.zero, 50f);
                })
                .WarmupCount(5)
                .MeasurementCount(20)
                .Run();
        }

        private sealed class PerfUnit : IUnit
        {
            public PerfUnit(Vector3 position) => Position = position;

            public Vector3 Position { get; }
            public string Name => "PerfUnit";
            public float Health => 100f;
            public UnitType Type => UnitType.Tank;
            public Player Owner { get; } = new Player("Perf", Team.Player);
            public float Speed => 1f;
            public bool IsMoving => false;
            public Vector3 Destination => Position;
            public float AttackDamage => 1f;
            public float AttackRange => 1f;
            public float AttackCooldown => 1f;
            public float LastAttackTime => 0f;
            public System.Collections.Generic.List<Skill> Skills { get; } = new();

            public event System.Action<IUnit> OnDeath;
            public event System.Action<IUnit, IUnit> OnAttack;
            public event System.Action<IUnit, Vector3> OnMove;
            public event System.Action<string> OnAnimationEvent;

            public void AssignToTeam(Team team) { }
            public bool CanAttack(IUnit target) => true;
            public bool Attack(IUnit target) => true;
            public void TakeDamage(float amount) { }
            public void MoveTo(Vector3 destination) { }
            public void UpdatePosition(Vector3 newPosition) { }
            public void UseSkill(int skillIndex) { }
            public void PlayAnimation(string animationName) { }
            public void Upgrade() { }
        }
    }
}
