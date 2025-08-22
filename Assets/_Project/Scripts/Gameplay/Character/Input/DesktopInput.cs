using System;
using UnityEngine;
using Zenject;

public class DesktopInput : IInput, ITickable
{
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    
    public Vector2 InputDirection => new Vector2(Input.GetAxis(HORIZONTAL), Input.GetAxis(VERTICAL));

    public event Action Jumped;

    public event Action Pushed;

    public void Tick()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Jumped?.Invoke();
        
        if (Input.GetKeyDown(KeyCode.M))
            Pushed?.Invoke();
    }
}