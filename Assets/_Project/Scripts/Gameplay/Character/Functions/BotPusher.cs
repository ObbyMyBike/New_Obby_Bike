using System;
using UnityEngine;

public class BotPusher
{
    public event Action<PushSide, Transform> Pushed;
    
    private readonly Transform playerTransform;
    private readonly float pushRadius;
    private readonly float pushForce;
    private readonly float cooldown;
    private readonly float pushDuration;

    private readonly Collider[] _hitsBuffer = new Collider[16];
    private int _mask;
    private float _nextAllowedTime;

    public BotPusher(Transform playerTransform, float pushRadius, float pushForce, float cooldown, float pushDuration, float startDelaySeconds = 0.5f, int layerMask = Physics.DefaultRaycastLayers)
    {
        this.playerTransform = playerTransform;
        this.pushRadius = pushRadius;
        this.pushForce = pushForce;
        this.cooldown = cooldown;
        this.pushDuration = pushDuration;
        
        _mask = layerMask;
        _nextAllowedTime = Time.time + startDelaySeconds;
    }

    public void Tick()
    {
        if (Time.time < _nextAllowedTime)
            return;
        
        if (TryPush(out PushSide side, out Transform target))
        {
            _nextAllowedTime = Time.time + cooldown;
            
            Pushed?.Invoke(side, target);
        }
    }
    
    public bool TryPush(out PushSide side, out Transform target)
    {
        side = PushSide.Right;
        target = null;
        
        if (Time.time < _nextAllowedTime)
            return false;

        if (!FindAndPushNearestBot(out side, out Transform botTransform))
            return false;

        target = botTransform;
        
        return true;
    }

    private bool FindAndPushNearestBot(out PushSide side, out Transform botTransform)
    {
        side = PushSide.Right;
        botTransform = null;

        Vector3 playerPosition = playerTransform.position;
        int count = Physics.OverlapSphereNonAlloc(playerPosition, pushRadius, _hitsBuffer, _mask, QueryTriggerInteraction.Collide);
        if (count == 0) return false;

        Collider nearest = null;
        BotController controller = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var c = _hitsBuffer[i];
            if (c == null) continue;
            if (!c.TryGetComponent(out BotController bot)) continue;

            float sqr = (c.transform.position - playerPosition).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; nearest = c; controller = bot; }
        }

        if (controller == null) return false;

        Vector3 toBot = nearest.transform.position - playerPosition; toBot.y = 0f;
        Vector3 dir = toBot.sqrMagnitude > 0.0001f ? toBot.normalized : Vector3.zero;

        float signed = Vector3.SignedAngle(playerTransform.forward, dir, Vector3.up);
        side = signed < 0f ? PushSide.Left : PushSide.Right;

        controller.ApplyPush(dir * pushForce, pushDuration);
        botTransform = controller.transform;
        return true;
        
        // side = PushSide.Right;
        // botTransform = null;
        //
        // Vector3 playerPosition = playerTransform.position;
        // int count = Physics.OverlapSphereNonAlloc(playerPosition, pushRadius, _hitsBuffer, _mask, QueryTriggerInteraction.Collide);
        //
        // if (count == 0)
        //     return false;
        //
        // Collider nearest = null;
        // BotDriver botDriver = null;
        // float bestSqr = float.MaxValue;
        //
        // for (int i = 0; i < count; i++)
        // {
        //     Collider collider = _hitsBuffer[i];
        //     
        //     if (collider == null)
        //         continue;
        //     
        //     if (!collider.TryGetComponent(out BotDriver bot))
        //         continue;
        //
        //     float sqr = (collider.transform.position - playerPosition).sqrMagnitude;
        //     
        //     if (sqr < bestSqr)
        //     {
        //         bestSqr = sqr;
        //         nearest = collider;
        //         botDriver = bot;
        //     }
        // }
        //
        // if (botDriver == null)
        //     return false;
        //
        // Vector3 toBot = nearest.transform.position - playerPosition;
        // toBot.y = 0f;
        //
        // Vector3 direction = toBot.sqrMagnitude > 0.0001f ? toBot.normalized : Vector3.zero;
        // float signed = Vector3.SignedAngle(playerTransform.forward, direction, Vector3.up);
        //
        // side = signed < 0f ? PushSide.Left : PushSide.Right;
        //
        // botDriver.ApplyPush(direction * pushForce, pushDuration);
        // botTransform = botDriver.transform;
        //
        // return true;
    }
}