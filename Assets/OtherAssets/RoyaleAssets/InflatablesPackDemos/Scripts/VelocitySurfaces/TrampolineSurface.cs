using UnityEngine;

// public class TrampolineSurface : GroundSurface
// {
//     [SerializeField] private Transform _upTransform;
//     [SerializeField] private float _power = 1;
//
//     private void Start()
//     {
//         if (!_upTransform)
//             _upTransform = transform;
//     }
//
//     public override void UpdateDisplacement(Vector3 point, Vector3 up, ref SurfaceDisplacement otherDisplacement)
//     {
//         otherDisplacement.DeltaVelocity += _power * _upTransform.up;
//     }
// }