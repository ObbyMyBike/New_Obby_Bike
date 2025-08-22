public class JumpBoostApplier
{
    public void TryApplyJumpBoost(BoostTarget target, BoostZonePreset preset)
    {
        if (target.TryGetComponent(out PlayerCharacterRoot controller))
        {
            controller.TryApplyTemporaryJumpBoost(preset.JumpMultiplier, preset.JumpDuration);
        }
    }
}