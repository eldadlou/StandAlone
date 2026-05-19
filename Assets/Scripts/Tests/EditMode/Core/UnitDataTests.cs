using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Game;
using NUnit.Framework;
using UnityEngine;

namespace MyGame.Tests.Core
{
    [TestFixture]
    [Category("Core")]
    public class UnitDataTests
    {
        [Test]
        public void SetMoving_SetsDestinationAndIsMoving()
        {
            var data = new UnitData(UnitType.Tank, 100f, 5f, new Player("P", Team.Player));
            var destination = new Vector3(10f, 0f, 5f);

            data.SetMoving(destination);

            Assert.IsTrue(data.IsMoving);
            Assert.AreEqual(destination, data.Destination);
        }

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            var data = new UnitData(UnitType.Soldier, 80f, 5f, new Player("P", Team.Player));

            data.TakeDamage(25f);

            Assert.AreEqual(55f, data.Health, 0.01f);
        }

        [Test]
        public void TakeDamage_WhenHealthReachesZero_InvokesOnDeath()
        {
            var data = new UnitData(UnitType.Soldier, 50f, 5f, new Player("P", Team.Player));
            var died = false;
            data.OnDeath += _ => died = true;

            data.TakeDamage(50f);

            Assert.IsTrue(died);
            Assert.LessOrEqual(data.Health, 0f);
        }
    }
}
