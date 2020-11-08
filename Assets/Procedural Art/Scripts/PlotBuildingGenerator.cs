using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlotBuildingGenerator : MonoBehaviour {
    public PlotScriptableObject PlotFile;
    public GeneralSettings GeneralSettings;
    public List<BuildingVersions> BuildingVersions;
    [Space]
    public Texture2D RichnessMap;
    [Tooltip("Generate a random richness map using perlin noise instead of the recommended map")]
    public bool GenerateRichnessMap;
    [Range(0f, 1f)] public float RandomRichnessChance = 0.25f;
    public List<string> IgnoreRandomRichness;
    [Space]
    public Texture2D HeightMap;
    [Tooltip("Generate a random height map using perlin noise instead of the recommended map")]
    public bool GenerateHeightMap;
    public float HeightMapMagnitude = 2f;
    [Space]
    public bool PlaceOnMinY;

    private List<Transform> children;
    private Color[] richnessColors;
    private int richnessMapSize;
    private Color[] heightColors;
    private int heightMapSize;

    // Update is called once per frame
    public void Fix() {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
        children = new List<Transform>();
    }

    public void Generate() {
        if (GenerateRichnessMap) {
            Rand.PushState();
            richnessColors = PerlinGenerator.Generate(1024, 300, Rand.Range(0, (float)(1<<12)));
            Rand.PopState();
            richnessMapSize = 1024;
        } else {
            richnessColors = RichnessMap.GetPixels();
            richnessMapSize = RichnessMap.width;
        }

        if (GenerateHeightMap) {
            Rand.PushState();
            heightColors = PerlinGenerator.Generate(1024, 300, Rand.Range(0, (float)(1<<12)));
            Rand.PopState();
            heightMapSize = 1024;
        } else {
            heightColors = HeightMap.GetPixels();
            heightMapSize = HeightMap.width;
        }

        var seed = GeneralSettings.Seed;
        if (GeneralSettings.AutoSeed) {
            seed = DateTime.Now.Ticks;
            GeneralSettings.Seed = seed;
        }

        Rand.PushState((int) seed);
        try {
            children.ForEach(child => DestroyImmediate(child.gameObject));
            children.Clear();
        } catch (Exception e) {
            Debug.LogError("Internal data ended up as null (probably from an assembly reload). Press the <<Fix Refs>> button on the Generator");
            throw;
        }
        // Reset static fields
        BuildingGenerator.DoneOnceField = false;
        PoorSBuildingGenerator.DoneOnceField = false;
        NormalSBuildingGenerator.DoneOnceField = false;
        LBuildingGenerator.DoneOnceField = false;
        PoorLBuildingGenerator.DoneOnceField = false;
        NormalLBuildingGenerator.DoneOnceField = false;
        WallBuildingGenerator.DoneOnceField = false;
        TowerBuildingGenerator.DoneOnceField = false;
        OverhangBuildingGenerator.DoneOnceField = false;

        for (var i = 0; i < PlotFile.PlotGrids.Count; i++) {
            var buildingVersions = BuildingVersions.Find(settings => settings.PlotLayerName == PlotFile.PlotGrids[i].Name);
            if (buildingVersions == null) continue;
            StartCoroutine(GenerateBuildings(buildingVersions, i));
        }
    }

    public IEnumerator GenerateBuilding(PlotData plot, BuildingTypeSettings settings, float heightAdjustment, Action<Transform> callback) {
        yield return new WaitForSecondsRealtime(0);
        var building = Instantiate(settings.GeneratorPrefab, new Vector3(plot.Bounds.center.x, transform.position.y, plot.Bounds.center.y), Quaternion.Euler(0, plot.Rotation, 0));
        PlaceBuilding(plot, building);
        building.GenerateFromPlot(plot, settings, heightAdjustment, transform.position);
        callback(building.transform);
    }

    private void PlaceBuilding(PlotData plot, BuildingGenerator building) {
        var (leftDown, rightDown, rightUp, leftUp) = MathUtils.RotatedRectangle(plot);
        var averageY = 0f;
        var count = 0;
        var minY = 1000f;
        if (Physics.Raycast(leftDown + Vector3.up * 200, Vector3.down, out var hitInfo1, LayerMask.GetMask("Terrain"))) {
            averageY += hitInfo1.point.y;
            minY = Math.Min(minY, hitInfo1.point.y);
            count++;
        }
        if (Physics.Raycast(leftUp + Vector3.up * 200, Vector3.down, out var hitInfo2, LayerMask.GetMask("Terrain"))) {
            averageY += hitInfo2.point.y;
            minY = Math.Min(minY, hitInfo2.point.y);
            count++;
        }
        if (Physics.Raycast(rightDown + Vector3.up * 200, Vector3.down, out var hitInfo3, LayerMask.GetMask("Terrain"))) {
            averageY += hitInfo3.point.y;
            minY = Math.Min(minY, hitInfo3.point.y);
            count++;
        }
        if (Physics.Raycast(rightUp + Vector3.up * 200, Vector3.down, out var hitInfo4, LayerMask.GetMask("Terrain"))) {
            averageY += hitInfo4.point.y;
            minY = Math.Min(minY, hitInfo4.point.y);
            count++;
        }

        averageY /= count;
        var currentPosition =building.transform.position;
        currentPosition.y = PlaceOnMinY ? minY : averageY;
        building.transform.position = currentPosition;
    }

    public IEnumerator GenerateBuildings(BuildingVersions settings, int plotGridIndex) {
        var plotGrid = PlotFile.PlotGrids[plotGridIndex];
        foreach (var plot in plotGrid.Plots) {
            var richness = (int) SampleHeightMap(plot, richnessColors, richnessMapSize, 3);
            if (RandUtils.BoolWeighted(RandomRichnessChance) && !IgnoreRandomRichness.Contains(plotGrid.Name))
                richness = Rand.RangeInclusive(0, 3);
            var heightAdjustment = SampleHeightMap(plot, heightColors, heightMapSize, HeightMapMagnitude);
            StartCoroutine(GenerateBuilding(plot, settings.Select(richness), heightAdjustment, transform1 => {
                transform1.parent = transform;
                children.Add(transform1);
                transform1.gameObject.name += $"__{richness}";
            }));
            yield return new WaitForSecondsRealtime(0);
        }
    }

    public float SampleHeightMap(PlotData plot, Color[] colors, int mapSize, float maxValue) {
        var center = plot.Bounds.center.Map(-200, 200, 0, mapSize).ToV2I(false, true);
        var radiusVec = (plot.Bounds.size / 2.0f).Map(0, 400, 0, mapSize);
        var radius = radiusVec.magnitude;
        var r = (int) radius;
        var r2 = (int) (radius * radius);

        int count = 0;
        float value = 0f;

        for (int i = center.y - r; i <= center.y + r; i++) {
            // test upper half of circle, stopping when top reached
            for (int j = center.x; (j - center.x) * (j - center.x) + (i - center.y) * (i - center.y) <= r2; j--) {
                value += colors[i * mapSize + j].grayscale;
                count++;
            }

            // test bottom half of circle, stopping when bottom reached
            for (int j = center.x + 1; (j - center.x) * (j - center.x) + (i - center.y) * (i - center.y) <= r2; j++) {
                value += colors[i * mapSize + j].grayscale;
                count++;
            }
        }

        value /= count;
        return Mathf.RoundToInt(value * maxValue);
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
    }
}