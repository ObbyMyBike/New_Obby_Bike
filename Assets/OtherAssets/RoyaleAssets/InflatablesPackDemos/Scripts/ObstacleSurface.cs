using UnityEngine;

public abstract class ObstacleSurface : MonoBehaviour
{
    public abstract void UpdateDisplacement(ref SurfaceHitInfo obstacleHit, ref SurfaceDisplacement thisDisplacement, ref SurfaceDisplacement otherDisplacement);
}