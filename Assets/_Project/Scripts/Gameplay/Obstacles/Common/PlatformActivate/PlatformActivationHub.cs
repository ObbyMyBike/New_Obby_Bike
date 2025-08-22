using System;
using UnityEngine;

public class PlatformActivationHub : MonoBehaviour
{
    public event Action FirstUserEntered;
    public event Action LastUserExited;
    
    [SerializeField] private MovingPlatformTriggered _platform;
    
    private int _users;

    private void Reset()
    {
        if (_platform == null)
            _platform = GetComponent<MovingPlatformTriggered>();
    }

    public void AddUser()
    {
        if (_platform == null)
            return;
        
        _users++;
        
        if (_users == 1)
        {
            _platform.SetPlayerInside(true);
            
            FirstUserEntered?.Invoke();
        }
    }

    public void RemoveUser()
    {
        if (_platform == null)
            return;
        
        _users = Mathf.Max(0, _users - 1);
        
        if (_users == 0)
        {
            _platform.SetPlayerInside(false);
            
            LastUserExited?.Invoke();
        }
    }
}