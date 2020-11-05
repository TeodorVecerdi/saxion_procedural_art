using System.Collections.Generic;
using UnityEngine;

public class PoorSBuildingGenerator : BuildingGenerator {
    public static new bool DoneOnceField = false;

    public override void DoOnce(ref bool doneOnceField) {
        if (doneOnceField) return;
        doneOnceField = true;
    }

    public override void Setup(BuildingTypeSettings settings) { }

    public override MeshData Generate(PlotData plot, BuildingTypeSettings settings, float heightAdjustment, Vector3 offset, int LOD) {
        DoOnce(ref DoneOnceField);
        var size = new Vector2Int(Mathf.RoundToInt(plot.Bounds.size.x), Mathf.RoundToInt(plot.Bounds.size.y));

        var boolArr = GenSquare(size, heightAdjustment);
        var roofs = GenSquareRoof();
        var path = MarchingSquares.March(boolArr);
        CleanupOutline(boolArr);
        var walls = GenWalls(path, LOD);
        var mesh = MeshUtils.Combine(roofs, walls);

        return mesh;
    }

    protected override MeshData AddWallFeatures(Vector3 wallStart, Vector3 wallEnd, float buildingHeight, float wallWidth, float wallAngle, int LOD, bool addPillar = true) {
        var thickness = GeneratorSettings.GeneralSettings.FeatureThickness;
        var diagonalAChance = GeneratorSettings.GeneralSettings.DiagonalAChance;
        var diagonalBChance = GeneratorSettings.GeneralSettings.DiagonalBChance;
        var windowChance = GeneratorSettings.GeneralSettings.WindowChance;

        var wallDirection = (wallEnd - wallStart).normalized;
        var wallPerpendicular = Vector3.Cross(wallDirection, Vector3.up);
        var wallSize = Mathf.RoundToInt((wallEnd - wallStart).magnitude);

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

        var splitSectionsVertical = new List<float> {0, 1};
        var lastSplitPoint = splitSectionsVertical[1];
        while (lastSplitPoint < buildingHeight) {
            var nextSize = RandUtils.RandomBetween(GeneratorSettings.GeneralSettings.VerticalSplitMinMax);
            if (lastSplitPoint + nextSize < buildingHeight && lastSplitPoint + nextSize > buildingHeight - 1) nextSize = buildingHeight - lastSplitPoint - 1;
            lastSplitPoint += nextSize;
            if (lastSplitPoint >= buildingHeight) lastSplitPoint = buildingHeight;
            splitSectionsVertical.Add(lastSplitPoint);
        }

        var splitSectionsHorizontal = new List<float> {0};
        var lastSplitPointH = splitSectionsHorizontal[0];
        while (lastSplitPointH < wallSize) {
            var nextSize = RandUtils.RandomBetween(GeneratorSettings.GeneralSettings.HorizontalSplitMinMax);
            if (lastSplitPointH + nextSize < wallSize && lastSplitPointH + nextSize > wallSize - 1) nextSize = wallSize - lastSplitPointH - 1;
            lastSplitPointH += nextSize;
            if (lastSplitPointH > wallSize) lastSplitPointH = wallSize;
            splitSectionsHorizontal.Add(lastSplitPointH);
        }

        for (var i = 1; i < splitSectionsVertical.Count; i++) {
            var sectionHeight = splitSectionsVertical[i] - splitSectionsVertical[i - 1];
            var addWindow = wallSize > 2 && RandUtils.BoolWeighted(windowChance) && LOD <= 1;
            var windowWidth = 0.0f;
            var windowHeight = 0.0f;

            if (addWindow) {
                windowHeight = Rand.Range(1f, Mathf.Max(1f, sectionHeight * 0.75f));
            }

            for (var i2 = 1; i2 < splitSectionsHorizontal.Count; i2++) {
                var sectionWidth = splitSectionsHorizontal[i2] - splitSectionsHorizontal[i2 - 1];
                var addWindowHorizontal = addWindow;
                if (RandUtils.BoolWeighted(windowChance/2.0f)) addWindowHorizontal = false;
                if (addWindowHorizontal) {
                    windowWidth = Rand.Range(1f, Mathf.Max(1f, sectionWidth * 0.75f));
                }

                if (LOD <= 1 && i2 != splitSectionsHorizontal.Count - 1) {
                    var verticalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * splitSectionsHorizontal[i2] + Vector3.up * (thickness / 2f), Quaternion.identity, new Dictionary<string, dynamic> {
                        {"start", Vector3.zero},
                        {"end", Vector3.up * buildingHeight},
                        {"thickness", thickness},
                        {"extrusion", thickness},
                        {"submeshIndex", 2},
                        {"extrusionCenter", true}
                    });
                    features.MergeMeshData(verticalLine);
                }

                var horizontalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + Vector3.up * splitSectionsVertical[i] - wallPerpendicular * thickness / 2f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero + Vector3.right * thickness / 2f},
                    {"end", Vector3.right * (wallWidth - thickness / 2f)},
                    {"thickness", thickness},
                    {"extrusion", thickness},
                    {"submeshIndex", 2},
                    {"rotateUV", true}
                });
                if (LOD <= 1)
                    features.MergeMeshData(horizontalLine);
                if (i == 1)
                    continue;

                if (LOD <= 1 && addWindowHorizontal) {
                    var windowPosition = splitSectionsHorizontal[i2] - 0.5f * (sectionWidth);
                    var window = MeshGenerator.GetMesh<PlaneGenerator>(wallStart + wallDirection * (windowPosition + 0.5f * windowWidth) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) + wallPerpendicular * thickness / 4f, Quaternion.Euler(0, wallAngle - 180, 0), new Dictionary<string, dynamic> {
                        {"sizeA", windowWidth},
                        {"sizeB", windowHeight},
                        {"orientation", PlaneGenerator.PlaneOrientation.XY},
                        {"submeshIndex", 3},
                        {"extraUvSettings", MeshGenerator.UVSettings.NoOffset}
                    });
                    features.MergeMeshData(window);

                    /* BEGIN Frame */
                    features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition - 0.5f * windowWidth) + Vector3.up * (splitSectionsVertical[i - 1] + 0.33334f * windowHeight), Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                        {"start", Vector3.zero},
                        {"end", Vector3.right * windowWidth},
                        {"thickness", 0.035f},
                        {"extrusion", 0.035f},
                        {"submeshIndex", 2},
                        {"rotateUV", true}
                    }));
                    features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition - 0.5f * windowWidth) + Vector3.up * (splitSectionsVertical[i - 1] + windowHeight - 0.33334f * windowHeight), Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                        {"start", Vector3.zero},
                        {"end", Vector3.right * windowWidth},
                        {"thickness", 0.035f},
                        {"extrusion", 0.035f},
                        {"submeshIndex", 2},
                        {"rotateUV", true}
                    }));
                    features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f * windowWidth - 0.33334f * windowWidth) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * 0.005f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                        {"start", Vector3.zero},
                        {"end", Vector3.up * (windowHeight - thickness / 2f)},
                        {"thickness", 0.035f},
                        {"extrusion", 0.035f},
                        {"submeshIndex", 2},
                        {"rotateUV", true}
                    }));
                    features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition - 0.5f * windowWidth + 0.33334f * windowWidth) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * 0.005f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                        {"start", Vector3.zero},
                        {"end", Vector3.up * (windowHeight - thickness / 2f)},
                        {"thickness", 0.035f},
                        {"extrusion", 0.035f},
                        {"submeshIndex", 2},
                        {"rotateUV", true}
                    }));
                    /* END Frame */

                    if (sectionHeight > 1) {
                        var line = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition - 0.5f * windowWidth) + Vector3.up * (splitSectionsVertical[i - 1] + windowHeight) - wallPerpendicular * thickness / 2f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                            {"start", Vector3.zero},
                            {"end", Vector3.right * windowWidth},
                            {"thickness", thickness},
                            {"extrusion", thickness},
                            {"submeshIndex", 2},
                            {"rotateUV", true}
                        });
                        features.MergeMeshData(line);
                    }

                    if (sectionWidth > 1) {
                        var lineLeft = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f * windowWidth + 0.5f * thickness) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * thickness / 2f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                            {"start", Vector3.zero},
                            {"end", Vector3.up * windowHeight},
                            {"thickness", thickness},
                            {"extrusion", thickness},
                            {"submeshIndex", 2},
                            {"rotateUV", true}
                        });
                        features.MergeMeshData(lineLeft);
                        var lineRight = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition - 0.5f * windowWidth - 0.5f * thickness) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * thickness / 2f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                            {"start", Vector3.zero},
                            {"end", Vector3.up * windowHeight},
                            {"thickness", thickness},
                            {"extrusion", thickness},
                            {"submeshIndex", 2},
                            {"rotateUV", true}
                        });
                        features.MergeMeshData(lineRight);
                    }
                }

                if (RandUtils.BoolWeighted(diagonalAChance) && LOD <= 0) {
                    var diff = splitSectionsHorizontal[i2] - splitSectionsHorizontal[i2 - 1];
                    var diagonalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * splitSectionsHorizontal[i2 - 1] + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * thickness / 2.5f, Quaternion.identity, new Dictionary<string, dynamic> {
                        {"start", Vector3.zero - Vector3.up * (i == 1 ? thickness / 2f : 0)},
                        {"end", wallDirection * diff + Vector3.up * (splitSectionsVertical[i] - splitSectionsVertical[i - 1] - thickness + (i == 1 ? 1f * thickness : 0))},
                        {"thickness", thickness},
                        {"extrusion", thickness},
                        {"submeshIndex", 2},
                        {"extrusionCenter", true}
                    });
                    features.MergeMeshData(diagonalLine);
                }

                if (RandUtils.BoolWeighted(diagonalBChance) && LOD <= 0) {
                    var diff = splitSectionsHorizontal[i2] - splitSectionsHorizontal[i2 - 1];
                    var diagonalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * splitSectionsHorizontal[i2 - 1] + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * thickness / 2.5f, Quaternion.identity, new Dictionary<string, dynamic> {
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

        return features;
    }
}