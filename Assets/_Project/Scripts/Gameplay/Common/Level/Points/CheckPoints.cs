using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckPoints : MonoBehaviour
{
    public static event Action<CheckPoints> Reached;

    [SerializeField] private int _number;
    [SerializeField] private Waypoint _associatedWaypoint;
    [SerializeField] private bool _isLevelEnd = false;

    [Header("Flag Animation Settings")]
    [SerializeField] private Renderer _flagInnerRenderer;
    [SerializeField] private float _flagRotateSpeed = 180f;
    [SerializeField] private Color _flagEndColor = Color.green;

    [Header("Effect Pool Settings")]
    [SerializeField] private Transform _effectsContainer;
    [SerializeField] private Transform[] _effectSpawnPoints;
    [SerializeField] private ParticleSystem _effectPrefab;
    [SerializeField] private int _initialPoolSize = 3;
    
    [Header("Occupancy Check")]
    [SerializeField] private float _occupancyCheckRadius = 1f;
    [SerializeField] private LayerMask _occupancyMask;
    
    private readonly List<ParticleSystem> finishLoopEffect = new List<ParticleSystem>();
    
    private FlagAnimation _flagAnimation;
    private CheckpointOccupancyChecker _checkpointOccupancyChecker;
    private ObjectPool<ParticleSystem> _effectPool;

    private bool _playerCollected;
    private bool _finishLoopStarted;
    
    public Waypoint AssociatedWaypoint => _associatedWaypoint;
    public int Number => _number;
    public bool IsLevelEnd => _isLevelEnd;
    
    private void Awake()
    {
        _flagAnimation = new FlagAnimation(transform, _flagInnerRenderer, _flagRotateSpeed, _flagEndColor);
        _effectPool = new ObjectPool<ParticleSystem>(_effectPrefab, _initialPoolSize, _effectsContainer);
        _checkpointOccupancyChecker = new CheckpointOccupancyChecker(_occupancyMask, _occupancyCheckRadius);
    }

    private void Start()
    {
        _playerCollected = PlayerSessionProgress.CollectedCheckpoints.Contains(_number);
    }
    
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent(out PlayerCharacterRoot player))
        {
            if (!_playerCollected)
            {
                PlayerSessionProgress.CollectedCheckpoints.Add(_number);
                PlayerSessionProgress.Point++;
                PlayerSessionProgress.LastCheckpointNum = _number;

                _playerCollected = true;
                
                StartCoroutine(_flagAnimation.Animate());
                
                PlayEffects();

                Reached?.Invoke(this);
            }
        }
    }
    
    private void OnValidate()
    {
        if (_checkpointOccupancyChecker == null)
            _checkpointOccupancyChecker = new CheckpointOccupancyChecker(_occupancyMask, _occupancyCheckRadius);
        else
            _checkpointOccupancyChecker.UpdateParams(_occupancyMask, _occupancyCheckRadius);
    }
    
    public bool CanSpawnOrRespawnHere() => _checkpointOccupancyChecker.IsClear(transform.position);
    
    public void StartFinishLoopEffect()
    {
        if (_finishLoopStarted || _effectPrefab == null)
            return;
        
        if (_effectSpawnPoints != null && _effectSpawnPoints.Length > 0)
        {
            foreach (Transform point in _effectSpawnPoints)
            {
                if (point == null)
                    continue;
                
                finishLoopEffect.Add(SpawnLoopEffectAt(point.position, point.rotation));
            }
        }
        else
        {
            finishLoopEffect.Add(SpawnLoopEffectAt(transform.position, Quaternion.identity));
        }

        _finishLoopStarted = true;
    }
    
    private ParticleSystem SpawnLoopEffectAt(Vector3 pos, Quaternion rot)
    {
        ParticleSystem effectInstance = Instantiate(_effectPrefab, pos, rot, _effectsContainer != null ? _effectsContainer : transform);
        ParticleSystem[] allEffects = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
        
        foreach (ParticleSystem effect in allEffects)
        {
            ParticleSystem.MainModule module = effect.main;
            module.loop = true;
        }
        
        effectInstance.Play(true);
        
        return effectInstance;
    }
    
    private void PlayEffects()
    {
        if (_effectSpawnPoints != null && _effectSpawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in _effectSpawnPoints)
            {
                if (spawnPoint == null)
                    continue;

                ParticleSystem effect = _effectPool.Get();
                effect.transform.position = spawnPoint.position;
                effect.transform.rotation = spawnPoint.rotation;

                StartCoroutine(PlayAndRelease(effect));
            }
        }
        else
        {
            ParticleSystem effect = _effectPool.Get();
            effect.transform.position = transform.position;
            effect.transform.rotation = Quaternion.identity;

            StartCoroutine(PlayAndRelease(effect));
        }
    }
    
    private IEnumerator PlayAndRelease(ParticleSystem effectSystem)
    {
        CheckPointEffect effect = new CheckPointEffect(effectSystem);
        
        yield return StartCoroutine(effect.Play());
        
        _effectPool.Release(effectSystem);
    }
    
// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, _occupancyCheckRadius);
//     }
// #endif
}