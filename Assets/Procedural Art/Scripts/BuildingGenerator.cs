using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BuildingGenerator : MonoBehaviour {
    public RandomSettings GeneratorSettings;

    private MeshFilter meshFilter;
    private Mesh mesh;

    private WeightedRandom buildingTypeSelector;

    private int buildingHeight;
    private Vector2Int dimensionsA;
    private Vector2Int dimensionsB;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void Clear() {
        mesh.Clear();
    }

    public void GenerateFromPlot(PlotData plot, int buildingType, Vector3 offset) {
        if (mesh != null)
            mesh.Clear();
        Setup();
        var size = new Vector2Int(Mathf.RoundToInt(plot.Bounds.size.x), Mathf.RoundToInt(plot.Bounds.size.y));
        var boolArr = buildingType == 0 ? GenSquare(size) : GenL(size);
        var overhang = buildingType == 0 ? GenSquareOverhang(boolArr) : GenLOverhang();
        var roofs = buildingType == 0 ? GenSquareRoof() : GenLRoof();
        var path = MarchingSquares.March(boolArr);
        CleanupOutline(boolArr);
        var walls = GenWalls(path);

        var combinedMesh = MeshUtils.Combine(roofs, walls, overhang);
        var finalMesh = MeshUtils.Translate(combinedMesh, -0.5f*plot.Bounds.size.ToVec3() + new Vector3(0.5f, 0, 0.5f) + offset);
        mesh = new Mesh {name = "Building"};

        if (meshFilter.sharedMesh != null)
            DestroyImmediate(meshFilter.sharedMesh);
        meshFilter.sharedMesh = mesh;
        mesh.SetVertices(finalMesh.Vertices);
        mesh.SetUVs(0, finalMesh.UVs);
        mesh.subMeshCount = GetComponent<MeshRenderer>().sharedMaterials.Length;
        foreach (var submesh in finalMesh.Triangles.Keys) {
            mesh.SetTriangles(finalMesh.Triangles[submesh], submesh);
        }

        mesh.RecalculateNormals();
    }

    public void Generate() {
        if (GeneratorSettings == null) {
            throw new Exception("Generator Settings cannot be null! Make sure to assign a RandomSettings object to the class before calling BuildingGenerator::Generate");
        }

        if (mesh != null)
            mesh.Clear();
        Setup();
        var buildingType = buildingTypeSelector.Value();
        var boolArr = buildingType == 0 ? GenSquare() : GenL();
        var overhang = buildingType == 0 ? GenSquareOverhang(boolArr) : GenLOverhang();
        var roofs = buildingType == 0 ? GenSquareRoof() : GenLRoof();
        var path = MarchingSquares.March(boolArr);
        CleanupOutline(boolArr);
        var walls = GenWalls(path);

        var finalMesh = MeshUtils.Combine(roofs, walls, overhang);

        mesh = new Mesh {name = "Building"};

        if (meshFilter.sharedMesh != null)
            DestroyImmediate(meshFilter.sharedMesh);
        meshFilter.sharedMesh = mesh;
        mesh.SetVertices(finalMesh.Vertices);
        mesh.SetUVs(0, finalMesh.UVs);
        mesh.subMeshCount = GetComponent<MeshRenderer>().sharedMaterials.Length;
        foreach (var submesh in finalMesh.Triangles.Keys) {
            mesh.SetTriangles(finalMesh.Triangles[submesh], submesh);
        }

        mesh.RecalculateNormals();
    }

    private void Setup() {
        // Seed
        var seed = GeneratorSettings.GeneralSettings.Seed;
        if (GeneratorSettings.GeneralSettings.AutoSeed) {
            seed = DateTime.Now.Ticks;
            GeneratorSettings.GeneralSettings.Seed = seed;
        }

        Random.InitState((int) seed);

        // Random Weight adjusting
        buildingTypeSelector = new WeightedRandom(GeneratorSettings.GeneralSettings.SquareChance, GeneratorSettings.GeneralSettings.LChance, GeneratorSettings.GeneralSettings.TChance);
        buildingTypeSelector.NormalizeWeights();
        buildingTypeSelector.CalculateAdditiveWeights();
    }

    private Arr2d<bool> GenSquare(Vector2Int size) {
        buildingHeight = RandUtils.RangeInclusive(GeneratorSettings.SquareBuildingSettings.MinSize.y, GeneratorSettings.SquareBuildingSettings.MaxSize.y);
        dimensionsA = new Vector2Int(size.x, size.y);
        dimensionsB = Vector2Int.zero;

        var boolArr = new Arr2d<bool>(size.x, size.y, true);
        return boolArr;
    }

    private Arr2d<bool> GenSquare() {
        var size = RandUtils.RandomBetween(GeneratorSettings.SquareBuildingSettings.MinSize, GeneratorSettings.SquareBuildingSettings.MaxSize);
        buildingHeight = size.y;
        dimensionsA = new Vector2Int(size.x, size.z);
        dimensionsB = Vector2Int.zero;

        var boolArr = new Arr2d<bool>(size.x, size.z, true);
        return boolArr;
    }
    
    private Arr2d<bool> GenL(Vector2Int size) {
        var widthA = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxWidthA);
        var lengthA = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxLengthA);
        var widthB = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxWidthB);

        // widthB = MathUtils.Clamp(widthB, 2, widthA - 1);
        var lengthB = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxLengthB);

        // lengthB = MathUtils.Clamp(lengthB, 1, Mathf.CeilToInt(lengthA / 2f));
        buildingHeight = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxHeight);
        
        // normalize size so it's in range of <size>
        var proportionWA = widthA / (float)(widthA + widthB);
        var proportionWB = widthB / (float)(widthA + widthB);
        var proportionLA = lengthA / (float)(lengthA + lengthB);
        var proportionLB = lengthB / (float)(lengthA + lengthB);
        var actualWidthA = Mathf.CeilToInt(proportionWA * size.x);
        var actualWidthB = Mathf.FloorToInt(proportionWB * size.x);
        var actualLengthA = Mathf.CeilToInt(proportionLA * size.y);
        var actualLengthB = Mathf.FloorToInt(proportionLB * size.y);
        actualWidthA += (size.x - actualWidthA - actualWidthB);
        actualLengthA += (size.y - actualLengthA - actualLengthB);
        
        
        dimensionsA = new Vector2Int(actualWidthA + actualWidthB, actualLengthA + actualLengthB);
        dimensionsB = new Vector2Int(actualWidthB, actualLengthB);
        var boolArr = new Arr2d<bool>(dimensionsA, true);
        CarveLShape(boolArr);

        return boolArr;
    }

    private Arr2d<bool> GenL() {
        var widthA = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxWidthA);
        var lengthA = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxLengthA);
        var widthB = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxWidthB);

        // widthB = MathUtils.Clamp(widthB, 2, widthA - 1);
        var lengthB = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxLengthB);

        // lengthB = MathUtils.Clamp(lengthB, 1, Mathf.CeilToInt(lengthA / 2f));
        buildingHeight = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxHeight);

        dimensionsA = new Vector2Int(widthA + widthB, lengthA + lengthB);
        dimensionsB = new Vector2Int(widthB, lengthB);
        var boolArr = new Arr2d<bool>(dimensionsA, true);
        CarveLShape(boolArr);

        return boolArr;
    }

    private MeshData GenSquareRoof() {
        var height = 1f;
        var thickness = 0.15f;
        var extrusion = 0.25f;
        var straightChance = 0.333333f;
        var doubleCornerChance = 0.5f;

        if (RandUtils.BoolWeighted(straightChance)) {
            var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", dimensionsA.x},
                {"height", height},
                {"thickness", thickness},
                {"length", dimensionsA.y / 2f},
                {"extrusion", extrusion},
                {"addCap", true},
                {"closeRoof", true}
            });
            var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", dimensionsA.x},
                {"height", height},
                {"thickness", thickness},
                {"length", dimensionsA.y / 2f},
                {"extrusion", extrusion},
                {"addCap", true},
                {"closeRoof", true}
            });
            return MeshUtils.Combine(roofA, roofA1);
        } else {
            if (RandUtils.BoolWeighted(doubleCornerChance)) {
                var cornerWidth = RandUtils.Range(dimensionsA.x / 4f, dimensionsA.x / 2f);
                var cornerWidthB = RandUtils.Range(dimensionsA.x / 4f, dimensionsA.x / 2f);
                var cornerA = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, -0.5f), Quaternion.Euler(0, -90, 0), new Dictionary<string, dynamic> {
                    {"width", dimensionsA.y / 2f},
                    {"height", height},
                    {"length", cornerWidth},
                    {"thickness", thickness},
                    {"addCap", true},
                    {"joinCaps", true}
                });
                var cornerA1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                    {"width", cornerWidth},
                    {"height", height},
                    {"length", dimensionsA.y / 2f},
                    {"thickness", thickness},
                    {"addCap", true},
                    {"joinCaps", true}
                });
                var cornerB = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(-0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                    {"width", cornerWidthB},
                    {"height", height},
                    {"length", dimensionsA.y / 2f},
                    {"thickness", thickness},
                    {"addCap", true},
                    {"joinCaps", true}
                });
                var cornerB1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(-0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                    {"width", dimensionsA.y / 2f},
                    {"height", height},
                    {"length", cornerWidthB},
                    {"thickness", thickness},
                    {"addCap", true},
                    {"joinCaps", true}
                });
                var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(cornerWidthB - 0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                    {"width", dimensionsA.x - cornerWidth - cornerWidthB},
                    {"height", height},
                    {"thickness", thickness},
                    {"length", dimensionsA.y / 2f},
                    {"extrusion", 0},
                    {"addCap", true},
                    {"closeRoof", true}
                });
                var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(dimensionsA.x - cornerWidth - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                    {"width", dimensionsA.x - cornerWidth - cornerWidthB},
                    {"height", height},
                    {"thickness", thickness},
                    {"length", dimensionsA.y / 2f},
                    {"extrusion", 0},
                    {"addCap", true},
                    {"closeRoof", true}
                });
                return MeshUtils.Combine(cornerA, cornerA1, cornerB, cornerB1, roofA, roofA1);
            } else {
                var cornerWidth = RandUtils.Range(dimensionsA.x / 4f, dimensionsA.x / 2f);
                var cornerA = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, -0.5f), Quaternion.Euler(0, -90, 0), new Dictionary<string, dynamic> {
                    {"width", dimensionsA.y / 2f},
                    {"height", height},
                    {"length", cornerWidth},
                    {"thickness", thickness},
                    {"addCap", true},
                    {"joinCaps", true}
                });
                var cornerB = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                    {"width", cornerWidth},
                    {"height", height},
                    {"length", dimensionsA.y / 2f},
                    {"thickness", thickness},
                    {"addCap", true},
                    {"joinCaps", true}
                });
                var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                    {"width", dimensionsA.x - cornerWidth},
                    {"height", height},
                    {"thickness", thickness},
                    {"length", dimensionsA.y / 2f},
                    {"extrusion", extrusion},
                    {"addCap", true},
                    {"extrusionRight", false},
                    {"closeRoof", true}
                });
                var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                    {"width", dimensionsA.x - cornerWidth},
                    {"height", height},
                    {"thickness", thickness},
                    {"length", dimensionsA.y / 2f},
                    {"extrusion", extrusion},
                    {"extrusionRight", false},
                    {"addCap", true},
                    {"flip", true},
                    {"closeRoof", true}
                });
                return MeshUtils.Combine(cornerA, cornerB, roofA, roofA1);
            }
        }
    }

    private MeshData GenLRoof() {
        var height = 1.5f;
        var thickness = 0.15f;
        var extrusion = 0.25f;
        var cornerRoofEndingChance = 0.0f;

        var roofs = new List<MeshData>();
        if (RandUtils.BoolWeighted(cornerRoofEndingChance)) {
            var roofInset = RandUtils.Range(dimensionsB.x / 2f, dimensionsB.x);
            var roofSizeB = (dimensionsA.x - roofInset) - (dimensionsA.x - dimensionsB.x) / 2f;
            var cornerA = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", roofInset},
                {"height", height},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"thickness", thickness},
                {"addCap", true},
                {"joinCaps", true},
            });
            var cornerA1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, -0.5f), Quaternion.Euler(0, 270, 0), new Dictionary<string, dynamic> {
                {"width", (dimensionsA.y - dimensionsB.y) / 2f},
                {"height", height},
                {"length", roofInset},
                {"thickness", thickness},
                {"addCap", true},
                {"joinCaps", true},
            });

            var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(dimensionsA.x - roofSizeB - roofInset - 0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", roofSizeB},
                {"height", height},
                {"thickness", thickness},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"extrusion", 0},
                {"addCap", true},
                {"closeRoof", true}
            });
            var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(dimensionsA.x - roofInset - 0.5f, buildingHeight, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", roofSizeB},
                {"height", height},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"thickness", thickness},
                {"extrusion", 0},
                {"addCap", true},
                {"closeRoof", true}
            });
            roofs.Add(cornerA);
            roofs.Add(cornerA1);
            roofs.Add(roofA);
            roofs.Add(roofA1);
        } else {
            var roofSize = (dimensionsA.x) - (dimensionsA.x - dimensionsB.x) / 2f;

            // var roofInsetB = roofInset;
            var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(dimensionsA.x - roofSize - 0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", roofSize},
                {"height", height},
                {"thickness", thickness},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"extrusion", extrusion},
                {"extrusionLeft", false},
                {"addCap", true},
                {"closeRoof", true}
            });
            var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", roofSize},
                {"height", height},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"thickness", thickness},
                {"extrusion", extrusion},
                {"extrusionRight", false},
                {"addCap", true},
                {"closeRoof", true}
            });
            roofs.Add(roofA);
            roofs.Add(roofA1);
        }

        var cornerB = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(-0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
            {"width", (dimensionsA.x - dimensionsB.x) / 2f},
            {"height", height},
            {"length", (dimensionsA.y - dimensionsB.y) / 2f},
            {"thickness", thickness},
            {"addCap", true},
            {"joinCaps", true}
        });

        roofs.Add(cornerB);

        var roofB = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
            {"width", dimensionsB.y + (dimensionsA.y - dimensionsB.y) / 2f},
            {"height", height},
            {"length", (dimensionsA.x - dimensionsB.x) / 2f},
            {"thickness", thickness},
            {"extrusion", extrusion},
            {"extrusionRight", false},
            {"addCap", true},
            {"closeRoof", true}
        });
        var roofB1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, buildingHeight, (dimensionsA.y - dimensionsB.y) / 2f - 0.5f), Quaternion.Euler(0, -90, 0), new Dictionary<string, dynamic> {
            {"width", dimensionsB.y + (dimensionsA.y - dimensionsB.y) / 2f},
            {"height", height},
            {"length", (dimensionsA.x - dimensionsB.x) / 2f},
            {"thickness", thickness},
            {"extrusion", extrusion},
            {"extrusionLeft", false},
            {"addCap", true},
            {"closeRoof", true},
        });
        roofs.Add(roofB);
        roofs.Add(roofB1);

        return MeshUtils.Combine(roofs);
    }

    private MeshData GenSquareOverhang(Arr2d<bool> layout) {
        if (!RandUtils.BoolWeighted(GeneratorSettings.SquareBuildingSettings.OverhangChance))
            return new MeshData();

        var horizontal = RandUtils.Bool;
        var width = horizontal ? RandUtils.RangeInclusive(Mathf.CeilToInt(dimensionsA.x / 4f), Mathf.CeilToInt(dimensionsA.x / 3f)) : RandUtils.RangeInclusive(Mathf.CeilToInt(dimensionsA.y / 4f), Mathf.CeilToInt(dimensionsA.y / 3f));
        if (horizontal) {
            layout.Fill(new Vector2Int(layout.Length1 - width, 0), new Vector2Int(layout.Length1, layout.Length2), false);
        } else {
            layout.Fill(new Vector2Int(0, layout.Length2 - width), new Vector2Int(layout.Length1, layout.Length2), false);
        }

        var meshes = new List<MeshData>();
        var thickness = 0.25f;
        var start = Mathf.FloorToInt(RandUtils.Range(1, 3f * buildingHeight / 4f));
        var height = buildingHeight - start;

        var archChance = 0.5f;
        var archLOD = 45;
        var shouldAddWallsA = false;
        var shouldAddWallsB = false;

        if (horizontal) {
            var wallA = MeshGenerator.GetMesh<PlaneGenerator>(new Vector3(dimensionsA.x - width - 0.5f, start, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"sizeA", width},
                {"sizeB", height},
                {"orientation", PlaneGenerator.PlaneOrientation.XY}
            });
            var wallB = MeshGenerator.GetMesh<PlaneGenerator>(new Vector3(dimensionsA.x - 0.5f, start, dimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"sizeA", width},
                {"sizeB", height},
                {"orientation", PlaneGenerator.PlaneOrientation.XY}
            });
            var wallC = MeshGenerator.GetMesh<PlaneGenerator>(new Vector3(dimensionsA.x - 0.5f, start, -0.5f), Quaternion.Euler(0, -90, 0), new Dictionary<string, dynamic> {
                {"sizeA", dimensionsA.y},
                {"sizeB", height},
                {"orientation", PlaneGenerator.PlaneOrientation.XY}
            });
            meshes.Add(wallA);
            meshes.Add(wallB);
            meshes.Add(wallC);
            if (RandUtils.BoolWeighted(archChance)) {
                // ARCH X
                if (RandUtils.BoolWeighted(archChance)) {
                    var archWidth = width - 2f * thickness;
                    var archOffset = thickness;
                    var archHeight = archWidth / 2f;
                    var arch = MeshGenerator.GetMesh<ArchGenerator>(new Vector3(dimensionsA.x - dimensionsB.x - 0.5f + archOffset, start - 1, dimensionsA.y - thickness - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                        {"width", archWidth},
                        {"height", archHeight},
                        {"length", thickness},
                        {"points", archLOD}
                    });
                }
            }
        } else {
            var wallA = MeshGenerator.GetMesh<WallGenerator>(new Vector3(-0.5f, start, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                {"width", width},
                {"height", height},
                {"thickness", 0.01f},
                {"thicknessOutwards", true}
            });
            var wallB = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - 0.5f, start, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                {"width", width},
                {"height", height},
                {"thickness", 0.01f},
                {"thicknessInwards", true},
            });
            var wallC = MeshGenerator.GetMesh<WallGenerator>(new Vector3(-0.5f, start, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", dimensionsA.x},
                {"height", height},
                {"thickness", 0.01f},
                {"thicknessInwards", true}
            });
            meshes.Add(wallA);
            meshes.Add(wallB);
            meshes.Add(wallC);
        }

        return MeshUtils.Combine(meshes);
    }

    private MeshData GenLOverhang() {
        if (!RandUtils.BoolWeighted(GeneratorSettings.LBuildingSettings.OverhangChance))
            return new MeshData();

        var featureThickness = 0.1f;
        var meshes = new List<MeshData>();
        var thickness = 0.25f;
        var start = 2;
        var end = RandUtils.Range(start + 1, buildingHeight - 1);
        var height = end - start;

        // WALLS
        var wallA = MeshGenerator.GetMesh<PlaneGenerator>(new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, start, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
            {"sizeA", dimensionsB.x},
            {"sizeB", height},
            {"orientation", PlaneGenerator.PlaneOrientation.XY},
            {"flip", true}
        });
        var wallB = MeshGenerator.GetMesh<PlaneGenerator>(new Vector3(dimensionsA.x - 0.5f, start, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
            {"sizeA", dimensionsB.y},
            {"sizeB", height},
            {"orientation", PlaneGenerator.PlaneOrientation.XY},
            {"flip", true}
        });
        var plane = MeshGenerator.GetMesh<PlaneGenerator>(new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, start + height, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
            {"sizeA", dimensionsB.x},
            {"sizeB", dimensionsB.y}
        });
        meshes.Add(wallA);
        meshes.Add(wallB);
        meshes.Add(plane);

        var fromA = new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, start, dimensionsA.y - 0.5f);
        var toA = fromA + Vector3.right * dimensionsB.x;
        var wallAPerpendicular = Vector3.Cross((toA - fromA).normalized, Vector3.up);
        meshes.Add(AddWallFeatures(fromA, toA, height, dimensionsB.x, 0, false));
        meshes.Add(MeshGenerator.GetMesh<LineGenerator>(fromA + Vector3.up * featureThickness - wallAPerpendicular * featureThickness / 2f, Quaternion.identity, new Dictionary<string, dynamic> {
            {"start", Vector3.zero + Vector3.right * featureThickness / 2f},
            {"end", Vector3.right * (dimensionsB.x - featureThickness / 2f)},
            {"thickness", featureThickness},
            {"extrusion", featureThickness},
            {"submeshIndex", 2},
            {"rotateUV", true}
        }));

        var fromB = new Vector3(dimensionsA.x - 0.5f, start, dimensionsA.y - 0.5f);
        var toB = fromB - Vector3.forward * dimensionsB.y;
        var wallBPerpendicular = Vector3.Cross((toB - fromB).normalized, Vector3.up);
        meshes.Add(AddWallFeatures(fromB, toB, height, dimensionsB.y, 90, false));
        meshes.Add(MeshGenerator.GetMesh<LineGenerator>(fromB + Vector3.up * featureThickness - wallBPerpendicular * featureThickness / 2f, Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
            {"start", Vector3.zero + Vector3.right * featureThickness / 2f},
            {"end", Vector3.right * (dimensionsB.y - featureThickness / 2f)},
            {"thickness", featureThickness},
            {"extrusion", featureThickness},
            {"submeshIndex", 2},
            {"rotateUV", true}
        }));
        meshes.Add(MeshGenerator.GetMesh<LineGenerator>(new Vector3(dimensionsA.x - 0.5f, 0, dimensionsA.y - 0.5f) - wallAPerpendicular * featureThickness / 2f, Quaternion.identity, new Dictionary<string, dynamic> {
            {"start", Vector3.zero},
            {"end", Vector3.up * (end + featureThickness / 2f)},
            {"thickness", featureThickness},
            {"extrusion", featureThickness},
            {"submeshIndex", 2},
            {"rotateUV", true}
        }));

        // ARCHES
        var archChance = 0.5f;
        var archLOD = 45;
        var shouldAddWallsA = false;
        var shouldAddWallsB = false;
        if (RandUtils.BoolWeighted(archChance)) {
            // ARCHES/ARCH X
            if (RandUtils.BoolWeighted(archChance)) {
                var diffA = dimensionsB.x - start;
                var archWidthA = dimensionsB.x - 2 * thickness;
                var archOffsetA = thickness;
                if (diffA > 0) {
                    archWidthA = start;
                    archOffsetA = diffA / 2f;
                    shouldAddWallsA = true;
                }

                var archA = MeshGenerator.GetMesh<ArchGenerator>(new Vector3(dimensionsA.x - dimensionsB.x - 0.5f + archOffsetA, start - 1, dimensionsA.y - thickness - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                    {"width", archWidthA},
                    {"height", 1},
                    {"length", thickness},
                    {"points", archLOD}
                });
                meshes.Add(archA);
                if (shouldAddWallsA) {
                    var wallC = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - 0.5f, 0, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                        {"width", diffA / 2f},
                        {"height", start},
                        {"thickness", 0.01f},
                        {"thicknessInwards", true},
                        {"flip", true}
                    });
                    var wallC1 = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, 0, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                        {"width", diffA / 2f},
                        {"height", start},
                        {"thickness", 0.01f},
                        {"thicknessInwards", true}
                    });
                    meshes.Add(wallC);
                    meshes.Add(wallC1);
                } else {
                    var pillarX = MeshGenerator.GetMesh<PillarGenerator>(new Vector3(dimensionsA.x - dimensionsB.x + thickness / 2f - 0.5f, 0, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                        {"width", thickness},
                        {"height", start},
                        {"thicknessInwards", true}
                    });
                    meshes.Add(pillarX);
                }
            } else {
                shouldAddWallsA = true;
                var wallX = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - 0.5f, 0, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                    {"width", dimensionsB.x},
                    {"height", start},
                    {"thickness", 0.01f},
                    {"thicknessInwards", true},
                    {"flip", true}
                });
                meshes.Add(wallX);
            }

            // ARCHES/ARCH Z
            if (RandUtils.BoolWeighted(archChance)) {
                var diffB = dimensionsB.y - start;
                var archWidthB = dimensionsB.y - 2 * thickness;
                var archOffsetB = thickness;
                if (diffB > 0) {
                    archWidthB = start;
                    archOffsetB = diffB / 2f;
                    shouldAddWallsB = true;
                }

                var archB = MeshGenerator.GetMesh<ArchGenerator>(new Vector3(dimensionsA.x - 0.5f, start - 1, dimensionsA.y - dimensionsB.y + archOffsetB - 0.5f), Quaternion.Euler(0, -90, 0), new Dictionary<string, dynamic> {
                    {"width", archWidthB},
                    {"height", 1},
                    {"length", thickness},
                    {"points", archLOD}
                });
                meshes.Add(archB);
                if (shouldAddWallsB) {
                    var wallD = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - 0.5f, 0, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                        {"width", diffB / 2f},
                        {"height", start},
                        {"thickness", 0.01f},
                        {"thicknessInwards", true}
                    });
                    var wallD1 = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - 0.5f, 0, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                        {"width", diffB / 2f},
                        {"height", start},
                        {"thickness", 0.01f},
                        {"thicknessInwards", true},
                        {"flip", true}
                    });
                    meshes.Add(wallD);
                    meshes.Add(wallD1);
                } else {
                    Debug.Log("Added pillars");
                    var pillarZ = MeshGenerator.GetMesh<PillarGenerator>(new Vector3(dimensionsA.x - thickness / 2f - 0.5f, 0, dimensionsA.y - dimensionsB.y + thickness - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                        {"width", thickness},
                        {"height", start},
                        {"thicknessInwards", true}
                    });
                    meshes.Add(pillarZ);
                }
            } else {
                shouldAddWallsB = true;
                var wallZ = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - 0.5f, 0, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                    {"width", dimensionsB.y},
                    {"height", start},
                    {"thickness", 0.01f},
                    {"thicknessInwards", true}
                });
                meshes.Add(wallZ);
            }
        }

        // MIDDLE PILLAR
        if (!shouldAddWallsA || !shouldAddWallsB) {
            var pillarMid = MeshGenerator.GetMesh<PillarGenerator>(new Vector3(dimensionsA.x - thickness / 2f - 0.5f, 0, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                {"width", thickness},
                {"height", start},
                {"thicknessInwards", true}
            });
            meshes.Add(pillarMid);
        }

        return MeshUtils.Combine(meshes);
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
                {"sizeB", buildingHeight},
                {"orientation", PlaneGenerator.PlaneOrientation.XY},
            });

            walls.MergeMeshData(wall);
            walls.MergeMeshData(AddWallFeatures(from, to, buildingHeight, wallWidth, angle));
            current = next;
        }

        return walls;
    }

    private MeshData AddWallFeatures(Vector3 wallStart, Vector3 wallEnd, int buildingHeight, float wallWidth, float wallAngle, bool addPillar = true) {
        var thickness = 0.1f;
        var fillWallSegmentChance = 0.1f;
        var diagonalAChance = 0.5f;
        var diagonalBChance = 0.5f;
        var windowChance = 0.2f;

        var wallDirection = (wallEnd - wallStart).normalized;
        var wallPerpendicular = Vector3.Cross(wallDirection, Vector3.up);

        var features = new MeshData();

        if (addPillar) {
            var pillar = MeshGenerator.GetMesh<LineGenerator>(wallEnd - Vector3.forward * thickness / 2f, Quaternion.identity, new Dictionary<string, dynamic> {
                {"end", Vector3.up * buildingHeight},
                {"thickness", thickness},
                {"extrusion", thickness},
                {"submeshIndex", 2},
                {"extrusionOutwards", false}
            });
            features.MergeMeshData(pillar);
        }

        var splitSectionsVertical = new List<int> {0, 1};
        var lastSplitPoint = splitSectionsVertical[1];
        while (lastSplitPoint < buildingHeight) {
            var nextSize = RandUtils.RangeInclusive(1, 2);
            lastSplitPoint += nextSize;
            if (lastSplitPoint >= buildingHeight) lastSplitPoint = buildingHeight;
            splitSectionsVertical.Add(lastSplitPoint);
        }

        for (var i = 1; i < splitSectionsVertical.Count; i++) {
            var horizontalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + Vector3.up * splitSectionsVertical[i] - wallPerpendicular * thickness / 2f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                {"start", Vector3.zero + Vector3.right * thickness / 2f},
                {"end", Vector3.right * (wallWidth - thickness / 2f)},
                {"thickness", thickness},
                {"extrusion", thickness},
                {"submeshIndex", 2},
                {"rotateUV", true}
            });
            features.MergeMeshData(horizontalLine);
            if (i == 1)
                continue;
            var wallSize = Mathf.RoundToInt((wallEnd - wallStart).magnitude);
            var sectionHeight = splitSectionsVertical[i] - splitSectionsVertical[i - 1];
            if (wallSize > 2 && RandUtils.BoolWeighted(windowChance)) {
                var windowPosition = wallSize / 2f;
                var window = MeshGenerator.GetMesh<PlaneGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) + wallPerpendicular * thickness / 4f, Quaternion.Euler(0, wallAngle - 180, 0), new Dictionary<string, dynamic> {
                    {"sizeA", 1},
                    {"sizeB", 1},
                    {"orientation", PlaneGenerator.PlaneOrientation.XY},
                    {"submeshIndex", 3},
                    {"extraUvSettings", MeshGenerator.UVSettings.NoOffset}
                });
                features.MergeMeshData(window);
                features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f - 1) + Vector3.up * (splitSectionsVertical[i - 1] + thickness/2f + 0.3f), Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero},
                    {"end", Vector3.right},
                    {"thickness", 0.035f},
                    {"extrusion", 0.035f},
                    {"submeshIndex", 2},
                    {"rotateUV", true}
                }));
                features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f - 1) + Vector3.up * (splitSectionsVertical[i - 1] + thickness/2f + 0.6f), Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero},
                    {"end", Vector3.right},
                    {"thickness", 0.035f},
                    {"extrusion", 0.035f},
                    {"submeshIndex", 2},
                    {"rotateUV", true}
                }));
                features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f - 1 + 0.375f) + Vector3.up * (splitSectionsVertical[i - 1] + thickness/2f) - wallPerpendicular * 0.005f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero},
                    {"end", Vector3.up * (1-thickness/2f)},
                    {"thickness", 0.035f},
                    {"extrusion", 0.035f},
                    {"submeshIndex", 2},
                    {"rotateUV", true}
                }));
                features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f - 1 + 0.725f) + Vector3.up * (splitSectionsVertical[i - 1] + thickness/2f) - wallPerpendicular * 0.005f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero},
                    {"end", Vector3.up * (1-thickness/2f)},
                    {"thickness", 0.035f},
                    {"extrusion", 0.035f},
                    {"submeshIndex", 2},
                    {"rotateUV", true}
                }));
                if (sectionHeight > 1) {
                    var line = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f - 1) + Vector3.up * (splitSectionsVertical[i - 1] + 1) - wallPerpendicular * thickness / 2f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                        {"start", Vector3.right * thickness},
                        {"end", Vector3.right},
                        {"thickness", thickness},
                        {"extrusion", thickness},
                        {"submeshIndex", 2},
                        {"rotateUV", true}
                    });
                    features.MergeMeshData(line);
                }

                var lineLeft = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - 1) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * thickness / 2f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero},
                    {"end", Vector3.up * (sectionHeight - thickness)},
                    {"thickness", thickness},
                    {"extrusion", thickness},
                    {"submeshIndex", 2},
                    {"rotateUV", true}
                });
                features.MergeMeshData(lineLeft);
                var lineRight = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * thickness / 2f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero},
                    {"end", Vector3.up * (sectionHeight - thickness)},
                    {"thickness", thickness},
                    {"extrusion", thickness},
                    {"submeshIndex", 2},
                    {"rotateUV", true}
                });
                features.MergeMeshData(lineRight);
            } else {
                var splitSectionsHorizontal = new List<int> {0};
                var lastSplitPointH = splitSectionsHorizontal[0];
                while (lastSplitPointH < wallSize) {
                    lastSplitPointH += RandUtils.RangeInclusive(1, 2);
                    if (lastSplitPointH > wallSize) lastSplitPointH = wallSize;
                    splitSectionsHorizontal.Add(lastSplitPointH);
                }

                for (var i2 = 1; i2 < splitSectionsHorizontal.Count; i2++) {
                    if (i2 != splitSectionsHorizontal.Count - 1) {
                        var verticalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * splitSectionsHorizontal[i2] + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f), Quaternion.identity, new Dictionary<string, dynamic> {
                            {"start", Vector3.up * (i == 1 ? -thickness / 2f : 0)},
                            {"end", Vector3.up * (splitSectionsVertical[i] - splitSectionsVertical[i - 1] - thickness)},
                            {"thickness", thickness},
                            {"extrusion", thickness},
                            {"submeshIndex", 2},
                            {"extrusionCenter", true}
                        });
                        features.MergeMeshData(verticalLine);
                    }

                    if (RandUtils.BoolWeighted(fillWallSegmentChance)) {
                        var fillWallSpacing = RandUtils.Range(0.01f, 0.02f);
                        var fillWallThickness = thickness - fillWallSpacing;
                        var totalWidth = splitSectionsHorizontal[i2] - splitSectionsHorizontal[i2 - 1] - fillWallSpacing;
                        var totalSteps = (int) (totalWidth / thickness);

                        for (var fillIndex = 0; fillIndex < totalSteps; fillIndex++) {
                            var verticalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (splitSectionsHorizontal[i2 - 1] + fillIndex * (fillWallThickness + fillWallSpacing) + thickness) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * thickness / 4f, Quaternion.identity, new Dictionary<string, dynamic> {
                                {"start", Vector3.zero - Vector3.up * (i == 1 ? thickness / 2f : 0)},
                                {"end", Vector3.up * (splitSectionsVertical[i] - splitSectionsVertical[i - 1] - thickness + (i == 1 ? 1.5f * thickness : 0))},
                                {"thickness", fillWallThickness},
                                {"extrusion", fillWallThickness},
                                {"submeshIndex", 2},
                                {"extrusionCenter", true}
                            });
                            features.MergeMeshData(verticalLine);
                        }
                    }

                    if (RandUtils.BoolWeighted(diagonalAChance)) {
                        var diff = splitSectionsHorizontal[i2] - splitSectionsHorizontal[i2 - 1];
                        var diagonalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * splitSectionsHorizontal[i2 - 1] + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * thickness / 6f, Quaternion.identity, new Dictionary<string, dynamic> {
                            {"start", Vector3.zero - Vector3.up * (i == 1 ? thickness / 2f : 0)},
                            {"end", wallDirection * diff + Vector3.up * (splitSectionsVertical[i] - splitSectionsVertical[i - 1] - thickness + (i == 1 ? 1f * thickness : 0))},
                            {"thickness", thickness},
                            {"extrusion", thickness},
                            {"submeshIndex", 2},
                            {"extrusionCenter", true}
                        });
                        features.MergeMeshData(diagonalLine);
                    }

                    if (RandUtils.BoolWeighted(diagonalBChance)) {
                        var diff = splitSectionsHorizontal[i2] - splitSectionsHorizontal[i2 - 1];
                        var diagonalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * splitSectionsHorizontal[i2 - 1] + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * thickness / 6f, Quaternion.identity, new Dictionary<string, dynamic> {
                            {"start", wallDirection * diff - Vector3.up * (i == 1 ? thickness / 2f : 0)},
                            {"end", Vector3.up * (splitSectionsVertical[i] - splitSectionsVertical[i - 1] - thickness + (i == 1 ? 1f * thickness : 0))},
                            {"thickness", thickness},
                            {"extrusion", thickness},
                            {"submeshIndex", 2},
                            {"extrusionCenter", true}
                        });

                        features.MergeMeshData(diagonalLine);
                    }
                }
            }
        }

        return features;
    }

    private void CleanupOutline(Arr2d<bool> boolArr) {
        var queuedToRemove = new List<Vector2Int>();
        for (int x = 1; x < boolArr.Length1 - 1; x++) {
            for (int y = 1; y < boolArr.Length2 - 1; y++) {
                var north = boolArr[x, y - 1];
                var south = boolArr[x, y + 1];
                var east = boolArr[x + 1, y];
                var west = boolArr[x - 1, y];
                if (!boolArr[x, y]) continue;

                // Remove if surrounded
                if (north && south && east && west) {
                    queuedToRemove.Add(new Vector2Int(x, y));
                }
            }
        }

        queuedToRemove.ForEach(coord => boolArr[coord] = false);
    }

    private void CarveLShape(Arr2d<bool> arr) {
        var from = new Vector2Int(dimensionsA.x - dimensionsB.x, dimensionsA.y - dimensionsB.y);
        var to = new Vector2Int(dimensionsA.x, dimensionsA.y);
        arr.Fill(from, to, false);
    }
}