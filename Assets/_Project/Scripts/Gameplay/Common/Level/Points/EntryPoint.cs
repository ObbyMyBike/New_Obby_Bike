using UnityEngine;
using Zenject;

public class EntryPoint : MonoBehaviour
{
    private IPlayer _player;
    private IInput _input;
    private IUIController _uiController;

    [Inject]
    public void Construct(IPlayer player, IInput input, IUIController uiController)
    {
        _player = player;
        _input = input;
        _uiController = uiController;
    }

    private void Start()
    {
        _player.SetInput(_input);
        _player.SetUIController(_uiController);
        _player.Activate();
    }
}