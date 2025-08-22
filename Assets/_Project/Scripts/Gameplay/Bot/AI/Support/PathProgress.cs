using UnityEngine;

public class PathProgress
{
    private readonly SmartBotParams settings;
    private readonly Transform bot;

    private Vector3 _lastPosition;
    
    private float _lastProgressDistance = float.PositiveInfinity;
    private float _stuckTimer;
    private float _smoothedSpeed;

    public PathProgress(SmartBotParams settings, Transform bot)
    {
        this.settings = settings;
        this.bot = bot;
        _lastPosition = bot.position;
    }

    public bool Update(Vector3 currentTargetPosition, bool isWaitingAtGate)
    {
        if (isWaitingAtGate)
        {
            _stuckTimer = 0f;
            _lastProgressDistance = Vector3.Distance(bot.position, currentTargetPosition);
            
            SmoothSpeed();
            
            return false;
        }

        float newDist = Vector3.Distance(bot.position, currentTargetPosition);

        if (newDist > _lastProgressDistance - settings.StuckDistanceEps)
            _stuckTimer += Time.deltaTime;
        else
            _stuckTimer = 0f;

        _lastProgressDistance = newDist;

        bool isStuck = _stuckTimer > settings.RepathIfStuckTime;
        
        if (isStuck)
            _stuckTimer = 0f;

        SmoothSpeed();
        
        return isStuck;
    }

    public void Reset()
    {
        _stuckTimer = 0f;
        _lastProgressDistance = float.PositiveInfinity;
    }
    
    private void SmoothSpeed()
    {
        float inst = (bot.position - _lastPosition).magnitude / Mathf.Max(Time.deltaTime, 1e-6f);
        
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, inst, 0.25f);
        _lastPosition = bot.position;
    }
}