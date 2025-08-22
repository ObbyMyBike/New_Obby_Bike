using UnityEngine;

public class LookAhead
{
    private readonly SmartBotParams settings;

    public LookAhead(SmartBotParams settings) => this.settings = settings;

    public void Compute(Transform botTransform, Waypoint current, Waypoint projectedNext, float distance, Vector3 targetPosition, Vector3 currentSteering,
        out Vector3 forwardDirection, out Vector3 lookPoint)
    {
        forwardDirection = (targetPosition.sqrMagnitude > 1e-4f) ? targetPosition.normalized : currentSteering;
        lookPoint  = current.transform.position;

        float switchDist = Mathf.Min(current.ActivationRadius, 0.6f);
        
        if (distance <= switchDist && current.NextWaypoints != null && current.NextWaypoints.Count > 0)
        {
            Waypoint next = projectedNext ?? current.NextWaypoints[0];
            
            if (next != null)
            {
                forwardDirection = (next.transform.position - botTransform.position);
                forwardDirection.y = 0f;
                
                if (forwardDirection.sqrMagnitude > 1e-4f)
                    forwardDirection.Normalize();

                lookPoint = botTransform.position + forwardDirection * Mathf.Max(0.5f, settings.LookAhead * 0.7f);
            }
        }

        if (distance > 0.001f)
        {
            float la = Mathf.Min(settings.LookAhead, distance);
            
            lookPoint = botTransform.position + forwardDirection * la;
        }
    }
}