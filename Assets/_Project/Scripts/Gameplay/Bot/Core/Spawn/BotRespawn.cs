using System;
using System.Collections;
using UnityEngine;

public class BotRespawn
{
    private readonly Rigidbody botRigidbody;
    private readonly float retry;
    private readonly float killBelowY;
    private readonly float maxFreeFallSeconds;
    private readonly float minFallSpeed;
    
    private readonly Func<IEnumerator, Coroutine> start;
    
    private CheckPoints _lastCheckpoint;
    private Waypoint _lastWaypoint;
    private Coroutine _retryRoutine;
    private Vector3 _lastPosition;
    
    private float _fallSince = -1f;

    public BotRespawn(Rigidbody botRigidbody, float retry, float killBelowY, float maxFreeFallSeconds, float minFallSpeed,
        Waypoint start, Func<IEnumerator, Coroutine> startCoroutine, Action<Coroutine> stopCoroutine)
    {
        this.botRigidbody = botRigidbody;
        this.retry = retry;
        this.killBelowY = killBelowY;
        this.maxFreeFallSeconds = maxFreeFallSeconds;
        this.minFallSpeed = minFallSpeed;
        
        _lastWaypoint  = start;
        _lastPosition = start != null ? start.transform.position : botRigidbody.position;

        this.start = startCoroutine;
    }

    public void SetCheckpoint(CheckPoints checkPoints)
    {
        if (checkPoints == null)
            return;
        
        _lastCheckpoint = checkPoints;
        _lastPosition = checkPoints.transform.position;
        
        if (checkPoints.AssociatedWaypoint != null)
            _lastWaypoint = checkPoints.AssociatedWaypoint;
    }

    public void TickFallKill(Vector3 position, float value)
    {
        if (position.y <= killBelowY)
        {
            Respawn(null);
            
            return;
        }

        if (value <= minFallSpeed)
        {
            if (_fallSince < 0f)
                _fallSince = Time.time;
            
            if (Time.time - _fallSince >= maxFreeFallSeconds)
            {
                Respawn(null);
                
                _fallSince = -1f;
            }
        }
        else
        {
            _fallSince = -1f;
        }
    }

    public void Respawn(BotInputAI ai)
    {
        if (_lastCheckpoint != null && !_lastCheckpoint.CanSpawnOrRespawnHere())
        {
            if (start == null)
            {
                DoRespawn(ai);
                
                return;
            }
            
            if (_retryRoutine == null)
                _retryRoutine = start(RetryRespawn(ai, _lastCheckpoint));
            
            return;
        }
        
        DoRespawn(ai);
    }

    private void DoRespawn(BotInputAI botAI)
    {
        botRigidbody.transform.position = _lastPosition;

        if (_lastWaypoint != null)
        {
            Vector3 plane = _lastWaypoint.transform.position - botRigidbody.transform.position;
            plane.y = 0f;
            
            if (plane.sqrMagnitude > 0.001f)
                botRigidbody.transform.rotation = Quaternion.LookRotation(plane.normalized);
        }

        botRigidbody.velocity = Vector3.zero;
        botRigidbody.angularVelocity = Vector3.zero;

        botAI?.ResetToWaypoint(_lastWaypoint);
        botAI?.Tick();
    }

    private IEnumerator RetryRespawn(BotInputAI botAI, CheckPoints checkPoints)
    {
        while (!checkPoints.CanSpawnOrRespawnHere())
            yield return new WaitForSeconds(retry);

        _retryRoutine = null;
        
        DoRespawn(botAI);
    }
}