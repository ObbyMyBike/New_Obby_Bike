using UnityEngine;

public class SimpleOrbitMovement : MonoBehaviour
{
    [SerializeField] private Transform _center;
    [SerializeField] private Vector3 _axis = Vector3.up;
    [SerializeField] private float _speed = 30f;
    [SerializeField] private float _deceleration = 45f;
    [SerializeField] private bool _useTrigger = false;
    
    private float _currentSpeed; 
    private bool _playerInside;
    
    public Vector3 AxisWorld => _axis.sqrMagnitude > 1e-6f ? _axis.normalized : Vector3.up;
    public bool IsPlayerInside => _playerInside;
    public bool UseTrigger
    {
        get => _useTrigger;
        set => _useTrigger = value;
    }
    
    private void Update()
    {
        if (_useTrigger)
        {
            if (_playerInside)
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _speed, _deceleration * Time.deltaTime);
            else
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, _deceleration * Time.deltaTime);
        }
        else
        {
            _currentSpeed = _speed;
        }

        if (Mathf.Abs(_currentSpeed) > 0.01f)
            transform.RotateAround(_center.position, _axis.normalized, _currentSpeed * Time.deltaTime);
    }

    public void SetPlayerInside(bool inside) => _playerInside = inside;
    
    private void Reset()
    {
        if (_center == null && transform.parent != null)
            _center = transform.parent;
    }
}