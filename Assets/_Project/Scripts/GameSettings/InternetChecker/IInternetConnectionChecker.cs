using System;

public interface IInternetConnectionChecker : IDisposable
{
    public event Action ConnectionLost;
    public event Action Connected;
    
    public bool IsConnected { get; } 
    
    public void ForceCheckNow();
}