using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BotInputAI : IInput
{
    public event Action Jumped;
    public event Action Pushed { add {} remove {} }

    private readonly SmartBotParams settings;
    private readonly Transform bot;

    private readonly WaypointChooser waypoints = new WaypointChooser();
    private readonly LaneWander lanesWander;
    private readonly AvoidanceField avoidance;
    private readonly PathProgress pathProgress;
    private readonly GateCheck gates = new GateCheck();
    private readonly LookAhead lookAhead;
    private readonly SpeedTuning speed;
    private readonly SteeringBlend steering = new SteeringBlend();
    private readonly JumpAssist jumpAssist;

    private readonly Waypoint firstWaypoint; 
    private readonly Collider[] nearbyShared = new Collider[16];
    
    private readonly float baseSpeedMul;

    private Waypoint _current;
    private Waypoint _waitingWaypoint;

    private Vector3 _steerDirection;
    
    private bool _isWaitingAtGate;
    private bool _isBypassStartGate = true; 

    public BotInputAI(Transform botTransform, Waypoint start, SmartBotParams settings)
    {
        bot = botTransform;
        this.settings = settings;
        firstWaypoint = start;
        _isBypassStartGate = true; 
        
        lanesWander = new LaneWander(settings);
        avoidance = new AvoidanceField(settings, nearbyShared);
        pathProgress = new PathProgress(settings, botTransform);
        lookAhead = new LookAhead(settings);
        speed = new SpeedTuning(settings);
        jumpAssist = new JumpAssist(assistDuration: 0.2f, postJumpMinHold: 0.12f, jumpCooldown: settings.JumpCooldown);

        _current = start;
        _steerDirection = bot.forward;
        baseSpeedMul = Random.Range(settings.BaseSpeedMul * 0.95f, settings.BaseSpeedMul * 1.05f);
    }

    public Vector2 InputDirection { get; private set; }
    
    public void Tick()
    {
        if (_current == null)
        {
            InputDirection = Vector2.zero;
            
            return;
        }

        lanesWander.Tick();

        Vector3 position = bot.position;
        Vector3 target = _current.transform.position;
        Vector3 toTarget = target - position;
        float distance = toTarget.magnitude;
        float distanceSqr = toTarget.sqrMagnitude;

        WaypointGate gate = _current.Gate;
        Waypoint projectedNext = (_current.NextWaypoints != null && _current.NextWaypoints.Count > 0) ? (waypoints.LastChosenNext ?? _current.NextWaypoints[0]) : null;
        
        float stopRadius = gates.GetStopRadius(gate);
        float collectRadius = gates.GetCollectRadius(_current, gate);
        float collectRadiusSqr = collectRadius * collectRadius;
        bool spawnGrace = _isBypassStartGate;
        
        if (jumpAssist.UpdateHold(gate, distance, stopRadius, toTarget, ref _steerDirection, baseSpeedMul, out Vector2 holdInput))
        {
            pathProgress.Update(target, true);
            
            InputDirection = holdInput;
            
            return;
        }
        
        if (gate != null)
        {
            float preWaitBand = Mathf.Max(0.6f, stopRadius * 1.5f);
            
            if (distance <= preWaitBand)
            {
                bool isReadyNear = gates.ReadyToPass(gate, _current, projectedNext, spawnGrace);

                if (!isReadyNear)
                {
                    float creepStop = Mathf.Max(0.9f * stopRadius, stopRadius - 0.05f);
                    
                    if (distance > creepStop && toTarget.sqrMagnitude > 1e-6f)
                    {
                        Vector3 creep = toTarget.normalized * 0.2f;
                        _steerDirection = Vector3.Slerp(_steerDirection, creep.normalized, 6f * Time.deltaTime);
                        InputDirection = new Vector2(creep.x, creep.z);
                    }
                    else
                    {
                        InputDirection = Vector2.zero;
                    }

                    _isWaitingAtGate = true;
                    _waitingWaypoint = _current;
                    
                    pathProgress.Update(target, true);
                    
                    return;
                }
            }
        }
        
        if (gate != null && distance <= stopRadius)
        {
            bool isReady = gates.ReadyToPass(gate, _current, projectedNext, spawnGrace);

            if (!isReady && !spawnGrace)
            {
                InputDirection = Vector2.zero;

                Vector3 aimDirection = projectedNext != null ? (projectedNext.transform.position - position) : (_steerDirection.sqrMagnitude > 1e-4f ? _steerDirection : bot.forward);

                if (aimDirection.sqrMagnitude > 1e-4f)
                    _steerDirection = Vector3.Slerp(_steerDirection, aimDirection.normalized, 6f * Time.deltaTime);

                _isWaitingAtGate = true;
                _waitingWaypoint = _current;
                
                pathProgress.Update(target, true);
                
                return;
            }
            else if (_isWaitingAtGate && _waitingWaypoint == _current)
            {
                bool isJumpRequire = _current.RequiresJump || gate.RequireJumpOnPass;

                _isWaitingAtGate = false;
                _waitingWaypoint = null;

                Waypoint justCollected = _current;
                waypoints.RememberVisited(justCollected);

                Vector3 previewDirection = (bot.position - justCollected.transform.position);
                previewDirection.y = 0f;
                
                Waypoint nextWaypoint = waypoints.PickNext(justCollected, settings.Aggression, previewDirection);
                
                if (nextWaypoint != null)
                    _current = nextWaypoint;

                if (_isBypassStartGate && justCollected == firstWaypoint)
                    _isBypassStartGate = false;
                
                target = _current.transform.position;
                toTarget = target - position;
                distance = toTarget.magnitude;

                lanesWander.ForceRepickEarly(0.5f);

                if (isJumpRequire)
                    TriggerJump(justCollected, position);
            }
        }
        
        if (distanceSqr < collectRadiusSqr)
        {
            if (gate != null)
            {
                bool isReady = gates.ReadyToPass(gate, _current, projectedNext, spawnGrace);
                
                if (!isReady)
                {
                    InputDirection = Vector2.zero;
                    _isWaitingAtGate = true;
                    _waitingWaypoint = _current;
                    
                    pathProgress.Update(target, true);
                    
                    return;
                }
            }
            
            bool isJumpRequire = _current.RequiresJump || (gate?.RequireJumpOnPass ?? false);
            Waypoint justCollectedSecond = _current;

            waypoints.RememberVisited(justCollectedSecond);
            
            Vector3 previewDirection = (bot.position - justCollectedSecond.transform.position);
            previewDirection.y = 0f;
            
            Waypoint nextSecondWaypoint = waypoints.PickNext(justCollectedSecond, settings.Aggression, previewDirection);
            
            if (nextSecondWaypoint != null)
                _current = nextSecondWaypoint;
            
            if (_isBypassStartGate && justCollectedSecond == firstWaypoint)
                _isBypassStartGate = false;
            
            target = _current.transform.position;
            toTarget = target - position;
            distance = toTarget.magnitude;

            lanesWander.ForceRepickEarly(0.5f);
            
            if (isJumpRequire)
                TriggerJump(justCollectedSecond, position);
        }
        
        lookAhead.Compute(bot, _current, projectedNext, distance, toTarget, _steerDirection, out var forwardDir, out var lookPoint);

        float laneBlend = steering.ComputeLaneBlend(_current, distance);
        Vector3 desired = steering.ComposeDesired(bot, position, forwardDir, lookPoint, lanesWander.CurrentOffset, laneBlend, avoidance);
        _steerDirection = steering.UpdateSteering(_steerDirection, desired);
        
        float turnMultiplier = speed.TurnMultiplier(_current, projectedNext, position);
        Vector3 moveDirection = _steerDirection * (baseSpeedMul * turnMultiplier);
        
        speed.ApplyApproachSlowdown(ref moveDirection, distance, stopRadius);
        jumpAssist.ApplyAssist(ref moveDirection);
        
        InputDirection = new Vector2(moveDirection.x, moveDirection.z);
        bool stuck = pathProgress.Update(_current.transform.position, _isWaitingAtGate);
        
        if (stuck)
        {
            TriggerJump(_current, position);
            
            waypoints.ForceRepathFrom(_current);
            lanesWander.ForceRepickEarly(0.25f);
        }
    }

    public void ResetToWaypoint(Waypoint waypoint)
    {
        if (waypoint == null)
            return;

        _current = waypoint;
        _isWaitingAtGate = false;
        _waitingWaypoint = null;
        _isBypassStartGate = (waypoint == firstWaypoint);

        pathProgress.Reset();
        lanesWander.ForceRepickEarly(0.2f);
        jumpAssist.Reset();
    }

    private void TriggerJump(Waypoint justCollected, Vector3 botPosition)
    {
        Vector3 forward = (_current != null) ? (_current.transform.position - botPosition) : justCollected.transform.forward;

        forward.y = 0f;
        
        if (forward.sqrMagnitude < 1e-4f)
            forward = (_steerDirection.sqrMagnitude > 1e-4f ? _steerDirection : bot.forward);
        
        forward.Normalize();

        WaypointGate nextWaypointGate = _current != null ? _current.Gate : null;

        jumpAssist.TriggerJump(() => Jumped?.Invoke(), forward, nextWaypointGate);
    }
}