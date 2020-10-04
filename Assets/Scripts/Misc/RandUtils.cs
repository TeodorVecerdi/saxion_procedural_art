using UnityEngine;

public static class RandUtils {
    public static bool Bool => Random.Range(0, 2) == 0;

    public static bool BoolWeighted(float weight) {
        return Random.Range(0f, 1f) < weight;
    }

    public static int RangeInclusive(int min, int max) {
        return Random.Range(min, max + 1);
    }

    public static float Range(float min, float max) {
        return Random.Range(min, max);
    }

    public static int RandomBetween(Vector2Int range) {
        return RangeInclusive(range.x, range.y);
    }

    public static Vector2Int RandomBetween(Vector2Int a, Vector2Int b) {
        return new Vector2Int(RangeInclusive(a.x, b.x), RangeInclusive(a.y, b.y));
    }
    
    public static Vector3Int RandomBetween(Vector3Int a, Vector3Int b) {
        return new Vector3Int(RangeInclusive(a.x, b.x), RangeInclusive(a.y, b.y), RangeInclusive(a.z, b.z));
    }
}