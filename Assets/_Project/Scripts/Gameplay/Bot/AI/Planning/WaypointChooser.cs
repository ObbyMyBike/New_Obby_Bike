using System.Collections.Generic;
using UnityEngine;

public class WaypointChooser
{
    private readonly HashSet<Waypoint> recentWaypoints = new HashSet<Waypoint>();
    
    private Waypoint _lastChosenNext;

    public Waypoint LastChosenNext => _lastChosenNext;
    
    public void RememberVisited(Waypoint waypoint)
    {
        if (waypoint == null)
            return;
        
        recentWaypoints.Add(waypoint);
        
        if (recentWaypoints.Count > 5)
            recentWaypoints.Clear();
    }

    public Waypoint PickNext(Waypoint from, float aggression, Vector3 previewDirectionFromCurrent)
    {
        if (from == null || from.NextWaypoints == null || from.NextWaypoints.Count == 0)
            return null;
                
        List<Waypoint> options = from.NextWaypoints;
        List<Waypoint> filtered = new List<Waypoint>(options.Count);
        
        bool hasPreview = previewDirectionFromCurrent.sqrMagnitude > 1e-6f;
        Vector3 previewDirection = hasPreview ? previewDirectionFromCurrent.normalized : Vector3.zero;
        float backDotThreshold = 0.2f;
        
        foreach (Waypoint waypoint in options)
        {
            if (waypoint == null || waypoint == from)
                continue;

            if (hasPreview)
            {
                Vector3 toCandidate = (waypoint.transform.position - from.transform.position);
                toCandidate.y = 0f;

                if (toCandidate.sqrMagnitude > 1e-6f)
                {
                    float dot = Vector3.Dot(toCandidate.normalized, previewDirection);
                    
                    if (dot > backDotThreshold)
                        continue;
                }
            }

            filtered.Add(waypoint);
        }
        
        if (filtered.Count == 0)
            filtered = options;

        int bestIndex = 0;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < filtered.Count; i++)
        {
            Waypoint waypoint = filtered[i];
            float random = Random.value;

            if (recentWaypoints.Contains(waypoint))
                random -= 0.3f;
            
            if (waypoint.RequiresJump)
                random += Mathf.Lerp(0f, 0.25f, aggression);
            
            if (waypoint == _lastChosenNext)
                random -= 0.2f;

            if (random > bestScore)
            {
                bestScore  = random;
                bestIndex  = i;
            }
        }

        _lastChosenNext = filtered[Mathf.Clamp(bestIndex, 0, filtered.Count - 1)];
        
        return _lastChosenNext;
    }

    public void ForceRepathFrom(Waypoint current)
    {
        if (current != null)
            recentWaypoints.Add(current);
        
        _lastChosenNext = null;
    }
}