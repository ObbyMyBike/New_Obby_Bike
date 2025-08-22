using UnityEngine;
using Zenject;

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private PlayerCharacterRoot _playerCharacterRoot;
    
    private CameraControl _cameraControl;
    private BoostTimerUI _boostTimerUI;
    
    [Inject]
    public void Construct(CameraControl cameraControl, BoostTimerUI boostTimerUI, Camera playerCamera)
    {
        _cameraControl = cameraControl;
        _boostTimerUI = boostTimerUI;
        
        _boostTimerUI.SetPlayerController(_playerCharacterRoot);
        
        if (playerCamera != null)
            _playerCharacterRoot.SetCamera(playerCamera.transform);
    }
    
    public PlayerCharacterRoot PlayerCharacterRoot => _playerCharacterRoot;
    
    public void SetInput(IInput input) => _playerCharacterRoot.SetInput(input);

    public void Activate() => _playerCharacterRoot.gameObject.SetActive(true);

    public void SetUIController(IUIController uiController)
    {
        _cameraControl.SetUIController(uiController);
        _cameraControl.gameObject.SetActive(true);
    }
}