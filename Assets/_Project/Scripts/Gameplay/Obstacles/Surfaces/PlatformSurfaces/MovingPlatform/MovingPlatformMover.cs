using UnityEngine;

public class MovingPlatformMover : MonoBehaviour
{
    private const string START_POINT = "start";
    private const string END_POINT = "end";
    
    [Header("Path")]
    [SerializeField] private Transform _start = null;
    [SerializeField] private Transform _end = null;
    
    [Header("Motion")]
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _angularSpeed = 0f;
    [SerializeField] private float _arriveEps = 1e-4f;

    private Rigidbody _rigidbody;
    private Vector3 _lastDeltaPosition;
    private Quaternion _lastDeltaRotation;

    private bool _isMovingForward = true;
    private bool _stepComputed;

    private Vector3 CurrentDestination => _isMovingForward ? _end.position : _start.position;
    private Vector3 UpDirection => transform.parent != null ? transform.parent.up : transform.up;
    private Vector3 PositionNow => _rigidbody != null ? _rigidbody.position : transform.position;
    private Quaternion RotationNow => _rigidbody != null ? _rigidbody.rotation  : transform.rotation;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        
        if (_start == null)
            _start = transform.Find(START_POINT);
        
        if (_end == null)
            _end = transform.Find(END_POINT);

        if (_start != null)
            _start.SetParent(transform.parent, true);
        
        if (_end != null)
            _end.SetParent(transform.parent, true);
    }
    
    private void FixedUpdate()
    {
        if (_start == null || _end == null)
        {
            _lastDeltaPosition = Vector3.zero;
            _lastDeltaRotation = Quaternion.identity;
            _stepComputed = false;
            
            return;
        }

        if (!_stepComputed)
            ComputeStep(Time.fixedDeltaTime);
        
        _rigidbody.MovePosition(PositionNow + _lastDeltaPosition);
        _rigidbody.MoveRotation(_lastDeltaRotation * RotationNow);
        
        if ((CurrentDestination - PositionNow).sqrMagnitude < _arriveEps)
            _isMovingForward = !_isMovingForward;

        _stepComputed = false;
    }

    private void ComputeStep(float dt)
    {
        Vector3 toDest = CurrentDestination - PositionNow;
        Vector3 lin = Vector3.MoveTowards(Vector3.zero, toDest, _speed * dt);

        _lastDeltaPosition = lin;
        _lastDeltaRotation = (_angularSpeed != 0f) ? Quaternion.AngleAxis(_angularSpeed * dt, UpDirection) : Quaternion.identity;

        _stepComputed = true;
    }
    
    public void GetDisplacement(out Vector3 deltaPosition, out Quaternion deltaRotation)
    {
        if (!_stepComputed)
            ComputeStep(Time.fixedDeltaTime);

        deltaPosition = _lastDeltaPosition;
        deltaRotation = _lastDeltaRotation;
    }
}