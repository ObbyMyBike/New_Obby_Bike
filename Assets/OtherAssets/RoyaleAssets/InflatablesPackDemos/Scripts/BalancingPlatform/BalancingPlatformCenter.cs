using UnityEngine;

// [RequireComponent(typeof(BalancingPlatformCenterMover))]
// public class BalancingPlatformCenter : GroundSurface
// {
//     [SerializeField] private Transform _balanceCenter = null;
//     [SerializeField] private float _centerThreshold = 0.5f;
//
//     private BalancingPlatformCenterMover _mover;
//
//     private void Start()
//     {
//         _mover = GetComponent<BalancingPlatformCenterMover>();
//         
//         _balanceCenter.parent = transform.parent;
//     }
//
//     public override void UpdateDisplacement(Vector3 hitPoint, Vector3 _, ref SurfaceDisplacement otherDisplacement)
//     {
//         ApplyDisplacement(hitPoint);
//     }
//
//     private void ApplyDisplacement(Vector3 hitPoint)
//     {
//         Vector3 projectedHitPoint = Vector3.ProjectOnPlane(hitPoint, _balanceCenter.up);
//         Vector3 hitVector = projectedHitPoint - _balanceCenter.position;
//         var hitLength = hitVector.magnitude;
//
//         if (hitLength < _centerThreshold)
//             return;
//
//         Vector3 upRotation = Vector3.Cross(_balanceCenter.up, hitVector);
//         _mover.Rotate(hitLength, upRotation);
//     }
// }