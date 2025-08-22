using UnityEngine;

public class ExitDelayPolicy
{
    private readonly float minHoldOnFirstEnter;
    private readonly float deactivateDelay;
    
    private float _firstEnterTime = -999f;

    public ExitDelayPolicy(float minHoldOnFirstEnter, float deactivateDelay)
    {
        this.minHoldOnFirstEnter = Mathf.Max(0f, minHoldOnFirstEnter);
        this.deactivateDelay = Mathf.Max(0f, deactivateDelay);
    }

    public void MarkFirstEnter() => _firstEnterTime = Time.time;
    
    public void ResetFirstEnter() => _firstEnterTime = -999f;

    public float ComputeDelay(bool stopImmediately)
    {
        if (stopImmediately)
            return 0f;

        float holdRemaining = Mathf.Max(0f, minHoldOnFirstEnter - (Time.time - _firstEnterTime));
        
        return Mathf.Max(deactivateDelay, holdRemaining);
    }
}