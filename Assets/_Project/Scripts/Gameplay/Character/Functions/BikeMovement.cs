using UnityEngine;

public class BikeMovement
{
    private readonly PlayerConfig config;
    private readonly JumpLogic jumpLogic;
    private readonly Rigidbody rigidbody;
    private readonly Transform transform;
    private readonly LayerMask layerMask;
    
    private readonly float jumpDistance;
    private readonly float climbAssist = 1f;
    private readonly float maxClimbableSlopeAngle = 50f;
    
    private Transform _cameraTransform;
    private Vector2 _smoothedInput;
    private Vector2 _rawInput;
    private Vector3 _groundNormal = Vector3.up;
    
    private float _headingYawDegrees;
    private float _targetHeadingYawDegrees;
    private float _headingSmoothVelocity;
    private float _currentLeanAngle;
    private float _leanVelocity;

    public BikeMovement(Rigidbody rigidbody, Transform transform, PlayerConfig config, LayerMask layerMask, JumpLogic jumpLogic, float jumpDistance, Transform cameraTransform = null)
    {
        this.rigidbody = rigidbody;
        this.transform = transform;
        this.config = config;
        this.layerMask = layerMask;
        this.jumpLogic = jumpLogic;
        this.jumpDistance = jumpDistance;
        
        _cameraTransform = cameraTransform;
        _headingYawDegrees = transform.eulerAngles.y;
        _targetHeadingYawDegrees = _headingYawDegrees;
        _headingSmoothVelocity = 0f;
    }
    
    public Transform TransformRef => transform;
    public bool IsGrounded { get; private set; }
    
    public void SetCamera(Transform cameraTransform) => _cameraTransform = cameraTransform;

    public void UpdateInput(IInput input)
    {
        _rawInput = input.InputDirection;
        _smoothedInput = Vector2.Lerp(_smoothedInput, _rawInput, Time.deltaTime * 10f);
    }

    private void UpdateGroundNormal()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 1.2f, layerMask, QueryTriggerInteraction.Ignore))
        {
            _groundNormal = hit.normal;
            IsGrounded = true;
        }
        else
        {
            _groundNormal = Vector3.up;
            IsGrounded = false;
        }
    }

    public void UpdatePhysics()
    {
        if (transform.TryGetComponent(out BoostTarget boostTarget))
            if (boostTarget != null && boostTarget.IsBoosting)
                return;
        
        float deltaTime = Time.fixedDeltaTime;
        bool jumpSystemGrounded = jumpLogic.IsGrounded(transform, jumpDistance, layerMask);
        
        UpdateGroundNormal();
        
        IsGrounded = jumpSystemGrounded || IsGrounded;

        Vector3 desiredDirectionWorld;
        if (_cameraTransform != null)
        {
            Vector3 cameraForward = _cameraTransform.forward; cameraForward.y = 0f; cameraForward.Normalize();
            Vector3 cameraRight = _cameraTransform.right; cameraRight.y = 0f; cameraRight.Normalize();

            desiredDirectionWorld = cameraRight * _smoothedInput.x + cameraForward * _smoothedInput.y;
        }
        else
        {
            desiredDirectionWorld = new Vector3(_smoothedInput.x, 0f, _smoothedInput.y);
        }
        
        if (desiredDirectionWorld.sqrMagnitude > 0.0001f) 
            desiredDirectionWorld.Normalize();
        else
            desiredDirectionWorld = Vector3.zero;

        bool hasInput = desiredDirectionWorld.sqrMagnitude > 0f;
        
        if (hasInput)
            _targetHeadingYawDegrees = Mathf.Atan2(desiredDirectionWorld.x, desiredDirectionWorld.z) * Mathf.Rad2Deg;

        _headingYawDegrees = Mathf.SmoothDampAngle(_headingYawDegrees, _targetHeadingYawDegrees, ref _headingSmoothVelocity, config.TurnSmoothTime, float.MaxValue, deltaTime);

        if (hasInput)
        {
            float delta = Mathf.DeltaAngle(_headingYawDegrees, _targetHeadingYawDegrees);
            
            if (Mathf.Abs(delta) < 0.5f)
            {
                _headingYawDegrees = _targetHeadingYawDegrees;
                _headingSmoothVelocity = 0f;
            }
        }

        Quaternion baseHeading = Quaternion.Euler(0f, _headingYawDegrees, 0f);
        Vector3 forwardOnGround = Vector3.ProjectOnPlane(baseHeading * Vector3.forward, _groundNormal).normalized;
        Vector3 rightOnGround = Vector3.ProjectOnPlane(baseHeading * Vector3.right, _groundNormal).normalized;

        float throttle = Mathf.Clamp01(_smoothedInput.magnitude);
        Vector3 velocity = rigidbody.velocity;
        
        float forwardVelocity = Vector3.Dot(velocity, forwardOnGround);
        float lateralVelocity = Vector3.Dot(velocity, rightOnGround);
        float slopeAngle = Vector3.Angle(_groundNormal, Vector3.up);
        float gravityAlongForward = Vector3.Dot(Physics.gravity, forwardOnGround);
        
        if (throttle > config.MinInputThreshold)
        {
            float desiredForwardSpeed = throttle * config.MaxSpeed;
            float speedDifference = desiredForwardSpeed - forwardVelocity;
            float baseAccel = Mathf.Clamp(speedDifference / Mathf.Max(deltaTime, 0.0001f), -config.Acceleration, config.Acceleration);
            float slopeCompensation = 0f;
            
            if (gravityAlongForward < 0f && slopeAngle <= maxClimbableSlopeAngle)
                slopeCompensation = -gravityAlongForward * climbAssist;
            else if (gravityAlongForward > 0f)
                slopeCompensation = gravityAlongForward * 0.5f;

            float totalAccel = baseAccel + slopeCompensation;
            
            rigidbody.AddForce(forwardOnGround * totalAccel, ForceMode.Acceleration);
        }
        else
        {
            float slowDownAccel = Mathf.Clamp(-forwardVelocity / Mathf.Max(deltaTime, 0.0001f), -config.Drag, config.Drag);
            
            rigidbody.AddForce(forwardOnGround * slowDownAccel, ForceMode.Acceleration);
        }
        
        float turnAngleDifference = Mathf.DeltaAngle(_headingYawDegrees, _targetHeadingYawDegrees);
        float turningIntensity = hasInput ? Mathf.Clamp01(Mathf.Abs(turnAngleDifference) / 60f) : 0f;
        float effectiveLateralFriction = config.LateralFriction * (1f - config.DriftFactor * turningIntensity);
        float lateralCorrectionAccel = Mathf.Clamp(-lateralVelocity / Mathf.Max(deltaTime, 0.0001f), -effectiveLateralFriction, effectiveLateralFriction);
        
        rigidbody.AddForce(rightOnGround * lateralCorrectionAccel, ForceMode.Acceleration);
        
        Vector3 flatVelocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
        float flatSpeed = flatVelocity.magnitude;
        float maxHorizSpeed = config.MaxSpeed * 2f;
        
        if (flatSpeed > maxHorizSpeed)
        {
            Vector3 limited = flatVelocity.normalized * maxHorizSpeed;
            
            rigidbody.velocity = new Vector3(limited.x, rigidbody.velocity.y, limited.z);
        }
        
        float headingRate = Mathf.DeltaAngle(_headingYawDegrees, _targetHeadingYawDegrees);
        float leanFactor = Mathf.Clamp01(Mathf.Abs(headingRate) / 45f);
        float targetLean = -Mathf.Sign(headingRate) * leanFactor * config.LeanAngleMax;
        
        _currentLeanAngle = Mathf.SmoothDamp(_currentLeanAngle, targetLean, ref _leanVelocity, config.LeanSmoothTime, float.MaxValue, deltaTime);

        Quaternion leanQuaternion = Quaternion.AngleAxis(_currentLeanAngle, baseHeading * Vector3.forward);
        Quaternion finalRotation = leanQuaternion * baseHeading;
        
        rigidbody.MoveRotation(finalRotation);
    }

    public void TryJump(bool jumpPressedThisFrame)
    {
        if (jumpPressedThisFrame && IsGrounded)
        {
            Vector3 jumpDir = transform.forward;
            jumpLogic.TryJump(rigidbody, jumpDir, true);
        }
    }
}