using System;
using System.Collections;
using UnityEngine;

public class TimedBoostApplier
{
    public event Action<float> SpeedBoostStarted;
    public event Action SpeedBoostEnded;
    public event Action<float> JumpBoostStarted;
    public event Action JumpBoostEnded;
    
    private readonly PlayerConfig config;
    private readonly JumpLogic jumpLogic;
    private readonly MonoBehaviour coroutineHost;

    private Coroutine _jumpBoostRoutine;
    private Coroutine _speedBoostRoutine;

    private float _originalSpeed;
    private float _originalJumpForce;

    public TimedBoostApplier(PlayerConfig config, JumpLogic jumpLogic, MonoBehaviour coroutineHost)
    {
        this.config = config;
        this.jumpLogic = jumpLogic;
        this.coroutineHost = coroutineHost;
        
        _originalJumpForce = this.config.JumpForce;
        _originalSpeed = this.config.MaxSpeed;
    }

    public void ApplyJumpBoost(float newJumpForce, float duration)
    {
        if (_jumpBoostRoutine != null)
            coroutineHost.StopCoroutine(_jumpBoostRoutine);

        _originalJumpForce = config.JumpForce;
        config.JumpForce = newJumpForce;
        
        jumpLogic.SetJumpForce(newJumpForce);

        JumpBoostStarted?.Invoke(duration);
        
        _jumpBoostRoutine = coroutineHost.StartCoroutine(JumpBoostCoroutine(duration));
    }

    public void ApplySpeedBoost(float newSpeed, float duration)
    {
        if (_speedBoostRoutine != null)
            coroutineHost.StopCoroutine(_speedBoostRoutine);

        _originalSpeed = config.MaxSpeed;
        config.MaxSpeed = newSpeed;

        SpeedBoostStarted?.Invoke(duration);
        
        _speedBoostRoutine = coroutineHost.StartCoroutine(SpeedBoostCoroutine(duration));
    }

    public void Reset()
    {
        if (_jumpBoostRoutine != null)
        {
            coroutineHost.StopCoroutine(_jumpBoostRoutine);
            
            _jumpBoostRoutine = null;
        }

        config.JumpForce = _originalJumpForce;
        jumpLogic.SetJumpForce(_originalJumpForce);
        
        JumpBoostEnded?.Invoke();

        if (_speedBoostRoutine != null)
        {
            coroutineHost.StopCoroutine(_speedBoostRoutine);
            
            _speedBoostRoutine = null;
        }

        config.MaxSpeed = _originalSpeed;
        
        SpeedBoostEnded?.Invoke();
    }

    private IEnumerator JumpBoostCoroutine(float duration)
    {
        float timer = duration;
        
        while (timer > 0f)
        {
            yield return null;
            
            timer -= Time.deltaTime;
        }
        
        config.JumpForce = _originalJumpForce;
        jumpLogic.SetJumpForce(_originalJumpForce);
        _jumpBoostRoutine = null;
        
        JumpBoostEnded?.Invoke();
    }

    private IEnumerator SpeedBoostCoroutine(float duration)
    {
        float timer = duration;
        
        while (timer > 0f)
        {
            yield return null;
            
            timer -= Time.deltaTime;
        }
        
        config.MaxSpeed = _originalSpeed;
        _speedBoostRoutine = null;
        
        SpeedBoostEnded?.Invoke();
    }
}