using System;
using UnityEngine;
using DG.Tweening;

public class PushFeedbackUI : IDisposable
{
    private readonly Player _player;
    private readonly PushPopup _popups;
    
    private readonly float _showChase = 0.30f;
    private readonly float _showDelay = 2f;
    
    private Tween _delayTween;
    
    public PushFeedbackUI(Player player, PushPopup popups)
    {
        _player = player;
        _popups = popups;

        if (_player?.PlayerCharacterRoot != null)
            _player.PlayerCharacterRoot.BotPushed += OnBotPushed;
    }

    public void Dispose()
    {
        if (_player?.PlayerCharacterRoot != null)
            _player.PlayerCharacterRoot.BotPushed -= OnBotPushed;

        _delayTween?.Kill();
        
        _delayTween = null;
    }
    
    private void OnBotPushed(PushSide side, Transform target)
    {
        if (target == null)
            return;
        
        if (UnityEngine.Random.value > _showChase)
            return;
        
        string text = side == PushSide.Left ? "Не толкайся!" : "Эй, ты чего толкаешься?";

        _delayTween?.Kill();
        
        _delayTween = DOVirtual.DelayedCall(_showDelay, () =>
        {
            if (target != null)
                _popups.ShowOver(target, text);
        });
    }
}