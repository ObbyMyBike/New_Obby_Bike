using System;
using UnityEngine;

public class DisabledInput : IInput
{
    public event Action Jumped;
    public event Action Pushed;
    
    public Vector2 InputDirection => Vector2.zero;
}