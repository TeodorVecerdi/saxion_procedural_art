using System;
using UnityEngine;
using V2i = UnityEngine.Vector2Int;
using V3i = UnityEngine.Vector3Int;
using V3 = UnityEngine.Vector3;
using V2 = UnityEngine.Vector2;

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

    public static V3 ClosestGridPoint(this V3 vector, bool useSubGrid) {
        var size = useSubGrid ? GlobalSettings.Instance.GridSizeMinor : GlobalSettings.Instance.GridSize;
        var minX = Mathf.Floor(vector.x / size) * size;
        var maxX = Mathf.Ceil(vector.x / size) * size;
        var minZ = Mathf.Floor(vector.z / size) * size;
        var maxZ = Mathf.Ceil(vector.z / size) * size;
        vector.x = Mathf.Abs(vector.x - minX) < Mathf.Abs(vector.x - maxX) ? minX : maxX;
        vector.z = Mathf.Abs(vector.z - minZ) < Mathf.Abs(vector.z - maxZ) ? minZ : maxZ;
        return vector;
    }

    public static V2 ClosestGridPoint(this V2 vector, bool useSubGrid) {
        var v3 = new V3(vector.x, 0, vector.y);
        var closest = v3.ClosestGridPoint(useSubGrid);
        var v2 = new V2(closest.x, closest.z);
        return v2;
    }

    public static bool OnlyTheseFlags(this Enum bitset, Enum bitmask) {
        var bitsetInt = Convert.ToInt32(bitset);
        var bitmaskInt = Convert.ToInt32(bitmask);
        
        var hasBits = (bitsetInt & bitmaskInt) != 0;
        var noOtherBits = (bitsetInt & ~bitmaskInt) == 0;
        
        return hasBits && noOtherBits;
    }
}