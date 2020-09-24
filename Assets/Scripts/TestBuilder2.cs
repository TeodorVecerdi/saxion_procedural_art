using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TestBuilder2 : MonoBehaviour {
    public GameObject BlockTemplate;
    public GameObject StraightPrefab;
    public GameObject CurvePrefab;
    public int MinWidth = 1;
    public int MaxWidth = 4;
    public int MinHeight = 1;
    public int MaxHeight = 4;

    private GameObject[][] blocksGridObjects;
    public int width;
    public int height;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
            Execute();
    }

    private void Execute() {
        Clear();
        var blocks = GenerateOutline();
        var tileDirections = GetDirections(blocks);
        CleanupOutline(tileDirections);
        BuildFloor(tileDirections);
    }

    private void Clear() {
        if(blocksGridObjects == null) return;
        for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
            Destroy(blocksGridObjects[i][j]);
        blocksGridObjects = null;
    }

    private bool[][] GenerateOutline() {
        width = Random.Range(MinWidth, MaxWidth);
        height = Random.Range(MinHeight, MaxHeight);

        var outline = new bool[width + 2][];
        for (var i = 0; i < width + 2; i++) {
            outline[i] = new bool[height + 2];
        }

        var choice = Random.Range(0, 3);
        if (choice == 0) {
            // square
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    outline[x + 1][y + 1] = true;
                }
            }
        } else if (choice == 1) {
            // L-shape
            var lWidthA = width;
            var lHeightA = Mathf.CeilToInt(height / 2f);
            var lWidthB = Mathf.CeilToInt(width / 2f);
            var lHeightB = Mathf.FloorToInt(height / 2f);

            // Block 1
            for (var x = 0; x < lWidthA; x++) {
                for (var y = 0; y < lHeightA; y++) {
                    outline[x + 1][y + 1] = true;
                }
            }

            // Block 2
            for (var x = 0; x < lWidthB; x++) {
                for (var y = 0; y < lHeightB; y++) {
                    outline[width - lWidthB + x + 1][y + lHeightA + 1] = true;
                }
            }
        } else {
            // T-shape
            var tDiff = Random.Range(0, width / 4) + 1;
            var tHeightA = height - 1;
            
            Debug.Log($"T building:\n{width} | {tDiff} | {height}");
            
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < tHeightA; y++) {
                    outline[x + 1][y + 1] = true;
                }
            }

            for (var x = tDiff; x < width - tDiff; x++) {
                for (var y = tHeightA; y < height; y++) {
                    outline[x + 1][y + 1] = true;
                }
            }
        }

        return outline;
    }

    private TileDirection[][] GetDirections(bool[][] outline) {
        // Straight wall conversion
        var tileDirections = new TileDirection[width][];
        var converted = new bool[width][];
        for (int index = 0; index < width; index++) {
            converted[index] = new bool[height];
            tileDirections[index] = new TileDirection[height];
        }

        for (int x = 1; x < width + 1; x++) {
            for (int y = 1; y < height + 1; y++) {
                // var northVec = current + Vector2Int.down;
                // var southVec = current + Vector2Int.up;
                // var westVec = current + Vector2Int.left;
                // var eastVec = current + Vector2Int.right;
                // var northEastVec = current + Vector2Int.down + Vector2Int.right;
                // var northWestVec = current + Vector2Int.down + Vector2Int.left;
                // var southEastVec = current + Vector2Int.up + Vector2Int.right;
                // var southWestVec = current + Vector2Int.up + Vector2Int.left;

                var north = outline[x][y - 1];
                var south = outline[x][y + 1];
                var west = outline[x - 1][y];
                var east = outline[x + 1][y];
                var northEast = outline[x + 1][y - 1];
                var northWest = outline[x - 1][y - 1];
                var southEast = outline[x + 1][y + 1];
                var southWest = outline[x - 1][y + 1];
                if (!outline[x][y] || converted[x - 1][y - 1]) continue;

                #region Arount 400 billion if checks
                // Straight walls
                if (west && east) {
                    if (!north) {
                        // straight north
                        tileDirections[x - 1][y - 1] = TileDirection.N;
                        converted[x - 1][y - 1] = true;
                    } else if (!south) {
                        // straight south
                        tileDirections[x - 1][y - 1] = TileDirection.S;
                        converted[x - 1][y - 1] = true;
                    }
                }

                if (north && south) {
                    if (!west) {
                        // straight west
                        tileDirections[x - 1][y - 1] = TileDirection.W;
                        converted[x - 1][y - 1] = true;
                    } else if (!east) {
                        // straight east
                        tileDirections[x - 1][y - 1] = TileDirection.E;
                        converted[x - 1][y - 1] = true;
                    }
                }

                // Curved walls
                if (east && south && !west && !north) {
                    // corner -90
                    tileDirections[x - 1][y - 1] = TileDirection.SE;
                    converted[x - 1][y - 1] = true;
                }

                if (east && north && !west && !south) {
                    // corner 0
                    tileDirections[x - 1][y - 1] = TileDirection.NE;
                    converted[x - 1][y - 1] = true;
                }

                if (west && south && !east && !north) {
                    // corner 180
                    tileDirections[x - 1][y - 1] = TileDirection.SW;
                    converted[x - 1][y - 1] = true;
                }

                if (west && north && !east && !south) {
                    // corner 90
                    tileDirections[x - 1][y - 1] = TileDirection.NW;
                    converted[x - 1][y - 1] = true;
                }

                if (east && northWest && !west && !south) {
                    // corner 0
                    tileDirections[x - 1][y - 1] = TileDirection.ENW;
                    converted[x - 1][y - 1] = true;
                }

                if (north && southEast && !west && !south) {
                    // corner 0
                    tileDirections[x - 1][y - 1] = TileDirection.NSE;
                    converted[x - 1][y - 1] = true;
                }
                #endregion
            }
        }

        return tileDirections;
    }

    private void CleanupOutline(TileDirection[][] outline) {
        var queuedToRemove = new List<(int, int)>();
        for (int x = 1; x < width-1; x++) {
            for (int y = 1; y < height-1; y++) {
                var north = outline[x][y - 1] != TileDirection.None;
                var south = outline[x][y + 1] != TileDirection.None;
                var east = outline[x + 1][y] != TileDirection.None;
                var west = outline[x - 1][y] != TileDirection.None;
                if (outline[x][y] == TileDirection.None) continue;

                // Remove if surrounded
                if (north && south && east && west) {
                    queuedToRemove.Add((x, y));
                }
            }
        }

        queuedToRemove.ForEach(pair => outline[pair.Item1][pair.Item2] = TileDirection.None);
    }

    public void BuildFloor(TileDirection[][] outline) {
        blocksGridObjects = new GameObject[width][];
        for (var x = 0; x < width; x++) {
            blocksGridObjects[x] = new GameObject[height];
            for (var y = 0; y < height; y++) {
                if (outline[x][y] == TileDirection.None) continue;

                var objectToSpawn = DirectionToType[outline[x][y]] == TileType.Straight ? StraightPrefab : CurvePrefab;
                var obj = Instantiate(objectToSpawn, new Vector3(x, 0, y), Quaternion.Euler(DirectionToRotation[outline[x][y]]), transform);
                blocksGridObjects[x][y] = obj;
            }
        }
    }

    public readonly Dictionary<TileDirection, Vector3> DirectionToRotation = new Dictionary<TileDirection, Vector3> {
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

    public readonly Dictionary<TileDirection, TileType> DirectionToType = new Dictionary<TileDirection, TileType> {
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
}

public enum TileType {
    None,
    Straight,
    Curve,
}

public enum TileDirection {
    None,
    N,
    S,
    E,
    W,
    SE,
    NE,
    SW,
    NW,
    ENW,
    NSE
}