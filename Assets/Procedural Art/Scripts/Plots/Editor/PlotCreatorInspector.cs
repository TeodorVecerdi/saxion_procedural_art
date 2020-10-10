using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[CustomEditor(typeof(PlotCreator))]
public class PlotCreatorInspector : Editor {
    private bool alreadySetTool;

    private void OnEnable() {
        alreadySetTool = false;
    }

    private void OnSceneGUI() {
        if (EditorTools.activeToolType != typeof(PlotCreatorTool) && !alreadySetTool) {
            EditorTools.SetActiveTool<PlotCreatorTool>();
            alreadySetTool = true;
        } 
    }

    public override void OnInspectorGUI() {
        var plotCreator = target as PlotCreator;
        if (GUILayout.Button(plotCreator.IsEnabled ? "Disable tool" : "Enable tool")) {
            plotCreator.IsEnabled = !plotCreator.IsEnabled;
        }

        if (GUILayout.Button("Save plots to file")) {
            var scriptableObject = CreateInstance<PlotScriptableObject>();
            scriptableObject.Plots = new List<Rect>();
            scriptableObject.Plots.AddRange(plotCreator.Plots.Select(plot => plot.Bounds));

            var path = EditorUtility.SaveFilePanelInProject("Save plot file", "New plot file", "asset", "");
            if (path.Length != 0) {
                AssetDatabase.CreateAsset(scriptableObject, path);
            }
        }

        if (GUILayout.Button("Load plots from file")) {
            if (plotCreator.Plots.Count > 0 && EditorUtility.DisplayDialog("Override Existing Plots", "Are you sure you want to override existing plots by loading a plot file?", "Yes", "No") || plotCreator.Plots.Count == 0) {
                var path = EditorUtility.OpenFilePanel("Load plot file", "Assets/", "asset");
                if (path.Length != 0) {
                    if (path.StartsWith(Application.dataPath)) {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    }

                    var asset = AssetDatabase.LoadAssetAtPath<PlotScriptableObject>(path);
                    if (asset != null) {
                        plotCreator.Plots = new List<Plot>();
                        plotCreator.Plots.AddRange(asset.Plots.Select(rect => Plot.FromStartEnd(rect.min, rect.max)));
                    } else {
                        EditorUtility.DisplayDialog("Invalid File", "The file you are trying to open is not a valid plot file!", "OK");
                    }
                }
            }
        }

        base.OnInspectorGUI();
    }
}