using UnityEngine;

public class CatapultLaunch
{
    private readonly float defaultLiftUpMeters = 0.15f;
    private readonly float defaultForwardNudgeMeters = 0.08f;

    public bool TryLaunch(BoostTarget target, Transform launchPoint, LaunchArc arc, float clearProbeRadius = 0.25f,
        int collisionOffFrames = 2)
    {
        if (target == null || launchPoint == null || !arc.IsValid)
            return false;

        Rigidbody body = target.Rigidbody;

        if (body == null)
            return false;

        Vector3 startGuess = launchPoint.position + Vector3.up * defaultLiftUpMeters + arc.DirectionXZ * defaultForwardNudgeMeters;
        Vector3 takeoff = FindClearTakeoffPosition(startGuess, arc.DirectionXZ, clearProbeRadius);

        PrepareRigidbodyForLaunch(body, takeoff);

        target.EnableCollisionDelayFrames(collisionOffFrames);

        body.velocity = arc.InitialVelocity;
        target.BoostArc(arc.InitialVelocity, arc.TimeOfFlightSeconds);

        return true;
    }

    private void PrepareRigidbodyForLaunch(Rigidbody body, Vector3 position)
    {
        body.isKinematic = false;
        body.useGravity = true;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.angularVelocity = Vector3.zero;
        body.velocity = Vector3.zero;
        body.position = position;
    }
    
    private Vector3 FindClearTakeoffPosition(Vector3 start, Vector3 dirXZ, float radius)
    {
        int maxIterations = 8;
        float stepUp = 0.08f;
        float stepForward = 0.06f;

        Vector3 startPosition = start;

        for (int i = 0; i < maxIterations; i++)
        {
            bool blocked = Physics.CheckSphere(startPosition, radius, ~0, QueryTriggerInteraction.Ignore);
            
            if (!blocked)
                return startPosition;

            startPosition += Vector3.up * stepUp + dirXZ * stepForward;
        }

        return startPosition;
    }
}