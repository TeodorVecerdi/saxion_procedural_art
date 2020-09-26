using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuildingGenerator : MonoBehaviour {
    public GameObject StraightPrefab;
    public GameObject CurvePrefab;
    public RoofGenerator RoofPrefab;
    public CornerRoofGenerator CornerRoofPrefab;
    public RandomSettings GeneratorSettings;
    public FeatureSettings Features;

    private WeightedRandom buildingTypeSelector;
    private Arr3d<GameObject> gridObjects;
    private List<GameObject> roofObjects;
    private List<Rectangle> rects;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
            Generate();
    }

    private void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        if (rects != null && gridObjects != null) {
            foreach (var rectangle in rects) {
                var avgV2 = rectangle.Min + rectangle.Max;
                var sizeV2 = rectangle.Max - rectangle.Min;
                var avg = new Vector3(avgV2.x / 2f - 0.5f, gridObjects.Length2, avgV2.y / 2f - 0.5f);
                var size = new Vector3(sizeV2.x - 0.2f, .1f, sizeV2.y - 0.2f);
                Gizmos.DrawCube(avg, size);
            }
        }
    }

    private void Generate() {
        if (GeneratorSettings == null) {
            throw new Exception("Generator Settings cannot be null! Make sure to assign a RandomSettings object to the class before calling BuildingGenerator::Generate");
        }

        Clear();
        Setup();
        var buildingType = buildingTypeSelector.Value();
        var boolArr = buildingType == 0 ? GenSquare() : buildingType == 1 ? GenL() : GenT();
        var tileDirections = GetDirections(boolArr);
        CleanupOutline(tileDirections);
        SpawnBuilding(tileDirections);
        rects = GetRoofRectangles(boolArr);

        // SpawnRoof(rects, boolArr.Length2);
    }

    private void Clear() {
        if (gridObjects != null)
            foreach (var obj in gridObjects) {
                Destroy(obj);
            }

        if (roofObjects != null)
            foreach (var obj in roofObjects) {
                Destroy(obj);
            }

        gridObjects = null;
        roofObjects = null;
    }

    private void Setup() {
        // Seed
        var seed = GeneratorSettings.GeneralSettings.Seed;
        if (GeneratorSettings.GeneralSettings.AutoSeed)
            seed = DateTime.Now.Ticks;
        Random.InitState((int) seed);

        // Random Weight adjusting
        buildingTypeSelector = new WeightedRandom(GeneratorSettings.GeneralSettings.SquareChance, GeneratorSettings.GeneralSettings.LChance, GeneratorSettings.GeneralSettings.TChance);
        buildingTypeSelector.NormalizeWeights();
        buildingTypeSelector.CalculateAdditiveWeights();
    }

    private Arr3d<bool> GenSquare() {
        var size = RandUtils.RandomBetween(GeneratorSettings.SquareBuildingSettings.MinSize, GeneratorSettings.SquareBuildingSettings.MaxSize);
        var boolArr = new Arr3d<bool>(size, true);
        return boolArr;
    }

    private Arr3d<bool> GenL() {
        var widthA = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxWidthA);
        var lengthA = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxLengthA);
        var widthB = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxWidthB);
        widthB = MathUtils.Clamp(widthB, 2, widthA - 1);
        var lengthB = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxLengthB);
        lengthB = MathUtils.Clamp(lengthB, 1, Mathf.CeilToInt(lengthA / 2f));
        var height = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxHeight);

        var dimensions = new Vector3Int(widthA, height, lengthA);
        var boolArr = new Arr3d<bool>(dimensions, true);
        CarveLShape(boolArr, dimensions, widthB, lengthB);
        SpawnLRoof(dimensions, widthB, lengthB);
        return boolArr;
    }

    private void SpawnLRoof(Vector3 dimensions, float widthB, float lengthB) {
        var height = 1f;
        var height2 = 1.5f;
        var thickness = 0.15f;

        // roof A/A'
        var roofA = Instantiate(RoofPrefab, new Vector3(-0.5f, dimensions.y, -0.5f), Quaternion.identity, transform);
        roofA.InitializeAndBuild(dimensions.x - widthB / 2f, height, (dimensions.z - lengthB) / 2f, thickness, Vector3.one, false);
        var roofA1 = Instantiate(RoofPrefab, new Vector3(-0.5f, dimensions.y, dimensions.z - lengthB - 0.5f), Quaternion.Euler(0, 180, 0), transform);
        roofA1.InitializeAndBuild(dimensions.x - widthB / 2f, height, (dimensions.z - lengthB) / 2f, thickness, Vector3.one, false, flip: true);

        // roof B/B'
        var roofB = Instantiate(RoofPrefab, new Vector3(dimensions.x - widthB - 0.5f, dimensions.y, dimensions.z - 0.5f), Quaternion.Euler(0, 90, 0), transform);
        roofB.InitializeAndBuild(lengthB + (dimensions.z - lengthB) / 2f, height2, widthB / 2f, thickness, Vector3.one, false);
        var roofB1 = Instantiate(RoofPrefab, new Vector3(dimensions.x - 0.5f, dimensions.y, dimensions.z - 0.5f), Quaternion.Euler(0, 270, 0), transform);
        roofB1.InitializeAndBuild(lengthB + (dimensions.z - lengthB) / 2f, height2, widthB / 2f, thickness, Vector3.one, false, flip: true);

        // corner
        var cornerA = Instantiate(CornerRoofPrefab, new Vector3(dimensions.x - 0.5f, dimensions.y, -0.5f), Quaternion.Euler(0, -90, 0), transform);
        cornerA.InitializeAndBuild((dimensions.z - lengthB) / 2f, height2, widthB / 2f, thickness, Vector3.zero, false);
        var cornerB = Instantiate(CornerRoofPrefab, new Vector3(dimensions.x - widthB - 0.5f, dimensions.y, -0.5f), Quaternion.Euler(0, -90, 0), transform);
        cornerB.InitializeAndBuild((dimensions.z - lengthB) / 2f, height2, widthB / 2f, thickness, Vector3.zero, false, flipZ: true);
        roofObjects = new List<GameObject> {
            roofA.gameObject,
            roofA1.gameObject,
            roofB.gameObject,
            roofB1.gameObject,
            cornerA.gameObject,
            cornerB.gameObject
        };
    }

    private Arr3d<bool> GenT() {
        var width = RandUtils.RandomBetween(GeneratorSettings.TBuildingSettings.MinMaxWidth);
        var length = RandUtils.RandomBetween(GeneratorSettings.TBuildingSettings.MinMaxLength);
        var extrusion = RandUtils.RandomBetween(GeneratorSettings.TBuildingSettings.MinMaxExtrusion);
        extrusion = MathUtils.Clamp(extrusion, 1, Mathf.CeilToInt(length / 2f));
        var inset = RandUtils.RandomBetween(GeneratorSettings.TBuildingSettings.MinMaxInset);
        inset = MathUtils.Clamp(inset, 1, Mathf.CeilToInt(width / 4f));
        var height = RandUtils.RandomBetween(GeneratorSettings.TBuildingSettings.MinMaxHeight);

        var dimensions = new Vector3Int(width, height, length);
        var boolArr = new Arr3d<bool>(dimensions, true);
        CarveTShape(boolArr, dimensions, extrusion, inset);
        return boolArr;
    }

    private Arr3d<TileDirection> GetDirections(Arr3d<bool> arr) {
        var tileDirections = new Arr3d<TileDirection>(arr.Length);
        var converted = new Arr3d<bool>(arr.Length);

        for (int x = 0; x < arr.Length1; x++) {
            for (int y = 0; y < arr.Length2; y++) {
                for (int z = 0; z < arr.Length3; z++) {
                    var north = arr[x, y, z - 1];
                    var south = arr[x, y, z + 1];
                    var west = arr[x - 1, y, z];
                    var east = arr[x + 1, y, z];
                    var northEast = arr[x + 1, y, z - 1];
                    var northWest = arr[x - 1, y, z - 1];
                    var southEast = arr[x + 1, y, z + 1];
                    var southWest = arr[x - 1, y, z + 1];
                    if (!arr[x, y, z] || converted[x, y, z]) continue;

                    #region Around 400 billion if checks
                    // Straight walls
                    if (west && east) {
                        if (!north) {
                            // straight north
                            tileDirections[x, y, z] = TileDirection.N;
                            converted[x, y, z] = true;
                        } else if (!south) {
                            // straight south
                            tileDirections[x, y, z] = TileDirection.S;
                            converted[x, y, z] = true;
                        }
                    }

                    if (north && south) {
                        if (!west) {
                            // straight west
                            tileDirections[x, y, z] = TileDirection.W;
                            converted[x, y, z] = true;
                        } else if (!east) {
                            // straight east
                            tileDirections[x, y, z] = TileDirection.E;
                            converted[x, y, z] = true;
                        }
                    }

                    // Curved walls
                    if (east && south && !west && !north) {
                        // corner -90
                        tileDirections[x, y, z] = TileDirection.SE;
                        converted[x, y, z] = true;
                    }

                    if (east && north && !west && !south) {
                        // corner 0
                        tileDirections[x, y, z] = TileDirection.NE;
                        converted[x, y, z] = true;
                    }

                    if (west && south && !east && !north) {
                        // corner 180
                        tileDirections[x, y, z] = TileDirection.SW;
                        converted[x, y, z] = true;
                    }

                    if (west && north && !east && !south) {
                        // corner 90
                        tileDirections[x, y, z] = TileDirection.NW;
                        converted[x, y, z] = true;
                    }

                    if (east && northWest && !west && !south) {
                        // corner 0
                        tileDirections[x, y, z] = TileDirection.ENW;
                        converted[x, y, z] = true;
                    }

                    if (north && southEast && !west && !south) {
                        // corner 0
                        tileDirections[x, y, z] = TileDirection.NSE;
                        converted[x, y, z] = true;
                    }
                    #endregion
                }
            }
        }

        return tileDirections;
    }

    private void CleanupOutline(Arr3d<TileDirection> tileDirections) {
        var queuedToRemove = new List<Vector3Int>();
        for (int x = 1; x < tileDirections.Length1 - 1; x++) {
            for (int y = 1; y < tileDirections.Length2 - 1; y++) {
                for (int z = 1; z < tileDirections.Length3 - 1; z++) {
                    var north = tileDirections[x, y, z - 1] != TileDirection.None;
                    var south = tileDirections[x, y, z + 1] != TileDirection.None;
                    var east = tileDirections[x + 1, y, z] != TileDirection.None;
                    var west = tileDirections[x - 1, y, z] != TileDirection.None;
                    if (tileDirections[x, y, z] == TileDirection.None) continue;

                    // Remove if surrounded
                    if (north && south && east && west) {
                        queuedToRemove.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        queuedToRemove.ForEach(coord => tileDirections[coord] = 0);
    }

    private void CarveLShape(Arr3d<bool> arr, Vector3Int dimensions, int widthB, int lengthB) {
        var from = new Vector3Int(0, 0, dimensions.z - lengthB);
        var to = new Vector3Int(dimensions.x - widthB, dimensions.y, dimensions.z);
        arr.Fill(from, to, false);
    }

    private void CarveTShape(Arr3d<bool> arr, Vector3Int dimensions, int extrusion, int inset) {
        var from = new Vector3Int(0, 0, dimensions.z - extrusion + 1);
        var to = new Vector3Int(inset, dimensions.y, dimensions.z);
        arr.Fill(from, to, false);

        // Move over to the other cut-out
        from += Vector3Int.right * (dimensions.x - inset);
        to += Vector3Int.right * (dimensions.x - inset);
        arr.Fill(from, to, false);
    }

    private void SpawnBuilding(Arr3d<TileDirection> tileDirections) {
        gridObjects = new Arr3d<GameObject>(tileDirections.Length);
        for (int x = 0; x < tileDirections.Length1; x++)
        for (int y = 0; y < tileDirections.Length2; y++)
        for (int z = 0; z < tileDirections.Length3; z++) {
            if (tileDirections[x, y, z] == 0) continue;
            var objectToSpawn = directionToType[tileDirections[x, y, z]] == TileType.Straight ? StraightPrefab : CurvePrefab;
            var obj = Instantiate(objectToSpawn, new Vector3(x, y, z), Quaternion.Euler(directionToRotation[tileDirections[x, y, z]]), transform);
            gridObjects[x, y, z] = obj;
        }
    }

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

    private void SpawnRoof(List<Rectangle> rectangles, int y) {
        roofObjects = new List<GameObject>();
        foreach (var rectangle in rectangles) {
            for (var i = rectangle.Min.x; i < rectangle.Max.x; i++) {
                for (var j = rectangle.Min.y; j < rectangle.Max.y; j++) {
                    var roof = Instantiate(RoofPrefab, new Vector3(i, y, j), Quaternion.identity, transform);
                    roof.InitializeAndBuild(1, 1, 1, .15f, Vector3.zero);
                    roofObjects.Add(roof.gameObject);
                }
            }
        }
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