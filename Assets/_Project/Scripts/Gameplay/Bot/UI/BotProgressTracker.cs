using UnityEngine;

// public class BotProgressTracker
// {
//     private readonly BotProgress botProgress;
//     private readonly ProgressBarView _progressBarView;
//     private readonly GameObject npc;
//     private readonly Transform botTransform;
//     private readonly bool initialized;
//
//     public bool IsAlive => npc != null;
//
//     public BotProgressTracker(BotProgress botProgress, ProgressBarView progressBarView, GameObject botInstance, Vector3 startPosition, Vector3 finishPosition, RacePath path)
//     {
//         this.botProgress = botProgress;
//         this._progressBarView = progressBarView;
//         npc = botInstance;
//         botTransform = botInstance.transform;
//         
//         if (path != null && path.IsValid)
//             this.botProgress.InitializePath(path);
//         else
//             this.botProgress.Initialize(startPosition, finishPosition);
//         
//         this._progressBarView.InitializeBot(npc);
//         
//         this.botProgress.ProgressUpdated += OnProgressUpdated;
//
//         initialized = true;
//     }
//
//     public void Tick()
//     {
//         if (!initialized || !IsAlive)
//             return;
//         
//         botProgress.UpdateProgressFromPosition(botTransform.position);
//     }
//
//     private void OnProgressUpdated(float progress)
//     {
//         if (_progressBarView != null && npc != null)
//             _progressBarView.UpdateBotProgress(npc, progress);
//     }
//
//     public void Dispose()
//     {
//         if (botProgress != null)
//             botProgress.ProgressUpdated -= OnProgressUpdated;
//     }
// }