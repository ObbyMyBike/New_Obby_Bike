using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Queue<T> _queue = new Queue<T>();
    private readonly Transform _parent;
    private readonly Func<T> _factory;

    public ObjectPool(T prefab, int initialSize = 0, Transform parent = null, Func<T> factory = null)
    {
        _prefab = prefab;
        _parent = parent != null ? parent : PoolContainer.Root;
        _factory = factory;
        
        for (int i = 0; i < initialSize; i++)
        {
            var instance = CreateInstance(active: false);
            _queue.Enqueue(instance);
        }
    }
    
    public T Get()
    {
        T instance = (_queue.Count > 0) ? _queue.Dequeue() : CreateInstance(active: false);
        
        instance.transform.SetParent(_parent, worldPositionStays: true);
        instance.gameObject.SetActive(true);
        
        return instance;
    }
    
    public void Release(T instance)
    {
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(_parent, worldPositionStays: true);
        _queue.Enqueue(instance);
    }
    
    private T CreateInstance(bool active)
    {
        T instance = _factory != null ? _factory() : GameObject.Instantiate(_prefab, _parent);

        instance.transform.SetParent(_parent, worldPositionStays: true);
        instance.gameObject.SetActive(active);
        
        return instance;
    }
}