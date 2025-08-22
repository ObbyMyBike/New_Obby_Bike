using UnityEngine;

public class TrampolineBounce
{
    public void TryBounce(BoostTarget target, BoostZonePreset preset, Collision collision, ref float lastBounceTime, bool landingEligible)
    {
        if (Time.time - lastBounceTime < preset.BounceCooldown)
            return;

        if (target.TryGetComponent(out Rigidbody rigidbody))
            if (rigidbody == null)
                return;

        Vector3 normal;
        
        if (preset.UseSurfaceNormal)
            normal = collision.GetContact(0).normal;
        else if (preset.CustomBounceDirection.sqrMagnitude > 0.001f)
            normal = preset.CustomBounceDirection.normalized;
        else
            normal = Vector3.up;

        if (Vector3.Dot(normal, Vector3.up) < 0f)
            normal = -normal;

        float incomingSpeed = Vector3.Dot(collision.relativeVelocity, -normal);
        float verticalVelAlongNormal = Vector3.Dot(rigidbody.velocity, normal);
        
        if (!landingEligible || incomingSpeed < preset.MinImpactSpeed || verticalVelAlongNormal >= 0f)
            return;

        float bounceStrength = Mathf.Max(preset.BounceForce, incomingSpeed);
        Vector3 bounceVel = normal * bounceStrength;

        if (preset.KeepHorizontalVelocity)
        {
            Vector3 tangent = Vector3.ProjectOnPlane(rigidbody.velocity, normal);
            bounceVel += tangent;
        }

        target.BoostArc(bounceVel);
        lastBounceTime = Time.time;

        if (target.TryGetComponent(out PlayerCharacterRoot playerController))
            if (playerController != null)
                playerController.MarkAirborne();
    }
}