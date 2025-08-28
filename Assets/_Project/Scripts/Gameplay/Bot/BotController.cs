using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
public class BotController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _acceleration = 100f;
    [SerializeField] private float _angularSpeed = 720f;
    [SerializeField] private float _jumpForce = 30f;
    [SerializeField] private float _jumpDistance = 1.2f;

    [Header("Push")]
    [SerializeField] private float _pushRadius = 2f;
    [SerializeField] private float _pushForce = 10f;
    [SerializeField] private float _pushDuration = 1f;
    [SerializeField] private float _pushCooldown = 1.2f;
    [SerializeField, Range(0f, 1f)] private float _pushChance = 0.2f;
    
    [Header("Respawn")]
    [SerializeField] private float _respawnRetryInterval = 0.2f;
    [SerializeField] private float _killBelowY = -40f;
    [SerializeField] private float _maxFreeFallSeconds = 2.5f;
    [SerializeField] private float _minFallSpeed = -7f;
    
    [Header("FX")]
    [SerializeField] private float _trailEnableDelay = 0.5f;
    
    private Rigidbody _botRigidbody;
    private LayerMask _groundMask;
    
    private BotInputAI _ai;
    private BotDriver _driver;
    private BotPushAI _push;
    private BotRespawn _respawn;
    private BotProgress _progress;
    private FacingRotator _rotator;
    private RigidbodyGuard _guard;
    
    private PlayerCharacterRoot _player;
    private RacePath _racePath;
    private ProgressBarView _progressBar;
    
    private float _nextLogTime;
    
    [Inject] private LevelDirector _levelDirector;
    
    [Inject]
    private void Construct(Player player)
    {
        _player = player != null ? player.PlayerCharacterRoot : null;
    }

    private void Awake()
    {
        _botRigidbody = GetComponent<Rigidbody>();
        _botRigidbody.freezeRotation = true;

        _groundMask = ~((1 << gameObject.layer) | (1 << 2));

        _driver = new BotDriver(_botRigidbody, _animator, _jumpForce, _jumpDistance, _speed, _acceleration);
        _rotator = new FacingRotator();
        _guard = new RigidbodyGuard();
    }

    private void Update()
    {
        _guard.Sanitize(_botRigidbody, transform);

        _ai?.Tick();
        _driver.UpdateInputAndFacing(_ai, _rotator, transform, _angularSpeed, _groundMask);
        _push?.Tick(Time.time);
        _respawn?.TickFallKill(transform.position, _botRigidbody.velocity.y);
        _progress?.Tick();
    }
    
    private void FixedUpdate()
    {
        if (_driver == null)
            return;

        _driver.FixedStepMove(_groundMask);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_respawn == null)
            return;

        if (other.TryGetComponent(out CheckPoints checkPoints))
        {
            _respawn.SetCheckpoint(checkPoints);
            
            if (checkPoints.IsLevelEnd && _levelDirector != null)
            {
                if (_levelDirector.TryGetNextLevelStartFrom(checkPoints, out int nextIdx, out Waypoint startWaypoint))
                {
                    _levelDirector.GoToLevel(nextIdx);

                    if (startWaypoint != null)
                    {
                        Transform waypointTransform = startWaypoint.transform;
                        Vector3 forward = waypointTransform.forward;

                        if (startWaypoint.NextWaypoints != null)
                        {
                            foreach (Waypoint waypoint in startWaypoint.NextWaypoints)
                            {
                                if (waypoint == null)
                                    continue;
                                
                                Vector3 direction = waypoint.transform.position - waypointTransform.position; direction.y = 0f;

                                if (direction.sqrMagnitude > 1e-3f)
                                {
                                    forward = direction.normalized;
                                    
                                    break;
                                }
                            }
                        }

                        Vector3 position = waypointTransform.position;
                        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

                        transform.SetPositionAndRotation(position, rotation);
                        
                        if (_botRigidbody != null)
                        {
                            _botRigidbody.velocity = Vector3.zero;
                            _botRigidbody.angularVelocity = Vector3.zero;
                        }
                        
                        RestartTrailAfter(_trailEnableDelay);

                        _respawn?.SetCheckpoint(null);
                        _ai?.ResetToWaypoint(startWaypoint);
                        _ai?.Tick();

                        _racePath = _levelDirector.GlobalPath;
                        _progress?.Tick();
                    }
                    
                    return;
                }
            }
        }
    }
    
    public void Initialize(SmartBotParams botParams, Waypoint startWaypoint, RacePath racePath, ProgressBarView progressBarView)
    {
        _racePath = racePath;
        _progressBar = progressBarView;

        _ai = new BotInputAI(transform, startWaypoint, botParams);
        _driver.SetInput(_ai);
        
        _push = new BotPushAI(transform, _player, _botRigidbody, _pushRadius, _pushForce, _pushDuration, _pushCooldown, _pushChance);
        _respawn = new BotRespawn(_botRigidbody, _respawnRetryInterval, _killBelowY, _maxFreeFallSeconds, _minFallSpeed, startWaypoint, StartCoroutine, StopCoroutine);
        _progress = new BotProgress(transform, progressBarView, racePath, startWaypoint != null ? startWaypoint.transform.position : Vector3.zero, racePath != null ? racePath.FinishPoint : Vector3.zero);
    }
    
    public void RestartTrailAfter(float delaySeconds) => StartCoroutine(RestartTrailCoroutine(delaySeconds));
    
    public void ApplyPush(Vector3 velocity, float duration) => _driver?.ApplyPush(velocity, duration);

    public void SuspendControl(float duration) => _driver?.ApplyPush(Vector3.zero, duration);
    
    public void ForceRespawn() => _respawn?.Respawn(_ai);
    
    private System.Collections.IEnumerator RestartTrailCoroutine(float delay)
    {
        TrailRenderer[] trails = GetComponentsInChildren<TrailRenderer>(true);
        
        foreach (TrailRenderer trail in trails)
        {
            if (trail == null || !trail.enabled)
                continue;
            
            trail.Clear();
            trail.enabled = false;
        }

        yield return new WaitForSeconds(delay);

        foreach (TrailRenderer trail in trails)
        {
            if (trail == null)
                continue;
            
            trail.enabled = true;
        }
    }
}