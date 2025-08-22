using UnityEngine;
using Zenject;

public class BotFactory
{
    private readonly DiContainer container;
    private readonly SmartBotParams[] presets;
    private readonly NameAssigner nameAssigner;
    private readonly ObjectPool<BotDriver> pool;
    private readonly float trailChance;

    public BotFactory(DiContainer container, NameAssigner nameAssigner, ObjectPool<BotDriver> pool, SmartBotParams[] presets, float trailChance)
    {
        this.container = container;
        this.nameAssigner = nameAssigner;
        this.pool = pool;
        this.presets = presets ?? new SmartBotParams[0];
        this.trailChance = Mathf.Clamp01(trailChance);
    }

    public BotDriver TrySpawnBot(Transform parent, Vector3 position, Quaternion rotation, SpawnOrigin origin)
    {
        BotDriver driver = pool.Get();
        GameObject botInstance = driver.gameObject;

        botInstance.transform.SetParent(parent, false);
        botInstance.transform.SetPositionAndRotation(position, rotation);

        nameAssigner?.AssignToBot(botInstance);

        TrailRenderer trail = botInstance.GetComponentInChildren<TrailRenderer>();
        
        if (trail)
            trail.enabled = (Random.value < trailChance);

        SmartBotParams botParams = PickParamsVariant();
        BotInputAI ai = new BotInputAI(botInstance.transform, origin.StartWaypoint, botParams);
        driver.SetInput(ai);

        BotAIController aiCtl = botInstance.GetComponent<BotAIController>() ?? botInstance.AddComponent<BotAIController>();
        aiCtl.SetAI(ai);

        if (botInstance.TryGetComponent(out BotRespawn respawn))
        {
            respawn.Initialize(ai, origin.StartWaypoint);
            
            if (origin.Checkpoint != null)
                respawn.SetCheckpoint(origin.Checkpoint);
        }

        return driver;
    }

    public void Release(BotDriver driver) => pool.Release(driver);

    private SmartBotParams PickParamsVariant()
    {
        if (presets.Length == 0)
            return ScriptableObject.CreateInstance<SmartBotParams>();

        return presets[Random.Range(0, presets.Length)];
    }
}