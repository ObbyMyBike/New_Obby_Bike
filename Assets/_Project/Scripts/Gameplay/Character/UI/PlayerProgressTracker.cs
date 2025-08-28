using UnityEngine;

public class PlayerProgressTracker
{
    private readonly Transform playerTransform;
    private readonly ProgressBarView barView;
    private readonly RacePath path;

    public PlayerProgressTracker(Transform playerTransform, ProgressBarView barView, RacePath path)
    {
        this.playerTransform = playerTransform;
        this.barView = barView;
        this.path = path;

        if (this.barView != null)
            this.barView.InitializePlayer();
    }

    public void Tick()
    {
        if (playerTransform == null || barView == null || path == null || !path.IsValid)
            return;
        
        float pathPlayer = path.ComputeProgress(playerTransform.position);
        
        barView.UpdatePlayerProgress(pathPlayer);
    }
}