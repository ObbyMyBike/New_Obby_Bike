using UnityEngine;

// [RequireComponent(typeof(BalancingPlatformAxesMover))]
// public class BalancingPlatformAxes : GroundSurface
// {
//     [SerializeField] Transform _balanceAxis = null;
//
//     private BalancingPlatformAxesMover _mover;
//
//     private void Start()
//     {
//         _mover = GetComponent<BalancingPlatformAxesMover>();
//         
//         _balanceAxis.parent = transform.parent;
//     }
//
//     public override void UpdateDisplacement(Vector3 hitPoint, Vector3 _, ref SurfaceDisplacement otherDisplacement)
//     {
//         ApplyDisplacement(hitPoint);
//     }
//
//     private void ApplyDisplacement(Vector3 hitPoint)
//     {
//         Vector3 axisForward = _balanceAxis.forward;
//         Vector3 axixsRight = _balanceAxis.right;
//         Vector3 hitVector = hitPoint - _balanceAxis.position;
//
//         Vector3 projectedVector = Vector3.Project(hitVector, axixsRight);
//         var projectedLength = projectedVector.magnitude;
//
//         var angleDelta = -Mathf.Sign(Vector3.Dot(axixsRight, hitVector)) * projectedLength;
//
//         _mover.Rotate(angleDelta, axisForward);
//     }
// }