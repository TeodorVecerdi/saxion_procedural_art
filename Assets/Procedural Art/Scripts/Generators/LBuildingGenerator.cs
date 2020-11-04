using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LBuildingGenerator : BuildingGenerator {
    public static new bool DoneOnceField = false;

    public override void DoOnce(ref bool doneOnceField) {
        if (doneOnceField) return;
        doneOnceField = true;
    }

    public override MeshData Generate(PlotData plot, BuildingTypeSettings settings, float heightAdjustment, Vector3 offset, int LOD) {
        DoOnce(ref DoneOnceField);

        var size = new Vector2Int(Mathf.RoundToInt(plot.Bounds.size.x), Mathf.RoundToInt(plot.Bounds.size.y));
        var boolArr = GenL(size, heightAdjustment);
        var overhang = GenLOverhang(LOD);
        var roofs = GenLRoof();
        var path = MarchingSquares.March(boolArr);
        CleanupOutline(boolArr);
        var walls = GenWalls(path, LOD);

        return MeshUtils.Combine(roofs, walls, overhang);
    }

    private Arr2d<bool> GenL(Vector2Int size, float heightAdjustment) {
        var widthA = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxWidthA);
        var lengthA = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxLengthA);
        var widthB = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxWidthB);

        // widthB = MathUtils.Clamp(widthB, 2, widthA - 1);
        var lengthB = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxLengthB);

        // lengthB = MathUtils.Clamp(lengthB, 1, Mathf.CeilToInt(lengthA / 2f));
        buildingHeight =  heightAdjustment+ RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxHeight);

        // normalize size so it's in range of <size>
        var proportionWA = widthA / (float) (widthA + widthB);
        var proportionWB = widthB / (float) (widthA + widthB);
        var proportionLA = lengthA / (float) (lengthA + lengthB);
        var proportionLB = lengthB / (float) (lengthA + lengthB);
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

    private MeshData GenLRoof() {
        var height = 1.5f;
        var thickness = 0.15f;
        var extrusion = 0.25f;
        var cornerRoofEndingChance = 0.0f;

        var roofs = new List<MeshData>();
        if (RandUtils.BoolWeighted(cornerRoofEndingChance)) {
            var roofInset = Rand.Range(dimensionsB.x / 2f, dimensionsB.x);
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

    private MeshData GenLOverhang(int LOD) {
        if (!RandUtils.BoolWeighted(GeneratorSettings.LBuildingSettings.OverhangChance))
            return new MeshData();

        var featureThickness = 0.1f;
        var meshes = new List<MeshData>();
        var thickness = 0.25f;
        var start = 2;
        var end = Rand.Range(start + 1, buildingHeight - 1);
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
        meshes.Add(AddWallFeatures(fromA, toA, height, dimensionsB.x, 0, LOD, false));
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
        meshes.Add(AddWallFeatures(fromB, toB, height, dimensionsB.y, 90, LOD, false));
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
        var archLOD = LOD == 0 ? 45 : LOD == 1 ? 21 : 7;
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

    private void CarveLShape(Arr2d<bool> arr) {
        var from = new Vector2Int(dimensionsA.x - dimensionsB.x, dimensionsA.y - dimensionsB.y);
        var to = new Vector2Int(dimensionsA.x, dimensionsA.y);
        arr.Fill(from, to, false);
    }
}