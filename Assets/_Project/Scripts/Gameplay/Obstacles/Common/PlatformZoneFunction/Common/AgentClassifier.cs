using UnityEngine;

public class AgentClassifier
{
    private const string PLAYER_TAG = "Player";
    private const string BOT_TAG = "Bot";

    private readonly bool reactPlayer;
    private readonly bool reactBots;
    private readonly bool useBotTagCheck;

    public AgentClassifier(bool reactPlayer, bool reactBots, bool useBotTagCheck)
    {
        this.reactPlayer = reactPlayer;
        this.reactBots = reactBots;
        this.useBotTagCheck = useBotTagCheck;
    }

    public bool TryGetAgentRoot(Collider other, out Component root)
    {
        root = null;

        if (reactPlayer)
        {
            if (other.GetComponentInParent<Player>() is Player player)
            {
                root = player;
                
                return true;
            }

            if (other.CompareTag(PLAYER_TAG))
            {
                root = other.attachedRigidbody != null ? (Component)other.attachedRigidbody : other.transform.root;
                
                return true;
            }
        }

        if (reactBots)
        {
            if (other.GetComponentInParent<BotDriver>() is BotDriver bot)
            {
                root = bot;
                
                return true;
            }

            if (useBotTagCheck && other.CompareTag(BOT_TAG))
            {
                root = other.attachedRigidbody != null ? (Component)other.attachedRigidbody : other.transform.root;
                
                return true;
            }
        }

        return false;
    }
}