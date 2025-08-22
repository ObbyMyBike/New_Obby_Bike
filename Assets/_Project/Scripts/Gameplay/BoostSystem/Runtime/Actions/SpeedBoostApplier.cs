using UnityEngine;

public class SpeedBoostApplier
{
    public void TryApplyBooster(BoostTarget target, BoostZonePreset boostZonePreset)
    {
        if (target == null || boostZonePreset == null)
            return;
        
        if (target.TryGetComponent(out PlayerCharacterRoot controller))
        {
            controller.TryApplyInstantSpeedBoost(boostZonePreset.SpeedMultiplier, boostZonePreset.DecelerationTime);
        }
        else if (target.Rigidbody != null)
        {
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(target.Rigidbody.velocity, Vector3.up);
            Vector3 direction = horizontalVelocity.sqrMagnitude < 0.01f ? new Vector3(target.transform.forward.x, 0f, target.transform.forward.z).normalized
                : horizontalVelocity.normalized;

            direction.y = 0f;
            direction.Normalize();

            target.Boost(direction, boostZonePreset.SpeedMultiplier, boostZonePreset.DecelerationTime);
        }
    }
}