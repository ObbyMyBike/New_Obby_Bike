using System;
using UnityEngine;

public class UIController : MonoBehaviour, IUIController
{
    [SerializeField] private Pause _pauseWindow;

    public event Action PauseRequested;
    public event Action ResumeRequested;

    private void OnEnable()
    {
        _pauseWindow.PauseStateSwitched += OnPauseStateSwitched;
    }

    private void OnDisable()
    {
        _pauseWindow.PauseStateSwitched -= OnPauseStateSwitched;
    }

    private void OnPauseStateSwitched(bool isPaused)
    {
        if (isPaused)
            PauseRequested?.Invoke();
        else
            ResumeRequested?.Invoke();
    }
}