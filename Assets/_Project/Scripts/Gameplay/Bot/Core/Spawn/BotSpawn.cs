using System;
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
    [SerializeField] private ExtraSpawnForLevel[] _extraByLevel;
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
    
    private readonly List<Coroutine> runningSpawnCoroutines = new List<Coroutine>();
    
    private ProgressBarView _progressBarView;
   
    private OriginsBuilder _originsBuilder;
    private CheckingAccessPlace _clearance;
    private LanePicker _lanePicker;
    private BotFactory _botFactory;
    private SpawnRandom _spawnRandom;
    private RacePath _racePath;

    private List<SpawnOrigin> _origins;
    private ObjectPool<BotController> _botPool;

    [Inject] private LevelDirector _levelDirector;
    [Inject] private BotRegistry _botRegistry;
    [Inject] private NameAssigner _nameAssigner;
    [Inject] private DiContainer _container;

    [Inject]
    private void Construct(ProgressBarView progressBarView)
    {
        if (_progressBarView == null)
            _progressBarView = progressBarView;
    }
    
    private void OnEnable()
    {
        if (_levelDirector != null)
            _levelDirector.ActiveLevelChanged += OnLevelChanged;
    }
    
    private void OnDisable()
    {
        if (_levelDirector != null)
            _levelDirector.ActiveLevelChanged -= OnLevelChanged;
    }
    
    private void Start()
    {
        if (_botPrefab == null || _progressBarView == null)
            return;

        _originsBuilder = new OriginsBuilder(_spawnBack);
        _clearance = new CheckingAccessPlace(_occupancyMask, _spawnCheckRadius);
        _lanePicker = new LanePicker(_laneSpacing, _laneSpread);
        
        BotController botController = _botPrefab.GetComponentInChildren<BotController>(true);
        
        if (botController == null)
            return;
        
        _botPool = new ObjectPool<BotController>(botController, _botCount, PoolContainer.Root,  () =>
        {
            GameObject botObject = _container.InstantiatePrefab(_botPrefab, PoolContainer.Root);
            
            return botObject.GetComponentInChildren<BotController>(true);
        });

        _botFactory = new BotFactory(_container, _nameAssigner, _botPool, _botPresets, _trailChance, _botRegistry);
        
        int levelIndex = _levelDirector != null ? _levelDirector.ActiveLevelIndex : 0;
        
        BuildForLevel(levelIndex);
        
        if (_origins != null && _origins.Count > 0)
        {
            if (_sequentialSpawn)
                StartCoroutine(SpawnSequential(_botCount));
            else
                StartCoroutine(SpawnInitialRandom());
        }
    }

    public void TryDespawnAndRespawn(BotController bot)
    {
        if (bot == null)
            return;

        if (bot.TryGetComponent(out Rigidbody botRigidbody))
        {
            botRigidbody.velocity = Vector3.zero;
            botRigidbody.angularVelocity = Vector3.zero;
        }
        
        _progressBarView?.RemoveBotMarker(bot.gameObject);
        _botRegistry?.Unregister(bot);
        _botPool.Release(bot);
        runningSpawnCoroutines?.Add(StartCoroutine(_spawnRandom.SpawnOne()));
    }
    
    private void DespawnAllBots()
    {
        if (_botRegistry == null || _botPool == null)
            return;
        
        List<BotController> copy = new List<BotController>(_botRegistry.All);
        
        foreach (BotController bot in copy)
        {
            if (bot == null)
                continue;
            
            if (bot.TryGetComponent(out Rigidbody botRigidbody))
            {
                botRigidbody.velocity = Vector3.zero;
                botRigidbody.angularVelocity = Vector3.zero;
            }
            
            _progressBarView?.RemoveBotMarker(bot.gameObject);
            _botRegistry.Unregister(bot);
            _botPool.Release(bot);
        }
    }
    
    private void BuildForLevel(int levelIndex)
    {
        Waypoint startWaypoint = null;
        
        _levelDirector?.TryGetLevelStart(levelIndex, out startWaypoint);

        IReadOnlyList<CheckPoints> levelCheckpoints = _levelDirector != null ? _levelDirector.GetLevelCheckpoints(levelIndex)
            : Array.Empty<CheckPoints>();
        
        HashSet<CheckPoints> mergedCheckpoints = new HashSet<CheckPoints>();
        
        if (levelCheckpoints != null)
        {
            foreach (CheckPoints checkpoint in levelCheckpoints)
            {
                if (checkpoint != null)
                    mergedCheckpoints.Add(checkpoint);
            }
        }
        List<CheckPoints> extrasCheckpoints = GetExtrasForLevel(levelIndex);
        
        foreach (CheckPoints extraCheckpoint in extrasCheckpoints)
        {
            if (extraCheckpoint != null)
                mergedCheckpoints.Add(extraCheckpoint);
        }

        CheckPoints[] mergedArray = new CheckPoints[mergedCheckpoints.Count];
        mergedCheckpoints.CopyTo(mergedArray);

        _origins = _originsBuilder.Build(startWaypoint, mergedArray);
        _racePath = _levelDirector != null ? _levelDirector.CurrentPath : null;
        _spawnRandom = new SpawnRandom(MAX_ATTEMPTS, _retryInterval, _leaveDistance, _origins, _lanePicker, _clearance,
            _botFactory, _racePath, _progressBarView, this);
    }
    
    private List<CheckPoints> GetExtrasForLevel(int levelIndex)
    {
        List<CheckPoints> result = new List<CheckPoints>();
        
        if (_extraByLevel == null || _extraByLevel.Length == 0)
            return result;

        foreach (ExtraSpawnForLevel pack in _extraByLevel)
        {
            if (pack.Checkpoints == null)
                continue;
            
            if (pack.LevelIndex == levelIndex)
            {
                foreach (CheckPoints checkpoint in pack.Checkpoints)
                {
                    if (checkpoint != null)
                        result.Add(checkpoint);
                }
            }
        }
        
        return result;
    }
    
    private void OnLevelChanged(int oldIndexLevel, int newIndexLevel)
    {
        StopAllSpawnCoroutines();
        
        _progressBarView?.ClearAllBotMarkers();
        
        DespawnAllBots();
        BuildForLevel(newIndexLevel);
        
        if (_origins != null && _origins.Count > 0)
        {
            if (_sequentialSpawn)
                runningSpawnCoroutines.Add(StartCoroutine(SpawnSequential(_botCount)));
            else
                runningSpawnCoroutines.Add(StartCoroutine(SpawnInitialRandom()));
        }
    }
    
    private void StopAllSpawnCoroutines()
    {
        foreach (Coroutine coroutine in runningSpawnCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        
        runningSpawnCoroutines.Clear();
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