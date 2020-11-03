using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlotBuildingGenerator : MonoBehaviour {
    public PlotScriptableObject PlotFile;
    public List<BuildingTypeSettings> BuildingTypeSettings;
    [HideInInspector] public List<Transform> Children;
    [HideInInspector] public List<(string name, int buildingType, bool isBuilding)> BuildingTypes;

    // Update is called once per frame
    public void Fix() {
        for (var i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }

        Children = new List<Transform>();
    }

    public void Generate() {
        Children.ForEach(child => DestroyImmediate(child.gameObject));
        Children.Clear();
        for (var i = 0; i < PlotFile.PlotGrids.Count; i++) {
            var buildingSettings = BuildingTypeSettings.Find(settings => settings.PlotLayerName == PlotFile.PlotGrids[i].Name);
            if (buildingSettings == null) continue;
            StartCoroutine(GenerateBuildings(buildingSettings, i));
        }

        /*for (var i = 0; i < PlotFile.PlotGrids.Count; i++) {
            if(!BuildingTypes[i].isBuilding) continue;
            var plotGrid = PlotFile.PlotGrids[i];
            foreach (var plot in plotGrid.Plots) {
                StartCoroutine(GenerateBuilding(plot, BuildingTypes[i].buildingType, transform1 => {
                    transform1.parent = transform;
                    Children.Add(transform1);
                }));
                // var building = Instantiate(Prefab, new Vector3(plot.Bounds.center.x, 0, plot.Bounds.center.y), Quaternion.Euler(0, plot.Rotation, 0), transform);
                // building.GenerateFromPlot(plot, BuildingTypes[i].buildingType, transform.position);
                // Children.Add(building.transform);
            }
        }*/
    }

    public IEnumerator GenerateBuilding(PlotData plot, BuildingTypeSettings settings, int buildingType, int richness, Action<Transform> callback) {
        yield return new WaitForSecondsRealtime(0.0001f);
        var building = Instantiate(settings.GeneratorPrefab, new Vector3(plot.Bounds.center.x, 0, plot.Bounds.center.y), Quaternion.Euler(0, plot.Rotation, 0));
        building.GenerateFromPlot(plot, settings, buildingType, transform.position, richness);
        callback(building.transform);
    }

    public IEnumerator GenerateBuildings(BuildingTypeSettings settings, int plotGridIndex) {
        yield return null;
        var plotGrid = PlotFile.PlotGrids[plotGridIndex];
        foreach (var plot in plotGrid.Plots) {
            var coroutine = StartCoroutine(GenerateBuilding(plot, settings, BuildingTypes[plotGridIndex].buildingType, CalculateRichness(plot), transform1 => {
                transform1.parent = transform;
                Children.Add(transform1);
            }));
            yield return new WaitForSecondsRealtime(0.0001f);
        }
    }

    public int CalculateRichness(PlotData plot) {
        return 0;
    }
}

[CustomEditor(typeof(PlotBuildingGenerator))]
public class PlotBuildingGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var plotter = target as PlotBuildingGenerator;
        if (GUILayout.Button("Fix Refs")) {
            plotter.Fix();
        }

        if (GUILayout.Button("Generate")) {
            plotter.Generate();
        }

        if (GUILayout.Button("Refresh") || plotter.BuildingTypes == null || plotter.BuildingTypes.Count != plotter.PlotFile.PlotGrids.Count) {
            plotter.BuildingTypes = new List<(string name, int buildingType, bool isBuilding)>();
            foreach (var plotGrid in plotter.PlotFile.PlotGrids) {
                plotter.BuildingTypes.Add((plotGrid.Name, 0, false));
            }
        }

        GUILayout.BeginVertical();
        for (var i = 0; i < plotter.BuildingTypes.Count; i++) {
            var pair = plotter.BuildingTypes[i];
            GUILayout.BeginHorizontal();
            GUILayout.Label(pair.name);
            pair.isBuilding = GUILayout.Toggle(pair.isBuilding, "Building");
            pair.buildingType = GUILayout.Toggle(pair.buildingType == 0, "Square Building") ? 0 : 1;
            GUILayout.EndHorizontal();
            plotter.BuildingTypes[i] = pair;
        }

        GUILayout.EndVertical();
    }
}