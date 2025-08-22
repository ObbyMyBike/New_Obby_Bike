using UnityEngine;

public struct SurfaceHitInfo
{
    public Vector3 Point;
    public Vector3 Normal;
    public ObstacleSurface Other;

    public bool HasValue => Other != null;

    public void Init(Vector3 point, Vector3 normal, ObstacleSurface obstacle)
    {
        Point = point;
        Normal = normal;
        Other = obstacle;
    }

    public void Reset()
    {
        Other = null;
    }
}