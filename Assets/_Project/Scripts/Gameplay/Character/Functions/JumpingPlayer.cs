using System;
using UnityEngine;

public class JumpingPlayer : IDisposable
{
    private const float JUMP_BUFFER_DURATION = 0.12f;
    private const float COYOTE_TIME_DURATION = 0.08f;
    
    private readonly IInput input;
    private readonly JumpLogic jumpSystem;
    private readonly PlayerAudio audio;
    private readonly Animations animation;
    private readonly Rigidbody rigidbody;
    private readonly BikeMovement bike;
    private readonly PlayerConfig config;
    private readonly LayerMask mask;
    
    private float _jumpBufferTimer;
    private float _coyoteTimer;
    private float _landingWindow;
    private bool _forcedAirborne;
    private bool _wasGroundedLastFrame;

    public JumpingPlayer(IInput input, JumpLogic jumpSystem, PlayerAudio audio, Animations animation, Rigidbody rigidbody, BikeMovement bike,
        PlayerConfig config, LayerMask layerMask)
    {
        this.input = input;
        this.jumpSystem = jumpSystem;
        this.audio = audio;
        this.animation = animation;
        this.rigidbody = rigidbody;
        this.bike = bike;
        this.config = config;
        this.mask = layerMask;

        this.input.Jumped += OnJumpPressed;
    }
    
    public bool LandingEligible => bike.IsGrounded || _coyoteTimer > 0f;
    
    void IDisposable.Dispose() => input.Jumped -= OnJumpPressed;

    public void FixedTick()
    {
        bool wasGrounded = bike.IsGrounded;
        
        bike.UpdatePhysics();
        animation.UpdateAnimator(rigidbody);

        bool nowIsGrounded = bike.IsGrounded;
        bool justIsLanded  = (!wasGrounded || _forcedAirborne) && nowIsGrounded && rigidbody.velocity.y <= 0f;

        if (nowIsGrounded)
            _coyoteTimer = COYOTE_TIME_DURATION;
        else
            _coyoteTimer = Mathf.Max(0f, _coyoteTimer - Time.fixedDeltaTime);

        if (justIsLanded && _jumpBufferTimer > 0f)
        {
            JumpImmediate();
            
            nowIsGrounded = false;
        }
        else
        {
            if (_jumpBufferTimer > 0f && (nowIsGrounded || _coyoteTimer > 0f))
            {
                JumpImmediate();
                
                nowIsGrounded = false;
            }
        }

        _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - Time.fixedDeltaTime);
        
        if (nowIsGrounded && !_wasGroundedLastFrame)
            animation.SetAirborne(false);

        _wasGroundedLastFrame = nowIsGrounded;
        _forcedAirborne = false;
    }

    public void MarkAirborne() => _forcedAirborne = true;
    
    private void OnJumpPressed()
    {
        _jumpBufferTimer = JUMP_BUFFER_DURATION;
        
        if (jumpSystem.IsGrounded(bike.TransformRef, config.JumpDistance, mask))
            JumpImmediate();
    }
    
    private void JumpImmediate()
    {
        animation.SetAirborne(true);
        
        Vector3 jumpDirection = bike.TransformRef.forward;
        
        jumpSystem.TryJump(rigidbody, jumpDirection, true);
        audio.PlayJump(config.JumpClip);
        
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;
        _forcedAirborne = true;
    }
}