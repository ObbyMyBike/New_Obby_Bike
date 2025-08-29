using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class FinishRestart : MonoBehaviour
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private float _autoNextDelay = 10f; 
    
    private GameManager _gameManager;
    private CheckPoints _finishCheckpoint;
    private Coroutine _autoNextRoutine;
    
    [Inject(Optional = true)] private CameraControl _cameraControl;
    [Inject(Optional = true)] private Player _player;
    [Inject(Optional = true)] private IInput _input;

    [Inject]
    public void Construct(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    private void OnEnable()
    {
        _restartButton?.onClick.AddListener(RestartLevel);
        _nextLevelButton?.onClick.AddListener(GoNextLevel);
        
        if (_finishPanel != null && _finishPanel.activeInHierarchy)
            StartAutoNext();
    }

    private void OnDisable()
    {
        _restartButton?.onClick.RemoveListener(RestartLevel);
        _nextLevelButton?.onClick.RemoveListener(GoNextLevel);
        
        StopAutoNext();
    }
    
    public void OnLevelFinished(CheckPoints finishCheckpoint)
    {
        _finishCheckpoint = finishCheckpoint;
        
        StartAutoNext();
    }

    private void StartAutoNext()
    {
        StopAutoNext();
        
        _autoNextRoutine = StartCoroutine(AutoNextRoutine());
    }

    private void StopAutoNext()
    {
        if (_autoNextRoutine != null)
        {
            StopCoroutine(_autoNextRoutine);
            
            _autoNextRoutine = null;
        }
    }
    
    private void GoNextLevel()
    {
        StopAutoNext();

        _finishPanel?.SetActive(false);
        _cameraControl?.StopAutoOrbit();
        
        if (_player != null && _input != null)
            _player.SetInput(_input);
        
        bool moved = _gameManager.AdvanceToNextLevelFrom(_finishCheckpoint);
        
        _cameraControl?.EnableManualControlAfterLevelChange();
        
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        CameraControl.IsPause = false;
        Time.timeScale = 1f;
    }

    private void RestartLevel()
    {
        StopAutoNext();

        _finishPanel?.SetActive(false);
        _cameraControl?.StopAutoOrbit();

        if (!Application.isMobilePlatform)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        Time.timeScale = 1f;
        CameraControl.IsPause = false;

        _gameManager.Reload();
    }
    
    private IEnumerator AutoNextRoutine()
    {
        yield return new WaitForSecondsRealtime(_autoNextDelay);
        
        GoNextLevel();
    }
}