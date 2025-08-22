using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BotSpawn : MonoBehaviour
{
    private const int MAX_ATTEMPTS = 24;
    
    [Header("Bot Settings")]
    [SerializeField] private SmartBotParams[] _botPresets;
    [SerializeField] private GameObject _botPrefab;
    [SerializeField, Range(0f, 1f)] private float _trailChance = 0.5f;
    
    [Header("Spawn settings")]
    [SerializeField] private CheckPoints[] _allCheckpoints;
    [SerializeField] private CheckPoints[] _extraSpawnCheckpoints;
    [SerializeField] private Waypoint _startPoint;
    [SerializeField] private Transform _finPoint;
    [SerializeField] private LayerMask _occupancyMask;
    [SerializeField] private int _botCount = 5;
    [SerializeField] private float _spawnCheckRadius = 1f;
    
    [Header("Spawn layout")]
    [SerializeField, Min(0f)] private float _spawnBack = 2.0f;
    [SerializeField, Min(0f)] private float _laneSpacing = 1.2f;
    [SerializeField] private int _laneSpread = 2;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float _initialSpawnDelay = 0.3f;
    [SerializeField, Min(0f)] private float _spawnInterval = 0.6f;
    [SerializeField, Min(0f)] private float _retryInterval = 0.5f;
    [SerializeField, Min(0f)] private float _leaveDistance = 1.2f;
    [SerializeField] private bool _sequentialSpawn = true;
    
    [Header("Progress Bar settings")]
    [SerializeField] private ProgressBarView _progressBarView;
    
    private OriginsBuilder _originsBuilder;
    private CheckingAccessPlace _clearance;
    private LanePicker _lanePicker;
    private BotFactory _botFactory;
    private ProgressBotTracking _progress;
    private SpawnRandom _spawnRandom;
    private RacePath _racePath;
    
    private List<SpawnOrigin> _origins;
    private ObjectPool<BotDriver> _botPool;
    
    [Inject] private NameAssigner _nameAssigner;
    [Inject] private DiContainer _container;

    private void Start()
    {
        if (_botPrefab == null || _startPoint == null || _finPoint == null || _progressBarView == null)
            return;
        
        _originsBuilder = new OriginsBuilder(_spawnBack);
        _clearance = new CheckingAccessPlace(_occupancyMask, _spawnCheckRadius);
        _lanePicker = new LanePicker(_laneSpacing, _laneSpread);
        _progress = new ProgressBotTracking(_progressBarView, _startPoint, _finPoint);
        _origins = _originsBuilder.Build(_startPoint, _extraSpawnCheckpoints);
        
        if (_origins.Count == 0)
            return;

        _racePath = new RacePath(_allCheckpoints);

        BotDriver driverOnPrefab = _botPrefab.GetComponentInChildren<BotDriver>(true);
        _botPool = new ObjectPool<BotDriver>(driverOnPrefab, _botCount, PoolContainer.Root, factory: () =>
        {
            GameObject botObject = _container.InstantiatePrefab(_botPrefab, PoolContainer.Root);
            
            return botObject.GetComponentInChildren<BotDriver>(true);
        });

        _botFactory = new BotFactory(_container, _nameAssigner, _botPool, _botPresets, _trailChance);
        _spawnRandom = new SpawnRandom(MAX_ATTEMPTS, _retryInterval, _leaveDistance, _origins, _lanePicker, _clearance,
            _botFactory, _progress, _racePath, this);
        
        if (_sequentialSpawn)
            StartCoroutine(SpawnSequential(_botCount));
        else
            StartCoroutine(SpawnInitialRandom());
    }

    private void Update()
    {
        _progress.Tick();
    }

    public void DespawnAndRespawn(BotDriver bot)
    {
        if (bot == null)
            return;

        _progress.Forget(bot.gameObject);

        if (bot.TryGetComponent(out Rigidbody botRigidbody))
        {
            botRigidbody.velocity = Vector3.zero;
            botRigidbody.angularVelocity = Vector3.zero;
        }

        _botPool.Release(bot);
        
        StartCoroutine(_spawnRandom.SpawnOne());
    }

    private IEnumerator SpawnInitialRandom()
    {
        for (int i = 0; i < _botCount; i++)
            yield return StartCoroutine(_spawnRandom.SpawnOne());
    }

    private IEnumerator SpawnSequential(int count)
    {
        yield return new WaitForSeconds(_initialSpawnDelay);

        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(_spawnRandom.SpawnOne());
            yield return new WaitForSeconds(_spawnInterval);
        }
    }
}