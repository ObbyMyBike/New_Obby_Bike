using UnityEngine;

public class CircularObstacleMover : MonoBehaviour
{
    [SerializeField] private Transform _center;
    [SerializeField] private Vector3 _axis = Vector3.up;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _angularSpeed = 30f;

    private Vector3 _centerPosition;
    private Vector3 _deltaPosition;
    private Quaternion _deltaRotation;
    private float _currentAngle;
    private bool _displacementFresh;

    private void Start()
    {
        if(_center == null)
            _center = transform.parent;
        
        _centerPosition = _center.position;
        _center.SetParent(null, true);
        
        Vector3 offset = transform.position - _center.position;
        _currentAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
    }

    private void Update()
    {
        if (!_displacementFresh)
            UpdateDisplacement(Time.deltaTime);
        
        transform.SetPositionAndRotation(transform.position + _deltaPosition, _deltaRotation * transform.rotation);
        _displacementFresh = false;
    }

    public void GetDisplacement(out Vector3 deltaPosition, out Quaternion deltaRotation)
    {
        if (!_displacementFresh)
            UpdateDisplacement(Time.deltaTime);
        
        deltaPosition = _deltaPosition;
        deltaRotation = _deltaRotation;
    }
    
    private void UpdateDisplacement(float deltaTime)
    {
        float nextAngle = _currentAngle + _angularSpeed * deltaTime;
        float angleFirst = _currentAngle * Mathf.Deg2Rad;
        float angleSecond = nextAngle * Mathf.Deg2Rad;
        
        Vector3 firstPosition = _centerPosition  + new Vector3(Mathf.Cos(angleFirst), 0, Mathf.Sin(angleFirst)) * _radius;
        Vector3 secondPosition = _centerPosition  + new Vector3(Mathf.Cos(angleSecond), 0, Mathf.Sin(angleSecond)) * _radius;
        
        _deltaPosition = secondPosition - firstPosition;
        _deltaRotation = Quaternion.AngleAxis(_angularSpeed * deltaTime, _axis);
        _currentAngle = nextAngle;
        _displacementFresh = true;
    }
    
    private void OnDrawGizmosSelected()
    {
        Vector3 centerPos = Application.isPlaying ? _centerPosition : (_center != null ? _center.position : transform.parent.position);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(centerPos - _axis.normalized * _radius, centerPos + _axis.normalized * _radius);
        
        Gizmos.color = Color.cyan;
        int seg = 64;
        Vector3 axisNorm = _axis.normalized;
        Vector3 v1 = Vector3.Cross(axisNorm, Vector3.up).sqrMagnitude > 0.001f ? Vector3.Cross(axisNorm, Vector3.up).normalized
            : Vector3.Cross(axisNorm, Vector3.right).normalized;
        Vector3 v2 = Vector3.Cross(axisNorm, v1).normalized;

        Vector3 prev = centerPos + v1 * _radius;
        for (int i = 1; i <= seg; i++)
        {
            float ang = (i / (float)seg) * Mathf.PI * 2f;
            Vector3 next = centerPos + (v1 * Mathf.Cos(ang) + v2 * Mathf.Sin(ang)) * _radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}