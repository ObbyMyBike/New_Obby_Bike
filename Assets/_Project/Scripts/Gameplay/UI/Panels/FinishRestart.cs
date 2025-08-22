using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class FinishRestart : MonoBehaviour
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private GameObject _finishPanel;

    private GameManager _gameManager;

    [Inject]
    public void Construct(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    private void OnEnable()
    {
        if (_restartButton != null)
            _restartButton.onClick.AddListener(RestartLevel);
    }

    private void OnDisable()
    {
        if (_restartButton != null)
            _restartButton.onClick.RemoveListener(RestartLevel);
    }

    private void RestartLevel()
    {
        if (_finishPanel != null)
            _finishPanel.SetActive(false);
        
        Time.timeScale = 1f;
        CameraControl.IsPause = false;

        _gameManager.Reload();
    }
}