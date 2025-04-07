using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pool
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _container;
        private readonly Queue<T> _pool = new();
        private readonly Action<T> _onGetFromPool;
        private readonly Action<T> _onReturnToPool;

        public ObjectPool(T prefab, int initialSize, Transform container,
            Action<T> onGetFromPool = null, Action<T> onReturnToPool = null)
        {
            _prefab = prefab;
            _container = container;
            _onGetFromPool = onGetFromPool;
            _onReturnToPool = onReturnToPool;

            for (var i = 0; i < initialSize; i++)
            {
                T obj = CreateNew();
                _pool.Enqueue(obj);
            }
        }

        private T CreateNew()
        {
            T obj = UnityEngine.Object.Instantiate(_prefab, _container);
            obj.gameObject.SetActive(false);
            return obj;
        }

        public T Get()
        {
            T obj = _pool.Count > 0 ? _pool.Dequeue() : CreateNew();
            obj.gameObject.SetActive(true);
            obj.transform.SetParent(null);
            _onGetFromPool?.Invoke(obj);
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null) return;

            _onReturnToPool?.Invoke(obj);
            obj.transform.SetParent(_container);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}