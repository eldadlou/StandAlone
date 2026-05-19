using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MyGame.Core;
using MyGame.Core.Events;

namespace MyGame.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class GameEventsIntegrationTests
    {
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GameEvents.ClearAllEvents();
            yield return null;
        }

        [UnityTest]
        public IEnumerator TriggerSystemsInitialized_WhenSubscribed_FiresInPlayMode()
        {
            var fired = false;
            GameEvents.OnSystemsInitialized += () => fired = true;

            GameEvents.TriggerSystemsInitialized();
            yield return null;

            Assert.IsTrue(fired);
        }
    }
}
