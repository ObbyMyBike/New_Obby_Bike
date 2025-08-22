using UnityEngine;

public class SpeedTuning
{
    private readonly SmartBotParams settings;

    public SpeedTuning(SmartBotParams settings) => this.settings = settings;

    public float TurnMultiplier(Waypoint current, Waypoint projectedNext, Vector3 botPosition)
    {
        if (current?.NextWaypoints == null || current.NextWaypoints.Count == 0)
            return 1f;

        Waypoint next = projectedNext ?? current.NextWaypoints[0];
        Vector3 currentPosition = current.transform.position;
        Vector3 seg1 = (currentPosition - botPosition).normalized;
        Vector3 seg2 = (next.transform.position - currentPosition).normalized;

        float angle = Vector3.Angle(seg1, seg2);
        
        if (angle < settings.TurnSlowdownAngle)
            return 1f;

        float k = Mathf.InverseLerp(settings.TurnSlowdownAngle, 120f, angle);
        
        return Mathf.Lerp(1f, settings.MinSpeedMulOnSharpTurn, k);
    }

    public void ApplyApproachSlowdown(ref Vector3 move, float distance, float stopRadius)
    {
        float approachR = Mathf.Max(stopRadius + 0.5f, stopRadius * 1.8f);
        
        if (distance <= approachR)
        {
            float slowK = Mathf.InverseLerp(stopRadius, approachR, distance);
            
            move *= Mathf.Clamp01(slowK);
        }
    }
}