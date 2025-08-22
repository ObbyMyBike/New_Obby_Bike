using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Restart : MonoBehaviour
{
    [SerializeField] private Pause _pause;
    [SerializeField] private GameObject _cursor;
    [SerializeField] private Button _pauseButton;
    
    private GameManager _gameManager;
    
    [Inject]
    public void Construct(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
    
    private void OnEnable()
    {
        _cursor.SetActive(false);
        
        if (_pauseButton != null)
            _pauseButton.gameObject.SetActive(false);
    }
    
    public void TryRestart()
    {
        GameObject player = _gameManager.Player.PlayerCharacterRoot.gameObject;
        
        if (player.TryGetComponent(out BoostTarget boostTarget))
            boostTarget.BoostArc(Vector3.zero);
        
        gameObject.SetActive(false);
        _gameManager.SetPosition();
        
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        Time.timeScale = 1;
        CameraControl.IsPause = false;
        
        if (_cursor != null)
            _cursor.SetActive(true);
        
        if (_pauseButton != null)
            _pauseButton.gameObject.SetActive(true);
    }
}