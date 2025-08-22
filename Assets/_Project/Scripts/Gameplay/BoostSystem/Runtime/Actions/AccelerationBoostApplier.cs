public class AccelerationBoostApplier
{
    public void TryApplyAcceleration(BoostTarget target, BoostZonePreset preset)
    {
        if (target.TryGetComponent(out PlayerCharacterRoot controller))
        {
            controller.TryApplyTemporarySpeedBoost(preset.AccelerationMultiplier, preset.AccelerationDuration);
        }
    }
}