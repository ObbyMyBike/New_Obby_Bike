using UnityEngine;

public class LaunchDispatcher
{
    private readonly CatapultLaunch catapult = new CatapultLaunch();

    public bool TryLaunch(BoostTarget target, Transform launchPoint, LaunchArc arc, ref float lastLaunchTime, float cooldownSeconds,
        float clearProbeRadius = BoostZoneConstants.DefaultClearProbeRadius, int collisionOffFrames = BoostZoneConstants.DefaultCollisionOffFrames)
    {
        if (!arc.IsValid)
            return false;

        if (Time.time - lastLaunchTime < cooldownSeconds)
            return false;

        bool launched = catapult.TryLaunch(target, launchPoint, arc, clearProbeRadius, collisionOffFrames);
        
        if (launched)
            lastLaunchTime = Time.time;

        return launched;
    }
}