using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Zenject;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    private const string ESCAPE_KEY = "escape";
    private const string BEST_TIME = "BestTime";
    
    [Header("Spawn")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _spawnBackOffset = 2f;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _timer;
    [SerializeField] private TextMeshProUGUI _timeNowText;
    [SerializeField] private TextMeshProUGUI _bestTimeText;
    [SerializeField] private Pause _pause;

    private readonly Dictionary<int, CheckPoints> numberToCheckpoint = new Dictionary<int, CheckPoints>();
    private readonly HashSet<int> allCheckpointNumbers = new HashSet<int>(); 
    private readonly HashSet<int> visitedNumbers = new HashSet<int>();
    
    private ProgressBarView _progressBarView;
    private RacePath _racePath;
    private Player _player;
    private PlayerProgressTracker _playerTracker;
    
    private Transform _lastCheckpointTransform;
    private float _time;
    private bool _didManualTeleportThisFrame;
    
    [Inject] private SkinSaver _skinSaver;
    [Inject] private NameAssigner _nameAssigner;
    [Inject] private LevelDirector _levelDirector;

    [Inject]
    public void Construct(Player player, IInput input, ProgressBarView progressBarView)
    {
        _player = player;
        _progressBarView = progressBarView;
    }

    public Player Player => _player;

    private void Awake()
    {
        Application.targetFrameRate = (Mathf.Max(60, Convert.ToInt32(Screen.currentResolution.refreshRateRatio.value)));
        QualitySettings.vSyncCount = 0;
    }
    
    private void OnEnable()
    {
        CheckPoints.Reached += OnCheckpointReached;
        
        if (_levelDirector != null)
            _levelDirector.ActiveLevelChanged += OnLevelChanged;
    }

    private void OnDisable()
    {
        CheckPoints.Reached -= OnCheckpointReached;
        
        if (_levelDirector != null)
            _levelDirector.ActiveLevelChanged -= OnLevelChanged;
    }

    private void Start()
    {
        RebuildLevelData();

        _pause.gameObject.SetActive(false);
        _pause.Resume();

        SetPosition();

        _nameAssigner.AssignToPlayer(_player);
        _progressBarView?.InitializePlayer();

        if (_player?.PlayerCharacterRoot != null && _progressBarView != null && _racePath != null && _racePath.IsValid)
        {
            _playerTracker = new PlayerProgressTracker(_player.PlayerCharacterRoot.transform, _progressBarView, _racePath);
            _playerTracker.Tick();
        }

        UpdateProgressBar();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(ESCAPE_KEY))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        
        _time = (float)Time.timeSinceLevelLoad;
        _timer.text = (Math.Round(_time, 2) + "");
        
        _playerTracker?.Tick();
        
        if (Advertising.IsReclama)
        {
            Advertising.IsReclama = false;
            
            _pause.gameObject.SetActive(true);
            _pause.TryUsePause();
            
            if (!Application.isMobilePlatform)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }
    
    private void LateUpdate()
    {
        _didManualTeleportThisFrame = false;
    }
    
    public void SetPosition()
    {
        GameObject player = _player.PlayerCharacterRoot.gameObject;
        
        if (player.TryGetComponent(out BikeRiderDetacher detacher))
            detacher.Reattach();

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        if (_lastCheckpointTransform != null)
        {
            Vector3 forward = _lastCheckpointTransform.forward.sqrMagnitude > 0.0001f ? _lastCheckpointTransform.forward.normalized : Vector3.forward;
            spawnPosition = _lastCheckpointTransform.position - forward * _spawnBackOffset;
            spawnRotation = Quaternion.LookRotation(forward, Vector3.up);
        }
        else
        {
            if (_levelDirector != null && _levelDirector.TryGetLevelStart(_levelDirector.ActiveLevelIndex, out Waypoint startWaypoint) && startWaypoint != null)
            {
                Transform waypointTransform = startWaypoint.transform;
                Vector3 forward = waypointTransform.forward;
                
                if (startWaypoint.NextWaypoints != null)
                {
                    foreach (Waypoint nextWaypoint in startWaypoint.NextWaypoints)
                    {
                        if (nextWaypoint == null)
                            continue;
                        
                        Vector3 direction = nextWaypoint.transform.position - waypointTransform.position; direction.y = 0f;

                        if (direction.sqrMagnitude > 1e-3f)
                        {
                            forward = direction.normalized;
                            
                            break;
                        }
                    }
                }

                spawnPosition = waypointTransform.position - forward * Mathf.Max(0f, _spawnBackOffset);
                spawnRotation = Quaternion.LookRotation(forward, Vector3.up);
            }
            else
            {
                if (_spawnPoint != null)
                {
                    spawnPosition = _spawnPoint.position;
                    spawnRotation = _spawnPoint.rotation;
                }
                else
                {
                    spawnPosition = Vector3.zero;
                    spawnRotation = Quaternion.identity;
                }
            }
        }

        player.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        player.SetActive(true);

        if (player.TryGetComponent(out Rigidbody playerRigidbody))
        {
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }
    
    public void Finish()
    {
        float bestTime = PlayerPrefs.GetFloat(BEST_TIME, float.MaxValue);
        
        if (_time < bestTime)
        {
            PlayerPrefs.SetFloat(BEST_TIME, _time);
            
            bestTime = _time;
        }
        
        _timeNowText.text = "Время: " + Math.Round(_time, 2);
        _bestTimeText.text = "Лучшее время: " + Math.Round(bestTime, 2);
        
        PlayerSessionProgress.Reset();
    }
    
    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void UpdateProgressBar()
    {
        if (_progressBarView == null || _racePath == null || !_racePath.IsValid || allCheckpointNumbers.Count == 0)
            return;
        
        HashSet<int> collectedSet = new HashSet<int>(visitedNumbers);
        
        if (PlayerSessionProgress.CollectedCheckpoints != null)
            foreach (int number in PlayerSessionProgress.CollectedCheckpoints)
                collectedSet.Add(number);
        
        int collectedCount = 0;
        
        foreach (int number in allCheckpointNumbers)
            if (collectedSet.Contains(number))
                collectedCount++;
        
        float fractionByPath = 0f;
        
        foreach (int number in collectedSet)
        {
            if (numberToCheckpoint.TryGetValue(number, out CheckPoints checkPoint) && checkPoint != null)
            {
                float progress = _racePath.ComputeProgress(checkPoint.transform.position);
                
                if (progress > fractionByPath)
                    fractionByPath = progress;
            }
        }
        
        if (fractionByPath <= 0f && _levelDirector != null && _levelDirector.TryGetLevelStart(_levelDirector.ActiveLevelIndex, out Waypoint startWp) &&
            startWp != null)
        {
            fractionByPath = _racePath.ComputeProgress(startWp.transform.position);
        }

        _progressBarView.UpdateCheckpointFill(fractionByPath);
    }
    
    private void OnCheckpointReached(CheckPoints checkPoints)
    {
        if (checkPoints != null)
        {
            visitedNumbers.Add(checkPoints.Number);
            _lastCheckpointTransform = checkPoints.transform;
            
            if (checkPoints.IsLevelEnd && _levelDirector != null)
            {
                if (_levelDirector.TryGetNextLevelStartFrom(checkPoints, out int nextIndex, out Waypoint nextStart))
                {
                    _levelDirector.GoToLevel(nextIndex);
                    visitedNumbers.Clear();

                    PlayerSessionProgress.CollectedCheckpoints.Clear();
                    PlayerSessionProgress.LastCheckpointNum = -1;
                    _lastCheckpointTransform = null;

                    RebuildLevelData();
                    TeleportPlayerBeforeCheckpoint(nextStart, _spawnBackOffset);
                    _didManualTeleportThisFrame = true;

                    _playerTracker = null;
                    if (_player?.PlayerCharacterRoot != null && _progressBarView != null && _racePath != null && _racePath.IsValid)
                        _playerTracker = new PlayerProgressTracker(_player.PlayerCharacterRoot.transform, _progressBarView, _racePath);

                    UpdateProgressBar();
                    
                    return;
                }
                else
                {
                    Finish();
                    
                    return;
                }
            }
        }
        
        UpdateProgressBar();
    }
    
    private void OnLevelChanged(int oldIdx, int newIdx)
    {
        RebuildLevelData();
        
        if (!_didManualTeleportThisFrame)
            SetPosition();
        
        _playerTracker = null;
        
        if (_player?.PlayerCharacterRoot != null && _progressBarView != null && _racePath != null && _racePath.IsValid)
            _playerTracker = new PlayerProgressTracker(_player.PlayerCharacterRoot.transform, _progressBarView, _racePath);

        UpdateProgressBar();
    }
    
    private void RebuildLevelData()
    {
        if (_levelDirector == null)
        {
            _racePath = new RacePath(Array.Empty<CheckPoints>());
            _lastCheckpointTransform = null;
            
            allCheckpointNumbers.Clear();
            visitedNumbers.Clear();
            
            return;
        }
        
        _racePath = _levelDirector.GlobalPath ?? new RacePath(Array.Empty<CheckPoints>());
        
        allCheckpointNumbers.Clear();
        numberToCheckpoint.Clear();
        
        IReadOnlyList<CheckPoints> allCheckpoints = _levelDirector.AllCheckpoints;
        
        if (allCheckpoints != null)
        {
            for (int i = 0; i < allCheckpoints.Count; i++)
            {
                CheckPoints checkpoint = allCheckpoints[i];
                
                if (checkpoint != null)
                {
                    allCheckpointNumbers.Add(checkpoint.Number);
                    numberToCheckpoint[checkpoint.Number] = checkpoint;
                }
            }
        }

        _lastCheckpointTransform = null;
        
        if (PlayerSessionProgress.LastCheckpointNum != -1 && allCheckpoints != null)
        {
            for (int i = 0; i < allCheckpoints.Count; i++)
            {
                CheckPoints checkpoint = allCheckpoints[i];
                
                if (checkpoint != null && checkpoint.Number == PlayerSessionProgress.LastCheckpointNum)
                {
                    _lastCheckpointTransform = checkpoint.transform;
                    
                    break;
                }
            }
        }
        
        visitedNumbers.Clear();
        
        if (PlayerSessionProgress.CollectedCheckpoints != null)
            foreach (int number in PlayerSessionProgress.CollectedCheckpoints)
                visitedNumbers.Add(number);
    }
    
    private void TeleportPlayerBeforeCheckpoint(Waypoint waypoints, float backOffset)
    {
        if (waypoints == null || _player == null)
            return;
        
        Transform waypointTransform = waypoints.transform;
        Vector3 forward = waypointTransform.forward;
        
        if (waypoints.NextWaypoints != null)
        {
            foreach (Waypoint waypoint in waypoints.NextWaypoints)
            {
                if (waypoint == null)
                    continue;
                
                Vector3 direction = waypoint.transform.position - waypointTransform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 1e-3f)
                {
                    forward = direction.normalized;
                    
                    break;
                }
            }
        }

        Vector3 position = waypointTransform.position - forward * Mathf.Max(0f, backOffset);
        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

        PlayerCharacterRoot player = _player.PlayerCharacterRoot;
        player.transform.SetPositionAndRotation(position, rotation);

        if (player.TryGetComponent(out Rigidbody playerRigidbody))
        {
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }
}