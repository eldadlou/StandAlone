using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using MyGame.Core;
using MyGame.Core.Events;
using Team = MyGame.Core.Team;
using MyGame.Core.Skills;
using MyGame.Core.Units;
using MyGame.Game;

namespace MyGame.Tests.Core
{
    [TestFixture]
    [Category("Core")]
    public class GameEventsTests
    {
        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAllEvents();
        }

        [Test]
        public void TriggerUnitSelected_WhenSubscribed_InvokesHandler()
        {
            IUnit received = null;
            GameEvents.OnUnitSelected += unit => received = unit;

            var unit = new TestUnit();
            GameEvents.TriggerUnitSelected(unit);

            Assert.AreSame(unit, received);
        }

        [Test]
        public void TriggerSelectionClear_WhenSubscribed_InvokesHandler()
        {
            var invoked = false;
            GameEvents.OnSelectionClear += () => invoked = true;

            GameEvents.TriggerSelectionClear();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void TriggerTeamDefeated_WhenSubscribed_PassesTeam()
        {
            Team? received = null;
            GameEvents.OnTeamDefeated += team => received = team;

            GameEvents.TriggerTeamDefeated(Team.AI);

            Assert.AreEqual(Team.AI, received);
        }

        [Test]
        public void GetAllUnits_WhenHandlerRegistered_ReturnsHandlerResult()
        {
            var expected = new List<IUnit> { new TestUnit(), new TestUnit() };
            GameEvents.OnGetAllUnits += () => expected;

            var units = GameEvents.GetAllUnits();

            Assert.AreEqual(2, units.Count);
        }

        [Test]
        public void ClearAllEvents_WhenCalled_RemovesAllSubscribers()
        {
            var invoked = false;
            GameEvents.OnSelectionClear += () => invoked = true;
            GameEvents.ClearAllEvents();

            GameEvents.TriggerSelectionClear();

            Assert.IsFalse(invoked);
        }

        private sealed class TestUnit : IUnit
        {
            public string Name => "TestUnit";
            public float Health { get; set; } = 100f;
            public Vector3 Position { get; set; }
            public UnitType Type => UnitType.Tank;
            public Player Owner { get; set; }
            public float Speed => 1f;
            public bool IsMoving => false;
            public Vector3 Destination => Position;
            public float AttackDamage => 10f;
            public float AttackRange => 5f;
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
