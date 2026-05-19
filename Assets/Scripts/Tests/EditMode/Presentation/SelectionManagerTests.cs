using System.Collections.Generic;
using MyGame.Core;
using MyGame.Core.Events;
using MyGame.Core.Services;
using MyGame.Core.Skills;
using MyGame.Core.Units;
using MyGame.Game;
using MyGame.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace MyGame.Tests.Presentation
{
    [TestFixture]
    [Category("Presentation")]
    [Category("Selection")]
    public class SelectionManagerTests
    {
        private GameObject _host;
        private SelectionManager _manager;

        [SetUp]
        public void SetUp()
        {
            DependencyContainer.Instance.Clear();
            GameEvents.ClearAllEvents();

            _host = new GameObject("SelectionManagerTests");
            _manager = _host.AddComponent<SelectionManager>();
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAllEvents();
            DependencyContainer.Instance.Clear();

            if (_host != null)
                Object.DestroyImmediate(_host);
        }

        [Test]
        public void SelectUnit_WhenPlayerUnit_AddsToSelection()
        {
            var unit = new MockSelectable(new Player("P", Team.Player));

            _manager.SelectUnit(unit);

            Assert.AreEqual(1, _manager.SelectedUnits.Count);
            Assert.IsTrue(unit.WasSelected);
        }

        [Test]
        public void SelectUnit_WhenAiUnit_DoesNotSelect()
        {
            var unit = new MockSelectable(new Player("AI", Team.AI));

            _manager.SelectUnit(unit);

            Assert.AreEqual(0, _manager.SelectedUnits.Count);
            Assert.IsFalse(unit.WasSelected);
        }

        [Test]
        public void SelectUnit_WhenSecondUnitSelected_DeselectsFirst()
        {
            var first = new MockSelectable(new Player("P1", Team.Player));
            var second = new MockSelectable(new Player("P2", Team.Player));

            _manager.SelectUnit(first);
            _manager.SelectUnit(second);

            Assert.AreEqual(1, _manager.SelectedUnits.Count);
            Assert.AreSame(second, _manager.SelectedUnits[0]);
            Assert.IsFalse(first.WasSelected);
            Assert.IsTrue(second.WasSelected);
        }

        [Test]
        public void DeselectAll_ClearsSelection()
        {
            var unit = new MockSelectable(new Player("P", Team.Player));
            _manager.SelectUnit(unit);

            _manager.DeselectAll();

            Assert.AreEqual(0, _manager.SelectedUnits.Count);
            Assert.IsFalse(unit.WasSelected);
        }

        [Test]
        public void SelectUnits_WhenMultiplePlayerUnits_SelectsAll()
        {
            var units = new List<ISelectableUnit>
            {
                new MockSelectable(new Player("P1", Team.Player)),
                new MockSelectable(new Player("P2", Team.Player)),
                new MockSelectable(new Player("AI", Team.AI))
            };

            _manager.SelectUnits(units);

            Assert.AreEqual(2, _manager.SelectedUnits.Count);
        }

        private sealed class MockSelectable : ISelectableUnit
        {
            public MockSelectable(Player owner) => Owner = owner;

            public bool WasSelected { get; private set; }
            public string Name => "Mock";
            public float Health => 100f;
            public Vector3 Position => Vector3.zero;
            public UnitType Type => UnitType.Tank;
            public Player Owner { get; }
            public float Speed => 1f;
            public bool IsMoving => false;
            public Vector3 Destination => Vector3.zero;
            public float AttackDamage => 1f;
            public float AttackRange => 1f;
            public float AttackCooldown => 1f;
            public float LastAttackTime => 0f;
            public List<Skill> Skills { get; } = new();

            public event System.Action<IUnit> OnDeath;
            public event System.Action<IUnit, IUnit> OnAttack;
            public event System.Action<IUnit, Vector3> OnMove;
            public event System.Action<string> OnAnimationEvent;

            public void SetSelected(bool selected) => WasSelected = selected;
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
