using UnityEngine;

public struct GroundHitInfo
{
    public readonly Vector3 Point;
    public readonly Vector3 Normal;
    public readonly Collider Collider;
    public readonly bool IsOnFloor;

    public GroundHitInfo(Vector3 point, Vector3 normal, Collider collider, bool isOnFloor)
    {
        Point = point;
        Normal = normal;
        Collider = collider;
        IsOnFloor = isOnFloor;
    }
}