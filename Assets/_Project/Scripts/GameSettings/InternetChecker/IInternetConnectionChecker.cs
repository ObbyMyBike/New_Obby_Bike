using System;

public interface IInternetConnectionChecker : IDisposable
{
    public bool IsConnected { get; }
    
    public event Action ConnectionLost;
    public event Action Connected;
    
    public void ForceCheckNow();
}