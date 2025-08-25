using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRandom
{
    private readonly MonoBehaviour runner;
    private readonly BotFactory factory;
    private readonly CheckingAccessPlace clearance;
    private readonly LanePicker lanePicker;
    private readonly RacePath racePath;
    private readonly ProgressBarView bar;

    private readonly List<SpawnOrigin> origins;
    private readonly int maxAttempts;
    private readonly float retryInterval;
    private readonly float leaveDistance;

    public SpawnRandom(int maxAttempts, float retryInterval, float leaveDistance, List<SpawnOrigin> origins, LanePicker lanePicker,
        CheckingAccessPlace clearance, BotFactory factory, RacePath racePath, ProgressBarView bar, MonoBehaviour runner)
    {
        this.maxAttempts = Mathf.Max(1, maxAttempts);
        this.retryInterval = Mathf.Max(0f, retryInterval);
        this.leaveDistance = Mathf.Max(0f, leaveDistance);
        this.origins = origins;
        this.lanePicker = lanePicker;
        this.clearance = clearance;
        this.factory = factory;
        this.racePath = racePath;
        this.bar = bar;
        this.runner = runner;
    }

    public IEnumerator SpawnOne()
    {
        SpawnOrigin origin = origins[Random.Range(0, origins.Count)];
        Vector3 picked = origin.Position;
        Quaternion rotation = origin.Rotation;
        int attempts = 0;

        while (attempts++ < maxAttempts)
        {
            Vector3 candidate = lanePicker.RandomCandidate(origin);
            bool isClear = clearance.IsClear(candidate);
            bool canSpawn = (origin.Checkpoint == null) || origin.Checkpoint.CanSpawnOrRespawnHere();
            
            if (isClear && canSpawn)
            {
                picked = candidate;

                break;
            }
            
            yield return new WaitForSeconds(retryInterval);
        }

        BotController controller = factory.TrySpawnBot(runner.transform, picked, rotation, origin, racePath, bar);

        float waited = 0f;
        float timeout = 5f;
        float nudgeCooldown = 0.2f;
        float nextNudgeAt = 0f;
        Transform controllerTransform = controller.transform;

        while (Vector3.Distance(controllerTransform.position, picked) < leaveDistance && waited < timeout)
        {
            if (Time.time >= nextNudgeAt)
            {
                Vector3 forward = rotation * Vector3.forward;
                controller.ApplyPush(forward * 2.5f, 0.25f);
                nextNudgeAt = Time.time + nudgeCooldown;
            }

            yield return null;
            
            waited += Time.deltaTime;
        }
        
        if (Vector3.Distance(controllerTransform.position, picked) < leaveDistance)
        {
            Vector3 forward = (rotation * Vector3.forward);
            controller.ApplyPush(forward * 2.5f, 0.25f);
        }
    }
}