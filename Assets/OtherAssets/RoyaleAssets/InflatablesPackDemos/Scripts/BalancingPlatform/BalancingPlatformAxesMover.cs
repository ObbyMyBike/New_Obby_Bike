using UnityEngine;

public class BalancingPlatformAxesMover : MonoBehaviour
{
    [SerializeField] private float _currentAngle;

    [SerializeField] private float _velocity = 1.0f;
    [SerializeField] private float _minAngle = -85f;
    [SerializeField] private float _maxAngle = 85f;

    private Quaternion _originRotation;

    private void Start()
    {
        _originRotation = transform.rotation;
    }

    public void Rotate(float deltaAngle, Vector3 rotationAxes)
    {
        deltaAngle = deltaAngle * _velocity * Time.deltaTime;
        _currentAngle = Mathf.Clamp(_currentAngle + deltaAngle, _minAngle, _maxAngle);

        var targetRotation = Quaternion.AngleAxis(_currentAngle, rotationAxes);
        transform.rotation = targetRotation * _originRotation;
    }
}