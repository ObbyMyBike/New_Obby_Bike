using UnityEngine;

public class LanePicker
{
    private readonly float laneSpacing;
    private readonly int laneSpread;

    public LanePicker(float laneSpacing, int laneSpread)
    {
        this.laneSpacing = Mathf.Max(0f, laneSpacing);
        this.laneSpread = Mathf.Max(0, laneSpread);
    }

    public Vector3 LaneOffset(Vector3 right, int lane) => right * (lane * laneSpacing);

    public Vector3 RandomCandidate(SpawnOrigin origin)
    {
        int lane = Random.Range(-laneSpread, laneSpread + 1);
        
        return origin.Position + LaneOffset(origin.LanesRight, lane);
    }
}