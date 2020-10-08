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
    private List<Rectangle> rects;

    private int buildingHeight;
    private Vector2Int dimensionsA;
    private Vector2Int dimensionsB;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void Clear() {
        mesh.Clear();
    }

    public void Generate() {
        if (GeneratorSettings == null) {
            throw new Exception("Generator Settings cannot be null! Make sure to assign a RandomSettings object to the class before calling BuildingGenerator::Generate");
        }

        mesh.Clear();
        Setup();
        var buildingType = buildingTypeSelector.Value();
        var boolArr = buildingType == 0 ? GenSquare() : GenL();
        var overhang = buildingType == 0 ? GenSquareOverhang(boolArr) : GenLOverhang();
        var roofs = buildingType == 0 ? GenSquareRoof() : GenLRoof();
        var path = MarchingSquares.March(boolArr);
        CleanupOutline(boolArr);
        var walls = GenWalls(path);

        var (vertices, triangles) = MeshUtils.Combine(roofs, walls, overhang);
        
        mesh = new Mesh{name = "Building"};
        meshFilter.sharedMesh = mesh;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
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

    private Arr2d<bool> GenSquare() {
        var size = RandUtils.RandomBetween(GeneratorSettings.SquareBuildingSettings.MinSize, GeneratorSettings.SquareBuildingSettings.MaxSize);
        buildingHeight = size.y;
        dimensionsA = new Vector2Int(size.x, size.z);
        dimensionsB = Vector2Int.zero;

        var boolArr = new Arr2d<bool>(size.x, size.z, true);
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

    private Arr2d<bool> GenT() {
        var width = RandUtils.RandomBetween(GeneratorSettings.TBuildingSettings.MinMaxWidth);
        var length = RandUtils.RandomBetween(GeneratorSettings.TBuildingSettings.MinMaxLength);
        var extrusion = RandUtils.RandomBetween(GeneratorSettings.TBuildingSettings.MinMaxExtrusion);
        extrusion = MathUtils.Clamp(extrusion, 1, Mathf.CeilToInt(length / 2f));
        var inset = RandUtils.RandomBetween(GeneratorSettings.TBuildingSettings.MinMaxInset);
        inset = MathUtils.Clamp(inset, 1, Mathf.CeilToInt(width / 4f));
        buildingHeight = RandUtils.RandomBetween(GeneratorSettings.TBuildingSettings.MinMaxHeight);

        dimensionsA = new Vector2Int(width, length);
        dimensionsB = new Vector2Int(extrusion, inset);
        var boolArr = new Arr2d<bool>(dimensionsA, true);
        CarveTShape(boolArr);
        return boolArr;
    }

    private (List<Vector3> vertices, List<int> triangles) GenSquareRoof() {
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
            var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", dimensionsA.x},
                {"height", height},
                {"thickness", thickness},
                {"length", dimensionsA.y / 2f},
                {"extrusion", extrusion},
                {"flip", true},
                {"addCap", true},
                {"closeRoof", true}
            });
            return MeshUtils.Combine(roofA, roofA1);
        } else {
            if (RandUtils.BoolWeighted(doubleCornerChance)) {
                var cornerWidth = RandUtils.Range(dimensionsA.x / 4f, dimensionsA.x / 2f);
                var cornerWidthB = RandUtils.Range(dimensionsA.x / 4f, dimensionsA.x / 2f);
                var cornerA = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                    {"width", cornerWidth},
                    {"height", height},
                    {"length", dimensionsA.y / 2f},
                    {"thickness", thickness},
                    {"flipX", true},
                    {"addCap", true},
                    {"joinCaps", true}
                });
                var cornerA1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                    {"width", cornerWidth},
                    {"height", height},
                    {"length", dimensionsA.y / 2f},
                    {"thickness", thickness},
                    {"flipX", true},
                    {"flipZ", true},
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
                var cornerB1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(-0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                    {"width", cornerWidthB},
                    {"height", height},
                    {"length", dimensionsA.y / 2f},
                    {"thickness", thickness},
                    {"flipZ", true},
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
                var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(cornerWidthB - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                    {"width", dimensionsA.x - cornerWidth - cornerWidthB},
                    {"height", height},
                    {"thickness", thickness},
                    {"length", dimensionsA.y / 2f},
                    {"extrusion", 0},
                    {"addCap", true},
                    {"flip", true},
                    {"closeRoof", true}
                });
                return MeshUtils.Combine(cornerA, cornerA1, cornerB, cornerB1, roofA, roofA1);
            } else {
                var cornerWidth = RandUtils.Range(dimensionsA.x / 4f, dimensionsA.x / 2f);
                var cornerA = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                    {"width", cornerWidth},
                    {"height", height},
                    {"length", dimensionsA.y / 2f},
                    {"thickness", thickness},
                    {"flipX", true},
                    {"addCap", true},
                    {"joinCaps", true}
                });
                var cornerB = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                    {"width", cornerWidth},
                    {"height", height},
                    {"length", dimensionsA.y / 2f},
                    {"thickness", thickness},
                    {"flipX", true},
                    {"flipZ", true},
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

    private (List<Vector3> vertices, List<int> triangles) GenLRoof() {
        var height = 1.5f;
        var height2 = 1.75f;
        var thickness = 0.15f;
        var extrusion = 0.25f;
        var roofInset = RandUtils.Range(dimensionsB.x / 2f, dimensionsB.x);
        var roofInsetB = Mathf.Min(roofInset, (dimensionsA.x - dimensionsB.x) / 2f);
        var cornerRoofEndingChance = 0.5f;

        var roofs = new List<(List<Vector3> vertices, List<int> triangles)>();
        if (RandUtils.BoolWeighted(cornerRoofEndingChance)) {
            var cornerA = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", roofInset},
                {"height", height},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"thickness", thickness},
                {"addCap", true},
                {"joinCaps", true},
                {"flipX", true}
            });
            var cornerA1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(dimensionsA.x - 0.5f, buildingHeight, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", roofInset},
                {"height", height},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"thickness", thickness},
                {"addCap", true},
                {"joinCaps", true},
                {"flipX", true},
                {"flipZ", true}
            });

            var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(roofInsetB - 0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", dimensionsA.x - roofInset - roofInsetB},
                {"height", height},
                {"thickness", thickness},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"extrusion", 0},
                {"addCap", true},
                {"closeRoof", true}
            });
            var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(roofInsetB - 0.5f, buildingHeight, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", dimensionsA.x - roofInset - roofInsetB},
                {"height", height},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"thickness", thickness},
                {"flip", true},
                {"extrusion", 0},
                {"addCap", true},
                {"closeRoof", true}
            });
            roofs.Add(cornerA);
            roofs.Add(cornerA1);
            roofs.Add(roofA);
            roofs.Add(roofA1);
        } else {
            var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(roofInsetB - 0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", dimensionsA.x - roofInsetB},
                {"height", height},
                {"thickness", thickness},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"extrusion", extrusion},
                {"extrusionLeft", false},
                {"addCap", true},
                {"closeRoof", true}
            });
            var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(roofInsetB - 0.5f, buildingHeight, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", dimensionsA.x - roofInsetB},
                {"height", height},
                {"length", (dimensionsA.y - dimensionsB.y) / 2f},
                {"thickness", thickness},
                {"flip", true},
                {"extrusion", extrusion},
                {"extrusionLeft", false},
                {"addCap", true},
                {"closeRoof", true}
            });
            roofs.Add(roofA);
            roofs.Add(roofA1);
        }

        var cornerB = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(-0.5f, buildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
            {"width", roofInsetB},
            {"height", height},
            {"length", (dimensionsA.y - dimensionsB.y) / 2f},
            {"thickness", thickness},
            {"addCap", true},
            {"joinCaps", true}
        });
        var cornerB1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(-0.5f, buildingHeight, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
            {"width", roofInsetB},
            {"height", height},
            {"length", (dimensionsA.y - dimensionsB.y) / 2f},
            {"thickness", thickness},
            {"addCap", true},
            {"joinCaps", true},
            {"flipZ", true}
        });
        roofs.Add(cornerB);
        roofs.Add(cornerB1);

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
        var roofB1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, -90, 0), new Dictionary<string, dynamic> {
            {"width", dimensionsB.y + (dimensionsA.y - dimensionsB.y) / 2f},
            {"height", height},
            {"length", (dimensionsA.x - dimensionsB.x) / 2f},
            {"thickness", thickness},
            {"extrusion", extrusion},
            {"extrusionRight", false},
            {"addCap", true},
            {"closeRoof", true},
            {"flip", true}
        });
        roofs.Add(roofB);
        roofs.Add(roofB1);

        return MeshUtils.Combine(roofs);
    }

    private (List<Vector3> vertices, List<int> triangles) GenTRoof() {
        return (new List<Vector3>(), new List<int>());
    }

    private (List<Vector3> vertices, List<int> triangles) GenSquareOverhang(Arr2d<bool> layout) {
        if (!RandUtils.BoolWeighted(GeneratorSettings.SquareBuildingSettings.OverhangChance))
            return (new List<Vector3>(), new List<int>());

        var horizontal = RandUtils.Bool;
        var width = horizontal ? RandUtils.RangeInclusive(Mathf.CeilToInt(dimensionsA.x / 4f), Mathf.CeilToInt(dimensionsA.x / 3f)) : RandUtils.RangeInclusive(Mathf.CeilToInt(dimensionsA.y / 4f), Mathf.CeilToInt(dimensionsA.y / 3f));
        if (horizontal) {
            layout.Fill(new Vector2Int(layout.Length1 - width, 0), new Vector2Int(layout.Length1, layout.Length2), false);
        } else {
            layout.Fill(new Vector2Int(0, layout.Length2 - width), new Vector2Int(layout.Length1, layout.Length2), false);
        }

        var meshes = new List<(List<Vector3> vertices, List<int> triangles)>();
        var thickness = 0.1f;
        var start = RandUtils.Range(1, 3f * buildingHeight / 4f);
        var height = buildingHeight - start;
        
        var archChance = 0.5f;
        var archLOD = 45;
        var shouldAddWallsA = false;
        var shouldAddWallsB = false;
        
        if (horizontal) {
            var wallA = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - width - 0.5f, start, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", width},
                {"height", height},
                {"thickness", thickness},
                {"thicknessInwards", true}
            });
            var wallB = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - width - 0.5f, start, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", width},
                {"height", height},
                {"thickness", thickness},
                {"thicknessInwards", true}
            });
            var wallC = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - 0.5f, start, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                {"width", dimensionsA.y},
                {"height", height},
                {"thickness", thickness},
                {"thicknessInwards", true}
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
                {"thickness", thickness},
                {"thicknessOutwards", true}
            });
            var wallB = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - 0.5f, start, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                {"width", width},
                {"height", height},
                {"thickness", thickness},
                {"thicknessInwards", true},
            });
            var wallC = MeshGenerator.GetMesh<WallGenerator>(new Vector3(-0.5f, start, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", dimensionsA.x},
                {"height", height},
                {"thickness", thickness},
                {"thicknessInwards", true}
            });
            meshes.Add(wallA);
            meshes.Add(wallB);
            meshes.Add(wallC);
        }

        var combinedMeshes = MeshUtils.Combine(meshes);
        return combinedMeshes;
    }

    private (List<Vector3> vertices, List<int> triangles) GenLOverhang() {
        if (!RandUtils.BoolWeighted(GeneratorSettings.LBuildingSettings.OverhangChance))
            return (new List<Vector3>(), new List<int>());

        var meshes = new List<(List<Vector3> vertices, List<int> triangles)>();
        var thickness = 0.1f;
        var start = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.OverhangMinMaxStart);
        var end = RandUtils.Range(start + 1, buildingHeight - 1);
        var height = end - start;

        // WALLS
        var wallA = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, start, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
            {"width", dimensionsB.x},
            {"height", height},
            {"thickness", thickness},
            {"thicknessInwards", true}
        });
        var wallB = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - 0.5f, start, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
            {"width", dimensionsB.y},
            {"height", height},
            {"thickness", thickness},
            {"thicknessInwards", true}
        });
        var plane = MeshGenerator.GetMesh<PlaneGenerator>(new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, start + height, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
            {"sizeA", dimensionsB.x},
            {"sizeB", dimensionsB.y}
        });
        meshes.Add(wallA);
        meshes.Add(wallB);
        meshes.Add(plane);

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
                        {"thickness", thickness},
                        {"thicknessInwards", true},
                        {"flip", true}
                    });
                    var wallC1 = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, 0, dimensionsA.y - 0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                        {"width", diffA / 2f},
                        {"height", start},
                        {"thickness", thickness},
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
                    {"thickness", thickness},
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
                        {"thickness", thickness},
                        {"thicknessInwards", true}
                    });
                    var wallD1 = MeshGenerator.GetMesh<WallGenerator>(new Vector3(dimensionsA.x - 0.5f, 0, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                        {"width", diffB / 2f},
                        {"height", start},
                        {"thickness", thickness},
                        {"thicknessInwards", true},
                        {"flip", true}
                    });
                    meshes.Add(wallD);
                    meshes.Add(wallD1);
                } else {
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
                    {"thickness", thickness},
                    {"thicknessInwards", true}
                });
                meshes.Add(wallZ);
            }
        }

        // MIDDLE PILLAR
        if (!shouldAddWallsA && !shouldAddWallsB) {
            var pillarMid = MeshGenerator.GetMesh<PillarGenerator>(new Vector3(dimensionsA.x - thickness / 2f - 0.5f, 0, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                {"width", thickness},
                {"height", start},
                {"thicknessInwards", true}
            });
            meshes.Add(pillarMid);
        }

        return MeshUtils.Combine(meshes);
    }

    private (List<Vector3> vertices, List<int> triangles) GenWalls(List<Vector2Int> path) {
        var thickness = 0.1f;
        var walls = new List<(List<Vector3>, List<int>)>();
        var current = Vector2Int.zero;
        foreach (var point in path) {
            var next = current + point;
            var from = new Vector3(current.x - 0.5f, 0, current.y - 0.5f);
            var to = new Vector3(next.x - 0.5f, 0, next.y - 0.5f);
            var diff = to - from;
            var angle = Vector3.SignedAngle(Vector3.right, diff, Vector3.up);
            var wall = MeshGenerator.GetMesh<WallGenerator>(from, Quaternion.Euler(0, angle, 0), new Dictionary<string, dynamic> {
                {"width", diff.magnitude},
                {"height", buildingHeight},
                {"thickness", thickness},
                {"thicknessInwards", true}
            });
            walls.Add(wall);
            current = next;
        }

        return MeshUtils.Combine(walls);
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

    private void CarveTShape(Arr2d<bool> arr) {
        var from = new Vector2Int(0, dimensionsA.y - dimensionsB.x + 1);
        var to = new Vector2Int(dimensionsB.y, dimensionsA.y);
        arr.Fill(from, to, false);

        // Move over to the other cut-out
        from += Vector2Int.right * (dimensionsA.x - dimensionsB.y);
        to += Vector2Int.right * (dimensionsA.x - dimensionsB.y);
        arr.Fill(from, to, false);
    }

    /*private void SpawnBuilding(Arr3d<TileDirection> tileDirections) {
        gridObjects = new Arr3d<GameObject>(tileDirections.Length);
        for (int x = 0; x < tileDirections.Length1; x++)
        for (int y = 0; y < tileDirections.Length2; y++)
        for (int z = 0; z < tileDirections.Length3; z++) {
            if (tileDirections[x, y, z] == 0) continue;
            var objectToSpawn = directionToType[tileDirections[x, y, z]] == TileType.Straight ? StraightPrefab : CurvePrefab;
            var obj = Instantiate(objectToSpawn, new Vector3(x, y, z), Quaternion.Euler(directionToRotation[tileDirections[x, y, z]]), transform);
            gridObjects[x, y, z] = obj;
        }
    }*/

    private List<Rectangle> GetRoofRectangles(Arr3d<bool> building) {
        var lastFloor = new Arr2d<bool>(building.Length1, building.Length3);
        for (var x = 0; x < building.Length1; x++)
        for (var y = 0; y < building.Length3; y++) {
            lastFloor[x, y] = building[x, building.Length2 - 1, y];
        }

        var rects = new List<Rectangle>();
        for (var i = 0; i < lastFloor.Length1; i++) {
            for (var j = 0; j < lastFloor.Length2; j++) {
                if (!lastFloor[i, j])
                    continue;
                var current = new Rectangle(new Vector2Int(i, j), new Vector2Int(i + 1, j + 1));
                ExtendRectangle(ref current, lastFloor);
                lastFloor[i, j] = false;
                rects.Add(current);
            }
        }

        return rects;
    }

    #region Rectangle Utils
    private void ExtendRectangle(ref Rectangle current, Arr2d<bool> blocks) {
        ExtendRectangle_H(ref current, blocks);
        ExtendRectangle_V(ref current, blocks);
    }

    private void ExtendRectangle_H(ref Rectangle current, Arr2d<bool> blocks) {
        for (int i = current.Min.x; i < blocks.Length1; i++) {
            if (blocks[i, current.Min.y]) {
                current.Max.x = i + 1;
                blocks[i, current.Min.y] = false;
            } else break;
        }
    }

    private void ExtendRectangle_V(ref Rectangle current, Arr2d<bool> blocks) {
        for (int i = current.Min.y + 1; i < blocks.Length2; i++) {
            if (ExtendRectangle_VerifyExtension(i, current, blocks)) {
                ExtendRectangle_SetExtension(i, current, blocks);
                current.Max.y = i + 1;
            } else break;
        }
    }

    private bool ExtendRectangle_VerifyExtension(int i, Rectangle current, Arr2d<bool> blocks) {
        for (int x = current.Min.x; x < current.Max.x; x++)
            if (!blocks[x, i])
                return false;
        return true;
    }

    private void ExtendRectangle_SetExtension(int i, Rectangle current, Arr2d<bool> blocks) {
        for (var x = current.Min.x; x < current.Max.x; x++)
            blocks[x, i] = false;
    }

    private struct Rectangle {
        public Vector2Int Min;
        public Vector2Int Max;

        public Rectangle(Vector2Int min, Vector2Int max) {
            Min = min;
            Max = max;
        }
    }
    #endregion

    private readonly Dictionary<TileDirection, Vector3> directionToRotation = new Dictionary<TileDirection, Vector3> {
        {TileDirection.N, new Vector3(0, -90, 0)},
        {TileDirection.S, new Vector3(0, -270, 0)},
        {TileDirection.E, new Vector3(0, 180, 0)},
        {TileDirection.W, new Vector3(0, 0, 0)},
        {TileDirection.SE, new Vector3(0, -90, 0)},
        {TileDirection.NE, new Vector3(0, 0, 0)},
        {TileDirection.SW, new Vector3(0, 180, 0)},
        {TileDirection.NW, new Vector3(0, 90, 0)},
        {TileDirection.ENW, new Vector3(0, 0, 0)},
        {TileDirection.NSE, new Vector3(0, 0, 0)}
    };
    private readonly Dictionary<TileDirection, TileType> directionToType = new Dictionary<TileDirection, TileType> {
        {TileDirection.None, TileType.None},
        {TileDirection.N, TileType.Straight},
        {TileDirection.S, TileType.Straight},
        {TileDirection.E, TileType.Straight},
        {TileDirection.W, TileType.Straight},
        {TileDirection.SE, TileType.Curve},
        {TileDirection.NE, TileType.Curve},
        {TileDirection.SW, TileType.Curve},
        {TileDirection.NW, TileType.Curve},
        {TileDirection.ENW, TileType.Curve},
        {TileDirection.NSE, TileType.Curve}
    };

    public enum TileType {
        None = 0,
        Straight = 1,
        Curve = 2,
    }

    public enum TileDirection {
        None = 0,
        N = 1,
        S = 2,
        E = 3,
        W = 4,
        SE = 5,
        NE = 6,
        SW = 7,
        NW = 8,
        ENW = 9,
        NSE = 10
    }
}