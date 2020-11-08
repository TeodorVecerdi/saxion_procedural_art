using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class Readme : MonoBehaviour {
}

[CustomEditor(typeof(Readme))]
public class ReadmeEditor : Editor {
    public override void OnInspectorGUI() {
        var heading = new GUIStyle(GUI.skin.label);
        heading.fontStyle = FontStyle.Bold;
        heading.fontSize = 24;
        var sectionTitle = new GUIStyle(GUI.skin.label);
        sectionTitle.fontStyle = FontStyle.Bold;
        sectionTitle.fontSize = 16;
        var label = new GUIStyle(GUI.skin.label);
        label.wordWrap = true;
        label.fontSize = 14;
        var buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        
        GUILayout.Label("Scene structure", heading);
        GUILayout.BeginVertical();
        GUILayout.Label("Camera Controller", sectionTitle);
        GUILayout.Label("The GameObject \"CameraContainer\" contains a script which rotates the game view camera and offers two different view modes which I thought were appropriate for a city scape. The overall high above view (the one that is visible now) and a low, sea level, view which I think shows the city scape and makes evident the verticality aspect.", label);
        if (GUILayout.Button("Show CameraContainer object", buttonStyle)) {
            EditorGUIUtility.PingObject(20208);
        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("Generator", sectionTitle);
        GUILayout.Label("The \"Builder\" GameObject is responsible for generating the procedural buildings. It has different settings, such as the Plot file (layout of city), general settings (seed/auto seeding) and richness/height maps among many others. Sometimes the internal structures that the script uses get nullified (mostly after assembly reloading) so if you're getting null ref exceptions press the \"Fix Refs\" button.\nFinally, the \"Generate\" button initiates the generation. Warning: you should wait for generation to finish before clicking the generate button again because otherwise weird things might happen.", label);
        if (GUILayout.Button("Show Builder object", buttonStyle)) {
            EditorGUIUtility.PingObject(20298);
        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("Folder structure", heading);
        if (GUILayout.Button("Show Prefabs folder"))
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/Procedural Art/Prefabs"));
        if (GUILayout.Button("Show Resources folder"))
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/Procedural Art/Resources"));
        if(GUILayout.Button("Show General settings"))
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/Procedural Art/Resources/General Settings.asset"));
        if(GUILayout.Button("Show Building settings"))
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/Procedural Art/Resources/Settings"));
        if(GUILayout.Button("Show Material settings"))
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/Procedural Art/Resources/Materials"));
        GUILayout.EndVertical();
    }
}