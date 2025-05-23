using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[CustomEditor(typeof(CustomMetatexImporter))]
public class CustomMetatexImporterEditor : ScriptedImporterEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dimensions"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("generator"));

        var generator = (Generator)serializedObject.FindProperty("generator").enumValueIndex;

        switch (generator)
        {
            case Generator.SolidColor:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("color"));
                break;
            case Generator.LinearGradient:
            case Generator.RadialGradient:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gradient"));
                break;
            case Generator.Checkerboard:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("color"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("color2"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("checkerSize"));
                break;
            case Generator.Shader:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shader"));
                break;
            case Generator.Material:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));
                break;
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("wrapMode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("filterMode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("anisoLevel"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("compression"));

        serializedObject.ApplyModifiedProperties();
        ApplyRevertGUI();
    }
}