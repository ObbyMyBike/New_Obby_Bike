using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BotRespawn))]
public class BotDriver : MonoBehaviour
{
    [SerializeField] private Animator _animator;
 
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _acceleration = 100f;
    [SerializeField] private float _angularSpeed = 720f;
    
    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 30f;
    [SerializeField] private float _jumpDistance = 1.2f;

    private IInput _input;
    private BotLocomotion _botLocomotionHandler;
    private JumpLogic _jumpLogic;
    private Animations _animationHandler;
    
    private Rigidbody _rigidbody;
    private LayerMask _groundMask;
    
    private PushState _pushState;
    private BotMoveIntent _moveIntent;
    private FacingRotator _rotation;
    private RigidbodyGuard _rigidbodyGuard;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        _rigidbody.freezeRotation = true;

        _groundMask  = ~((1 << gameObject.layer) | (1 << 2));

        _botLocomotionHandler = new BotLocomotion();
        _jumpLogic = new JumpLogic(_jumpForce);
        _animationHandler = new Animations(_animator);
        _rotation = new FacingRotator();
        _rigidbodyGuard = new RigidbodyGuard();
    }

    private void OnDisable()
    {
        if (_input != null)
            _input.Jumped -= OnJumped;
    }
    
    private void Update()
    {
        _moveIntent.UpdateFromInput(_input);
        
        if (_pushState.IsSuspended)
            return;

        bool isGrounded = _jumpLogic.IsGrounded(transform, _jumpDistance, _groundMask);

        if (_moveIntent.ConsumeJumpRequest())
            _jumpLogic.TryJump(_rigidbody, _moveIntent.MoveDirectionWorld, isGrounded);
        
        _rotation.RotateTowardsMotion(transform, _rigidbody, _moveIntent.MoveDirectionWorld, _angularSpeed);
    }

    private void FixedUpdate()
    {
        _rigidbodyGuard.Sanitize(_rigidbody, transform);
        
        if (_pushState.HasPendingVelocity)
        {
            _rigidbody.velocity = _pushState.ConsumePendingVelocity();
            
            return;
        }
        
        _pushState.Tick(Time.fixedDeltaTime);
        
        if (_pushState.IsSuspended)
            return;

        bool isGrounded = _jumpLogic.IsGrounded(transform, _jumpDistance, _groundMask);

        if (_moveIntent.MoveDirectionWorld.sqrMagnitude > 0.0001f)
            _botLocomotionHandler.HandleMovement(_rigidbody, _moveIntent.MoveDirectionWorld, isGrounded, _speed, _acceleration);

        _animationHandler.UpdateAnimator(_rigidbody);
    }

    public void SuspendControl(float duration)
    {
        _pushState.Suspend(duration);
    }
    
    public void SetInput(IInput input)
    {
        if (_input != null)
            _input.Jumped -= OnJumped;

        _input = input;

        if (_input != null)
            _input.Jumped += OnJumped;
    }
    
    public void ApplyPush(Vector3 worldVelocity, float pushDuration)
    {
        _pushState.ApplyPendingVelocity(worldVelocity);
        _pushState.Suspend(pushDuration);
    }
    
    private void OnJumped()
    {
        _moveIntent.RequestJump();
    }
}