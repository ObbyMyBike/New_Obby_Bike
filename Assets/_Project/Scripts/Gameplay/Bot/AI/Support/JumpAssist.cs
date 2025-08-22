using System;
using UnityEngine;

public class JumpAssist
{
    private readonly float assistDuration;
    private readonly float postJumpMinHold;
    private readonly float jumpCooldown;

    private Vector3 _assistDir;
    
    private float _assistTimer;
    private float _holdUntilTime;
    private float _lastJumpTime = -999f;
    
    private bool _holdActive;

    public JumpAssist(float assistDuration, float postJumpMinHold, float jumpCooldown)
    {
        this.assistDuration = assistDuration;
        this.postJumpMinHold = postJumpMinHold;
        this.jumpCooldown = jumpCooldown;
    }

    public bool UpdateHold(WaypointGate gate, float distance, float stopRadius, Vector3 target, ref Vector3 steeringDirection, float baseSpeed, out Vector2 inputDirection)
    {
        inputDirection = Vector2.zero;

        if (!_holdActive)
            return false;

        if (gate == null)
        {
            _holdActive = false;
            
            return false;
        }

        bool isStop = distance <= Mathf.Max(stopRadius, 0.001f);
        bool isTimeOk = Time.time >= _holdUntilTime;

        Vector3 desiredDirection = target.sqrMagnitude > 1e-4f ? target.normalized : steeringDirection;
        Vector3 move = desiredDirection * baseSpeed;

        float approachR = Mathf.Max(stopRadius + 0.5f, stopRadius * 1.8f);
        
        if (!isStop && distance <= approachR)
        {
            float slow = Mathf.InverseLerp(stopRadius, approachR, distance);
            
            move *= Mathf.Clamp01(slow);
        }

        if (!isStop || !isTimeOk)
        {
            if (move.sqrMagnitude > 1e-4f)
                steeringDirection = Vector3.Slerp(steeringDirection, move.normalized, 6f * Time.deltaTime);

            inputDirection = new Vector2(steeringDirection.x, steeringDirection.z);
            
            return true;
        }

        _holdActive = false;
        
        return false;
    }

    public void TriggerJump(Action Jumped, Vector3 forwardDirection, WaypointGate nextGate)
    {
        if (Time.time - _lastJumpTime >= jumpCooldown)
        {
            Jumped?.Invoke();
            
            _lastJumpTime = Time.time;
        }

        _assistDir = forwardDirection.normalized;
        _assistTimer = assistDuration;

        if (nextGate != null)
        {
            _holdActive  = true;
            _holdUntilTime = Time.time + postJumpMinHold;
        }
        else
        {
            _holdActive = false;
        }
    }

    public void ApplyAssist(ref Vector3 moveVector)
    {
        if (_assistTimer <= 0f)
            return;

        Vector3 assist = new Vector3(_assistDir.x, 0f, _assistDir.z).normalized;
        moveVector = Vector3.Lerp(moveVector, assist, 0.7f);
        
        _assistTimer -= Time.deltaTime;
    }

    public void Reset()
    {
        _assistTimer = 0f;
        _holdActive = false;
        _holdUntilTime = -999f;
    }
}