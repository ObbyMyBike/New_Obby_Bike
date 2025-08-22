using UnityEngine;

// public class RigidbodyObstacle : ObstacleSurface
// {
//     [SerializeField] private float _displacementFactor = 10;
//
//     private Rigidbody _rigidbody;
//     private Vector3 _lastPosition;
//     private Vector3 _deltaPosition;
//
//     private void Awake()
//     {
//         _rigidbody = GetComponent<Rigidbody>();
//         
//         _lastPosition = transform.position;
//     }
//
//     private void Update()
//     {
//         _deltaPosition = transform.position - _lastPosition;
//         _lastPosition = transform.position;
//     }
//
//     public override void UpdateDisplacement(ref SurfaceHitInfo hitInfo, ref SurfaceDisplacement thisDisplacement, ref SurfaceDisplacement otherDisplacement)
//     {
//         Vector3 deltaForce = (_displacementFactor / _rigidbody.mass) * Time.deltaTime * Vector3.ProjectOnPlane(thisDisplacement.DeltaVelocity, Vector3.up);
//         
//         _rigidbody.AddForce(deltaForce, ForceMode.Force);
//
//         otherDisplacement.DeltaPosition = _rigidbody.mass * _deltaPosition;
//     }
// }