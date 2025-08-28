using UnityEngine;

public abstract class WaypointGate : MonoBehaviour
{
    public abstract float StopRadius { get; }
    public abstract bool RequireJumpOnPass { get; }
    
    public abstract bool IsSatisfied(Waypoint current, Waypoint projectedNext);
    
    public virtual void SetWaiting(bool waiting) { }
}