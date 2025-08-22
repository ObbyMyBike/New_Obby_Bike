using System;
using UnityEngine;

public interface IInput
{
    public Vector2 InputDirection { get; }
    
    public event Action Jumped;
    public event Action Pushed;
}