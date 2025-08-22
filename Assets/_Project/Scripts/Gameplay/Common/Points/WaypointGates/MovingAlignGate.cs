using UnityEngine;

public class MovingAlignGate : WaypointGate
{
    [Header("General")]
    [SerializeField, Min(0f)] private float _stopRadius = 0.7f;
    [SerializeField] private bool _requireJumpOnPass = false;

    [Header("Moving Platform Align (jump hop)")]
    [SerializeField] private Transform _hopFrom;
    [SerializeField] private Transform _hopTo;
    [SerializeField, Min(0f)] private float _maxHorizontalGap = 0.6f;
    [SerializeField, Min(0f)] private float _maxVerticalDelta = 0.25f;
    [SerializeField, Min(0f)] private float _alignGrace = 0.12f;

    private float _lastOkTime = -999f;

    public override float StopRadius => _stopRadius;
    public override bool RequireJumpOnPass => _requireJumpOnPass;

    public override bool IsSatisfied(Waypoint current, Waypoint projectedNext)
    {
        if (_hopFrom == null || _hopTo == null)
            return true;

        Vector3 firstPoint = _hopFrom.position;
        Vector3 secondPoint = _hopTo.position;

        float horizontal = Vector2.Distance(new Vector2(firstPoint.x, firstPoint.z), new Vector2(secondPoint.x, secondPoint.z));
        float vertical = Mathf.Abs(firstPoint.y - secondPoint.y);
        bool isCoreOk = (horizontal <= _maxHorizontalGap) && (vertical <= _maxVerticalDelta);
        
        if (isCoreOk)
            _lastOkTime = Time.time;

        bool graceOk = (Time.time - _lastOkTime) <= _alignGrace;
        
        return isCoreOk || graceOk;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_hopFrom == null || _hopTo == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_hopFrom.position + Vector3.up * 0.05f, 0.05f);
        Gizmos.DrawSphere(_hopTo.position   + Vector3.up * 0.05f, 0.05f);
        Gizmos.DrawLine(_hopFrom.position, _hopTo.position);
    }
#endif
}