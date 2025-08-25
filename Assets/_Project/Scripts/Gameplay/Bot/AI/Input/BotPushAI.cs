using UnityEngine;

public class BotPushAI
{
    private readonly Transform bot;
    private readonly PlayerCharacterRoot player;
    private readonly Rigidbody botRigidbody;

    private readonly float radius;
    private readonly float force;
    private readonly float duration;
    private readonly float cooldown;
    private readonly float chance;
    
    private float _nextTime;

    public BotPushAI(Transform bot, PlayerCharacterRoot player, Rigidbody botRigidbody, float radius, float force, float duration, float cooldown, float chance)
    {
        this.bot = bot;
        this.player = player;
        this.botRigidbody = botRigidbody;
        this.radius = radius;
        this.force = force;
        this.duration = duration;
        this.cooldown = cooldown;
        this.chance = chance;
        
        _nextTime = Time.time + Random.Range(0.2f, 0.6f);
    }

    public void Tick(float now)
    {
        if (player == null)
            return;
        
        if (now < _nextTime)
            return;

        _nextTime = now + cooldown;

        Vector3 distance = player.transform.position - bot.position; distance.y = 0f;
        
        if (distance.sqrMagnitude > radius * radius)
            return;
        
        if (distance.sqrMagnitude < 1e-6f)
            return;
        
        if (Random.value > chance)
            return;

        Vector3 direction = distance.normalized;

        player.TryApplyPush(direction * force, duration);
        
        botRigidbody.velocity += -direction * force * 0.1f;
    }
}