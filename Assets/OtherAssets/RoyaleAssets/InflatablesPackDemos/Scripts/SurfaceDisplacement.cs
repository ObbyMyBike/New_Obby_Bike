using UnityEngine;

public struct SurfaceDisplacement
{
    public Vector3 DeltaPosition;
    public Vector3 DeltaVelocity;
    public Vector3 DeltaAngularVelocity;

    public float DeltaRotation;
        
    public void MergeWith(SurfaceDisplacement other)
    {
        DeltaPosition += other.DeltaPosition;
        DeltaRotation += other.DeltaRotation;
        DeltaVelocity += other.DeltaVelocity;
        DeltaAngularVelocity += other.DeltaAngularVelocity;
    }

    public void Reset()
    {
        DeltaPosition = Vector3.zero;
        DeltaRotation = 0;

        DeltaVelocity = Vector3.zero;
        DeltaAngularVelocity = Vector3.zero;
    }
}