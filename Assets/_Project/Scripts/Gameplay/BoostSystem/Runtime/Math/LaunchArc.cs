using UnityEngine;

public struct LaunchArc
{
    public readonly float TimeOfFlightSeconds;
    public readonly bool IsValid;
    
    public Vector3 DirectionXZ;
    public Vector3 InitialVelocity;
    
    public LaunchArc(Vector3 directionXZ, Vector3 velocity, float time)
    {
        IsValid = directionXZ.sqrMagnitude > 1e-6f && velocity.sqrMagnitude > 1e-6f && time > 0f;
        DirectionXZ = directionXZ;
        InitialVelocity = velocity;
        TimeOfFlightSeconds = time;
    }
}