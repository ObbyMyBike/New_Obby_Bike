using System.Collections.Generic;

public class BotRegistry
{
    private readonly HashSet<BotController> bots = new HashSet<BotController>();

    public IReadOnlyCollection<BotController> All => bots;
    
    public void Register(BotController bot) => bots?.Add(bot);

    public void Unregister(BotController bot) => bots?.Remove(bot);
}