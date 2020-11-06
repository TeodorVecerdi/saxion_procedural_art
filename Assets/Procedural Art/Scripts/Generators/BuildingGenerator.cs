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
    [Space]
    public GameObject SmokePrefab;

    public LODData LOD0Data;
    public LODData LOD1Data;
    public LODData LOD2Data;

    protected float BuildingHeight;
    protected float RoofHeight;
    protected Vector2Int DimensionsA;
    protected Vector2Int DimensionsB;
    protected SLSettings GeneratorSettings;

    private bool addedSmoke;
    private bool triedToAddSmoke;

    public virtual void DoOnce(ref bool doneOnceField) {
        if (doneOnceField) return;
        doneOnceField = true;
    }

    public void GenerateFromPlot(PlotData plot, BuildingTypeSettings settings, float heightAdjustment, Vector3 offset) {
        GeneratorSettings = settings.GeneratorSettings as SLSettings;
        SetupLOD();
        Setup(settings);
        SetupMaterials(settings);
        addedSmoke = false;

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
        var finalMesh0 = MeshUtils.Translate(lod0, -0.5f * plot.Bounds.size.ToVec3() + new Vector3(0.5f, 0, 0.5f) /* + offset*/);
        var finalMesh1 = MeshUtils.Translate(lod1, -0.5f * plot.Bounds.size.ToVec3() + new Vector3(0.5f, 0, 0.5f) /* + offset*/);
        var finalMesh2 = MeshUtils.Translate(lod2, -0.5f * plot.Bounds.size.ToVec3() + new Vector3(0.5f, 0, 0.5f) /* + offset*/);
        SetMesh(LOD0Data, finalMesh0);
        SetMesh(LOD1Data, finalMesh1);
        SetMesh(LOD2Data, finalMesh2);
    }

    private void SetupLOD() {
        LOD0Data = new LODData(LOD0, "LOD0");
        LOD1Data = new LODData(LOD1, "LOD1");
        LOD2Data = new LODData(LOD2, "LOD2");
    }

    public virtual void Setup(BuildingTypeSettings settings) { }

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
        var roofs = GenSquareRoof();
        var path = MarchingSquares.March(boolArr);
        CleanupOutline(boolArr);
        var walls = GenWalls(path, LOD);

        var mesh = MeshUtils.Combine(roofs, walls);
        return mesh;
    }

    protected Arr2d<bool> GenSquare(Vector2Int size, float heightAdjustment) {
        BuildingHeight = heightAdjustment + RandUtils.RandomBetween(GeneratorSettings.SquareSettings.MinMaxHeight);
        DimensionsA = new Vector2Int(size.x, size.y);
        DimensionsB = Vector2Int.zero;
        var boolArr = new Arr2d<bool>(DimensionsA.x, DimensionsA.y, true);
        return boolArr;
    }

    protected virtual MeshData GenSquareRoof() {
        RoofHeight = RandUtils.RandomBetween(GeneratorSettings.SquareSettings.MinMaxRoofHeight);
        var thickness = GeneratorSettings.GeneralSettings.RoofThickness;
        var extrusion = GeneratorSettings.GeneralSettings.RoofExtrusion;
        var roofTypeRandomizer = new WeightedRandom(GeneratorSettings.SquareSettings.StraightRoofChance, GeneratorSettings.SquareSettings.DoubleCornerRoofChance, GeneratorSettings.SquareSettings.CornerRoofChance);
        roofTypeRandomizer.NormalizeWeights();
        roofTypeRandomizer.CalculateAdditiveWeights();
        var roofValue = roofTypeRandomizer.Value();
        var straightRoof = roofValue == 0;
        var doubleCornerRoof = roofValue == 1;
        var chimneyChance = GeneratorSettings.GeneralSettings.ChimneyChance;
        var chimneySmokeChance = GeneratorSettings.GeneralSettings.ChimneySmokeChance;
        var chimneyThickness = RandUtils.RandomBetween(GeneratorSettings.GeneralSettings.ChimneyThicknessMinMax);
        var chimney = new MeshData();
        if (RandUtils.BoolWeighted(chimneyChance)) {
            var chimneyHeight = RoofHeight / 2.0f + Rand.Range(0f, RoofHeight / 2.0f);
            var chimneyX = Rand.Range(chimneyThickness, DimensionsA.x - chimneyThickness);
            var chimneyZ = Rand.Range(chimneyThickness, 0.2f * DimensionsA.y);
            chimney.MergeMeshData(MeshGenerator.GetMesh<LineGenerator>(new Vector3(chimneyX, BuildingHeight, chimneyZ), Quaternion.identity, new Dictionary<string, dynamic> {
                {"start", Vector3.zero},
                {"end", Vector3.up * chimneyHeight},
                {"thickness", chimneyThickness},
                {"extrusion", chimneyThickness},
                {"extrusionCenter", true},
                {"submeshIndex", 4}
            }));
            if (RandUtils.BoolWeighted(chimneySmokeChance) && !addedSmoke && !triedToAddSmoke) {
                var smoke = Instantiate(SmokePrefab, Vector3.zero, Quaternion.identity);
                smoke.transform.parent = transform;
                smoke.transform.localPosition = new Vector3(chimneyX - DimensionsA.x / 2.0f + 0.5f, BuildingHeight + chimneyHeight - 0.5f, chimneyZ - DimensionsA.y / 2.0f + 0.5f);
                addedSmoke = true;
            }

            triedToAddSmoke = true;
        }

        if (straightRoof) {
            var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, BuildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", DimensionsA.x},
                {"height", RoofHeight},
                {"thickness", thickness},
                {"length", DimensionsA.y / 2f},
                {"extrusion", extrusion},
                {"addCap", true},
                {"closeRoof", true}
            });
            var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(DimensionsA.x - 0.5f, BuildingHeight, DimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", DimensionsA.x},
                {"height", RoofHeight},
                {"thickness", thickness},
                {"length", DimensionsA.y / 2f},
                {"extrusion", extrusion},
                {"addCap", true},
                {"closeRoof", true}
            });
            return MeshUtils.Combine(chimney, roofA, roofA1);
        }

        var cornerWidth = DimensionsA.y * RandUtils.RandomBetween(GeneratorSettings.SquareSettings.RoofWidthToBuildingWidthRatio);
        if (doubleCornerRoof) {
            var cornerA = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(DimensionsA.x - 0.5f, BuildingHeight, -0.5f), Quaternion.Euler(0, -90, 0), new Dictionary<string, dynamic> {
                {"width", cornerWidth},
                {"height", RoofHeight},
                {"length", DimensionsA.x / 2.0f},
                {"thickness", thickness},
                {"addCap", true},
                {"joinCaps", true}
            });
            var cornerA1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(DimensionsA.x - 0.5f, BuildingHeight, DimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", DimensionsA.x / 2.0f},
                {"height", RoofHeight},
                {"length", cornerWidth},
                {"thickness", thickness},
                {"addCap", true},
                {"joinCaps", true}
            });
            var cornerB = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(-0.5f, BuildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", DimensionsA.x / 2.0f},
                {"height", RoofHeight},
                {"length", cornerWidth},
                {"thickness", thickness},
                {"addCap", true},
                {"joinCaps", true}
            });
            var cornerB1 = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(-0.5f, BuildingHeight, DimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                {"width", cornerWidth},
                {"height", RoofHeight},
                {"length", DimensionsA.x / 2.0f},
                {"thickness", thickness},
                {"addCap", true},
                {"joinCaps", true}
            });
            var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(DimensionsA.x - 0.5f, BuildingHeight, cornerWidth - 0.5f), Quaternion.Euler(0, -90, 0), new Dictionary<string, dynamic> {
                {"width", DimensionsA.y - 2 * cornerWidth},
                {"height", RoofHeight},
                {"thickness", thickness},
                {"length", DimensionsA.x / 2f},
                {"extrusion", 0},
                {"addCap", true},
                {"closeRoof", true}
            });
            var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, BuildingHeight, DimensionsA.y - cornerWidth - 0.5f), Quaternion.Euler(0, 90, 0), new Dictionary<string, dynamic> {
                {"width", DimensionsA.y - 2 * cornerWidth},
                {"height", RoofHeight},
                {"thickness", thickness},
                {"length", DimensionsA.x / 2f},
                {"extrusion", 0},
                {"addCap", true},
                {"closeRoof", true}
            });
            return MeshUtils.Combine(chimney, cornerA, cornerA1, cornerB, cornerB1, roofA, roofA1);
        } else {
            var cornerA = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(DimensionsA.x - 0.5f, BuildingHeight, -0.5f), Quaternion.Euler(0, -90, 0), new Dictionary<string, dynamic> {
                {"width", DimensionsA.y / 2f},
                {"height", RoofHeight},
                {"length", cornerWidth},
                {"thickness", thickness},
                {"addCap", true},
                {"joinCaps", true}
            });
            var cornerB = MeshGenerator.GetMesh<CornerRoofGenerator>(new Vector3(DimensionsA.x - 0.5f, BuildingHeight, DimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", cornerWidth},
                {"height", RoofHeight},
                {"length", DimensionsA.y / 2f},
                {"thickness", thickness},
                {"addCap", true},
                {"joinCaps", true}
            });
            var roofA = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, BuildingHeight, -0.5f), Quaternion.identity, new Dictionary<string, dynamic> {
                {"width", DimensionsA.x - cornerWidth},
                {"height", RoofHeight},
                {"thickness", thickness},
                {"length", DimensionsA.y / 2f},
                {"extrusion", extrusion},
                {"addCap", true},
                {"extrusionRight", false},
                {"closeRoof", true}
            });
            var roofA1 = MeshGenerator.GetMesh<StraightRoofGenerator>(new Vector3(-0.5f, BuildingHeight, DimensionsA.y - 0.5f), Quaternion.Euler(0, 180, 0), new Dictionary<string, dynamic> {
                {"width", DimensionsA.x - cornerWidth},
                {"height", RoofHeight},
                {"thickness", thickness},
                {"length", DimensionsA.y / 2f},
                {"extrusion", extrusion},
                {"extrusionRight", false},
                {"addCap", true},
                {"flip", true},
                {"closeRoof", true}
            });
            return MeshUtils.Combine(chimney, cornerA, cornerB, roofA, roofA1);
        }
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
                {"sizeB", BuildingHeight},
                {"orientation", PlaneGenerator.PlaneOrientation.XY},
            });

            walls.MergeMeshData(wall);
            walls.MergeMeshData(AddWallFeatures(from, to, BuildingHeight, wallWidth, angle, LOD));
            current = next;
        }

        return walls;
    }

    protected virtual MeshData AddWallFeatures(Vector3 wallStart, Vector3 wallEnd, float buildingHeight, float wallWidth, float wallAngle, int LOD, bool addPillar = true) {
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
                if (RandUtils.BoolWeighted(windowChance / 2.0f)) addWindowHorizontal = false;
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

                    if (RandUtils.BoolWeighted(GeneratorSettings.GeneralSettings.FillWallSegmentChance) && LOD <= 0) {
                        var fillWallSpacing = RandUtils.RandomBetween(GeneratorSettings.GeneralSettings.FillWallSegmentSpacing);
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
                        {"start", Vector3.zero},
                        {"end", wallDirection * diff + Vector3.up * (splitSectionsVertical[i] - splitSectionsVertical[i - 1] - thickness)},
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
                        {"start", wallDirection * diff},
                        {"end", Vector3.up * (splitSectionsVertical[i] - splitSectionsVertical[i - 1] - thickness)},
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
}