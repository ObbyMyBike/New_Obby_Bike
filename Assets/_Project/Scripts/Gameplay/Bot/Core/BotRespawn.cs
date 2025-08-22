using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BotRespawn : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _respawnCooldown = 0.5f;
    [SerializeField, Min(0f)] private float _respawnRetryInterval = 0.2f;
    
    [Header("Fall failsafe")]
    [SerializeField] private float _killBelowY = -40f;
    [SerializeField] private float _maxFreeFallSeconds = 2.5f;
    [SerializeField] private float _minFallSpeed = -7f;
    
    private BotInputAI _botInputAI;
    private Waypoint _startPoint;
    private Waypoint _lastCheckpointWaypoint;
    private CheckPoints _lastCheckpoint;
    
    private Rigidbody _rigidbody;
    private Vector3 _lastCheckpointPosition;
    
    private float _fallSince = -1f;
    private float _lastRespawnTime = -Mathf.Infinity;
    
    private bool _waitingForClear;
    private bool _hasCheckpoint;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
    }

    private void Update()
    {
        if (transform.position.y <= _killBelowY)
        {
            Respawn();
            
            _fallSince = -1f;
            
            return;
        }
        
        if (_rigidbody != null)
        {
            if (_rigidbody.velocity.y <= _minFallSpeed)
            {
                if (_fallSince < 0f) _fallSince = Time.time;

                if (Time.time - _fallSince >= _maxFreeFallSeconds)
                {
                    Respawn();
                    
                    _fallSince = -1f;
                }
            }
            else
            {
                _fallSince = -1f;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CheckPoints checkpoint))
            SetCheckpoint(checkpoint);
    }
    
    public void Initialize(BotInputAI botInputAI, Waypoint startPoint)
    {
        _botInputAI = botInputAI;
        _startPoint = startPoint;
        _lastCheckpointWaypoint = startPoint;
        _lastCheckpointPosition = startPoint.transform.position;
        _hasCheckpoint = false;
    }

    public void SetCheckpoint(CheckPoints checkpoint)
    {
        if (checkpoint == null)
            return;

        _lastCheckpoint = checkpoint;
        _lastCheckpointPosition = checkpoint.transform.position;
        _hasCheckpoint = true;
        
        if (checkpoint.AssociatedWaypoint != null)
            _lastCheckpointWaypoint = checkpoint.AssociatedWaypoint;
    }
    
    public void Respawn()
    {
        if (_botInputAI == null)
            return;

        if (Time.time - _lastRespawnTime < _respawnCooldown)
            return;

        if (!_hasCheckpoint && _startPoint != null)
        {
            _lastCheckpointPosition = _startPoint.transform.position;
            _lastCheckpointWaypoint = _startPoint;
        }
        
        if (_lastCheckpoint is CheckPoints checkpoint)
        {
            if (!checkpoint.CanSpawnOrRespawnHere())
            {
                if (!_waitingForClear)
                    StartCoroutine(RetryRespawnUntilClear(checkpoint));
                
                return;
            }
        }

        _lastRespawnTime = Time.time;
        
        TryRespawn();
    }
    
    private void TryRespawn()
    {
        transform.position = _lastCheckpointPosition;

        if (_lastCheckpointWaypoint != null)
        {
            Vector3 plane = _lastCheckpointWaypoint.transform.position - transform.position;
            plane.y = 0;

            if (plane.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(plane.normalized);
        }

        if (_rigidbody != null)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        _botInputAI.ResetToWaypoint(_lastCheckpointWaypoint);
        _botInputAI.Tick();
    }

    private IEnumerator RetryRespawnUntilClear(CheckPoints checkpoint)
    {
        _waitingForClear = true;

        while (true)
        {
            yield return new WaitForSeconds(_respawnRetryInterval);
            
            if (checkpoint.CanSpawnOrRespawnHere())
                break;
        }

        _waitingForClear = false;
        _lastRespawnTime = Time.time;
        
        TryRespawn();
    }
}