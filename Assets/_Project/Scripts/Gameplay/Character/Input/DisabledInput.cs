using System;
using UnityEngine;

public class DisabledInput : IInput
{
    public event Action Jumped { add {} remove {} }
    public event Action Pushed { add {} remove {} }
    
    public Vector2 InputDirection => Vector2.zero;
}