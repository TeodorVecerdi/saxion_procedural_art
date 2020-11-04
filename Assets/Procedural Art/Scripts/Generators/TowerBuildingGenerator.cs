using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerBuildingGenerator : BuildingGenerator {
    private WallSettings towerSettings;
    public static new bool DoneOnceField;
    private static float roofHeight;
    private static float towerHeight;

    public override MeshData Generate(PlotData plot, BuildingTypeSettings settings, float heightAdjustment, Vector3 offset, int LOD) {
        towerSettings = settings.GeneratorSettings as WallSettings;
        DoOnce(ref DoneOnceField);

        var size = new Vector2Int(Mathf.RoundToInt(plot.Bounds.size.x), Mathf.RoundToInt(plot.Bounds.size.y));
        dimensionsA = new Vector2Int(size.x, size.y);
        dimensionsB = Vector2Int.zero;
        var boolArr = new Arr2d<bool>(dimensionsA.x, dimensionsA.y, true);
        var roof = GenRoof();
        var path = MarchingSquares.March(boolArr);
        CleanupOutline(boolArr);
        var walls = GenWalls(path);
        var features = GenFeatures(path);

        return MeshUtils.Combine(roof, walls, features);
    }

    public override void DoOnce(ref bool doneOnceField) {
        if (doneOnceField) return;
        doneOnceField = true;
        roofHeight = Rand.Range(towerSettings.RoofHeightVariation.x, towerSettings.RoofHeightVariation.y);
        towerHeight = towerSettings.Height + Rand.Range(towerSettings.HeightVariation.x, towerSettings.HeightVariation.y);
    }

    public override void Setup(BuildingTypeSettings settings) {
        
    }

    private MeshData GenWalls(List<Vector2Int> path) {
        var walls = new MeshData();
        var current = Vector2Int.zero;
        foreach (var point in path) {
            var next = current + point;
            var from = new Vector3(current.x - 0.5f, 0, current.y - 0.5f);
            var to = new Vector3(next.x - 0.5f, 0, next.y - 0.5f);
            var diff = to - from;
            var angle = Vector3.SignedAngle(Vector3.right, diff, Vector3.up);
            var wallWidth = diff.magnitude;
            var wall = MeshGenerator.GetMesh<PlaneGenerator>(to, Quaternion.Euler(0, angle - 180, 0), new Dictionary<string, dynamic> {
                {"sizeA", wallWidth},
                {"sizeB", towerHeight},
                {"orientation", PlaneGenerator.PlaneOrientation.XY},
                {"submeshIndex", 0}
            });

            walls.MergeMeshData(wall);
            current = next;
        }

        return walls;
    }

    private MeshData GenRoof() {
        var cornerA = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, towerHeight, -0.5f), Quaternion.Euler(0, -90, 0), new Dictionary<string, dynamic> {
            {"width", dimensionsA.y / 2f},
            {"height", roofHeight},
            {"length", dimensionsA.x / 2.0f},
            {"thickness", towerSettings.RoofThickness},
            {"addCap", true},
            {"joinCaps", true}
        });
        var cornerA1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, towerHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
            {"width", dimensionsA.x / 2.0f},
            {"height", roofHeight},
            {"length", dimensionsA.y / 2f},
            {"thickness", towerSettings.RoofThickness},
            {"addCap", true},
            {"joinCaps", true}
        });
        var cornerB = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(-0.5f, towerHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
            {"width", dimensionsA.x / 2.0f},
            {"height", roofHeight},
            {"length", dimensionsA.y / 2f},
            {"thickness", towerSettings.RoofThickness},
            {"addCap", true},
            {"joinCaps", true}
        });
        var cornerB1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(-0.5f, towerHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
            {"width", dimensionsA.y / 2f},
            {"height", roofHeight},
            {"length", dimensionsA.x / 2.0f},
            {"thickness", towerSettings.RoofThickness},
            {"addCap", true},
            {"joinCaps", true}
        });
        return MeshUtils.Combine(cornerA, cornerA1, cornerB, cornerB1);
    }

    private MeshData GenFeatures(List<Vector2Int> path) {
        return new MeshData();
    }
}