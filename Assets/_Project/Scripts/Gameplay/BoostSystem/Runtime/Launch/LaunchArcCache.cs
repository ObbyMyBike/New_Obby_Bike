using UnityEngine;

public class LaunchArcCache
{
    private PresetSignature _cachedSignature;
    
    private Vector3 _rightOnGround;
    private Vector3 _forwardOnGround;
    
    private float _gravityAbs;
    private int _presetIndex;
    
    public LaunchArc CachedArc { get; private set; }

    public bool ShouldRebuild(BoostZonePreset preset, Transform launchPoint, int currentPresetIndex)
    {
        if (preset == null || launchPoint == null)
            return CachedArc.IsValid;

        if (_presetIndex != currentPresetIndex)
            return true;
        
        if (!Mathf.Approximately(_gravityAbs, Mathf.Abs(Physics.gravity.y)))
            return true;
        
        if (!_cachedSignature.EqualsTo(preset))
            return true;
        
        Vector3 right = Vector3.ProjectOnPlane(launchPoint.right, Vector3.up).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(launchPoint.forward, Vector3.up).normalized;

        if (right.sqrMagnitude < BoostZoneConstants.HorizontalEpsilon || forward.sqrMagnitude < BoostZoneConstants.HorizontalEpsilon)
            return !CachedArc.IsValid;

        float dot = BoostZoneConstants.OrientationDotThreshold;
        bool rightChanged = Vector3.Dot(right, _rightOnGround) < dot;
        bool forwardChanged = Vector3.Dot(forward, _forwardOnGround) < dot;

        return rightChanged || forwardChanged;
    }

    public void Rebuild(BoostZonePreset preset, Transform launchPoint, int currentPresetIndex)
    {
        if (preset == null || launchPoint == null)
        {
            CachedArc = default;
            
            return;
        }

        Vector3 right = Vector3.ProjectOnPlane(launchPoint.right, Vector3.up).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(launchPoint.forward, Vector3.up).normalized;

        if (right.sqrMagnitude < BoostZoneConstants.HorizontalEpsilon || forward.sqrMagnitude < BoostZoneConstants.HorizontalEpsilon)
        {
            CachedArc = default;
            
            return;
        }
        
        Vector3 offset = right * preset.LaunchDistanceX + forward * preset.LaunchDistanceZ;
        float horizontalDistance = offset.magnitude;
        
        if (horizontalDistance < BoostZoneConstants.MinHorizDistanceCache)
        {
            CachedArc = default;
            
            return;
        }

        Vector3 directionXZ = offset / horizontalDistance;

        float gravityAbs = Mathf.Abs(Physics.gravity.y);
        float angleRad = Mathf.Clamp(preset.LaunchAngle, BoostZoneConstants.AngleMinDeg, BoostZoneConstants.AngleMaxDeg) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        float tan = Mathf.Tan(angleRad);
        float denom = 2f * cos * cos * (horizontalDistance * tan - preset.LandingHeight);
        
        if (denom <= BoostZoneConstants.HorizontalEpsilon)
        {
            CachedArc = default;
            
            return;
        }

        float baseSpeed = Mathf.Sqrt(gravityAbs * horizontalDistance * horizontalDistance / denom);
        float launchSpeed = (preset.BoostType == BoostType.Rocket) ? baseSpeed : baseSpeed * Mathf.Max(0.01f, preset.LaunchForce);

        Vector3 initialVelocity = directionXZ * (launchSpeed * cos) + Vector3.up * (launchSpeed * sin);

        float verticalSpeed = launchSpeed * sin;
        float disc = Mathf.Max(0f, verticalSpeed * verticalSpeed - 2f * gravityAbs * preset.LandingHeight);
        float timeOfFlight = (verticalSpeed + Mathf.Sqrt(disc)) / gravityAbs;

        CachedArc = new LaunchArc(directionXZ, initialVelocity, timeOfFlight);
        
        _rightOnGround = right;
        _forwardOnGround = forward;
        _gravityAbs = gravityAbs;
        _presetIndex = currentPresetIndex;
        
        _cachedSignature = new PresetSignature(preset);
    }
}