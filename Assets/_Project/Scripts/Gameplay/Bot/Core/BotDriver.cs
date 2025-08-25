using UnityEngine;

public class BotDriver
{
    private readonly Rigidbody rigidbody;
    private readonly Animations animations;
    private readonly JumpLogic jump;
    private readonly BotLocomotion locomotion = new BotLocomotion();

    private readonly float jumpDistance;
    private readonly float speed;
    private readonly float accel;

    private BotMoveIntent _intent;
    private PushState _push = new PushState();
    private IInput _subscribedInput;
    
    public BotDriver(Rigidbody rigidbody, Animator animator, float jumpForce, float jumpDistance, float speed, float accel)
    {
        this.rigidbody = rigidbody;
        
        animations = new Animations(animator);
        jump = new JumpLogic(jumpForce);
        
        this.jumpDistance = jumpDistance;
        this.speed = speed;
        this.accel = accel;
    }

    public void SetInput(IInput input)
    {
        if (_subscribedInput != null)
            _subscribedInput.Jumped -= RequestJump;

        _subscribedInput = input;

        if (_subscribedInput != null)
            _subscribedInput.Jumped += RequestJump;
    }

    public bool FixedStepMove(LayerMask ground)
    {
        if (_push.HasPendingVelocity)
        {
            rigidbody.velocity = _push.ConsumePendingVelocity();
            
            return true;
        }

        _push.Tick(Time.fixedDeltaTime);

        if (_push.IsSuspended)
            return false;

        bool grounded = jump.IsGrounded(rigidbody.transform, jumpDistance, ground);

        if (_intent.MoveDirectionWorld.sqrMagnitude > 1e-4f)
            locomotion.HandleMovement(rigidbody, _intent.MoveDirectionWorld, grounded, speed, accel);
        
        animations.UpdateAnimator(rigidbody);
        
        return true;
    }
    
    public void UpdateInputAndFacing(IInput input, FacingRotator rotator, Transform objectTransform, float angularSpeed, LayerMask ground)
    {
        _intent.UpdateFromInput(input);

        if (_push.IsSuspended)
            return;

        bool grounded = jump.IsGrounded(objectTransform, jumpDistance, ground);
        
        if (_intent.ConsumeJumpRequest())
            jump.TryJump(rigidbody, _intent.MoveDirectionWorld, grounded);

        rotator.RotateTowardsMotion(objectTransform, rigidbody, _intent.MoveDirectionWorld, angularSpeed);
    }

    public void ApplyPush(Vector3 velocity, float duration)
    {
        _push.ApplyPendingVelocity(velocity);
        _push.Suspend(duration);
    }

    private void RequestJump() => _intent.RequestJump();
}