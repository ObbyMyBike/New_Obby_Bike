using System.Collections.Generic;
using UnityEngine;

public class RotatorAlignGate : WaypointGate
{
    [Header("References")]
    [SerializeField] private Transform _rotator;
    [SerializeField] private SimpleOrbitMovement _orbit;
    [SerializeField] private List<Transform> _faces = new();
    [SerializeField] private Transform _targetPoint;
    [SerializeField] private OrbitActivationHub _orbitHub;

    [Header("Settings")]
    [SerializeField, Min(0f)] private float _angleTolerance = 12f;
    [SerializeField, Min(0f)] private float _openGraceSeconds = 0.12f;
    [SerializeField] private bool _checkRayClear = false;
    [SerializeField] private bool _useAnchorForward = true;
    [SerializeField] private bool _ignoreOwnRotatorInRay = true;
    [SerializeField] private LayerMask _rayMask = ~0;

    [Header("Gate")]
    [SerializeField, Min(0f)] private float _stopRadius = 0.6f;
    [SerializeField] private bool _requireJumpOnPass = false;
    [SerializeField] private bool _ignoreSpawnGrace = true;
    
    private float _lastOkTime = -999f;
    private bool _isWaiting;

    public bool IgnoreSpawnGrace => _ignoreSpawnGrace;
    public override float StopRadius => _stopRadius;
    public override bool RequireJumpOnPass => _requireJumpOnPass;
    
    public override bool IsSatisfied(Waypoint current, Waypoint projectedNext)
    {
        if (_rotator == null || _targetPoint == null || _faces == null || _faces.Count == 0)
            return true;

        Vector3 planeNormal = (_orbit != null ? _orbit.AxisWorld : Vector3.up);
        planeNormal = planeNormal.sqrMagnitude > 1e-6f ? planeNormal.normalized : Vector3.up;

        Vector3 desiredDirection = _useAnchorForward ? -_targetPoint.forward : (_targetPoint.position - _rotator.position);
        desiredDirection = Vector3.ProjectOnPlane(desiredDirection, planeNormal);
        desiredDirection.y = 0f;
        
        if (desiredDirection.sqrMagnitude < 1e-6f)
            return false;
        
        desiredDirection.Normalize();

        bool coreOk = false;
        float bestAngle = 999f;

        for (int i = 0; i < _faces.Count; i++)
        {
            Transform face = _faces[i];
            
            if (face == null)
                continue;

            Vector3 faceDirection = Vector3.ProjectOnPlane(face.forward, planeNormal);
            faceDirection.y = 0f;
            
            if (faceDirection.sqrMagnitude < 1e-6f)
                continue;
            
            faceDirection.Normalize();

            float angle = Vector3.Angle(faceDirection, desiredDirection);
            
            if (angle < bestAngle)
            {
                bestAngle = angle;
            }
            
            if (angle <= _angleTolerance)
                coreOk = true;
        }

        if (coreOk)
            _lastOkTime = Time.time;

        bool isGraceOk = (Time.time - _lastOkTime) <= _openGraceSeconds;
        bool ok = coreOk || isGraceOk;
        
        if (ok && _checkRayClear && current != null && projectedNext != null)
        {
            Vector3 origin = current.transform.position + Vector3.up * 0.2f;
            Vector3 direction = projectedNext.transform.position - current.transform.position;
            float distance = direction.magnitude;

            if (distance > 1e-4f)
            {
                direction /= distance;

                bool blocked = false;
                RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, _rayMask, QueryTriggerInteraction.Ignore);
                
                foreach (RaycastHit hit in hits)
                {
                    if (_ignoreOwnRotatorInRay && _rotator != null && (hit.collider.transform == _rotator || hit.collider.transform.IsChildOf(_rotator)))
                        continue;

                    blocked = true;
                    
                    break;
                }

                ok &= !blocked;
            }
        }

        return ok;
    }

    public override void SetWaiting(bool waiting)
    {
        if (_isWaiting == waiting)
            return;
        
        _isWaiting = waiting;

        if (_orbitHub != null)
            _orbitHub.RequestOpen(this, waiting);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_rotator == null) return;

        if (_targetPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_rotator.position + Vector3.up * 0.1f, _targetPoint.position + Vector3.up * 0.1f);
            Gizmos.DrawSphere(_targetPoint.position + Vector3.up * 0.1f, 0.05f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_targetPoint.position + Vector3.up * 0.05f, _targetPoint.forward * 0.6f);
        }

        if (_faces != null)
        {
            foreach (var f in _faces)
            {
                if (f == null) continue;
                Vector3 p = f.position + Vector3.up * 0.05f;
                Gizmos.color = Color.green;
                Gizmos.DrawRay(p, f.forward * 0.6f);
            }
        }
    }
#endif
}