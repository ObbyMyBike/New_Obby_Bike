using UnityEngine;

public class BalancingPlatformCenterMover : MonoBehaviour
{
    [SerializeField] private Vector3 _currentUp;
    [SerializeField] private float _currentAngle;
    [SerializeField] private float _velocity = 1.0f;
    [SerializeField] private float _velocityUp = 1f;
    [SerializeField] private float _minAngle = -40f;
    [SerializeField] private float _maxAngle = 40f;

    public void Rotate(float deltaAngle, Vector3 up)
    {
        deltaAngle = deltaAngle * _velocity * Time.deltaTime;
        _currentAngle = Mathf.Clamp(_currentAngle + deltaAngle, _minAngle, _maxAngle);
        _currentUp = Vector3.Slerp(_currentUp, up, Time.deltaTime * _velocityUp);

        Quaternion deltaRotation = Quaternion.AngleAxis(_currentAngle, _currentUp);
        transform.rotation = deltaRotation;
    }
}