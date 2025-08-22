public interface IPlayer
{
    public void Activate();
    
    public void SetInput(IInput input);
    
    public void SetUIController(IUIController uiController);
}
