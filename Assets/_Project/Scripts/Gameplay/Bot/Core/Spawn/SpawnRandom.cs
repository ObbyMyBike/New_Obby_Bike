using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRandom
{
    private readonly MonoBehaviour runner;
    private readonly BotFactory factory;
    private readonly ProgressBotTracking progress;
    private readonly CheckingAccessPlace clearance;
    private readonly LanePicker lanePicker;
    private readonly RacePath racePath;
    
    private readonly List<SpawnOrigin> origins;
    
    private readonly int maxAttempts;
    private readonly float retryInterval;
    private readonly float leaveDistance;

    public SpawnRandom(int maxAttempts, float retryInterval, float leaveDistance, List<SpawnOrigin> origins, LanePicker lanePicker,
        CheckingAccessPlace clearance, BotFactory factory, ProgressBotTracking progress, RacePath racePath, MonoBehaviour runner)
    {
        this.maxAttempts = Mathf.Max(1, maxAttempts);
        this.retryInterval = Mathf.Max(0f, retryInterval);
        this.leaveDistance = Mathf.Max(0f, leaveDistance);
        this.origins = origins;
        this.lanePicker = lanePicker;
        this.clearance = clearance;
        this.factory = factory;
        this.progress = progress;
        this.racePath = racePath;
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
            bool canSpawnOrRespawnHere = (origin.Checkpoint == null) || origin.Checkpoint.CanSpawnOrRespawnHere();

            if (isClear && canSpawnOrRespawnHere)
            {
                picked = candidate;
                
                break;
            }
            
            yield return new WaitForSeconds(retryInterval);
        }
        
        BotDriver driver = factory.TrySpawnBot(runner.transform, picked, rotation, origin);
        progress.AttachIfPossible(driver, racePath);
        
        float waited = 0f;
        float timeout = 5f;
        Transform botTransform = driver.transform;

        while (Vector3.Distance(botTransform.position, picked) < leaveDistance && waited < timeout)
        {
            yield return null;
            
            waited += Time.deltaTime;
        }

        if (Vector3.Distance(botTransform.position, picked) < leaveDistance)
        {
            Vector3 forward = (rotation * Vector3.forward);
            driver.ApplyPush(forward * 2.5f, 0.25f);
        }
    }
}