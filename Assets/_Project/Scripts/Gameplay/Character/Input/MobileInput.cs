using System;
using UnityEngine;
using UnityEngine.UI;

public class MobileInput : MonoBehaviour, IInput
{
    public event Action Jumped;
    public event Action Pushed;
    
    [SerializeField] private DynamicJoystick _dynamicJoystick;
    [SerializeField] private Button _jumpingButton;
    [SerializeField] private Button _pushButton;

    private Vector2 _inputDirection;
    
    public Vector2 InputDirection => _inputDirection;
    private bool IsMobile => Application.isMobilePlatform;

    private void Awake()
    {
        if (!IsMobile)
        {
            gameObject.SetActive(false);
            
            return;
        }
        
        ToggleMobileControls(true);
    }
    
    private void OnEnable()
    {
        if (!IsMobile)
            return;
        
        _jumpingButton.onClick.AddListener(OnJumpClick);
        _pushButton.onClick.AddListener(OnPushClick);
    }

    private void OnDisable()
    {
        if (!IsMobile)
            return;
        
        _jumpingButton.onClick.RemoveListener(OnJumpClick);
        _pushButton.onClick.RemoveListener(OnPushClick);
    }

    private void Update()
    {
        _inputDirection = _dynamicJoystick.Direction;
    }

    private void OnJumpClick() => Jumped?.Invoke();
    
    private void OnPushClick() => Pushed?.Invoke();

    public void Activate()
    { 
        if (IsMobile)
        {
            gameObject.SetActive(true);
            
            ToggleMobileControls(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    private void ToggleMobileControls(bool visible)
    {
        _dynamicJoystick?.gameObject.SetActive(visible);
        _jumpingButton?.gameObject.SetActive(visible);
        _pushButton?.gameObject.SetActive(visible);
    }
}