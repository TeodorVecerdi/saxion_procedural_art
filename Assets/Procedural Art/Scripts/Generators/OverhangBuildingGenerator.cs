using System;
using System.Collections.Generic;
using UnityEngine;

public class OverhangBuildingGenerator : BuildingGenerator {
    private OverhangSettings overhangSettings;
    public static new bool DoneOnceField;
    private static float roofHeight;
    private static float overhangHeight;
    private static float overhangGroundOffset;

    public override MeshData Generate(PlotData plot, BuildingTypeSettings settings, float heightAdjustment, Vector3 offset, int LOD) {
        overhangSettings = settings.GeneratorSettings as OverhangSettings;
        DoOnce(ref DoneOnceField);
        var rotation = 0.0f;
        var size = new Vector2Int(Mathf.RoundToInt(plot.Bounds.size.x), Mathf.RoundToInt(plot.Bounds.size.y));
        if (size.x < size.y) {
            var sx = size.x;
            size.x = size.y;
            size.y = sx;
            rotation = 90;
        }
        DimensionsA = new Vector2Int(size.x, size.y);
        DimensionsB = Vector2Int.zero;
        var boolArr = new Arr2d<bool>(DimensionsA.x, DimensionsA.y, true);
        var roof = GenRoof();
        var path = MarchingSquares.March(boolArr);
        CleanupOutline(boolArr);
        var walls = GenWalls(path);
        var features = GenFeatures(path);
        var mesh = MeshUtils.Combine(roof, walls, features);
        mesh.Rotate(Quaternion.Euler(0, rotation, 0), new Vector3(size.y / 2.0f, 0, size.x / 2.0f));
        return mesh;
    }

    public override void DoOnce(ref bool doneOnceField) {
        if (doneOnceField) return;
        doneOnceField = true;
        roofHeight = Rand.Range(overhangSettings.RoofHeightVariation.x, overhangSettings.RoofHeightVariation.y);
        overhangGroundOffset = overhangSettings.GroundOffset + Rand.Range(overhangSettings.GroundOffsetVariation.x, overhangSettings.GroundOffsetVariation.y);
        overhangHeight = overhangSettings.Height + Rand.Range(overhangSettings.HeightVariation.x, overhangSettings.HeightVariation.y);
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
            var wall = MeshGenerator.GetMesh<PlaneGenerator>(to + Vector3.up * overhangGroundOffset, Quaternion.Euler(0, angle - 180, 0), new Dictionary<string, dynamic> {
                {"sizeA", wallWidth},
                {"sizeB", overhangHeight},
                {"orientation", PlaneGenerator.PlaneOrientation.XY},
                {"submeshIndex", 0}
            });

            walls.MergeMeshData(wall);
            current = next;
        }

        return walls;
    }

    private MeshData GenRoof() {
        var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, overhangGroundOffset + overhangHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
            {"width", DimensionsA.x},
            {"height", roofHeight},
            {"thickness", overhangSettings.RoofThickness},
            {"length", DimensionsA.y / 2f},
            {"extrusion", overhangSettings.RoofExtrusion},
            {"addCap", true},
            {"closeRoof", true}
        });
        var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(DimensionsA.x - 0.5f, overhangGroundOffset + overhangHeight, DimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
            {"width", DimensionsA.x},
            {"height", roofHeight},
            {"thickness", overhangSettings.RoofThickness},
            {"length", DimensionsA.y / 2f},
            {"extrusion", overhangSettings.RoofExtrusion},
            {"addCap", true},
            {"closeRoof", true}
        });
        return MeshUtils.Combine(roofA, roofA1);
    }

    private MeshData GenFeatures(List<Vector2Int> path) {
        return new MeshData();
    }
}