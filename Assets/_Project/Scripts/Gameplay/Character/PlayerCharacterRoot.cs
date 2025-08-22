using System;
using System.Collections;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCharacterRoot : MonoBehaviour
{
    public event Action<PushSide, Transform> BotPushed;
    public event Action<float> SpeedBoostStarted;
    public event Action SpeedBoostEnded;
    public event Action<float> JumpBoostStarted;
    public event Action JumpBoostEnded;

    [SerializeField] private PlayerConfig _playerConfig;
    [SerializeField] private Animator _animator;

    private Rigidbody _rigidbody;
    private LayerMask _layerMask;
    private Transform _cameraTransform;
    
    private PlayerAudio _audio;
    private Animations _animations;
    private JumpLogic _jumpLogic;
    private BikeMovement _bike;
    private TimedBoostApplier _timedBoostApplier;
    private JumpingPlayer _jumping;
    private BoostingPlayer _boosting;
    private PushState _pushState;
    
    private IUpdatable _riding;
    private IUpdatable _pushing;
    private IInput _input;
    private IInput _originalInputForLock;
    
    private Coroutine _controlLockRoutine;
    
    [Inject]
    private void Construct(IInput input, Camera playerCamera)
    {
        _input = input;
        _cameraTransform = playerCamera != null ? playerCamera.transform : null;
    }
    
    public bool LandingEligible => _jumping != null && _jumping.LandingEligible;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        _audio = new PlayerAudio(gameObject, spatialBlend: 0f);
        _animations  = new Animations(_animator);

        _layerMask = ~((1 << gameObject.layer) | (1 << 2));
        
        _cameraTransform = Camera.main ? Camera.main.transform : null;
        
        _jumpLogic = new JumpLogic(_playerConfig.JumpForce,null, null);
        _bike = new BikeMovement(_rigidbody, transform, _playerConfig, _layerMask, _jumpLogic, _playerConfig.JumpDistance, _cameraTransform);

        BuildFeatureObjects();
    }

    private void OnDestroy()
    {
        DisposeFeatureObjects();
    }

    private void Update()
    {
        if (_pushState.IsSuspended)
            return;
                
        _riding?.Tick();
        _pushing?.Tick();
    }

    private void FixedUpdate()
    {
        if (_pushState.HasPendingVelocity)
        {
            _rigidbody.velocity = _pushState.ConsumePendingVelocity();
            
            return;
        }

        _pushState.Tick(Time.fixedDeltaTime);

        if (_pushState.IsSuspended)
            return;
        
        _jumping?.FixedTick();
    }

    public void LockControl(float seconds)
    {
        if (_controlLockRoutine != null)
            StopCoroutine(_controlLockRoutine);

        _originalInputForLock = _input;
        SetInput(new DisabledInput());

        _controlLockRoutine = StartCoroutine(UnlockAfter(seconds));
        
    }
    public void SetCamera(Transform cameraTransform)
    {
        _cameraTransform = cameraTransform;
        
        _bike?.SetCamera(_cameraTransform);
    }

    public void SetAnimator(Animator animator)
    {
        _animator = animator;
        _animator.Rebind();
        
        _animations = new Animations(_animator);
    }

    public void SetInput(IInput input)
    {
        if (_input == input)
            return;
        
        DisposeFeatureObjects();
        
        _input = input;
        
        BuildFeatureObjects();
    }

    public void TryApplyPush(Vector3 worldVelocity, float pushDuration)
    {
        _pushState.ApplyPendingVelocity(worldVelocity);
        _pushState.Suspend(pushDuration);
    }
    
    public void TryApplyTemporaryJumpBoost(float newForce, float duration) => _boosting?.ApplyTempJump(newForce, duration);

    public void TryApplyTemporarySpeedBoost(float newSpeed, float duration) => _boosting?.ApplyTempSpeed(newSpeed, duration);

    public void TryApplyInstantSpeedBoost(float speedMultiplier, float decelerationTime) => _boosting?.ApplyInstantSpeedBoost(speedMultiplier, decelerationTime);

    public void MarkAirborne() => _jumping?.MarkAirborne();
    
    private void BuildFeatureObjects()
    {
        _timedBoostApplier = new TimedBoostApplier(_playerConfig, _jumpLogic, this);
        _boosting = new BoostingPlayer(_timedBoostApplier, _rigidbody, transform, _playerConfig.MaxSpeed, this);
        
        _boosting.SpeedBoostStarted += OnSpeedBoostStarted;
        _boosting.SpeedBoostEnded += OnSpeedBoostEnded;
        _boosting.JumpBoostStarted += OnJumpBoostStarted;
        _boosting.JumpBoostEnded += OnJumpBoostEnded;
        
        _riding = new RidingPlayer(_bike, _input);
        _jumping = new JumpingPlayer(_input, _jumpLogic, _audio, _animations, _rigidbody, _bike, _playerConfig, _layerMask);
        BotPusher botPusher = new BotPusher(transform, _playerConfig.PushRadius, _playerConfig.PushForce, _playerConfig.PushCooldown, _playerConfig.PushDuration);
        _pushing = new PushingPlayer(botPusher, _input);
        
        ((PushingPlayer)_pushing).Pushed += OnPushPerformed;
    }

    private void DisposeFeatureObjects()
    {
        if (_pushing != null)
        {
            ((PushingPlayer)_pushing).Pushed -= OnPushPerformed;
            
            ((IDisposable)_pushing).Dispose();
            
            _pushing = null;
        }

        if (_jumping != null)
        {
            ((IDisposable)_jumping).Dispose();
            
            _jumping = null;
        }

        if (_boosting != null)
        {
            _boosting.SpeedBoostStarted -= OnSpeedBoostStarted;
            _boosting.SpeedBoostEnded -= OnSpeedBoostEnded;
            _boosting.JumpBoostStarted -= OnJumpBoostStarted;
            _boosting.JumpBoostEnded -= OnJumpBoostEnded;
            
            ((IDisposable)_boosting).Dispose();
            
            _boosting = null;
        }

        _riding = null;
        _timedBoostApplier = null;
    }

    private void OnPushPerformed(PushSide side, Transform target)
    {
        _animations.PlayPush(side);
        
        if (_playerConfig.PushClip != null)
            _audio.PlayPush(_playerConfig.PushClip);
        
        if (target != null)
            BotPushed?.Invoke(side, target);
    }
    
    private void OnSpeedBoostStarted(float value) => SpeedBoostStarted?.Invoke(value);
    
    private void OnSpeedBoostEnded() => SpeedBoostEnded?.Invoke();
    
    private void OnJumpBoostStarted(float value) => JumpBoostStarted?.Invoke(value);
    
    private void OnJumpBoostEnded() => JumpBoostEnded?.Invoke();
    
    private IEnumerator UnlockAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        if (_originalInputForLock != null)
            SetInput(_originalInputForLock);

        _originalInputForLock = null;
        _controlLockRoutine = null;
    }
}