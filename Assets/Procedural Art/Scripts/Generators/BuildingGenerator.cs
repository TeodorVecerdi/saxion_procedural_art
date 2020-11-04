using System.Collections.Generic;
using UnityEngine;

public class LODData {
    public readonly Mesh Mesh;
    public readonly MeshFilter MeshFilter;
    public readonly MeshRenderer MeshRenderer;

    public LODData(GameObject parent, string LODName) {
        Mesh = new Mesh {name = $"Building_{LODName}"};
        MeshFilter = parent.GetComponent<MeshFilter>();
        MeshRenderer = parent.GetComponent<MeshRenderer>();
    }
}

[ExecuteInEditMode]
public class BuildingGenerator : MonoBehaviour {
    public static bool DoneOnceField = false;
    public GameObject LOD0;
    public GameObject LOD1;
    public GameObject LOD2;
    [HideInInspector] public RandomSettings GeneratorSettings;

    public LODData LOD0Data;
    public LODData LOD1Data;
    public LODData LOD2Data;

    private WeightedRandom buildingTypeSelector;

    protected float buildingHeight;
    protected Vector2Int dimensionsA;
    protected Vector2Int dimensionsB;

    public virtual void DoOnce(ref bool doneOnceField) {
        if (doneOnceField) return;
        doneOnceField = true;
    }

    public void GenerateFromPlot(PlotData plot, BuildingTypeSettings settings, float heightAdjustment, Vector3 offset) {
        GeneratorSettings = settings.GeneratorSettings as RandomSettings;
        SetupLOD();
        Setup(settings);
        SetupMaterials(settings);

        // This keeps the buildings mainly the same over the different LOD levels
        var buildingSeed = Rand.Int;
        Rand.PushState(buildingSeed);
        var lod0 = Generate(plot, settings, heightAdjustment, offset, 0);
        Rand.PopState();
        Rand.PushState(buildingSeed);
        var lod1 = Generate(plot, settings, heightAdjustment, offset, 1);
        Rand.PopState();
        Rand.PushState(buildingSeed);
        var lod2 = Generate(plot, settings, heightAdjustment, offset, 2);
        Rand.PopState();
        var finalMesh0 = MeshUtils.Translate(lod0, -0.5f * plot.Bounds.size.ToVec3() + new Vector3(0.5f, 0, 0.5f) + offset);
        var finalMesh1 = MeshUtils.Translate(lod1, -0.5f * plot.Bounds.size.ToVec3() + new Vector3(0.5f, 0, 0.5f) + offset);
        var finalMesh2 = MeshUtils.Translate(lod2, -0.5f * plot.Bounds.size.ToVec3() + new Vector3(0.5f, 0, 0.5f) + offset);
        SetMesh(LOD0Data, finalMesh0);
        SetMesh(LOD1Data, finalMesh1);
        SetMesh(LOD2Data, finalMesh2);
    }

    private void SetupLOD() {
        LOD0Data = new LODData(LOD0, "LOD0");
        LOD1Data = new LODData(LOD1, "LOD1");
        LOD2Data = new LODData(LOD2, "LOD2");
    }

    public virtual void Setup(BuildingTypeSettings settings) {
        var generatorSettings = settings.GeneratorSettings as RandomSettings;

        // Random Weight adjusting
        buildingTypeSelector = new WeightedRandom(generatorSettings.GeneralSettings.SquareChance, generatorSettings.GeneralSettings.LChance);
        buildingTypeSelector.NormalizeWeights();
        buildingTypeSelector.CalculateAdditiveWeights();
    }

    private void SetupMaterials(BuildingTypeSettings settings) {
        var sharedMaterials = new Material[6];
        LOD0Data.MeshRenderer.sharedMaterial = settings.MaterialSetting.WallMaterial;
        LOD1Data.MeshRenderer.sharedMaterial = settings.MaterialSetting.WallMaterial;
        LOD2Data.MeshRenderer.sharedMaterial = settings.MaterialSetting.WallMaterial;
        sharedMaterials[0] = settings.MaterialSetting.WallMaterial;
        sharedMaterials[1] = settings.MaterialSetting.RoofMaterial;
        sharedMaterials[2] = settings.MaterialSetting.FeatureMaterial1 != null ? settings.MaterialSetting.FeatureMaterial1 : settings.MaterialSetting.WallMaterial;
        sharedMaterials[3] = settings.MaterialSetting.WindowMaterial;
        sharedMaterials[4] = settings.MaterialSetting.FeatureMaterial2 != null ? settings.MaterialSetting.FeatureMaterial2 : settings.MaterialSetting.WallMaterial;
        sharedMaterials[5] = settings.MaterialSetting.FeatureMaterial3 != null ? settings.MaterialSetting.FeatureMaterial3 : settings.MaterialSetting.WallMaterial;
        LOD0Data.MeshRenderer.sharedMaterials = sharedMaterials;
        LOD1Data.MeshRenderer.sharedMaterials = sharedMaterials;
        LOD2Data.MeshRenderer.sharedMaterials = sharedMaterials;
    }

    private void SetMesh(LODData lodData, MeshData mesh) {
        if (lodData.MeshFilter.sharedMesh != null)
            DestroyImmediate(lodData.MeshFilter.sharedMesh);
        lodData.MeshFilter.sharedMesh = lodData.Mesh;

        lodData.Mesh.SetVertices(mesh.Vertices);
        lodData.Mesh.SetUVs(0, mesh.UVs);
        lodData.Mesh.subMeshCount = lodData.MeshRenderer.sharedMaterials.Length;
        foreach (var submesh in mesh.Triangles.Keys) {
            lodData.Mesh.SetTriangles(mesh.Triangles[submesh], submesh);
        }

        lodData.Mesh.RecalculateNormals();
    }

    public virtual MeshData Generate(PlotData plot, BuildingTypeSettings settings, float heightAdjustment, Vector3 offset, int LOD) {
        DoOnce(ref DoneOnceField);
        var size = new Vector2Int(Mathf.RoundToInt(plot.Bounds.size.x), Mathf.RoundToInt(plot.Bounds.size.y));

        var boolArr = GenSquare(size, heightAdjustment);
        var overhang = GenSquareOverhang(boolArr, LOD);
        var roofs = GenSquareRoof();
        var path = MarchingSquares.March(boolArr);
        CleanupOutline(boolArr);
        var walls = GenWalls(path, LOD);

        return MeshUtils.Combine(roofs, walls, overhang);
    }

    private Arr2d<bool> GenSquare(Vector2Int size, float heightAdjustment) {
        buildingHeight = heightAdjustment + Rand.RangeInclusive(GeneratorSettings.SquareBuildingSettings.MinSize.y, GeneratorSettings.SquareBuildingSettings.MaxSize.y);
        dimensionsA = new Vector2Int(size.x, size.y);
        dimensionsB = Vector2Int.zero;
        var boolArr = new Arr2d<bool>(dimensionsA.x, dimensionsA.y, true);
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
                var cornerWidth = Rand.Range(dimensionsA.x / 4f, dimensionsA.x / 2f);
                var cornerWidthB = Rand.Range(dimensionsA.x / 4f, dimensionsA.x / 2f);
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
                var cornerWidth = Rand.Range(dimensionsA.x / 4f, dimensionsA.x / 2f);
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

    private MeshData GenSquareOverhang(Arr2d<bool> layout, int LOD) {
        if (!RandUtils.BoolWeighted(GeneratorSettings.SquareBuildingSettings.OverhangChance))
            return new MeshData();

        var horizontal = Rand.Bool;
        var width = horizontal ? Rand.RangeInclusive(Mathf.CeilToInt(dimensionsA.x / 4f), Mathf.CeilToInt(dimensionsA.x / 3f)) : Rand.RangeInclusive(Mathf.CeilToInt(dimensionsA.y / 4f), Mathf.CeilToInt(dimensionsA.y / 3f));
        if (horizontal) {
            layout.Fill(new Vector2Int(layout.Length1 - width, 0), new Vector2Int(layout.Length1, layout.Length2), false);
        } else {
            layout.Fill(new Vector2Int(0, layout.Length2 - width), new Vector2Int(layout.Length1, layout.Length2), false);
        }

        var meshes = new List<MeshData>();
        var thickness = 0.25f;
        var start = Mathf.FloorToInt(Rand.Range(1, 3f * buildingHeight / 4f));
        var height = buildingHeight - start;

        var archChance = 0.5f;
        var archLOD = LOD == 0 ? 45 : LOD == 1 ? 21 : 7;

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

    protected MeshData GenWalls(List<Vector2Int> path, int LOD) {
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
            walls.MergeMeshData(AddWallFeatures(from, to, buildingHeight, wallWidth, angle, LOD));
            current = next;
        }

        return walls;
    }

    protected MeshData AddWallFeatures(Vector3 wallStart, Vector3 wallEnd, float buildingHeight, float wallWidth, float wallAngle, int LOD, bool addPillar = true) {
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

        
        var splitSectionsVertical = new List<float> {0, 1};
        var lastSplitPoint = splitSectionsVertical[1];
        while (lastSplitPoint < buildingHeight) {
            var nextSize = Rand.RangeInclusive(1, 2);
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
            if (LOD <= 1)
                features.MergeMeshData(horizontalLine);
            if (i == 1)
                continue;
            var wallSize = Mathf.RoundToInt((wallEnd - wallStart).magnitude);
            var sectionHeight = splitSectionsVertical[i] - splitSectionsVertical[i - 1];
            if (wallSize > 2 && RandUtils.BoolWeighted(windowChance) && LOD <= 1) {
                var windowPosition = wallSize / 2f;
                var window = MeshGenerator.GetMesh<PlaneGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) + wallPerpendicular * thickness / 4f, Quaternion.Euler(0, wallAngle - 180, 0), new Dictionary<string, dynamic> {
                    {"sizeA", 1},
                    {"sizeB", 1},
                    {"orientation", PlaneGenerator.PlaneOrientation.XY},
                    {"submeshIndex", 3},
                    {"extraUvSettings", MeshGenerator.UVSettings.NoOffset}
                });
                features.MergeMeshData(window);
                features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f - 1) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f + 0.3f), Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero},
                    {"end", Vector3.right},
                    {"thickness", 0.035f},
                    {"extrusion", 0.035f},
                    {"submeshIndex", 2},
                    {"rotateUV", true}
                }));
                features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f - 1) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f + 0.6f), Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero},
                    {"end", Vector3.right},
                    {"thickness", 0.035f},
                    {"extrusion", 0.035f},
                    {"submeshIndex", 2},
                    {"rotateUV", true}
                }));
                features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f - 1 + 0.375f) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * 0.005f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero},
                    {"end", Vector3.up * (1 - thickness / 2f)},
                    {"thickness", 0.035f},
                    {"extrusion", 0.035f},
                    {"submeshIndex", 2},
                    {"rotateUV", true}
                }));
                features.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (windowPosition + 0.5f - thickness / 2f - 1 + 0.725f) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * 0.005f, Quaternion.Euler(0, wallAngle, 0), new Dictionary<string, dynamic> {
                    {"start", Vector3.zero},
                    {"end", Vector3.up * (1 - thickness / 2f)},
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
                    lastSplitPointH += Rand.RangeInclusive(1, 2);
                    if (lastSplitPointH > wallSize) lastSplitPointH = wallSize;
                    splitSectionsHorizontal.Add(lastSplitPointH);
                }

                for (var i2 = 1; i2 < splitSectionsHorizontal.Count; i2++) {
                    if (LOD <= 1 && i2 != splitSectionsHorizontal.Count - 1) {
                        var verticalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * splitSectionsHorizontal[i2] + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f), Quaternion.identity, new Dictionary<string, dynamic> {
                            {"start", Vector3.zero},
                            {"end", Vector3.up * (splitSectionsVertical[i] - splitSectionsVertical[i - 1] - thickness)},
                            {"thickness", thickness},
                            {"extrusion", thickness},
                            {"submeshIndex", 2},
                            {"extrusionCenter", true}
                        });
                            features.MergeMeshData(verticalLine);
                    }

                    if (RandUtils.BoolWeighted(fillWallSegmentChance) && LOD <= 0) {
                        var fillWallSpacing = Rand.Range(0.01f, 0.02f);
                        var fillWallThickness = thickness - fillWallSpacing;
                        var totalWidth = splitSectionsHorizontal[i2] - splitSectionsHorizontal[i2 - 1] - fillWallSpacing;
                        var totalSteps = (int) (totalWidth / thickness);

                        for (var fillIndex = 0; fillIndex < totalSteps; fillIndex++) {
                            var verticalLine = MeshGenerator.GetMesh<LineGenerator>(wallStart + wallDirection * (splitSectionsHorizontal[i2 - 1] + fillIndex * (fillWallThickness + fillWallSpacing) + thickness) + Vector3.up * (splitSectionsVertical[i - 1] + thickness / 2f) - wallPerpendicular * thickness / 4f, Quaternion.identity, new Dictionary<string, dynamic> {
                                {"start", Vector3.zero - Vector3.zero},
                                {"end", Vector3.up * (splitSectionsVertical[i] - splitSectionsVertical[i - 1] - thickness + (i == 1 ? 1.5f * thickness : 0))},
                                {"thickness", fillWallThickness},
                                {"extrusion", fillWallThickness},
                                {"submeshIndex", 2},
                                {"extrusionCenter", true}
                            });
                            features.MergeMeshData(verticalLine);
                        }
                    }

                    if (RandUtils.BoolWeighted(diagonalAChance) && LOD <= 0) {
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

                    if (RandUtils.BoolWeighted(diagonalBChance) && LOD <= 0) {
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

    protected void CleanupOutline(Arr2d<bool> boolArr) {
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