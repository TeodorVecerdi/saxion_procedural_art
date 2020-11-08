using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Rendering;
using UnityEngine;

[CustomEditor(typeof(PlotCreator))]
public class PlotCreatorInspector : Editor {
    private bool alreadySetTool;

    private void OnEnable() {
        alreadySetTool = false;
    }

    private void OnSceneGUI() {
        if (EditorTools.activeToolType != typeof(PlotCreatorTool) && !alreadySetTool && (target as PlotCreator).IsEnabled) {
            EditorTools.SetActiveTool<PlotCreatorTool>();
            alreadySetTool = true;
        }
    }

    public override void OnInspectorGUI() {
        var plotCreator = target as PlotCreator;
        if (plotCreator.PlotGrids.Count == 0) {
            plotCreator.PlotGrids.Add(new PlotGrid {Name = "Default", Color = Color.green, Plots = new List<Plot>()});
            plotCreator.SelectedPlotGridIndex = 0;
        }
        if (GUILayout.Button(plotCreator.IsEnabled ? "Disable tool" : "Enable tool")) {
            plotCreator.IsEnabled = !plotCreator.IsEnabled;
        }

        if (GUILayout.Button("Fix plot alignment")) {
            foreach (var plotGrid in plotCreator.PlotGrids)
            foreach (var plot in plotGrid.Plots) {
                plot.Bounds.position = plot.Bounds.position.ClosestGridPoint(true);
            }
        }

        if (GUILayout.Button("Save plots to file")) {
            var scriptableObject = CreateInstance<PlotScriptableObject>();
            scriptableObject.PlotGrids = new List<PlotGridData>();
            foreach (var plotGrid in plotCreator.PlotGrids) {
                var plotGridData = new PlotGridData {Color = plotGrid.Color, Name = plotGrid.Name, Plots = new List<PlotData>()};
                plotGridData.Plots.AddRange(plotGrid.Plots.Select(plot => new PlotData {Bounds = plot.Bounds, Rotation = plot.Rotation}));
                scriptableObject.PlotGrids.Add(plotGridData);
            }

            var path = EditorUtility.SaveFilePanelInProject("Save plot file", "New plot file", "asset", "");
            if (path.Length != 0) {
                AssetDatabase.CreateAsset(scriptableObject, path);
            }
        }

        if (GUILayout.Button("Load plots from file")) {
            if (plotCreator.PlotGrids.Count > 0 && EditorUtility.DisplayDialog("Override Existing Plots", "Are you sure you want to override existing plots by loading a plot file?", "Yes", "No") || plotCreator.PlotGrids.Count == 0) {
                var path = EditorUtility.OpenFilePanel("Load plot file", "Assets/", "asset");
                if (path.Length != 0) {
                    if (path.StartsWith(Application.dataPath)) {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    }

                    var asset = AssetDatabase.LoadAssetAtPath<PlotScriptableObject>(path);
                    if (asset != null) {
                        plotCreator.PlotGrids = new List<PlotGrid>();
                        asset.PlotGrids.ForEach(data => {
                            var plotGrid = new PlotGrid {Color = data.Color, Name = data.Name, Plots = new List<Plot>()};
                            plotGrid.Plots.AddRange(data.Plots.Select(plotData => Plot.FromStartEnd(plotData.Bounds.min, plotData.Bounds.max, plotData.Rotation)));
                            plotCreator.PlotGrids.Add(plotGrid);
                        });
                        plotCreator.SelectedPlotGridIndex = 0;
                    } else {
                        EditorUtility.DisplayDialog("Invalid File", "The file you are trying to open is not a valid plot file!", "OK");
                    }
                }
            }
        }

        GUILayout.BeginVertical("Plot Grids", GUIStyle.none);
        if (GUILayout.Button("New layer")) {
            plotCreator.PlotGrids.Add(new PlotGrid {Name ="New Grid", Color = Color.magenta, Plots = new List<Plot>()});
        }

        var copy = new List<PlotGrid>(plotCreator.PlotGrids);
        for (var i = 0; i < copy.Count; i++) {
            GUILayout.BeginHorizontal();
            GUI.enabled = i != plotCreator.SelectedPlotGridIndex;
            if (GUILayout.Button($"Select")) {
                plotCreator.SelectedPlotGridIndex = i;
            }

            GUI.enabled = true;
            plotCreator.PlotGrids[i].Color = EditorGUILayout.ColorField(plotCreator.PlotGrids[i].Color);
            plotCreator.PlotGrids[i].Name = EditorGUILayout.TextField(plotCreator.PlotGrids[i].Name);

            if (GUILayout.Button("x") && copy.Count > 1) {
                plotCreator.PlotGrids.RemoveAt(i);
                plotCreator.SelectedPlotGridIndex = 0;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        base.OnInspectorGUI();
    }
}