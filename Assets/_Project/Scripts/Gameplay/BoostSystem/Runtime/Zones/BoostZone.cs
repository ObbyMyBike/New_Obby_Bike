using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BoostZone : MonoBehaviour
{
    [SerializeField] private BoostLibrary _library;
    [SerializeField] private Transform _launchPoint;
    [SerializeField] private int _presetIndex = 0;

    private readonly LaunchArcCache _launchArcCache = new LaunchArcCache();
    private readonly LaunchDispatcher _launchDispatcher = new LaunchDispatcher();
    private readonly Collider[] overlapBuffer = new Collider[BoostZoneConstants.OverlapBufferSize];
    
    private SpeedBoostApplier _speedBoostApplier;
    private TrampolineBounce _trampoline;
    private JumpBoostApplier _jumpBoostApplier;
    private AccelerationBoostApplier _accelerationBoostApplier;
    private TargetQuery _targetQuery;
    
    private Collider _zoneCollider;
    
    private float _lastBounceTime = -999f;
    private float _lastLaunchTime = -999f;

    private void Awake()
    {
        _zoneCollider = GetComponent<Collider>();
        
        _speedBoostApplier = new SpeedBoostApplier();
        _trampoline = new TrampolineBounce();
        _jumpBoostApplier = new JumpBoostApplier();
        _accelerationBoostApplier = new AccelerationBoostApplier();
        _targetQuery = new TargetQuery();

        ApplyColliderMode();
        
        _launchArcCache.Rebuild(Current, GetLaunchPoint(), _presetIndex);
    }

    private void FixedUpdate()
    {
        if (_launchArcCache.ShouldRebuild(Current, GetLaunchPoint(), _presetIndex))
            _launchArcCache.Rebuild(Current, GetLaunchPoint(), _presetIndex);

        if (Current?.BoostType == BoostType.Launcher)
            TryLaunch();
    }

    public BoostType ZoneType => Current?.BoostType ?? BoostType.BoosterSpeed;

    public void ApplyBoost(BoostTarget target) => HandleInteraction(target, null);

    private BoostZonePreset Current => (_library != null && _presetIndex >= 0 && _presetIndex < _library.Presets.Count)
        ? _library.Presets[_presetIndex] : null;

    private Transform GetLaunchPoint() => _launchPoint != null ? _launchPoint : transform;
    
    private void TryLaunch()
    {
        BoostZonePreset preset = Current;
        
        if (preset == null || !_launchArcCache.CachedArc.IsValid)
            return;

        Transform launchPoint = GetLaunchPoint();

        int hits = _targetQuery.FindTargetsInSphere(launchPoint.position, preset.DetectionRadius, overlapBuffer, layerMask: ~0,
            triggerMode: QueryTriggerInteraction.Collide);

        for (int i = 0; i < hits; i++)
        {
            BoostTarget target = _targetQuery.ExtractBoostTarget(overlapBuffer[i]);
            
            if (target == null || target.IsBoosting)
                continue;

            if (_launchDispatcher.TryLaunch(target, launchPoint, _launchArcCache.CachedArc, ref _lastLaunchTime, BoostZoneConstants.LauncherCooldownSeconds,
                    BoostZoneConstants.DefaultClearProbeRadius, BoostZoneConstants.DefaultCollisionOffFrames))
            {
                break;
            }
        }
    }

    private void HandleInteraction(Component hit, Collision collision)
    {
        BoostZonePreset preset = Current;
        
        if (preset == null)
            return;

        if (!hit.TryGetComponent(out BoostTarget target))
            target = hit.GetComponentInParent<BoostTarget>();

        if (target == null)
            return;

        switch (preset.BoostType)
        {
            case BoostType.BoosterSpeed:
                _speedBoostApplier.TryApplyBooster(target, preset);
                break;

            case BoostType.Trampoline:
                
                if (collision != null)
                {
                    bool landingEligible = false;
                    PlayerCharacterRoot player = target.GetComponentInParent<PlayerCharacterRoot>();
                    
                    if (player != null)
                        landingEligible = player.LandingEligible;

                    _trampoline.TryBounce(target, preset, collision, ref _lastBounceTime, landingEligible);
                }
                
                break;

            case BoostType.Launcher:
                _launchDispatcher.TryLaunch(target, GetLaunchPoint(), _launchArcCache.CachedArc, ref _lastBounceTime, BoostZoneConstants.CooldownSeconds, 
                    BoostZoneConstants.DefaultClearProbeRadius, BoostZoneConstants.DefaultCollisionOffFrames);
                
                break;

            case BoostType.Rocket:
                _launchDispatcher.TryLaunch(target, GetLaunchPoint(), _launchArcCache.CachedArc, ref _lastBounceTime, BoostZoneConstants.RocketCooldownSeconds, 
                    BoostZoneConstants.DefaultClearProbeRadius, BoostZoneConstants.DefaultCollisionOffFrames);
                
                break;

            case BoostType.Acceleration:
                
                if (target.GetComponentInParent<PlayerCharacterRoot>() != null)
                    _accelerationBoostApplier.TryApplyAcceleration(target, preset);
                
                break;

            case BoostType.Jump:
                
                if (target.GetComponentInParent<PlayerCharacterRoot>() != null)
                    _jumpBoostApplier.TryApplyJumpBoost(target, preset);
                
                break;
        }
    }

    private void ApplyColliderMode()
    {
        if (Current == null || _zoneCollider == null)
            return;

        bool isPickupZone = Current.BoostType == BoostType.BoosterSpeed || Current.BoostType == BoostType.Acceleration ||
                            Current.BoostType == BoostType.Rocket || Current.BoostType == BoostType.Jump;

        _zoneCollider.isTrigger = isPickupZone;
    }

// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         DrawArcGizmos();
//     }
//
//     private void DrawArcGizmos()
//     {
//         BoostZonePreset preset = Current;
//
//         if (preset == null)
//             return;
//
//         bool isArcType = preset.BoostType == BoostType.Launcher || preset.BoostType == BoostType.Rocket;
//
//         if (!isArcType)
//             return;
//
//         Transform lp = _launchPoint != null ? _launchPoint : transform;
//         Vector3 right = lp.right;
//         Vector3 forward = lp.forward;
//         Vector3 hRight = Vector3.ProjectOnPlane(right, Vector3.up).normalized;
//         Vector3 hForward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
//
//         if (hRight.sqrMagnitude < 1e-6f || hForward.sqrMagnitude < 1e-6f)
//             return;
//
//         float x = preset.LaunchDistanceX;
//         float z = preset.LaunchDistanceZ;
//         float h = preset.LandingHeight;
//         Vector3 offsetDir = hRight * x + hForward * z;
//         float d = offsetDir.magnitude;
//
//         if (d < 0.001f)
//             return;
//
//         Vector3 dirXZ = offsetDir / d;
//
//         float g = Mathf.Abs(Physics.gravity.y);
//         float angleRad = Mathf.Clamp(preset.LaunchAngle, 1f, 89f) * Mathf.Deg2Rad;
//         float cosA = Mathf.Cos(angleRad);
//         float sinA = Mathf.Sin(angleRad);
//         float tanA = Mathf.Tan(angleRad);
//         float denom = 2f * cosA * cosA * (d * tanA - h);
//
//         if (denom <= 0f)
//             return;
//
//         float vBase = Mathf.Sqrt(g * d * d / denom);
//         bool isRocket = preset.BoostType == BoostType.Rocket;
//         float vReal = isRocket ? vBase : vBase * Mathf.Max(0.01f, preset.LaunchForce);
//         Vector3 v0 = dirXZ * (vReal * cosA) + Vector3.up * (vReal * sinA);
//
//         Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
//         Gizmos.DrawLine(lp.position, lp.position + hForward * 1.0f);
//         Gizmos.DrawLine(lp.position, lp.position + hRight * 1.0f);
//
//         if (preset.BoostType == BoostType.Launcher && preset.DetectionRadius > 0f)
//         {
//             Gizmos.color = Color.cyan;
//             Gizmos.DrawWireSphere(lp.position, preset.DetectionRadius);
//         }
//
//         Vector3 pos = lp.position;
//         Vector3 vel = v0;
//
//         Gizmos.color = (preset.BoostType == BoostType.Rocket) ? new Color(1f, 0.2f, 0.8f, 1f) : Color.white;
//
//         int maxSteps = 200;
//         float dt = 1f / 50f;
//
//         for (int i = 0; i < maxSteps; i++)
//         {
//             Vector3 nextVel = vel + Physics.gravity * dt;
//             Vector3 nextPos = pos + vel * dt;
//
//             Gizmos.DrawLine(pos, nextPos);
//
//             pos = nextPos;
//             vel = nextVel;
//
//             if (pos.y <= lp.position.y + h)
//                 break;
//         }
//
//         Vector3 landingWanted = lp.position + dirXZ * d + Vector3.up * h;
//
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(landingWanted, 0.25f);
//
//         float vsin = vReal * sinA;
//         float disc = vsin * vsin - 2f * g * h;
//
//         if (disc < 0f)
//             disc = 0f;
//
//         float tFlight = (vsin + Mathf.Sqrt(disc)) / g;
//         float vx = vReal * cosA;
//
//         Vector3 landingPred = lp.position + dirXZ * (vx * tFlight);
//         landingPred.y = lp.position.y + vsin * tFlight - 0.5f * g * tFlight * tFlight;
//
//         Gizmos.color = (preset.BoostType == BoostType.Rocket) ? new Color(1f, 0f, 0.6f, 1f) : Color.red;
//         Gizmos.DrawWireSphere(landingPred, 0.2f);
//         Gizmos.color = Color.green;
//         Gizmos.DrawRay(lp.position, v0.normalized * 0.75f);
//     }
// #endif
}