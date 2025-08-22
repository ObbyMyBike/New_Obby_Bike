using System;

public interface IUIController
{
    public event Action PauseRequested;
    public event Action ResumeRequested;
}