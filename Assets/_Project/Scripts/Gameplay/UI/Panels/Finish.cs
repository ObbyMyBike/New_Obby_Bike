using UnityEngine;
using Zenject;

public class Finish : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private BotSpawn _botSpawn;
    [SerializeField] private CheckPoints _finishCheckpoint;
    [SerializeField] private FinishRestart _finishController;

    [Inject] private Player _player;
    [Inject] private CameraControl _cameraControl;
    
    private void Awake()
    {
        if (_finishCheckpoint == null)
            _finishCheckpoint = GetComponent<CheckPoints>() ?? GetComponentInParent<CheckPoints>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacterRoot _))
        {
            _gameManager.Finish();
            
            _finishPanel?.SetActive(true);
            
            if (!Application.isMobilePlatform)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
            
            _finishCheckpoint?.StartFinishLoopEffect();
            _player.SetInput(new DisabledInput());
            _cameraControl?.StartAutoOrbit(_player.PlayerCharacterRoot.transform);
            _finishController?.OnLevelFinished(_finishCheckpoint);
            
            return;
        }
        
        if (other.TryGetComponent(out BotController bot))
            _botSpawn?.TryDespawnAndRespawn(bot);
    }
}