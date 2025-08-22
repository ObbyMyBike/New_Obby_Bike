using UnityEngine;

// [RequireComponent(typeof(MovingPlatformMover))]
// public class MovingPlatformGround : GroundSurface
// {
//     private MovingPlatformMover _mover;
//
//     private void Start()
//     {
//         _mover = GetComponent<MovingPlatformMover>();
//     }
//
//     public override void UpdateDisplacement(Vector3 point, Vector3 up, ref SurfaceDisplacement otherDisplacement)
//     {
//         _mover.GetDisplacement(out Vector3 platformDeltaPosition, out Quaternion platformDeltaRotation);
//         Vector3 localPosition = point - transform.position;
//         Vector3 deltaPosition = platformDeltaPosition + platformDeltaRotation * localPosition - localPosition;
//
//         platformDeltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
//         angle *= Mathf.Sign(Vector3.Dot(axis, up));
//
//         otherDisplacement.DeltaPosition += deltaPosition;
//         otherDisplacement.DeltaRotation += angle;
//     }
// }