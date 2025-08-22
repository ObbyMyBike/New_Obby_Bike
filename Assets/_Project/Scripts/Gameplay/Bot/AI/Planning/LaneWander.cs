using UnityEngine;

public class LaneWander
{
    private readonly SmartBotParams settings;
    
    private float _laneOffset;
    private float _nextRepickTime;

    public LaneWander(SmartBotParams settings)
    {
        this.settings = settings;
        
        Repick(now: 0f);
    }

    public float CurrentOffset => _laneOffset;

    public void Tick()
    {
        if (Time.time >= _nextRepickTime)
            Repick(Time.time);
    }

    public void ForceRepickEarly(float leadSeconds = 0.5f)
    {
        _nextRepickTime = Mathf.Min(_nextRepickTime, Time.time + Mathf.Max(0.1f, leadSeconds));
    }

    private void Repick(float now)
    {
        float max = Mathf.Lerp(settings.MaxLaneOffset * 0.4f, settings.MaxLaneOffset, settings.Aggression);
        
        _laneOffset = Random.Range(-max, max);
        _nextRepickTime = (now <= 0f ? Time.time : now) + Random.Range(settings.LaneRepickInterval.x, settings.LaneRepickInterval.y);
    }
}