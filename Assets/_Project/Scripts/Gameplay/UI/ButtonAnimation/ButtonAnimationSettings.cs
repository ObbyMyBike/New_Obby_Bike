using UnityEngine;

[CreateAssetMenu(fileName = "ButtonAnimationSettings", menuName = "Configs/Button/Button Animation Settings")]
public class ButtonAnimationSettings : ScriptableObject
{
    public float PressScale = 0.9f;
    public float PressTime = 0.1f;
    public float ShakeTime = 0.3f;
    public float ShakeStrength = 20f;
    public int ShakeVibrato = 10;
}