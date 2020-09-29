using UnityEngine;
// ReSharper disable InconsistentNaming

public static class Direction {
    public static readonly Vector2Int E = Vector2Int.right;
    public static readonly Vector2Int W = Vector2Int.left;
    public static readonly Vector2Int N = Vector2Int.down; 
    public static readonly Vector2Int S = Vector2Int.up; 

    public static readonly Vector2Int NE = N + E;
    public static readonly Vector2Int NW = N + W;
    public static readonly Vector2Int SE = S + E;
    public static readonly Vector2Int SW = S + W;
}