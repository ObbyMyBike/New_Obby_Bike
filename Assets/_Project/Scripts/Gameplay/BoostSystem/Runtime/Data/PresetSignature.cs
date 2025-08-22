using UnityEngine;

public class PresetSignature
{
    private readonly BoostType type;
    private readonly float angle;
    private readonly float distanceX;
    private readonly float distanceZ;
    private readonly float height;
    private readonly float launchForce;

    public PresetSignature(BoostZonePreset preset)
    {
        type = preset.BoostType;
        angle = preset.LaunchAngle;
        distanceX = preset.LaunchDistanceX;
        distanceZ = preset.LaunchDistanceZ;
        height = preset.LandingHeight;
        launchForce = preset.LaunchForce;
    }
    
    public bool EqualsTo(BoostZonePreset preset)
    {
        if (type != preset.BoostType)
            return false;
        
        if (!Mathf.Approximately(angle, preset.LaunchAngle))
            return false;
        
        if (!Mathf.Approximately(distanceX, preset.LaunchDistanceX))
            return false;
        
        if (!Mathf.Approximately(distanceZ, preset.LaunchDistanceZ))
            return false;
        
        if (!Mathf.Approximately(height, preset.LandingHeight))
            return false;
        
        if (preset.BoostType != BoostType.Rocket && !Mathf.Approximately(launchForce, preset.LaunchForce))
            return false;

        return true;
    }
}