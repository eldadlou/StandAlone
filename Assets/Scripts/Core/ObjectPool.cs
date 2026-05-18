using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Core
{
    /// <summary>
    /// Generic object pool for better performance with frequently created/destroyed objects
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly List<T> _activeObjects = new List<T>();
        private readonly int _initialSize;
        private readonly int _maxSize;

        public ObjectPool(T prefab, Transform parent = null, int initialSize = 10, int maxSize = 100)
        {
            _prefab = prefab;
            _parent = parent;
            _initialSize = initialSize;
            _maxSize = maxSize;
            
            PrewarmPool();
        }

        private void PrewarmPool()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private T CreateNewObject()
        {
            T obj = UnityEngine.Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
            return obj;
        }

        public T Get()
        {
            T obj;
            
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else if (_activeObjects.Count < _maxSize)
            {
                obj = CreateNewObject();
            }
            else
            {
                // Reuse the oldest active object
                obj = _activeObjects[0];
                _activeObjects.RemoveAt(0);
                obj.gameObject.SetActive(false);
            }

            obj.gameObject.SetActive(true);
            _activeObjects.Add(obj);
            
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null) return;

            obj.gameObject.SetActive(false);
            _activeObjects.Remove(obj);
            _pool.Enqueue(obj);
        }

        public void ReturnAll()
        {
            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                Return(_activeObjects[i]);
            }
        }

        public int ActiveCount => _activeObjects.Count;
        public int PooledCount => _pool.Count;
        public int TotalCount => ActiveCount + PooledCount;
    }

    /// <summary>
    /// Static object pool manager for easy access
    /// </summary>
    public static class ObjectPoolManager
    {
        private static readonly Dictionary<Type, object> _pools = new Dictionary<Type, object>();

        public static ObjectPool<T> GetPool<T>(T prefab, Transform parent = null, int initialSize = 10, int maxSize = 100) where T : Component
        {
            var type = typeof(T);
            
            if (!_pools.TryGetValue(type, out var pool))
            {
                pool = new ObjectPool<T>(prefab, parent, initialSize, maxSize);
                _pools[type] = pool;
            }
            
            return (ObjectPool<T>)pool;
        }

        public static void ClearAllPools()
        {
            _pools.Clear();
        }
    }
}