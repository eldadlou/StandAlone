using System;
using NUnit.Framework;
using MyGame.Core;

namespace MyGame.Tests.Core
{
    [TestFixture]
    [Category("Core")]
    public class DependencyContainerTests
    {
        [SetUp]
        public void SetUp()
        {
            DependencyContainer.Instance.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            DependencyContainer.Instance.Clear();
        }

        [Test]
        public void Register_WhenServiceProvided_ResolveReturnsSameInstance()
        {
            var service = new TestService();
            DependencyContainer.Instance.Register<ITestService>(service);

            Assert.AreSame(service, DependencyContainer.Instance.Resolve<ITestService>());
        }

        [Test]
        public void RegisterFactory_WhenResolved_CreatesAndCachesInstance()
        {
            var createCount = 0;
            DependencyContainer.Instance.Register<ITestService>(() =>
            {
                createCount++;
                return new TestService();
            });

            var first = DependencyContainer.Instance.Resolve<ITestService>();
            var second = DependencyContainer.Instance.Resolve<ITestService>();

            Assert.AreEqual(1, createCount);
            Assert.AreSame(first, second);
        }

        [Test]
        public void RegisterSingletonFactory_WhenResolvedMultipleTimes_ReturnsSameInstance()
        {
            DependencyContainer.Instance.RegisterSingleton<ITestService>(() => new TestService());

            var first = DependencyContainer.Instance.Resolve<ITestService>();
            var second = DependencyContainer.Instance.Resolve<ITestService>();

            Assert.AreSame(first, second);
        }

        [Test]
        public void Resolve_WhenNotRegistered_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => DependencyContainer.Instance.Resolve<ITestService>());
        }

        [Test]
        public void TryResolve_WhenFactoryThrows_ReturnsNull()
        {
            DependencyContainer.Instance.Register<ITestService>(() => throw new InvalidOperationException("factory failed"));

            Assert.IsNull(DependencyContainer.Instance.TryResolve<ITestService>());
        }

        [Test]
        public void Clear_WhenCalled_RemovesAllRegistrations()
        {
            DependencyContainer.Instance.Register<ITestService>(new TestService());
            DependencyContainer.Instance.Clear();

            Assert.IsFalse(DependencyContainer.Instance.IsRegistered<ITestService>());
        }

        [Test]
        public void IsRegistered_WhenServiceRegistered_ReturnsTrue()
        {
            DependencyContainer.Instance.Register<ITestService>(new TestService());
            Assert.IsTrue(DependencyContainer.Instance.IsRegistered<ITestService>());
        }

        private interface ITestService { }

        private sealed class TestService : ITestService { }
    }
}
