using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WallBuildingGenerator : BuildingGenerator {
    private WallSettings wallSettings;
    private static bool doneOnceField;
    private static float roofHeight;

    public override MeshData Generate(PlotData plot, BuildingTypeSettings settings, int buildingType, Vector3 offset) {
        wallSettings = settings.GeneratorSettings as WallSettings;
        DoOnce(ref doneOnceField);

        var size = new Vector2Int(Mathf.RoundToInt(plot.Bounds.size.x), Mathf.RoundToInt(plot.Bounds.size.y));
        dimensionsA = new Vector2Int(size.x, size.y);
        dimensionsB = Vector2Int.zero;
        buildingHeight = wallSettings.Height;
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
        roofHeight = RandUtils.Range(wallSettings.RoofHeightVariation.x, wallSettings.RoofHeightVariation.y);
    }

    public override void Setup(BuildingTypeSettings settings) {
        var wallSettings = settings.GeneratorSettings as WallSettings;
        var seed = wallSettings.Seed;
        if (wallSettings.AutoSeed) {
            seed = DateTime.Now.Ticks;
            wallSettings.Seed = seed;
        }

        Random.InitState((int) seed);
    }

    private new MeshData GenWalls(List<Vector2Int> path) {
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
                {"sizeB", buildingHeight},
                {"orientation", PlaneGenerator.PlaneOrientation.XY},
                {"submeshIndex", 0}
            });

            walls.MergeMeshData(wall);
            current = next;
        }

        return walls;
    }

    private MeshData GenRoof() {
        var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
            {"width", dimensionsA.x},
            {"height", roofHeight},
            {"thickness", wallSettings.RoofThickness},
            {"length", dimensionsA.y / 2f},
            {"extrusion", wallSettings.RoofExtrusion},
            {"addCap", true},
            {"closeRoof", true}
        });
        var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
            {"width", dimensionsA.x},
            {"height", roofHeight},
            {"thickness", wallSettings.RoofThickness},
            {"length", dimensionsA.y / 2f},
            {"extrusion", wallSettings.RoofExtrusion},
            {"addCap", true},
            {"closeRoof", true}
        });
        return MeshUtils.Combine(roofA, roofA1);
    }

    private MeshData GenFeatures(List<Vector2Int> path) {
        return new MeshData();
    }
}