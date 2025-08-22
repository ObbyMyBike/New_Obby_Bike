using UnityEngine;

public struct PushState
{
    private Vector3 _pendingVelocity;
    private float _timeLeft;
    private bool _isSuspended;

    public bool IsSuspended => _isSuspended;
    public bool HasPendingVelocity => _pendingVelocity.sqrMagnitude > 1e-6f;

    public void ApplyPendingVelocity(Vector3 velocity) => _pendingVelocity = velocity;

    public Vector3 ConsumePendingVelocity()
    {
        Vector3 velocity = _pendingVelocity;
        _pendingVelocity = Vector3.zero;
        
        return velocity;
    }

    public void Suspend(float durationSeconds)
    {
        _isSuspended = durationSeconds > 0f;
        _timeLeft = Mathf.Max(0f, durationSeconds);
    }

    public void Tick(float deltaTime)
    {
        if (!_isSuspended)
            return;

        _timeLeft -= deltaTime;
        
        if (_timeLeft <= 0f)
        {
            _isSuspended = false;
            _timeLeft = 0f;
        }
    }
}