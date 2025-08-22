using UnityEngine;

public struct BotMoveIntent
{
    private bool _isJumpRequested;
    
    public Vector3 MoveDirectionWorld { get; private set; }

    public void UpdateFromInput(IInput input)
    {
        if (input == null)
        {
            MoveDirectionWorld = Vector3.zero;
            
            return;
        }

        Vector2 raw = input.InputDirection;
        Vector3 direction = new Vector3(raw.x, 0f, raw.y);
        float magnitude = direction.magnitude;
        
        MoveDirectionWorld = magnitude > 1f ? direction / magnitude : direction;
    }

    public void RequestJump() => _isJumpRequested = true;

    public bool ConsumeJumpRequest()
    {
        bool was = _isJumpRequested;
        _isJumpRequested = false;
        
        return was;
    }
}