using System;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public event Action<bool> PauseStateSwitched;
    
    [SerializeField] private GameObject _cursore;
    
    private bool _isActive;
    
    private void OnEnable()
    {
        if (_cursore != null)
            _cursore.SetActive(false);
    }

    public void TryUsePause() => Show();
    
    public void Resume() => Hide();
    
    private void Show()
    {
        if (_isActive)
            return;
        
        _isActive = true;
        
        PauseStateSwitched?.Invoke(true);

        if (!Application.isMobilePlatform)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
    
    private void Hide()
    {
        if (!_isActive)
            return;
        
        _isActive = false;

        PauseStateSwitched?.Invoke(false);

        if (!Application.isMobilePlatform)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (_cursore != null)
            _cursore.SetActive(true);
    }
}