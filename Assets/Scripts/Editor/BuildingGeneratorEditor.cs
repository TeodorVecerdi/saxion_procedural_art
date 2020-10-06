using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingGenerator))]
public class BuildingGeneratorEditor : Editor {
    private bool showGeneratorSettings = true;
    private bool showGeneralSettings = true;
    private bool showSquareBuildingSettings = true;
    private bool showLBuildingSettings = true;

    private bool recalculateStyles;

    private GUIStyle bigLabelStyle;
    private GUIStyle mediumLabelStyle;
    private GUIStyle linkButtonStyle;

    private void OnEnable() {
        recalculateStyles = true;
    }

    public override void OnInspectorGUI() {
        if (bigLabelStyle == null) recalculateStyles = true;
        if (recalculateStyles) {
            bigLabelStyle = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft, fontSize = 18, richText = true};
            mediumLabelStyle = new GUIStyle(bigLabelStyle) {fontSize = 15};
            linkButtonStyle = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleRight, richText = true, normal = {textColor = new Color(0.36f, 0.61f, 0.79f)}};
        }

        var generator = target as BuildingGenerator;
        serializedObject.Update();
        GUILayout.BeginHorizontal();
        
        
        if (generator.GeneratorSettings == null) GUI.enabled = false;
        if (GUILayout.Button("Generate", GUILayout.Height(24)))
            generator.Generate();
        if (GUILayout.Button("Clear", GUILayout.Height(24)))
            generator.Clear();
        GUI.enabled = true;

        GUILayout.EndHorizontal();
        GUILayout.Space(16);
        generator.GeneratorSettings = EditorGUILayout.ObjectField("Random Settings", generator.GeneratorSettings, typeof(RandomSettings), false) as RandomSettings;
        GUILayout.Space(12);

        if (generator.GeneratorSettings != null) {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{generator.GeneratorSettings.name} (RandomSettings)", bigLabelStyle, GUILayout.Height(24));
            if (GUILayout.Button($"[<b>{(showGeneratorSettings ? "hide" : "show")}</b>]", linkButtonStyle, GUILayout.Height(24))) {
                showGeneratorSettings = !showGeneratorSettings;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            if (showGeneratorSettings) {
               // GENERAL SETTINGS
                GUILayout.BeginHorizontal("box");
                GUILayout.Space(16);
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"General Settings", mediumLabelStyle, GUILayout.Height(24));
                if (GUILayout.Button($"[<b>{(showGeneralSettings ? "hide" : "show")}</b>]", linkButtonStyle, GUILayout.Height(24))) {
                    showGeneralSettings = !showGeneralSettings;
                }

                GUILayout.EndHorizontal();
                if (showGeneralSettings) {
                    if (generator.GeneratorSettings.GeneralSettings.AutoSeed)
                        GUI.enabled = false;
                    generator.GeneratorSettings.GeneralSettings.Seed = EditorGUILayout.LongField("Seed", generator.GeneratorSettings.GeneralSettings.Seed);
                    GUI.enabled = true;
                    generator.GeneratorSettings.GeneralSettings.AutoSeed = EditorGUILayout.Toggle("Auto Seed", generator.GeneratorSettings.GeneralSettings.AutoSeed);
                    GUILayout.Space(4);
                    generator.GeneratorSettings.GeneralSettings.SquareChance = EditorGUILayout.FloatField("Square Building Chance", generator.GeneratorSettings.GeneralSettings.SquareChance);
                    generator.GeneratorSettings.GeneralSettings.LChance = EditorGUILayout.FloatField("L Building Chance", generator.GeneratorSettings.GeneralSettings.LChance);
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                // SQUARE BUILDING SETTINGS
                GUILayout.BeginHorizontal("box");
                GUILayout.Space(16);
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Square Building Settings", mediumLabelStyle, GUILayout.Height(24));
                if (GUILayout.Button($"[<b>{(showSquareBuildingSettings ? "hide" : "show")}</b>]", linkButtonStyle, GUILayout.Height(24))) {
                    showSquareBuildingSettings = !showSquareBuildingSettings;
                }

                GUILayout.EndHorizontal();
                if (showSquareBuildingSettings) { }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                
                // SQUARE BUILDING SETTINGS
                GUILayout.BeginHorizontal("box");
                GUILayout.Space(16);
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"L Building Settings", mediumLabelStyle, GUILayout.Height(24));
                if (GUILayout.Button($"[<b>{(showLBuildingSettings ? "hide" : "show")}</b>]", linkButtonStyle, GUILayout.Height(24))) {
                    showLBuildingSettings = !showLBuildingSettings;
                }

                GUILayout.EndHorizontal();
                if (showLBuildingSettings) { }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RegisterCompleteObjectUndo(generator.GeneratorSettings, "Changed random settings");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}