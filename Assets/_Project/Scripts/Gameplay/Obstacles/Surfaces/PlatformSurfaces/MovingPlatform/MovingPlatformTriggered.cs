using UnityEngine;

[RequireComponent(typeof(MovingPlatformMover))]
public class MovingPlatformTriggered : MonoBehaviour
{
    [SerializeField] private ActivationMode _mode = ActivationMode.Triggered;
    [SerializeField] private MovingPlatformMover _mover;
    
    private bool _activeByTrigger;
    
    private void Awake()
    {
        _mover = GetComponent<MovingPlatformMover>();
        
        ApplyActiveState();
    }
    
    private void OnEnable()
    {
        ApplyActiveState();
    }

    public void SetPlayerInside(bool inside)
    {
        if (_mode == ActivationMode.Always)
            return;

        _activeByTrigger = inside;
        
        ApplyActiveState();
    }
    
    private void ApplyActiveState()
    {
        if (_mover == null) 
            return;
        
        bool shouldMove = (_mode == ActivationMode.Always) || _activeByTrigger;
        
        if (_mover.enabled != shouldMove)
            _mover.enabled = shouldMove;
    }
    
    private void Reset()
    {
        if (_mover == null)
            _mover = GetComponent<MovingPlatformMover>();
    }
}