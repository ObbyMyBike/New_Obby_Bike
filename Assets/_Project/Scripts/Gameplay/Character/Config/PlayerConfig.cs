using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player/Player Settings", fileName = "New Player Settings")]
public class PlayerConfig : ScriptableObject
{
    public int StartCountGold = 100;
    
    [Header("Movement Settings")]
    [Tooltip("Максимальная скорость")] public float MaxSpeed = 20f;
    [Tooltip("Коэффициент ускорения")] public float Acceleration = 15f;
    [Tooltip("Естественное замедление, когда нет газа")] public float Drag = 2f;
    [Tooltip("Сглаживание yaw при повороте")] public float TurnSmoothTime = 0.15f;
    [Tooltip("Максимальный наклон (в градусах)")] public float LeanAngleMax = 18f;
    [Tooltip("Сглаживание самого lean")] public float LeanSmoothTime = 0.1f;
    [Tooltip("0 = нет дрифта, 1 = сильный дрифт при повороте")] public float DriftFactor = 0.35f;
    [Tooltip("Базовое затухание боковой скорости")] public float LateralFriction = 5f;
    [Tooltip("Коэффициент ускорения")] public float MinInputThreshold = 0.01f;
    
    [Header("Input Shaping")]
    [Tooltip("Мёртвая зона джойстика")] public float InputDeadzone = 0.12f;
    [Tooltip("Экспонента кривой стика (>1 = плавнее старт, резче к максимуму)")] public float InputExponent = 1.6f;

    [Header("Stability / Handling")]
    [Tooltip("Помощь в выравнивании скорости к направлению руля")] public float SteeringAssist = 12f;
    [Tooltip("Сила быстрого торможения при резкой смене направления")] public float BrakeStrength = 25f;
    [Tooltip("Порог скорости для триггера быстрого разворота (м/с)")] public float QuickTurnSpeedThreshold = 6f;
    [Tooltip("Угол между скоростью и желаемым направлением для быстрого разворота")] public float OppositeInputAngle = 120f;
    [Tooltip("Сила прилипания к земле при небольшом отрыве")] public float GroundStickForce = 30f;

    [Tooltip("Макс. сглаживание при большом заносе")] public float MaxLateralFriction = 18f;
    [Tooltip("Порог боковой скорости, после которого усиливаем «трение»")] public float LateralSlipThreshold = 6f;

    [Header("Turn Smoothing")]
    [Tooltip("Минимальное сглаживание поворота при сильном газе")] public float TurnSmoothTimeFast = 0.08f;
    
    [Header("Jump Settings")]
    public AudioClip JumpClip;
    public float JumpForce = 30f;
    public float JumpDistance = 1.2f;

    [Header("Push Settings")]
    public AudioClip PushClip;
    public float PushRadius = 2f;
    public float PushForce = 10f;
    public float PushCooldown = 1f;
    public float PushDuration = 1f;
    
    [Header("Available skins")]
    public List<SkinDefinition> AvailableSkins;
}