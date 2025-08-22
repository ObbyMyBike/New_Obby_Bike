using System;
using UnityEngine;

[Serializable]
public class BoostZonePreset
{
    public string PresetName = "NewPreset";
    public BoostType BoostType = BoostType.BoosterSpeed;
    
    [Header("Boost Speed")]
    [Min(0)] public float SpeedMultiplier = 2f;
    [Min(0)] public float DecelerationTime = 2f;
    
    [Header("Launcher")]
    public float LaunchDistanceX = 5f;
    public float LaunchDistanceZ = 5f;
    public float LandingHeight = 0f;
    [Range(1f, 89f)] public float LaunchAngle = 45f;
    [Min(0)] public float DetectionRadius = 5f;
    [Min(0.01f)] public float LaunchForce = 1f;
    
    [Header("Trampoline")]
    public Vector3 CustomBounceDirection = Vector3.up;
    [Min(0)] public float BounceForce = 10f;
    [Min(0)] public float MinImpactSpeed = 1f;
    [Min(0)] public float BounceCooldown = 0.05f;
    public bool KeepHorizontalVelocity = true;
    public bool UseSurfaceNormal = true;
    
    [Header("Boost Acceleration")]
    [Min(0)] public float AccelerationMultiplier = 8f;
    [Min(0)] public float AccelerationDuration = 120f;
    
    [Header("Boost Jump")]
    [Min(0)] public float JumpMultiplier = 1.5f;
    [Min(0)] public float JumpDuration = 120f;
}