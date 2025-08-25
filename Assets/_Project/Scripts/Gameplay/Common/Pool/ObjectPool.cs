using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Queue<T> queue = new Queue<T>();
    private readonly Transform parent;
    private readonly Func<T> factory;
    private readonly bool useFactory;
    
    public ObjectPool(T prefab, int initialSize = 0, Transform parent = null, Func<T> factory = null)
    {
        this.prefab = prefab;
        this.parent = parent != null ? parent : PoolContainer.Root;
        this.factory = factory;
        this.useFactory = factory != null;
        
        for (int i = 0; i < initialSize; i++)
            this.queue.Enqueue(CreateInstance(false));
    }
    
    public T Get()
    {
        T instance = null;
        
        while (queue.Count > 0 && !instance)
            instance = queue.Dequeue();

        if (!instance)
            instance = CreateInstance(false);

        instance.transform.SetParent(parent, true);
        instance.gameObject.SetActive(true);
        
        return instance;
    }
    
    public void Release(T instance)
    {
        if (!instance)
            return;
        
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(parent, true);
        queue.Enqueue(instance);
    }
    
    private T CreateInstance(bool active)
    {
        T instance;
        if (useFactory)
            instance = factory();
        else
            instance = GameObject.Instantiate(prefab, parent);

        instance.transform.SetParent(parent, worldPositionStays: true);
        instance.gameObject.SetActive(active);
        
        return instance;
    }
}