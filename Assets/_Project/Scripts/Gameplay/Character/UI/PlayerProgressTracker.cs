using UnityEngine;

public class PlayerProgressTracker
{
    private readonly Transform playerTransform;
    private readonly ProgressBarView _barView;
    private readonly RacePath path;

    public PlayerProgressTracker(Transform player, ProgressBarView barView, RacePath path)
    {
        playerTransform = player;
        this._barView = barView;
        this.path = path;

        if (this._barView != null)
            this._barView.InitializePlayer();
    }

    public void Tick()
    {
        if (playerTransform == null || _barView == null || path == null || !path.IsValid)
            return;

        float progress = path.ComputeProgress(playerTransform.position);
        _barView.UpdatePlayerProgress(progress);
    }
}