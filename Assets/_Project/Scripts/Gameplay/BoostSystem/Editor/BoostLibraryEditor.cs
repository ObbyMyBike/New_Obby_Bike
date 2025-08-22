using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(BoostLibrary))]
public class BoostLibraryEditor : Editor
{
    private ReorderableList list;

    private void OnEnable()
    {
        SerializedProperty presetsProperty = serializedObject.FindProperty("Presets");

        list = new ReorderableList(serializedObject, presetsProperty, true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Presets");
        list.elementHeightCallback = index =>
        {
            SerializedProperty element = presetsProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 6f;
        };
        list.drawElementCallback = (rect, index, active, focused) =>
        {
            rect.y += 2f;
            SerializedProperty element = presetsProperty.GetArrayElementAtIndex(index);
            SerializedProperty nameProp = element.FindPropertyRelative("PresetName");
            
            EditorGUI.PropertyField(rect, element, new GUIContent(nameProp.stringValue), true);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}