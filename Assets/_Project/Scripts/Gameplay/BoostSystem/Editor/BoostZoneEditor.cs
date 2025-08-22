using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoostZone))]
public class BoostZoneEditor : Editor
{
    private SerializedProperty libraryProperty;
    private SerializedProperty presetIndexProperty;
    private SerializedProperty launchPointProp;

    private void OnEnable()
    {
        libraryProperty = serializedObject.FindProperty("_library");
        presetIndexProperty = serializedObject.FindProperty("_presetIndex");
        launchPointProp = serializedObject.FindProperty("_launchPoint");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(libraryProperty);

        BoostLibrary library = libraryProperty.objectReferenceValue as BoostLibrary;
        
        if (library == null || library.Presets.Count == 0)
        {
            EditorGUILayout.HelpBox("Assign a BoostLibrary with at least one preset.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            
            return;
        }

        string[] names = new string[library.Presets.Count];
        
        for (int i = 0; i < names.Length; i++)
            names[i] = $"{i}: {library.Presets[i].PresetName}";

        presetIndexProperty.intValue = Mathf.Clamp(presetIndexProperty.intValue, 0, library.Presets.Count - 1);
        presetIndexProperty.intValue = EditorGUILayout.Popup("Preset", presetIndexProperty.intValue, names);

        BoostZonePreset current = library.Presets[presetIndexProperty.intValue];

        if (current.BoostType == BoostType.Launcher || current.BoostType == BoostType.Rocket)
        {
            EditorGUILayout.PropertyField(launchPointProp);
            
            if (launchPointProp.objectReferenceValue == null)
                EditorGUILayout.HelpBox("LaunchPoint is recommended to control Rocket/Launcher direction.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preset Preview", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.EnumPopup("Boost Type", current.BoostType);
            
            if (current.BoostType == BoostType.BoosterSpeed)
            {
                EditorGUILayout.FloatField("Speed Multiplier", current.SpeedMultiplier);
                EditorGUILayout.FloatField("Deceleration Time", current.DecelerationTime);
            }
            else if (current.BoostType == BoostType.Launcher)
            {
                EditorGUILayout.FloatField("Distance X", current.LaunchDistanceX);
                EditorGUILayout.FloatField("Distance Z", current.LaunchDistanceZ);
                EditorGUILayout.FloatField("Landing Height", current.LandingHeight);
                EditorGUILayout.FloatField("Angle (deg)", current.LaunchAngle);
                EditorGUILayout.FloatField("Detection Radius", current.DetectionRadius);
            }
            else if (current.BoostType == BoostType.Trampoline)
            {
                EditorGUILayout.Toggle("Use Surface Normal", current.UseSurfaceNormal);
                EditorGUILayout.Vector3Field("Custom Direction", current.CustomBounceDirection);
                EditorGUILayout.FloatField("Bounce Force", current.BounceForce);
                EditorGUILayout.FloatField("Min Impact Speed", current.MinImpactSpeed);
                EditorGUILayout.FloatField("Bounce Cooldown", current.BounceCooldown);
                EditorGUILayout.Toggle("Keep Horizontal Velocity", current.KeepHorizontalVelocity);
            }
            else if (current.BoostType == BoostType.Rocket)
            {
                EditorGUILayout.FloatField("Distance X", current.LaunchDistanceX);
                EditorGUILayout.FloatField("Distance Z", current.LaunchDistanceZ);
                EditorGUILayout.FloatField("Landing Height", current.LandingHeight);
                EditorGUILayout.FloatField("Angle (deg)", current.LaunchAngle);
                EditorGUILayout.FloatField("Launch Force", current.LaunchForce);
            }
            else if (current.BoostType == BoostType.Acceleration)
            {
                EditorGUILayout.FloatField("Acceleration Multiplier", current.AccelerationMultiplier);
                EditorGUILayout.FloatField("Acceleration Duration", current.AccelerationDuration);
            }
            else if (current.BoostType == BoostType.Jump)
            {
                EditorGUILayout.FloatField("Jump Multiplier", current.JumpMultiplier);
                EditorGUILayout.FloatField("Jump Duration", current.JumpDuration);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}