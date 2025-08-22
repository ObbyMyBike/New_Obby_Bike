using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HammerSwing : MonoBehaviour
{
    [Header("Swing")]
    [SerializeField] private Vector3 _localAxis = Vector3.forward;
    [SerializeField] private float _leftAngle = -80f;
    [SerializeField] private float _rightAngle = 80f;
    [SerializeField] private float _peroid = 2f;
    [SerializeField, Range(0f, 1f)] private float _startPhase = 0f;

    [Header("Hit")]
    [SerializeField] private Transform _head;
    [SerializeField] private float _hitForce = 10f;
    [SerializeField] private float _minImpactSpeed = 0.1f;
    
    private Rigidbody _rigidbody;
    private Collider _headCollider;
    private Quaternion _baseLocalRotation;
    private float _time;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _baseLocalRotation = transform.localRotation;
        
        if(_head == null)
            _head = transform;
        
        _time = _startPhase * _peroid;
    }

    private void FixedUpdate()
    {
        _time += Time.fixedDeltaTime;

        float norm = 0.5f * (Mathf.Sin((_time / _peroid) * Mathf.PI * 2f) + 1f);
        float angle = Mathf.Lerp(_leftAngle, _rightAngle, norm);

        Quaternion targetLocalRotation = _baseLocalRotation * Quaternion.AngleAxis(angle, _localAxis.normalized);
        Quaternion worldRotation = transform.parent != null ? transform.parent.rotation * targetLocalRotation : targetLocalRotation;

        _rigidbody.MoveRotation(worldRotation);
    }

    private void OnCollisionEnter(Collision collision) => TryPush(collision);

    private void TryPush(Collision collision)
    {
        if (_headCollider != null && collision.collider != _headCollider)
            return;
        
        Rigidbody otherRigidbody = collision.collider.attachedRigidbody ?? collision.collider.GetComponentInParent<Rigidbody>();
        
        if (otherRigidbody == null)
            return;
        
        Vector3 relVel = collision.relativeVelocity;
        
        if (relVel.magnitude < _minImpactSpeed)
            return;
        
        Vector3 normal = collision.GetContact(0).normal;
        
        if (Vector3.Dot(normal, Vector3.up) < 0f)
            normal = -normal;
        
        Vector3 impulse = normal * _hitForce;
        
        otherRigidbody.AddForce(impulse, ForceMode.VelocityChange);
    }
}