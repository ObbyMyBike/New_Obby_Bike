using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Random = UnityEngine.Random;

public class BotSpawn : MonoBehaviour
{
    private const int MAX_ATTEMPTS = 12;
    
    [Header("Spawn settings")]
    [SerializeField] private SmartBotParams[] _botPresets;
    [SerializeField] private GameObject _botPrefab;
    [SerializeField] private Waypoint _startPoint;
    [SerializeField] private Transform _finPoint;
    [SerializeField] private Vector3 _spawnOffset = new Vector3(1, 0, 0);
    [SerializeField] private LayerMask _occupancyMask;
    [SerializeField] private int _botCount = 5;
    [SerializeField] private float _spawnCheckRadius = 1f;
    
    [Header("Extra spawn checkpoints")]
    [SerializeField] private CheckPoints[] _extraSpawnCheckpoints;
    [SerializeField] private bool _includeStartPoint = true;
    
    [Header("Spawn layout")]
    [SerializeField, Min(0f)] private float _spawnBack = 2.0f;
    [SerializeField, Min(0f)] private float _laneSpacing = 1.2f;
    [SerializeField] private int _laneSpread = 2;
    //[SerializeField] private bool _centerLanes = true;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float _retryInterval = 0.5f;
    [SerializeField, Min(0f)] private float _leaveDistance = 1.2f;

    [Header("Trail settings")]
    [SerializeField, Range(0f, 1f)] private float _trailChance = 0.5f;

    [FormerlySerializedAs("_progressBar")]
    [Header("Progress Bar settings")]
    [SerializeField] private ProgressBarView _progressBarView;
    
    [Header("Bot behaviour")]
    [SerializeField] private LayerMask _agentsMask;
    
    private readonly List<BotProgressTracker> progressTrackers = new List<BotProgressTracker>();
    private readonly Dictionary<GameObject, BotProgressTracker> trackerByBot = new();

    private RacePath _racePath;
    private ObjectPool<BotDriver> _botPool;
    private List<SpawnOrigin> _origins;
    
    [Inject] private NameAssigner _nameAssigner;
    [Inject] private DiContainer _container;
    
    private void Start()
    {
        if (_botPrefab == null || _startPoint == null || _finPoint == null || _progressBarView == null)
            return;
        
        _origins = BuildOrigins();
        
        if (_origins.Count == 0)
            return;
        
        CheckPoints[] allCps = FindObjectsOfType<CheckPoints>(true);
        _racePath = new RacePath(allCps);
        
        BotDriver driverOnPrefab = _botPrefab.GetComponentInChildren<BotDriver>(true);
        
        _botPool = new ObjectPool<BotDriver>(driverOnPrefab, _botCount, PoolContainer.Root, factory: () =>
            {
                GameObject instance = _container.InstantiatePrefab(_botPrefab, PoolContainer.Root);
                
                return instance.GetComponentInChildren<BotDriver>(true);
            });
        
        StartCoroutine(SpawnInitialRandom());
        
        // if (_botPrefab == null || _startPoint == null || _finPoint == null || _progressBarView == null)
        //     return;
        //
        // if (!_includeStartPoint && (_extraSpawnCheckpoints == null || _extraSpawnCheckpoints.Length == 0))
        //     return;
        //
        // CheckPoints[] allCps = FindObjectsOfType<CheckPoints>(true);
        // _racePath = new RacePath(allCps);
        //
        // StartCoroutine(SpawnBotsSequentially());
    }
    
    private void Update()
    {
        for (int i = progressTrackers.Count - 1; i >= 0; i--)
        {
            BotProgressTracker tracker = progressTrackers[i];
            
            if (!tracker.IsAlive)
            {
                tracker.Dispose();
                progressTrackers.RemoveAt(i);
            }
            else
            {
                tracker.Tick();
            }
        }
    }

    public void DespawnAndRespawn(BotDriver bot)
    {
        if (bot == null)
            return;
        
        if (trackerByBot.TryGetValue(bot.gameObject, out BotProgressTracker tracker))
        {
            tracker.Dispose();
            progressTrackers.Remove(tracker);
            trackerByBot.Remove(bot.gameObject);
        }
        
        if (bot.TryGetComponent(out Rigidbody botRigidbody))
        {
            if (botRigidbody)
            {
                botRigidbody.velocity = Vector3.zero;
                botRigidbody.angularVelocity = Vector3.zero;
            }
        }
        
        _botPool.Release(bot);
        
        StartCoroutine(SpawnOneAtRandomOrigin());
    }
    
    private SmartBotParams PickParamsVariant()
    {
        return (_botPresets != null && _botPresets.Length > 0) ? _botPresets[Random.Range(0, _botPresets.Length)]
            : ScriptableObject.CreateInstance<SmartBotParams>();
    }
    
    private SpawnOrigin RandomOrigin() => _origins[Random.Range(0, _origins.Count)];
    
    private SpawnOrigin BuildOriginFromStart()
    {
        BasisFromWaypoint(_startPoint, out Vector3 forward, out Vector3 rotation);

        return new SpawnOrigin
        {
            Position = _startPoint != null ? _startPoint.transform.position - forward * _spawnBack : transform.position,
            Rotation = Quaternion.LookRotation(forward, Vector3.up),
            StartWaypoint = _startPoint,
            Checkpoint = null,
            LanesRight = rotation
        };
    }
    
    private List<SpawnOrigin> BuildOrigins()
    {
        List<SpawnOrigin> list = new List<SpawnOrigin>(1 + (_extraSpawnCheckpoints?.Length ?? 0));
        
        if (_includeStartPoint)
            list.Add(BuildOriginFromStart());
        
        if (_extraSpawnCheckpoints != null)
        {
            foreach (CheckPoints checkPoints in _extraSpawnCheckpoints)
                if (TryBuildOriginFromCheckpoint(checkPoints, out SpawnOrigin spawnOrigin))
                    list.Add(spawnOrigin);
        }
        
        return list;
    }
    
    private bool IsPositionClear(Vector3 position, float radius, LayerMask mask)
    {
        Collider[] hits = Physics.OverlapSphere(position, radius, mask);
        
        return hits == null || hits.Length == 0;
    }
    
    private bool TryBuildOriginFromCheckpoint(CheckPoints checkPoints, out SpawnOrigin origin)
    {
        origin = default;
        
        if (checkPoints == null || checkPoints.AssociatedWaypoint == null)
            return false;

        Waypoint waypoint = checkPoints.AssociatedWaypoint;
        BasisFromWaypoint(waypoint, out Vector3 forward, out Vector3 rotation);

        Vector3 basePosition = checkPoints.transform.position - forward * _spawnBack;

        origin = new SpawnOrigin
        {
            Position = basePosition,
            Rotation = Quaternion.LookRotation(forward, Vector3.up),
            StartWaypoint = waypoint,
            Checkpoint = checkPoints,
            LanesRight = rotation
        };
        
        return true;
    }
    
    private void BasisFromWaypoint(Waypoint waypoint, out Vector3 forward, out Vector3 rotation)
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
                    
                    Vector3 vector = nextWaypoint.transform.position - waypoint.transform.position;
                    vector.y = 0f;

                    if (vector.sqrMagnitude > 1e-4f)
                    {
                        forward = vector;
                        break;
                    }
                }
            }
            
            if (forward.sqrMagnitude <= 1e-4f)
                forward = waypoint.transform.forward;
        }

        forward.y = 0f;
        
        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.forward;
        
        forward.Normalize();
        
        rotation = Vector3.Cross(Vector3.up, forward).normalized;
    }
    
    // private IEnumerator SpawnBotsSequentially()
    // {
    //     List<SpawnOrigin> origins = new List<SpawnOrigin>(1 + (_extraSpawnCheckpoints?.Length ?? 0)){ BuildOriginFromStart()};
    //
    //     if (_extraSpawnCheckpoints != null)
    //     {
    //         foreach (CheckPoints checkPoints in _extraSpawnCheckpoints)
    //         {
    //             if (TryBuildOriginFromCheckpoint(checkPoints, out SpawnOrigin spawnOrigin))
    //                 origins.Add(spawnOrigin);
    //         }
    //     }
    //
    //     if (origins.Count == 0)
    //         yield break;
    //
    //     float centerShift = _centerLanes ? -(_botCount - 1) * 0.5f : 0f;
    //
    //     for (int i = 0; i < _botCount; i++)
    //     {
    //         SpawnOrigin spawnOrigin = origins[i % origins.Count];
    //         
    //         Vector3 laneShift = spawnOrigin.LanesRight * _laneSpacing * (i + centerShift);
    //         Vector3 spawnPosition = spawnOrigin.Position + laneShift;
    //         Quaternion spawnRotation = spawnOrigin.Rotation;
    //         
    //         while (!IsPositionClear(spawnPosition, _spawnCheckRadius, _occupancyMask) ||
    //                (spawnOrigin.Checkpoint != null && !spawnOrigin.Checkpoint.CanSpawnOrRespawnHere()))
    //         {
    //             yield return new WaitForSeconds(_retryInterval);
    //         }
    //
    //         GameObject botInstance = _container.InstantiatePrefab(_botPrefab, spawnPosition, spawnRotation, transform);
    //         _nameAssigner.AssignToBot(botInstance);
    //
    //         bool useTrail = Random.value < _trailChance;
    //         TrailRenderer trail = botInstance.GetComponentInChildren<TrailRenderer>(); 
    //         
    //         if (trail != null)
    //             trail.enabled = useTrail;
    //
    //         BotDriver controller = botInstance.GetComponentInChildren<BotDriver>();
    //         SmartBotParams paramsSet = PickParamsVariant();
    //         BotInputAI inputAI = new BotInputAI(botInstance.transform, spawnOrigin.StartWaypoint, paramsSet);
    //         controller.SetInput(inputAI);
    //         
    //         if (botInstance.TryGetComponent(out BotPushAI aggressor))
    //             if (aggressor == null)
    //                 aggressor = _container.InstantiateComponent<BotPushAI>(botInstance);
    //
    //         if (botInstance.TryGetComponent(out BotRespawn respawn))
    //         {
    //             respawn.Initialize(inputAI, spawnOrigin.StartWaypoint);
    //             
    //             if (spawnOrigin.Checkpoint != null)
    //                 respawn.SetCheckpoint(spawnOrigin.Checkpoint);
    //         }
    //         
    //         BotProgress progress = botInstance.GetComponent<BotProgress>();
    //         BotProgressTracker tracker  = new BotProgressTracker(progress, _progressBarView, botInstance, _startPoint.transform.position, _finPoint.position, _racePath);
    //         progressTrackers.Add(tracker);
    //
    //         botInstance.SetActive(true);
    //         
    //         StartCoroutine(TickBot(inputAI));
    //         
    //         float timeout = 5f, waited = 0f;
    //         
    //         while (Vector3.Distance(botInstance.transform.position, spawnPosition) < _leaveDistance && waited < timeout)
    //         {
    //             yield return null;
    //             
    //             waited += Time.deltaTime;
    //         }
    //         if (Vector3.Distance(botInstance.transform.position, spawnPosition) < _leaveDistance)
    //         {
    //             Vector3 forward = (spawnOrigin.Rotation * Vector3.forward);
    //             controller.ApplyPush(forward * 2.5f, 0.25f);
    //         }
    //     }
    // }
    
    // private IEnumerator TickBot(BotInputAI inputAI)
    // {
    //     while (true)
    //     {
    //         inputAI.Tick();
    //
    //         yield return null;
    //     }
    // }
    
    private IEnumerator SpawnInitialRandom()
    {
        for (int i = 0; i < _botCount; i++)
            yield return StartCoroutine(SpawnOneAtRandomOrigin());
    }
    
    private IEnumerator SpawnOneAtRandomOrigin()
    {
        SpawnOrigin origin = RandomOrigin();
        
        Vector3 laneRight = origin.LanesRight;
        Vector3 spawnPosition = origin.Position;
        Quaternion spawnRotation = origin.Rotation;
        
        int attempts = 0;
        
        while (attempts++ < MAX_ATTEMPTS)
        {
            int lane = Random.Range(-_laneSpread, _laneSpread + 1);
            Vector3 candidate = origin.Position + laneRight * (lane * _laneSpacing);

            bool free = IsPositionClear(candidate, _spawnCheckRadius, _occupancyMask) && (origin.Checkpoint == null 
                || origin.Checkpoint.CanSpawnOrRespawnHere());
            
            if (free)
            {
                spawnPosition = candidate;
                
                break;
            }

            yield return new WaitForSeconds(_retryInterval);
        }
        
        BotDriver driver = _botPool.Get();
        GameObject bot = driver.gameObject;
        
        bot.transform.SetParent(transform, false);
        bot.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        
        _nameAssigner.AssignToBot(bot);
        
        bool useTrail = Random.value < _trailChance;
        TrailRenderer trail = bot.GetComponentInChildren<TrailRenderer>(); 
            
        if (trail != null)
            trail.enabled = useTrail;
        
        SmartBotParams botParams = PickParamsVariant();
        BotInputAI ai = new BotInputAI(bot.transform, origin.StartWaypoint, botParams);
        driver.SetInput(ai);
        
        BotAIController aiCtl = bot.GetComponent<BotAIController>() ?? bot.AddComponent<BotAIController>();
        aiCtl.SetAI(ai);

        if (bot.TryGetComponent(out BotRespawn respawn))
        {
            respawn.Initialize(ai, origin.StartWaypoint);
            
            if (origin.Checkpoint != null)
                respawn.SetCheckpoint(origin.Checkpoint);
        }
        
        if (_progressBarView != null && _racePath != null)
        {
            if (bot.TryGetComponent(out BotProgress progress))
            {
                BotProgressTracker tracker = new BotProgressTracker(progress, _progressBarView, bot, _startPoint.transform.position, _finPoint.position, _racePath);
                
                progressTrackers.Add(tracker);
                trackerByBot[bot] = tracker;
            }
        }
        
        float waited = 0f, timeout = 5f;
        
        while (Vector3.Distance(bot.transform.position, spawnPosition) < _leaveDistance && waited < timeout)
        {
            yield return null;
            
            waited += Time.deltaTime;
        }
        
        if (Vector3.Distance(bot.transform.position, spawnPosition) < _leaveDistance)
        {
            Vector3 forward = (spawnRotation * Vector3.forward);
            driver.ApplyPush(forward * 2.5f, 0.25f);
        }
    }
}