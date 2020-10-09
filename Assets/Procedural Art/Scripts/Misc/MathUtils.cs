using UnityEngine;

using V2i = UnityEngine.Vector2Int;
using V3i = UnityEngine.Vector3Int;
using V3 = UnityEngine.Vector3;

public static class MathUtils {
    public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

    public static V2i Clamp(this V2i value, V2i min, V2i max) {
        return new V2i(Clamp(value.x, min.x, max.x), Clamp(value.y, min.y, max.y));
    }
    public static V3i Clamp(this V3i value, V3i min, V3i max) {
        return new V3i(Clamp(value.x, min.x, max.x), Clamp(value.y, min.y, max.y), Clamp(value.z, min.z, max.z));
    }

    public static V3 Abs(this V3 value) {
        return new V3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}