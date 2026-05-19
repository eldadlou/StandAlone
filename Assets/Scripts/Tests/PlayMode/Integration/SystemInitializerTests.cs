using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MyGame.Core;
using MyGame.Core.SpatialPartitioning;
using MyGame.RuntimeSystems.Combat;
using MyGame.RuntimeSystems.Movement;
using MyGame.Presentation;
using MyGame.Input;
using MyGame.Game;

namespace MyGame.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class SystemInitializerTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SystemInitializer.ClearAllSystems();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SystemInitializer.ClearAllSystems();
            yield return null;
        }

        [UnityTest]
        public IEnumerator InitializeSystems_WhenAutoSetupDisabled_RegistersCoreServices()
        {
            var host = new GameObject("SystemInitializer");
            var initializer = host.AddComponent<SystemInitializer>();

            initializer.InitializeSystems();
            yield return null;

            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<SpatialGrid>());
            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<LightweightFireSystem>());
            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<MovementSystem>());
            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<SelectionManager>());
            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<InputHandler>());
            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<CommandSystem>());

            Object.Destroy(host);
        }

        [UnityTest]
        public IEnumerator ClearAllSystems_WhenCalled_ClearsDependencyContainer()
        {
            var host = new GameObject("SystemInitializer");
            var initializer = host.AddComponent<SystemInitializer>();
            initializer.InitializeSystems();
            yield return null;

            SystemInitializer.ClearAllSystems();

            Assert.IsFalse(DependencyContainer.Instance.IsRegistered<SpatialGrid>());

            Object.Destroy(host);
        }
    }
}
