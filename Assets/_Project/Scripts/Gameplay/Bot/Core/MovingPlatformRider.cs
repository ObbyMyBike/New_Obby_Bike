using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatformRider : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;

    private MovingPlatformMover _platform;
    
    private bool _grounded;

    private void Reset()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        if (_grounded && _platform != null)
        {
            _platform.GetDisplacement(out Vector3 deltaPosition, out Quaternion direction);
            
            _rigidbody.MovePosition(_rigidbody.position + deltaPosition);
            
            if (direction != Quaternion.identity)
                _rigidbody.MoveRotation(direction * _rigidbody.rotation);
        }
    }
    
    private void OnCollisionStay(Collision collision)
    {
        if (!IsGroundNormal(collision))
            return;

        MovingPlatformMover mover = collision.collider.GetComponentInParent<MovingPlatformMover>();
        
        _platform = mover;
        _grounded = true;
    }

    private void OnCollisionExit(Collision c)
    {
        MovingPlatformMover mover = c.collider.GetComponentInParent<MovingPlatformMover>();
        
        if (_platform != null && mover == _platform)
        {
            _platform = null;
            _grounded = false;
        }
    }
    
    private bool IsGroundNormal(Collision collision)
    {
        foreach (var point in collision.contacts)
            if (Vector3.Dot(point.normal, Vector3.up) > 0.55f)
                return true;
        
        return false;
    }
}