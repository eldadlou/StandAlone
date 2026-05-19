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
    public class SelectionUtilityTests
    {
        private readonly List<GameObject> _created = new();

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAllEvents();
            foreach (var go in _created)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }

            _created.Clear();
        }

        [Test]
        public void IsPlayerSelectable_WithPlayerOwner_ReturnsTrue()
        {
            var unit = new MockUnit(new Player("P", Team.Player));
            Assert.IsTrue(SelectionUtility.IsPlayerSelectable(unit));
        }

        [Test]
        public void IsPlayerSelectable_WithAiOwner_ReturnsFalse()
        {
            var unit = new MockUnit(new Player("AI", Team.AI));
            Assert.IsFalse(SelectionUtility.IsPlayerSelectable(unit));
        }

        [Test]
        public void GetUnitsInScreenRect_WhenUnitInRect_IncludesUnit()
        {
            var camera = CreateCamera();
            var unit = CreateWorldUnit(new Vector3(0f, 0f, 5f), Team.Player);

            GameEvents.OnGetAllUnits += () => new List<IUnit> { unit };

            var screen = camera.WorldToScreenPoint(unit.Position);
            var rect = new Rect(screen.x - 50f, screen.y - 50f, 100f, 100f);

            var selected = SelectionUtility.GetUnitsInScreenRect(rect, camera);

            Assert.AreEqual(1, selected.Count);
            Assert.AreSame(unit, selected[0]);
        }

        [Test]
        public void TryGetMoveTargetAtScreen_WhenGroundHit_ReturnsGroundPoint()
        {
            var camera = CreateCamera();
            CreateGround(new Vector3(0f, 0f, 0f), new Vector3(20f, 0.1f, 20f));

            var screen = camera.WorldToScreenPoint(new Vector3(0f, 0f, 0f));
            var found = SelectionUtility.TryGetMoveTargetAtScreen(screen, out var point);

            Assert.IsTrue(found);
            Assert.Less(Mathf.Abs(point.y), 1f);
        }

        private Camera CreateCamera()
        {
            var go = new GameObject("TestCamera");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 15f, -10f);
            camera.transform.LookAt(Vector3.zero);
            _created.Add(go);
            return camera;
        }

        private void CreateGround(Vector3 center, Vector3 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Ground";
            go.transform.position = center;
            go.transform.localScale = size;
            _created.Add(go);
        }

        private static MockSelectableBehaviour CreateWorldUnit(Vector3 position, Team team)
        {
            var go = new GameObject("TestUnit");
            go.transform.position = position;
            go.layer = team == Team.Player ? 6 : 7;
            var unit = go.AddComponent<MockSelectableBehaviour>();
            unit.Configure(new Player(team == Team.Player ? "P" : "AI", team), position);
            return unit;
        }

        private sealed class MockUnit : IUnit
        {
            public MockUnit(Player owner) => Owner = owner;

            public string Name => "Mock";
            public float Health { get; set; } = 100f;
            public Vector3 Position { get; set; }
            public UnitType Type => UnitType.Tank;
            public Player Owner { get; }
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
            public void TakeDamage(float amount) { }
            public void MoveTo(Vector3 destination) { }
            public void UpdatePosition(Vector3 newPosition) { }
            public void UseSkill(int skillIndex) { }
            public void PlayAnimation(string animationName) { }
            public void Upgrade() { }
        }

        private sealed class MockSelectableBehaviour : MonoBehaviour, ISelectableUnit
        {
            private Player _owner;
            private Vector3 _position;

            public string Name => "Selectable";
            public float Health => 100f;
            public Vector3 Position => _position;
            public UnitType Type => UnitType.Tank;
            public Player Owner => _owner;
            public float Speed => 1f;
            public bool IsMoving => false;
            public Vector3 Destination => _position;
            public float AttackDamage => 1f;
            public float AttackRange => 1f;
            public float AttackCooldown => 1f;
            public float LastAttackTime => 0f;
            public List<Skill> Skills { get; } = new();
            public bool IsSelected { get; private set; }

            public event System.Action<IUnit> OnDeath;
            public event System.Action<IUnit, IUnit> OnAttack;
            public event System.Action<IUnit, Vector3> OnMove;
            public event System.Action<string> OnAnimationEvent;

            public void Configure(Player owner, Vector3 position)
            {
                _owner = owner;
                _position = position;
                transform.position = position;
            }

            public void SetSelected(bool selected) => IsSelected = selected;
            public void AssignToTeam(Team team) { }
            public bool CanAttack(IUnit target) => true;
            public bool Attack(IUnit target) => true;
            public void TakeDamage(float amount) { }
            public void MoveTo(Vector3 destination) => _position = destination;
            public void UpdatePosition(Vector3 newPosition) => _position = newPosition;
            public void UseSkill(int skillIndex) { }
            public void PlayAnimation(string animationName) { }
            public void Upgrade() { }
        }
    }
}
