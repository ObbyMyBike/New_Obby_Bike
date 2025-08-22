using System.Collections.Generic;
using UnityEngine;

public class ProgressBotTracking
{
    private readonly ProgressBarView viewBar;
    private readonly Transform startPosition;
    private readonly Transform finishPosition;
    private readonly RacePath racePath;

    private readonly List<BotProgressTracker> trackers = new();
    private readonly Dictionary<GameObject, BotProgressTracker> byBot = new();

    public ProgressBotTracking(ProgressBarView viewBar, Waypoint startPoint, Transform finPoint)
    {
        this.viewBar = viewBar;
        startPosition = startPoint != null ? startPoint.transform : null;
        finishPosition = finPoint;
    }

    public void AttachIfPossible(BotDriver driver, RacePath path)
    {
        if (viewBar == null || path == null)
            return;

        if (driver.gameObject.TryGetComponent(out BotProgress progress))
        {
            BotProgressTracker tracker = new BotProgressTracker(progress, viewBar, driver.gameObject, startPosition != null ? startPosition.position : Vector3.zero,
                finishPosition != null ? finishPosition.position : Vector3.zero, path);

            trackers.Add(tracker);
            
            byBot[driver.gameObject] = tracker;
        }
    }

    public void Forget(GameObject bot)
    {
        if (byBot.TryGetValue(bot, out BotProgressTracker tracker))
        {
            tracker.Dispose();
            trackers.Remove(tracker);
            byBot.Remove(bot);
        }
    }

    public void Tick()
    {
        for (int i = trackers.Count - 1; i >= 0; i--)
        {
            BotProgressTracker tracker = trackers[i];
            
            if (!tracker.IsAlive)
            {
                tracker.Dispose();
                trackers.RemoveAt(i);
            }
            else
            {
                tracker.Tick();
            }
        }
    }
}