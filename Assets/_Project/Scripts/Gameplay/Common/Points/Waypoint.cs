using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] private WaypointGate _gate;
    [SerializeField] private List<Waypoint> _nextWaypoints;
    [SerializeField] private bool _requiresJump = false;
    [SerializeField] private float _activationRadius = 0.5f;

    public WaypointGate Gate => _gate;
    public List<Waypoint> NextWaypoints => _nextWaypoints;
    public bool RequiresJump => _requiresJump;
    public float ActivationRadius => _activationRadius;

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = _requiresJump ? Color.yellow : Color.green;
    //     Gizmos.DrawSphere(transform.position, _activationRadius);
    //
    //     if (_gate != null)
    //     {
    //         Gizmos.color = Color.cyan;
    //         Gizmos.DrawWireSphere(transform.position, _gate.StopRadius);
    //     }
    //
    //     Gizmos.color = Color.white;
    //     
    //     foreach (Waypoint newWaypoint in _nextWaypoints)
    //         if (newWaypoint != null)
    //             Gizmos.DrawLine(transform.position, newWaypoint.transform.position);
    // }
}