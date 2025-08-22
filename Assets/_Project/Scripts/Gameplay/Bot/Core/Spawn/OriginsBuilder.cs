using System.Collections.Generic;
using UnityEngine;

public class OriginsBuilder
{
    private readonly float spawnBack;

    public OriginsBuilder(float spawnBack) => this.spawnBack = Mathf.Max(0f, spawnBack);

    public List<SpawnOrigin> Build(Waypoint startPoint, CheckPoints[] extra)
    {
        List<SpawnOrigin> list = new List<SpawnOrigin>(1 + (extra?.Length ?? 0));
        
        if (startPoint != null)
            list.Add(FromStart(startPoint));

        if (extra != null)
            foreach (CheckPoints checkPoint in extra)
                if (TryFromCheckpoint(checkPoint, out SpawnOrigin spawnOrigin)) list.Add(spawnOrigin);
        
        return list;
    }

    private SpawnOrigin FromStart(Waypoint start)
    {
        BasisFromWaypoint(start, out Vector3 forward, out Vector3 right);
        
        return new SpawnOrigin
        {
            Position = start.transform.position - forward * spawnBack,
            Rotation = Quaternion.LookRotation(forward, Vector3.up),
            StartWaypoint = start,
            Checkpoint = null,
            LanesRight = right
        };
    }

    private bool TryFromCheckpoint(CheckPoints checkPoints, out SpawnOrigin origin)
    {
        origin = default;
        
        if (checkPoints == null || checkPoints.AssociatedWaypoint == null)
            return false;

        Waypoint waypoint = checkPoints.AssociatedWaypoint;
        BasisFromWaypoint(waypoint, out Vector3 forward, out Vector3 right);

        origin = new SpawnOrigin
        {
            Position = checkPoints.transform.position - forward * spawnBack,
            Rotation = Quaternion.LookRotation(forward, Vector3.up),
            StartWaypoint = waypoint,
            Checkpoint = checkPoints,
            LanesRight = right
        };
        
        return true;
    }

    private void BasisFromWaypoint(Waypoint waypoint, out Vector3 forward, out Vector3 right)
    {
        forward = Vector3.forward;
        
        if (waypoint != null)
        {
            if (waypoint.NextWaypoints != null)
            {
                for (int i = 0; i < waypoint.NextWaypoints.Count; i++)
                {
                    Waypoint nextWaypoint = waypoint.NextWaypoints[i];
                    
                    if (nextWaypoint == null)
                        continue;

                    Vector3 direction = nextWaypoint.transform.position - waypoint.transform.position;
                    direction.y = 0f;
                    
                    if (direction.sqrMagnitude > 1e-4f)
                        forward = direction; break;
                }
            }
            
            if (forward.sqrMagnitude <= 1e-4f)
                forward = waypoint.transform.forward;
        }
        
        forward.y = 0f;
        
        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.forward;
        
        forward.Normalize();
        
        right = Vector3.Cross(Vector3.up, forward).normalized;
    }
}