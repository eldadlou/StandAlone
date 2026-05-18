using System;

namespace MyGame.Core.Services
{
    /// <summary>
    /// Abstraction over the game's service container for tests and loose coupling.
    /// </summary>
    public interface IDependencyResolver
    {
        void Register<T>(T service) where T : class;

        /// <summary>
        /// Registers <paramref name="implementation"/> under <typeparamref name="TService"/> (typically an interface).
        /// </summary>
        void RegisterAs<TService>(object implementation) where TService : class;

        T Resolve<T>() where T : class;
        T TryResolve<T>() where T : class;
        bool IsRegistered<T>();
        void Clear();
    }
}
