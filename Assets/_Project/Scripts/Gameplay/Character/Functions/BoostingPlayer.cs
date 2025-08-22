using System;
using System.Collections;
using UnityEngine;

public class BoostingPlayer : IDisposable
{
    public event Action<float> SpeedBoostStarted;
    public event Action SpeedBoostEnded;
    public event Action<float> JumpBoostStarted;
    public event Action JumpBoostEnded;

    private readonly MonoBehaviour runner;
    private readonly TimedBoostApplier _timedBoost;
    private readonly Rigidbody rigidbody;
    private readonly Transform forwardRef;
    private readonly float baseSpeed;

    private Coroutine _instantSpeedBoostRoutine;

    public BoostingPlayer(TimedBoostApplier timedBoost, Rigidbody rigidbody, Transform forwardRef, float baseSpeed, MonoBehaviour runner)
    {
        this._timedBoost = timedBoost;
        this.rigidbody = rigidbody;
        this.forwardRef = forwardRef;
        this.baseSpeed = baseSpeed;
        this.runner = runner;

        this._timedBoost.SpeedBoostStarted += OnSpeedTimedBoostStarted;
        this._timedBoost.SpeedBoostEnded += OnSpeedTimedBoostEnded;
        this._timedBoost.JumpBoostStarted += OnJumpTimedBoostStarted;
        this._timedBoost.JumpBoostEnded += OnJumpTimedBoostEnded;
    }

    void IDisposable.Dispose()
    {
        _timedBoost.SpeedBoostStarted -= OnSpeedTimedBoostStarted;
        _timedBoost.SpeedBoostEnded -= OnSpeedTimedBoostEnded;
        _timedBoost.JumpBoostStarted -= OnJumpTimedBoostStarted;
        _timedBoost.JumpBoostEnded -= OnJumpTimedBoostEnded;
        
        _timedBoost.Reset();
    }

    public void ApplyTempJump(float newForce, float duration) => _timedBoost.ApplyJumpBoost(newForce, duration);
    public void ApplyTempSpeed(float newSpeed, float duration) => _timedBoost.ApplySpeedBoost(newSpeed, duration);

    public void ApplyInstantSpeedBoost(float speedMultiplier, float decelerationTime)
    {
        if (_instantSpeedBoostRoutine != null)
            runner.StopCoroutine(_instantSpeedBoostRoutine);

        _instantSpeedBoostRoutine = runner.StartCoroutine(InstantSpeedBoostRoutine(speedMultiplier, decelerationTime));
    }

    private void OnSpeedTimedBoostStarted(float value) => SpeedBoostStarted?.Invoke(value);
    
    private void OnSpeedTimedBoostEnded() => SpeedBoostEnded?.Invoke();
    
    private void OnJumpTimedBoostStarted(float value) => JumpBoostStarted?.Invoke(value);
    
    private void OnJumpTimedBoostEnded() => JumpBoostEnded?.Invoke();

    private IEnumerator InstantSpeedBoostRoutine(float speedMultiplier, float decelerationTime)
    {
        Vector3 flatVelocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
        Vector3 forward = forwardRef.forward; forward.y = 0f; forward.Normalize();

        float currentForwardSpeed = Mathf.Max(0f, Vector3.Dot(flatVelocity, forward));
        float targetSpeed = baseSpeed * speedMultiplier;
        float delta = targetSpeed - currentForwardSpeed;

        rigidbody.velocity += forward * delta;

        float time = decelerationTime;
        
        while (time > 0f)
        {
            float a = 1f - (time / decelerationTime);
            float desired = Mathf.Lerp(targetSpeed, baseSpeed, a);

            Vector3 currentFlat = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
            float currentForward = Vector3.Dot(currentFlat, forward);
            float adjust = desired - currentForward;
            
            if (Mathf.Abs(adjust) > 0.001f)
                rigidbody.velocity += forward * adjust;

            time -= Time.deltaTime;
            yield return null;
        }

        _instantSpeedBoostRoutine = null;
    }
}