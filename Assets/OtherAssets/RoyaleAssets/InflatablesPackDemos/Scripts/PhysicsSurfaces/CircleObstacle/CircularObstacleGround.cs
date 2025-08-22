using UnityEngine;
//
// [RequireComponent(typeof(CircularObstacleMover))]
// public class CircularObstacleGround : GroundSurface
// {
//     private CircularObstacleMover _mover;
//
//     private void Start()
//     {
//         _mover = GetComponent<CircularObstacleMover>();
//     }
//     
//     public override void UpdateDisplacement(Vector3 point, Vector3 up, ref SurfaceDisplacement otherDisplacement)
//     {
//         _mover.GetDisplacement(out Vector3 distancePoint, out Quaternion direction);
//         
//         Vector3 local = point - transform.position;
//         Vector3 movedLocal = direction * local;
//         Vector3 distance = distancePoint + movedLocal - local;
//
//         otherDisplacement.DeltaPosition += distance;
//         
//         direction.ToAngleAxis(out float angleDeg, out Vector3 axis);
//         angleDeg *= Mathf.Sign(Vector3.Dot(axis, up));
//         otherDisplacement.DeltaRotation += angleDeg;
//     }
// }