using System.Collections.Generic;
using UnityEngine;

public class InsideSet
{
    private readonly HashSet<int> ids = new HashSet<int>();
    
    public int Count => ids.Count;

    public bool Add(Component component) => ids.Add(component.GetInstanceID());
    
    public bool Remove(Component component) => ids.Remove(component.GetInstanceID());
    
    public void Clear() => ids.Clear();
}