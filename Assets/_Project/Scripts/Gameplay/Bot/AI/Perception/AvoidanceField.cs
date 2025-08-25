using UnityEngine;

public class AvoidanceField
{
    private readonly SmartBotParams settings;
    private readonly Collider[] buffer;
    private float _nextLog;

    public AvoidanceField(SmartBotParams settings, Collider[] sharedBuffer = null)
    {
        this.settings = settings;
        buffer = sharedBuffer ?? new Collider[16];
    }

    public Vector3 Compute(Vector3 position, Vector3 forwardDirection, Transform self)
    {
        Vector3 result = Vector3.zero;

        int count = Physics.OverlapSphereNonAlloc(position, settings.SeparationRadius, buffer, settings.AgentsMask);

        for (int i = 0; i < count; i++)
        {
            Transform transform = buffer[i].transform;
            
            if (transform == self)
                continue;

            Vector3 toMe = position - transform.position;
            float distance = toMe.magnitude + 1e-3f;
            
            result += toMe / (distance * distance);
        }

        if (count > 0)
            result *= settings.SeparationStrength;
        
        if (Physics.Raycast(position + Vector3.up * 0.2f, forwardDirection, out RaycastHit _, settings.AvoidRayLength, settings.AgentsMask))
        {
            Vector3 side = Vector3.Cross(Vector3.up, forwardDirection).normalized;
            float leftScore  = Physics.Raycast(position, -side, settings.AvoidRayLength * 0.5f, settings.AgentsMask) ? 1f : 0f;
            float rightScore = Physics.Raycast(position,  side, settings.AvoidRayLength * 0.5f, settings.AgentsMask) ? 1f : 0f;
            Vector3 steerSide = (rightScore <= leftScore) ? side : -side;
        
            result += steerSide * settings.AvoidStrength;
        }
        
        Vector3 forward = forwardDirection;
        
        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.forward;
        
        forward.y = 0f;
        forward.Normalize();
        
        Vector3 sideDirection = Vector3.Cross(Vector3.up, forward).normalized;
        
        if (sideDirection.sqrMagnitude < 1e-6f)
            sideDirection = Vector3.right;
        
        Vector3 ahead = position + forward * settings.EdgeProbeAhead;
        
        int groundMask = settings.GroundMask.value == 0 ? Physics.DefaultRaycastLayers : settings.GroundMask.value;
        bool leftHasGround  = Physics.Raycast(ahead - sideDirection * settings.EdgeProbeSide + Vector3.up * 0.2f, Vector3.down, settings.EdgeProbeDown, groundMask, QueryTriggerInteraction.Ignore);
        bool rightHasGround = Physics.Raycast(ahead + sideDirection * settings.EdgeProbeSide + Vector3.up * 0.2f, Vector3.down, settings.EdgeProbeDown, groundMask, QueryTriggerInteraction.Ignore);
        bool aheadHasGround = Physics.Raycast(ahead + Vector3.up * 0.2f, Vector3.down, settings.EdgeProbeDown, groundMask, QueryTriggerInteraction.Ignore);
        
        if (leftHasGround != rightHasGround)
        {
            Vector3 push = (leftHasGround ? -sideDirection : sideDirection) * settings.EdgeAvoidStrength;
            result += push;
        }
        
        if (!aheadHasGround)
        {
            Vector3 lateral = leftHasGround && !rightHasGround ? -sideDirection : rightHasGround && !leftHasGround ?  sideDirection : Vector3.zero;
            result += (lateral - forward) * (settings.EdgeAvoidStrength * 0.6f);
        }
        
        return result;
    }
}