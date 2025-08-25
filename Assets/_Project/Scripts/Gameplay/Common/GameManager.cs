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
    
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private GameObject[] _checkPoints;
    [SerializeField] private TextMeshProUGUI _timer;
    [SerializeField] private TextMeshProUGUI _timeNowText;
    [SerializeField] private TextMeshProUGUI _bestTimeText;
    [SerializeField] private Pause _pause;
    [SerializeField] private float _spawnBackOffset = 2f;

    private ProgressBarView _progressBarView;
    private RacePath _racePath;
    private Player _player;
    private PlayerProgressTracker _playerTracker;
    
    private Transform _lastCheckpointTransform;
    private Dictionary<int, Transform> _checkpointByNumber;
    private float _time;
    
    [Inject] private SkinSaver _skinSaver;
    [Inject] private NameAssigner _nameAssigner;

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
    }

    private void OnDisable()
    {
        CheckPoints.Reached -= OnCheckpointReached;
    }

    private void Start()
    {
        _checkpointByNumber = new Dictionary<int, Transform>(_checkPoints.Length);
        
        for (int i = 0; i < _checkPoints.Length; i++)
        {
            GameObject checkPoint = _checkPoints[i];
            
            if (checkPoint == null)
                continue;

            if (checkPoint.TryGetComponent(out CheckPoints points))
                if (!_checkpointByNumber.ContainsKey(points.Number))
                    _checkpointByNumber[points.Number] = points.transform;
        }
        
        _lastCheckpointTransform = null;
        
        if (PlayerSessionProgress.LastCheckpointNum != -1)
            if (_checkpointByNumber.TryGetValue(PlayerSessionProgress.LastCheckpointNum, out Transform t))
                _lastCheckpointTransform = t;
        
        _pause.gameObject.SetActive(false);
        _pause.Resume();
        
        List<CheckPoints> checkPoints = new List<CheckPoints>(_checkPoints.Length);
        
        for (int i = 0; i < _checkPoints.Length; i++)
        {
            GameObject checkPoint = _checkPoints[i];
            
            if (checkPoint != null && checkPoint.TryGetComponent(out CheckPoints cp))
                checkPoints.Add(cp);
        }
        
        _racePath = new RacePath(checkPoints.ToArray());
        
        SetPosition();
        
        _nameAssigner.AssignToPlayer(_player);
        
        if (_progressBarView != null)
            _progressBarView.InitializePlayer();
        
        if (_player != null && _player.PlayerCharacterRoot != null && _progressBarView != null && _racePath != null && _racePath.IsValid)
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
    
    public void SetPosition()
    {
        GameObject player = _player.PlayerCharacterRoot.gameObject;
        
        if (player.TryGetComponent(out BikeRiderDetacher detacher))
            detacher.Reattach();

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        Transform lastCheckpointTransform = null;
        
        if (_lastCheckpointTransform != null)
            lastCheckpointTransform = _lastCheckpointTransform;
        else if (PlayerSessionProgress.LastCheckpointNum != -1 && _checkpointByNumber != null && _checkpointByNumber.TryGetValue(PlayerSessionProgress.LastCheckpointNum, out Transform checkpointTransform))
            lastCheckpointTransform = checkpointTransform;

        if (lastCheckpointTransform == null)
        {
            if (_spawnPoint != null)
            {
                spawnPosition = _spawnPoint.position;
                spawnRotation = _spawnPoint.rotation;
            }
            else if (_checkPoints.Length > 0)
            {
                Transform first = _checkPoints[0].transform;
                Vector3 forward = first.forward.sqrMagnitude > 0.0001f ? first.forward.normalized : Vector3.forward;
                
                spawnPosition = first.position - forward * _spawnBackOffset;
                spawnRotation = Quaternion.LookRotation(forward, Vector3.up);
            }
            else
            {
                spawnPosition = Vector3.zero;
                spawnRotation = Quaternion.identity;
            }
        }
        else
        {
            Vector3 forward = lastCheckpointTransform.forward.sqrMagnitude > 0.0001f ? lastCheckpointTransform.forward.normalized : Vector3.forward;
            
            spawnPosition = lastCheckpointTransform.position - forward * _spawnBackOffset;
            spawnRotation = Quaternion.LookRotation(forward, Vector3.up);
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
    
    private void OnCheckpointReached(CheckPoints checkPoints)
    {
        _lastCheckpointTransform = checkPoints != null ? checkPoints.transform : null;
        
        UpdateProgressBar();
    }
    
    private void UpdateProgressBar()
    {
        if (_progressBarView == null || _racePath == null || !_racePath.IsValid || _player == null || _player.PlayerCharacterRoot == null)
            return;
        
        _progressBarView.AnimatePlayerProgress(_racePath.ComputeProgress(_player.PlayerCharacterRoot.transform.position) * 100f);
    }
}