using UnityEngine;

public class BotProgress
{
    private readonly Transform botTransform;
    private readonly ProgressBarView barView;
    private readonly RacePath path;
    private readonly Vector3 finishPosition;
    private readonly float totalDistance;

    public BotProgress(Transform botTransform, ProgressBarView barView, RacePath path, Vector3 start, Vector3 finishPosition)
    {
        this.botTransform = botTransform;
        this.barView = barView;
        this.path = path;
        this.finishPosition = finishPosition;
        totalDistance = Vector3.Distance(start, finishPosition);
        
        barView?.InitializeBot(botTransform.gameObject);
    }

    public void Tick()
    {
        if (barView == null || botTransform == null)
            return;

        float pathValue = 0f;
        
        if (path != null && path.IsValid)
            pathValue = path.ComputeProgress(botTransform.position);
        else if (totalDistance > 1e-4f)
            pathValue = Mathf.Clamp01(1f - Vector3.Distance(botTransform.position, finishPosition) / totalDistance);
        
        barView.UpdateBotProgress(botTransform.gameObject, pathValue);
    }
}