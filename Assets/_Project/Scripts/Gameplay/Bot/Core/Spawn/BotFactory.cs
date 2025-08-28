using UnityEngine;
using Zenject;

public class BotFactory
{
    private readonly DiContainer container;
    private readonly SmartBotParams[] presets;
    private readonly NameAssigner nameAssigner;
    private readonly BotRegistry botRegistry;
    private readonly ObjectPool<BotController> pool;
    private readonly float trailChance;

    public BotFactory(DiContainer container, NameAssigner nameAssigner, ObjectPool<BotController> pool, SmartBotParams[] presets, float trailChance, BotRegistry botRegistry)
    {
        this.container = container;
        this.nameAssigner = nameAssigner;
        this.pool = pool;
        this.presets = presets ?? new SmartBotParams[0];
        this.trailChance = Mathf.Clamp01(trailChance);
        this.botRegistry = botRegistry;
    }

    public BotController TrySpawnBot(Transform parent, Vector3 position, Quaternion rotation, SpawnOrigin origin, RacePath racePath, ProgressBarView bar)
    {
        BotController controller = pool.Get();
        GameObject botInstance = controller.gameObject;

        botInstance.transform.SetParent(parent, false);
        botInstance.transform.SetPositionAndRotation(position, rotation);

        nameAssigner?.AssignToBot(botInstance);
        
        bool trailEnabled = false;
        TrailRenderer trail = botInstance.GetComponentInChildren<TrailRenderer>();
        
        if (trail)
        {
            trailEnabled = (Random.value < trailChance);
            trail.enabled = trailEnabled;
        }
        
        SmartBotParams botParams = PickParamsVariant();
        controller.Initialize(botParams, origin.StartWaypoint, racePath, bar);
        
        if (trailEnabled)
            controller.RestartTrailAfter(0.5f);
        
        botRegistry?.Register(controller);
        
        return controller;
    }

    private SmartBotParams PickParamsVariant()
    {
        if (presets.Length == 0)
            return ScriptableObject.CreateInstance<SmartBotParams>();
        
        return presets[Random.Range(0, presets.Length)];
    }
}