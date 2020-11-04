using System;
using UnityEngine;
using UnityEngine.Rendering;
using V2i = UnityEngine.Vector2Int;
using V3i = UnityEngine.Vector3Int;
using V3 = UnityEngine.Vector3;
using V2 = UnityEngine.Vector2;

public static class MathUtils {
    public static float Map(this float value, float a1, float a2, float b1, float b2) => b1 + (value - a1) * (b2 - b1) / (a2 - a1);
    public static V2 Map(this V2 value, float a1, float a2, float b1, float b2) => new V2(Map(value.x, a1, a2, b1, b2), Map(value.y, a1, a2, b1, b2));
    public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

    public static V2i ToV2I(this V2 value, bool round = false, bool floor = false) =>
        new V2i(round ? Mathf.RoundToInt(value.x) : floor ? Mathf.FloorToInt(value.x) : Mathf.CeilToInt(value.x),
            round ? Mathf.RoundToInt(value.y) : floor ? Mathf.FloorToInt(value.y) : Mathf.CeilToInt(value.y));

    public static V2i Clamp(this V2i value, V2i min, V2i max) {
        return new V2i(Clamp(value.x, min.x, max.x), Clamp(value.y, min.y, max.y));
    }

    public static V3i Clamp(this V3i value, V3i min, V3i max) {
        return new V3i(Clamp(value.x, min.x, max.x), Clamp(value.y, min.y, max.y), Clamp(value.z, min.z, max.z));
    }

    public static V3 Abs(this V3 value) {
        return new V3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }

    public static float ClosestGridPoint(this float f, bool useSubGrid) {
        var size = useSubGrid ? GlobalSettings.Instance.GridSizeMinor : GlobalSettings.Instance.GridSize;
        var min = Mathf.Floor(f / size) * size;
        var max = Mathf.Ceil(f / size) * size;
        return Mathf.Abs(f - min) < Mathf.Abs(f - max) ? min : max;
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

    public static V3 ToVec3(this V2 vector, float extraValue = 0) {
        return new V3(vector.x, extraValue, vector.y);
    }

    public static V2 ToVec2(this V3 vector) {
        return new V2(vector.x, vector.z);
    }

    public static bool OnlyTheseFlags(this Enum bitset, Enum bitmask) {
        var bitsetInt = Convert.ToInt32(bitset);
        var bitmaskInt = Convert.ToInt32(bitmask);

        var hasBits = (bitsetInt & bitmaskInt) != 0;
        var noOtherBits = (bitsetInt & ~bitmaskInt) == 0;

        return hasBits && noOtherBits;
    }

    public static (Vector3 leftDown, Vector3 rightDown, Vector3 rightUp, Vector3 leftUp) RotatedRectangle(Plot plot) {
        var rotation = Quaternion.Euler(0, plot.Rotation, 0);
        var leftDown = new Vector3(plot.Bounds.min.x, 0, plot.Bounds.min.y);
        var rightDown = new Vector3(plot.Bounds.min.x + plot.Bounds.width, 0, plot.Bounds.min.y);
        var rightUp = new Vector3(plot.Bounds.min.x + plot.Bounds.width, 0, plot.Bounds.min.y + plot.Bounds.height);
        var leftUp = new Vector3(plot.Bounds.min.x, 0, plot.Bounds.min.y + plot.Bounds.height);
        leftDown = leftDown - plot.Bounds.min.ToVec3() - plot.Bounds.size.ToVec3() / 2f;
        rightDown = rightDown - plot.Bounds.min.ToVec3() - plot.Bounds.size.ToVec3() / 2f;
        rightUp = rightUp - plot.Bounds.min.ToVec3() - plot.Bounds.size.ToVec3() / 2f;
        leftUp = leftUp - plot.Bounds.min.ToVec3() - plot.Bounds.size.ToVec3() / 2f;
        leftDown = rotation * leftDown;
        rightDown = rotation * rightDown;
        rightUp = rotation * rightUp;
        leftUp = rotation * leftUp;
        leftDown = leftDown + plot.Bounds.min.ToVec3() + plot.Bounds.size.ToVec3() / 2f;
        rightDown = rightDown + plot.Bounds.min.ToVec3() + plot.Bounds.size.ToVec3() / 2f;
        rightUp = rightUp + plot.Bounds.min.ToVec3() + plot.Bounds.size.ToVec3() / 2f;
        leftUp = leftUp + plot.Bounds.min.ToVec3() + plot.Bounds.size.ToVec3() / 2f;
        return (leftDown, rightDown, rightUp, leftUp);
    }

    public static bool RectangleOverlap(Rect a, Plot b) {
        var pointsB = RotatedRectangle(b);
        return a.Contains(pointsB.leftDown) || a.Contains(pointsB.leftUp) || a.Contains(pointsB.rightDown) || a.Contains(pointsB.rightUp);
    }

    public static bool RectangleContains(Rect a, Plot b) {
        var pointsB = RotatedRectangle(b);
        return a.Contains(pointsB.leftDown.ToVec2()) && a.Contains(pointsB.leftUp.ToVec2()) && a.Contains(pointsB.rightDown.ToVec2()) && a.Contains(pointsB.rightUp.ToVec2());
    }

    public static bool PointInRotatedRectangle(Vector2 point, Plot plot, float? rotationAngle = null) {
        var rot = -plot.Rotation;
        if (rotationAngle != null) rot = -rotationAngle.Value;
        var rotation = Quaternion.Euler(0, rot, 0);
        var localPoint = (point - plot.Bounds.center).ToVec3();
        var rotatedPoint = (rotation * localPoint).ToVec2() + plot.Bounds.center;
        return plot.Bounds.Contains(rotatedPoint);
    }
}