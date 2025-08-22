using System;
using UnityEngine;

public class PushingPlayer : IUpdatable, IDisposable
{
    public event Action<PushSide, Transform> Pushed;

    private readonly BotPusher botPusher;
    private readonly IInput input;

    public PushingPlayer(BotPusher botPusher, IInput input)
    {
        this.botPusher = botPusher;
        this.input = input;

        this.botPusher.Pushed += OnAutoPush;
        this.input.Pushed += OnManualPush;
    }

    void IDisposable.Dispose()
    {
        botPusher.Pushed -= OnAutoPush;
        input.Pushed -= OnManualPush;
    }

    void IUpdatable.Tick()
    {
        botPusher.Tick();
    }

    private void OnManualPush()
    {
        if (botPusher.TryPush(out PushSide side, out Transform target))
            Pushed?.Invoke(side, target);
        else
            Pushed?.Invoke(UnityEngine.Random.value < 0.5f ? PushSide.Left : PushSide.Right, null);
    }

    private void OnAutoPush(PushSide side, Transform target) => Pushed?.Invoke(side, target);
}