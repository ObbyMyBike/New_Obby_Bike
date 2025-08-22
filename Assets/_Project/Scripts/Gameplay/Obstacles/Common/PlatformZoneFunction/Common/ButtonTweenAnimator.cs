using UnityEngine;
using DG.Tweening;

public sealed class ButtonTweenAnimator
{
    private readonly Transform target;
    private readonly Ease ease;
    
    private readonly float downOffsetY;
    private readonly float duration;
    private readonly bool isUseLocal;

    private Vector3 _startPosition;
    private Tween _active;
    
    private bool _isCaptured;

    public ButtonTweenAnimator(Transform target, bool isUseLocal, Ease ease, float downOffsetY, float duration)
    {
        this.target = target;
        this.isUseLocal = isUseLocal;
        this.ease = ease;
        this.downOffsetY = downOffsetY;
        this.duration = duration;
    }

    public void CaptureStart()
    {
        if (target == null)
            return;
        
        _startPosition = isUseLocal ? target.localPosition : target.position;
        _isCaptured = true;
    }

    public void PressDown()
    {
        if (target == null || !_isCaptured)
            return;
        
        Kill();
        
        float y = _startPosition.y - downOffsetY;
        _active = isUseLocal ? target.DOLocalMoveY(y, duration).SetEase(ease) : target.DOMoveY(y, duration).SetEase(ease);
    }

    public void Release(bool returnOnExit)
    {
        if (target == null || !_isCaptured)
            return;
        
        Kill();
        
        if (returnOnExit)
            _active = isUseLocal ? target.DOLocalMoveY(_startPosition.y, duration).SetEase(ease) : target.DOMoveY(_startPosition.y, duration).SetEase(ease);
    }

    public void ResetToStart()
    {
        Kill();
        
        if (target == null || !_isCaptured)
            return;
        
        if (isUseLocal)
            target.localPosition = _startPosition;
        else
            target.position = _startPosition;
    }
    
    public void Kill()
    {
        if (_active != null && _active.IsActive())
        {
            _active.Kill(false);
            
            _active = null;
        }
    }
}