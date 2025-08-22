using System.Collections.Generic;
using UnityEngine;

public class OrbitActivationHub : MonoBehaviour
{
    [SerializeField] private SimpleOrbitMovement _orbit;
    
    private readonly HashSet<object> requesters = new HashSet<object>();
    
    private int _presence;

    public void AddUser()
    {
        _presence = Mathf.Max(0, _presence + 1);
        
        UpdateActive();
    }

    public void RemoveUser()
    {
        _presence = Mathf.Max(0, _presence - 1);
        
        UpdateActive();
    }

    public void RequestOpen(object requester, bool on)
    {
        bool changed = on ? requesters.Add(requester) : requesters.Remove(requester);
        
        if (changed)
            UpdateActive();
    }

    private void UpdateActive()
    {
        if (_orbit == null)
            return;
        
        _orbit.UseTrigger = true;

        bool isActive = _presence > 0;
        
        if (_orbit.IsPlayerInside != isActive)
            _orbit.SetPlayerInside(isActive);
    }
}