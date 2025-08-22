using UnityEngine;

public class SteeringBlend
{
    public float ComputeLaneBlend(Waypoint current, float distance)
    {
        float fadeStart = current.ActivationRadius * 1.3f;
        float fadeEnd = current.ActivationRadius * 0.6f;
        
        return Mathf.Clamp01(Mathf.InverseLerp(fadeEnd, fadeStart, distance));
    }

    public Vector3 ComposeDesired(Transform bot, Vector3 botPosition, Vector3 forwardDirection, Vector3 lookPoint, float laneOffset, float laneBlend, AvoidanceField avoidance)
    {
        Vector3 side = Vector3.Cross(Vector3.up, forwardDirection).normalized;
        
        if (side.sqrMagnitude < 1e-4f)
            side = Vector3.Cross(Vector3.up, bot.forward).normalized;

        Vector3 laneBias = side * (laneOffset * laneBlend);
        Vector3 desired  = (lookPoint + laneBias) - botPosition;

        desired += avoidance.Compute(botPosition, forwardDirection, bot);
        
        return desired;
    }

    public Vector3 UpdateSteering(Vector3 currentSteering, Vector3 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude <= 1e-4f)
            return currentSteering;
        
        return Vector3.Slerp(currentSteering, desiredDirection.normalized, 6f * Time.deltaTime);
    }
}