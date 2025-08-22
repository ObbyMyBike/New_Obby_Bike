using UnityEngine;

[CreateAssetMenu(menuName = "Game/Bots/Smart Bot Params", fileName = "SmartBotParams")]
public class SmartBotParams : ScriptableObject
{
    [Header("Personality / Profile")]
    [Range(0.7f, 1.3f)] public float BaseSpeedMul = 1.0f;
    [Range(0f, 1f)] public float Aggression = 0.5f;

    [Header("Lane & Wander")]
    public float MaxLaneOffset = 1.0f;
    public Vector2 LaneRepickInterval = new Vector2(2f, 5f);

    [Header("Steering")]
    public float LookAhead = 2.0f;
    public float TurnSlowdownAngle = 45f;
    public float MinSpeedMulOnSharpTurn = 0.6f;

    [Header("Crowd / Avoidance")]
    public float SeparationRadius = 2.0f;
    public float SeparationStrength = 1.0f;
    public float AvoidRayLength = 2.5f;
    public float AvoidStrength = 1.2f;
    public LayerMask AgentsMask;

    [Header("Stuck Handling")]
    public float RepathIfStuckTime = 2.0f;
    public float StuckDistanceEps = 0.2f;

    [Header("Jump")]
    public float JumpCooldown = 0.4f;
    
    [Header("Track safety / Edge guard")]
    public LayerMask GroundMask = ~0;
    [Min(0.1f)] public float EdgeProbeAhead = 1.4f;
    [Min(0.05f)] public float EdgeProbeSide = 0.7f;
    [Min(0.1f)] public float EdgeProbeDown = 3.0f;
    [Range(0f, 5f)] public float EdgeAvoidStrength = 2.2f;
}