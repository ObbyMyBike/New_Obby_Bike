using UnityEngine;
using Zenject;

public class Finish : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private BotSpawn _botSpawn;
    [SerializeField] private CheckPoints _finishCheckpoint;

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
            
            if (_finishPanel != null)
                _finishPanel.SetActive(true);
            
            if (!Application.isMobilePlatform)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
            
            if (_finishCheckpoint != null)
                _finishCheckpoint.StartFinishLoopEffect();
            
            _player.SetInput(new DisabledInput());
            
            if (_cameraControl != null)
                _cameraControl.StartAutoOrbit(_player.PlayerCharacterRoot.transform);
            
            return;
        }
        
        if (other.TryGetComponent(out BotController bot))
            _botSpawn?.DespawnAndRespawn(bot);
    }
}