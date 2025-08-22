using UnityEngine;

// [RequireComponent(typeof(MovingPlatformMover))]
// public class MovingPlatformObstacle : ObstacleSurface
// {
//     private MovingPlatformMover _mover;
//
//     private void Start()
//     {
//         _mover = GetComponent<MovingPlatformMover>();
//     }
//
//     public override void UpdateDisplacement(ref SurfaceHitInfo surface, ref SurfaceDisplacement thisDisplacement, ref SurfaceDisplacement otherDisplacement)
//     {
//         _mover.GetDisplacement(out Vector3 platformDeltaPosition, out Quaternion platformDeltaRotation);
//
//         if (Vector3.Dot(platformDeltaPosition.normalized, -surface.Normal) < 0.2)
//             return;
//
//         Vector3 localPosition = surface.Point - transform.position;
//         Vector3 deltaPosition = platformDeltaPosition + platformDeltaRotation * localPosition - localPosition;
//
//         otherDisplacement.DeltaPosition = deltaPosition;
//     }
// }