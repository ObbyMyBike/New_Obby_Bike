using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BoostZonePreset))]
public class BoostZonePresetDrawer : PropertyDrawer
{
    private float spacing => EditorGUIUtility.standardVerticalSpacing;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0f;

        height += EditorGUIUtility.singleLineHeight + spacing;

        height += Height(property, "PresetName");
        height += Height(property, "BoostType");

        var typeProp = property.FindPropertyRelative("BoostType");
        BoostType type = (BoostType)typeProp.enumValueIndex;

        if (type == BoostType.BoosterSpeed)
        {
            height += Height(property, "SpeedMultiplier");
            height += Height(property, "DecelerationTime");
        }
        else if (type == BoostType.Launcher)
        {
            height += Height(property, "LaunchDistanceX");
            height += Height(property, "LaunchDistanceZ");
            height += Height(property, "LandingHeight");
            height += Height(property, "LaunchAngle");
            height += Height(property, "DetectionRadius");
        }
        else if (type == BoostType.Trampoline)
        {
            height += Height(property, "CustomBounceDirection");
            height += Height(property, "BounceForce");
            height += Height(property, "MinImpactSpeed");
            height += Height(property, "BounceCooldown");
            height += Height(property, "KeepHorizontalVelocity");
            height += Height(property, "UseSurfaceNormal");
        }
        else if (type == BoostType.Rocket)
        {
            height += Height(property, "LaunchDistanceX");
            height += Height(property, "LaunchDistanceZ");
            height += Height(property, "LandingHeight");
            height += Height(property, "LaunchAngle");
            height += Height(property, "LaunchForce");
        }
        else if (type == BoostType.Acceleration)
        {
            height += Height(property, "AccelerationMultiplier");
            height += Height(property, "AccelerationDuration");
        }
        else if (type == BoostType.Jump)
        {
            height += Height(property, "JumpMultiplier");
            height += Height(property, "JumpDuration");
        }
        
        height += spacing;

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect rect = position;
        rect.height = EditorGUIUtility.singleLineHeight;

        EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
        rect.y += rect.height + spacing;

        DrawProperty(ref rect, property, "PresetName");
        DrawProperty(ref rect, property, "BoostType");

        var typeProp = property.FindPropertyRelative("BoostType");
        BoostType type = (BoostType)typeProp.enumValueIndex;

        if (type == BoostType.BoosterSpeed)
        {
            DrawProperty(ref rect, property, "SpeedMultiplier");
            DrawProperty(ref rect, property, "DecelerationTime");
        }
        else if (type == BoostType.Launcher)
        {
            DrawProperty(ref rect, property, "LaunchDistanceX");
            DrawProperty(ref rect, property, "LaunchDistanceZ");
            DrawProperty(ref rect, property, "LandingHeight");
            DrawProperty(ref rect, property, "LaunchAngle");
            DrawProperty(ref rect, property, "DetectionRadius");
        }
        else if (type == BoostType.Trampoline)
        {
            SerializedProperty useNormal = property.FindPropertyRelative("UseSurfaceNormal");
            
            DrawProperty(ref rect, property, "UseSurfaceNormal");
            
            using (new EditorGUI.DisabledScope(useNormal.boolValue))
            {
                DrawProperty(ref rect, property, "CustomBounceDirection");
            }

            DrawProperty(ref rect, property, "BounceForce");
            DrawProperty(ref rect, property, "MinImpactSpeed");
            DrawProperty(ref rect, property, "BounceCooldown");
            DrawProperty(ref rect, property, "KeepHorizontalVelocity");
        }
        else if (type == BoostType.Rocket)
        {
            DrawProperty(ref rect, property, "LaunchDistanceX");
            DrawProperty(ref rect, property, "LaunchDistanceZ");
            DrawProperty(ref rect, property, "LandingHeight");
            DrawProperty(ref rect, property, "LaunchAngle");
            DrawProperty(ref rect, property, "LaunchForce");
        }
        else if (type == BoostType.Acceleration)
        {
            DrawProperty(ref rect, property, "AccelerationMultiplier");
            DrawProperty(ref rect, property, "AccelerationDuration");
        }
        else if (type == BoostType.Jump)
        {
            DrawProperty(ref rect, property, "JumpMultiplier");
            DrawProperty(ref rect, property, "JumpDuration");
        }

        EditorGUI.EndProperty();
    }

    private float Height(SerializedProperty parent, string relPath)
    {
        return EditorGUI.GetPropertyHeight(parent.FindPropertyRelative(relPath), true) + spacing;
    }

    private void DrawProperty(ref Rect rect, SerializedProperty parent, string relPath)
    {
        SerializedProperty property = parent.FindPropertyRelative(relPath);
        float h = EditorGUI.GetPropertyHeight(property, true);
        rect.height = h;
        EditorGUI.PropertyField(rect, property, true);
        rect.y += h + spacing;
    }
}