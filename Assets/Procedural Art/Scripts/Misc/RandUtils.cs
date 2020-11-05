
using UnityEngine;

public static class RandUtils {
    public static bool BoolWeighted(float weight) {
        return Rand.Value < weight;
    }

    public static float RandomBetween(Vector2 range) {
        return Rand.Range(range.x, range.y);
    }
    public static int RandomBetween(Vector2Int range) {
        return Rand.RangeInclusive(range.x, range.y);
    }

    public static Vector2Int RandomBetween(Vector2Int a, Vector2Int b) {
        return new Vector2Int(Rand.RangeInclusive(a.x, b.x), Rand.RangeInclusive(a.y, b.y));
    }
    
    public static Vector3Int RandomBetween(Vector3Int a, Vector3Int b) {
        return new Vector3Int(Rand.RangeInclusive(a.x, b.x), Rand.RangeInclusive(a.y, b.y), Rand.RangeInclusive(a.z, b.z));
    }
}