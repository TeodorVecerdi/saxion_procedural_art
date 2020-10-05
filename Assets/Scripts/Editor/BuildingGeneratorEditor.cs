using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingGenerator))]
public class BuildingGeneratorEditor : Editor {
    private bool showGeneratorSettings = true;
    private SerializedProperty randomSettings;
    private SerializedProperty features;

    private void OnEnable() {
        randomSettings = serializedObject.FindProperty("GeneratorSettings");
        features = serializedObject.FindProperty("Features");
    }

    public override void OnInspectorGUI() {
        // base.OnInspectorGUI();
        var generator = target as BuildingGenerator;
        serializedObject.Update();
        if (GUILayout.Button("Generate", GUILayout.Height(24)))
            generator.Generate();
        GUILayout.Space(16);
        generator.GeneratorSettings = EditorGUILayout.ObjectField("Random Settings", generator.GeneratorSettings, typeof(RandomSettings), false) as RandomSettings;
        generator.Features = EditorGUILayout.ObjectField("Feature Settings", generator.Features, typeof(FeatureSettings), false) as FeatureSettings;
        GUILayout.Space(12);

        var style = EditorStyles.largeLabel;
        style.alignment = TextAnchor.MiddleLeft;
        style.fontSize = 16;
        var secondStyle = EditorStyles.linkLabel;
        secondStyle.alignment = TextAnchor.MiddleRight;
        if (randomSettings.objectReferenceValue != null) {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{generator.GeneratorSettings.name} (RandomSettings)", style, GUILayout.Height(24));
            if (GUILayout.Button($"[{(showGeneratorSettings ? "hide" : "show")}]", secondStyle, GUILayout.Height(24))) {
                showGeneratorSettings = !showGeneratorSettings;
            }
            GUILayout.EndHorizontal();
            if (showGeneratorSettings) {
                GUILayout.Space(4);
                if (generator.GeneratorSettings.GeneralSettings.AutoSeed)
                    GUI.enabled = false;
                generator.GeneratorSettings.GeneralSettings.Seed = EditorGUILayout.LongField("Seed", generator.GeneratorSettings.GeneralSettings.Seed);
                GUI.enabled = true;
                generator.GeneratorSettings.GeneralSettings.AutoSeed = EditorGUILayout.Toggle("Auto Seed", generator.GeneratorSettings.GeneralSettings.AutoSeed);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}