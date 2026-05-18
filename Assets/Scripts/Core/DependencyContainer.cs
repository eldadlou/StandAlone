using System;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Core.Services;

namespace MyGame.Core
{
    /// <summary>
    /// Dependency injection container. Resolves by compile-time service type (often an interface).
    /// </summary>
    public class DependencyContainer : IDependencyResolver
    {
        private static DependencyContainer _instance;
        public static DependencyContainer Instance => _instance ??= new DependencyContainer();

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

        public void Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            _services[typeof(T)] = service;
        }

        public void Register<T>(Func<T> factory) where T : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            _factories[typeof(T)] = () => factory();
        }

        public void RegisterSingleton<T>(Func<T> factory) where T : class
        {
            T instance = null;
            Register<T>(() => instance ??= factory());
        }

        public void RegisterAs<TService>(object implementation) where TService : class
        {
            if (implementation == null)
                throw new ArgumentNullException(nameof(implementation));
            if (implementation is not TService)
                throw new ArgumentException($"Implementation must be assignable to {typeof(TService).Name}.", nameof(implementation));
            _services[typeof(TService)] = implementation;
        }

        public T Resolve<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
                return (T)service;

            if (_factories.TryGetValue(type, out var factory))
            {
                service = factory();
                _services[type] = service;
                return (T)service;
            }

            throw new InvalidOperationException($"Service of type {type.Name} is not registered");
        }

        public T TryResolve<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
                return (T)service;

            if (!_factories.TryGetValue(type, out var factory))
                return null;

            try
            {
                service = factory();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"DependencyContainer: factory for {type.Name} failed: {ex.Message}");
                return null;
            }

            _services[type] = service;
            return (T)service;
        }

        public void Clear()
        {
            _services.Clear();
            _factories.Clear();
        }

        public bool IsRegistered<T>()
        {
            var type = typeof(T);
            return _services.ContainsKey(type) || _factories.ContainsKey(type);
        }
    }
}
