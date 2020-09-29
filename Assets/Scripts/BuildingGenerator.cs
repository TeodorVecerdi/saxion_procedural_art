using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BuildingGenerator : MonoBehaviour {
    public RandomSettings GeneratorSettings;
    public FeatureSettings Features;

    private MeshFilter meshFilter;
    private Mesh mesh;
    
    private WeightedRandom buildingTypeSelector;
    private List<Rectangle> rects;

    private int buildingHeight;
    private Vector2Int dimensionsA;
    private Vector2Int dimensionsB;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) { 
            Generate();
        }
    }

    private void OnDrawGizmos() {
        /*Gizmos.matrix = transform.localToWorldMatrix;
        if(boolArr == null) return;
        var path = MarchingSquares.March(boolArr.CrossSection(0));
        Debug.Log(path.Count);
        var colorStep = 1.0f / path.Count;
        var current = Vector2Int.zero;
        for (var i = 0; i < path.Count; i++) {
            Gizmos.color = Color.HSVToRGB(colorStep * i, 1, 1);
            var next = current + path[i];
            var from = new Vector3(current.x-0.5f, 0, current.y-0.5f);
            var to = new Vector3(next.x-0.5f, 0, next.y-0.5f);
            Gizmos.DrawLine(from, to);
            current = next;
        }
        Gizmos.color = Color.HSVToRGB(colorStep * (path.Count-1), 1, 1);
        var fromLast = new Vector3(current.x-0.5f, 0, current.y-0.5f);
        var toLast = new Vector3(0-0.5f, 0, 0-0.5f);
        Gizmos.DrawLine(fromLast, toLast);*/

        /*if (rects != null && gridObjects != null) {
            foreach (var rectangle in rects) {
                var avgV2 = rectangle.Min + rectangle.Max;
                var sizeV2 = rectangle.Max - rectangle.Min;
                var avg = new Vector3(avgV2.x / 2f - 0.5f, gridObjects.Length2, avgV2.y / 2f - 0.5f);
                var size = new Vector3(sizeV2.x - 0.2f, .1f, sizeV2.y - 0.2f);
                Gizmos.DrawCube(avg, size);
            }
        }*/
    }

    private void Generate() {
        if (GeneratorSettings == null) {
            throw new Exception("Generator Settings cannot be null! Make sure to assign a RandomSettings object to the class before calling BuildingGenerator::Generate");
        }

        mesh.Clear();
        Setup();
        var buildingType = buildingTypeSelector.Value();
        var boolArr = buildingType == 0 ? GenSquare() : buildingType == 1 ? GenL() : GenT();
        var roofs = buildingType == 0 ? GenSquareRoof() : buildingType == 1 ? GenLRoof() : GenTRoof();
        var path = MarchingSquares.March(boolArr);
        CleanupOutline(boolArr);
        var walls = GenWalls(path);
        
        var (vertices, triangles) = ListUtils.Combine(roofs, walls);
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
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
        widthB = MathUtils.Clamp(widthB, 2, widthA - 1);
        var lengthB = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxLengthB);
        lengthB = MathUtils.Clamp(lengthB, 1, Mathf.CeilToInt(lengthA / 2f));
        buildingHeight = RandUtils.RandomBetween(GeneratorSettings.LBuildingSettings.MinMaxHeight);

        dimensionsA = new Vector2Int(widthA, lengthA);
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

    private (List<Vector3> vertices, List<int> triangles) GenSquareRoof() { return (new List<Vector3>(), new List<int>());}

    private (List<Vector3> vertices, List<int> triangles) GenLRoof() {
        var height = 1f;
        var height2 = 1.5f;
        var thickness = 0.15f;

        var roofA = StraightRoofGenerator.Generate(dimensionsA.x - dimensionsB.x / 2f, height, (dimensionsA.y - dimensionsB.y) / 2f, thickness, new Vector3(-0.5f, buildingHeight, -0.5f), Quaternion.identity, Vector3.zero);
        var roofA1 = StraightRoofGenerator.Generate(dimensionsA.x - dimensionsB.x / 2f, height, (dimensionsA.y - dimensionsB.y) / 2f, thickness, new Vector3(-0.5f, buildingHeight, dimensionsA.y - dimensionsB.y - 0.5f), Quaternion.Euler(0, 180, 0), Vector3.zero, flip: true);

        var roofB = StraightRoofGenerator.Generate(dimensionsB.y + (dimensionsA.y - dimensionsB.y) / 2f, height2, dimensionsB.x / 2f, thickness, new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 90, 0), Vector3.zero);
        var roofB1 = StraightRoofGenerator.Generate(dimensionsB.y + (dimensionsA.y - dimensionsB.y) / 2f, height2, dimensionsB.x / 2f, thickness, new Vector3(dimensionsA.x - 0.5f, buildingHeight, dimensionsA.y - 0.5f), Quaternion.Euler(0, 270, 0), Vector3.zero, flip: true);

        var cornerA = CornerRoofGenerator.Generate((dimensionsA.y - dimensionsB.y) / 2f, height2, dimensionsB.x / 2f, thickness, new Vector3(dimensionsA.x - 0.5f, buildingHeight, -0.5f), Quaternion.Euler(0, -90, 0), Vector3.zero);
        var cornerB = CornerRoofGenerator.Generate((dimensionsA.y - dimensionsB.y) / 2f, height2, dimensionsB.x / 2f, thickness, new Vector3(dimensionsA.x - dimensionsB.x - 0.5f, buildingHeight, -0.5f), Quaternion.Euler(0, -90, 0), Vector3.zero, flipZ: true);

        return ListUtils.Combine(roofA, roofA1, roofB, roofB1, cornerA, cornerB);
    }

    private (List<Vector3> vertices, List<int> triangles) GenTRoof() { return (new List<Vector3>(), new List<int>());}

    private (List<Vector3> vertices, List<int> triangles) GenWalls(List<Vector2Int> path) {
        var thickness = 0.1f;
        var walls = new List<(List<Vector3>, List<int>)>();
        var current = Vector2Int.zero;
        foreach (var point in path) {
            var next = current + point;
            var from = new Vector3(current.x-0.5f, 0, current.y-0.5f);
            var to = new Vector3(next.x-0.5f, 0, next.y-0.5f);
            var diff = to - from;
            var angle = Vector3.SignedAngle(Vector3.right, diff, Vector3.up);
            var wall = WallGenerator.Generate(diff.magnitude, buildingHeight, thickness, from, Quaternion.Euler(0, angle, 0), true);
            walls.Add(wall);
            current = next;
        }

        return ListUtils.Combine(walls.ToArray());
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
        var from = new Vector2Int(0, dimensionsA.y - dimensionsB.y);
        var to = new Vector2Int(dimensionsA.x - dimensionsB.x, dimensionsA.y);
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