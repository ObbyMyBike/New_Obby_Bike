using System.Collections.Generic;
using UnityEngine;

public class GateCheck
{
    private readonly Dictionary<WaypointGate, bool> lastReady = new Dictionary<WaypointGate, bool>(32);

    public float GetStopRadius(WaypointGate gate) => (gate != null) ? Mathf.Max(0.05f, gate.StopRadius) : 0f;

    public float GetCollectRadius(Waypoint wp, WaypointGate gate)
    {
        float stop = GetStopRadius(gate);

        return (gate != null) ? Mathf.Max(wp.ActivationRadius, stop) : wp.ActivationRadius;
    }

    public bool ReadyToPass(WaypointGate gate, Waypoint current, Waypoint projectedNext, bool spawnGrace)
    {
        if (gate == null)
            return true;

        bool isReady = gate.IsSatisfied(current, projectedNext);

        bool allowGrace = spawnGrace;

        if (!isReady && spawnGrace && gate is RotatorAlignGate rot && rot.IgnoreSpawnGrace)
            allowGrace = false;

        if (!isReady && allowGrace)
            isReady = true;

        if (!lastReady.TryGetValue(gate, out var was))
            was = !isReady;

        if (was != isReady)
            lastReady[gate] = isReady;

        gate.SetWaiting(!isReady);
        
        return isReady;
    }
}