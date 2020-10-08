using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public static class UVUtils {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static List<Vector2> QuadUVS(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, MeshGenerator.UVSettings uvSettings) {
        var compVec = new List<Vector2>();
        var v0_1 = v0 - v0;
        var v1_1 = v1 - v0;
        var v2_1 = v2 - v0;
        var v3_1 = v3 - v0;
        var triangleNormal = Vector3.Cross(v1_1 - v0_1, v3_1 - v0_1).Abs();

        if (triangleNormal.x >= triangleNormal.y && triangleNormal.x >= triangleNormal.z) {
            v0_1.x = 0;
            v1_1.x = 0;
            v2_1.x = 0;
            v3_1.x = 0;
            compVec.AddRange(new[] {new Vector2(v0_1.y, v0_1.z), new Vector2(v1_1.y, v1_1.z), new Vector2(v2_1.y, v2_1.z), new Vector2(v3_1.y, v3_1.z)});
        } else if (triangleNormal.y >= triangleNormal.x && triangleNormal.y >= triangleNormal.z) {
            v0_1.y = 0;
            v1_1.y = 0;
            v2_1.y = 0;
            v3_1.y = 0;
            compVec.AddRange(new[] {new Vector2(v0_1.x, v0_1.z), new Vector2(v1_1.x, v1_1.z), new Vector2(v2_1.x, v2_1.z), new Vector2(v3_1.x, v3_1.z)});
        } else if (triangleNormal.z >= triangleNormal.x && triangleNormal.z >= triangleNormal.y) {
            v0_1.z = 0;
            v1_1.z = 0;
            v2_1.z = 0;
            v3_1.z = 0;
            compVec.AddRange(new[] {new Vector2(v0_1.x, v0_1.y), new Vector2(v1_1.x, v1_1.y), new Vector2(v2_1.x, v2_1.y), new Vector2(v3_1.x, v3_1.y)});
        }

        if (compVec.Count == 0)
            Debug.Log($"CompVec ended up empty. Triangle normal: {triangleNormal}");
        /* STEP 2: Do calculations to sort vertices properly:
         * This orientation   1 -- 2
         *                    |    |
         *                    0 -- 3
        */

        var uvs = new List<Vector2>();
        if (IsAboveBelow(compVec[0], compVec[1])) {
            var verticalSize = (v1 - v0).magnitude;
            var horizontalSize = (v3 - v0).magnitude;
            if (IsRight(compVec[2], compVec[1])) {
                uvs.AddRange(new[] {Vector2.zero, Vector2.up * verticalSize, Vector2.up * verticalSize + Vector2.right * horizontalSize, Vector2.right * horizontalSize});
            } else {
                uvs.AddRange(new[] {Vector2.up * verticalSize + Vector2.right * horizontalSize, Vector2.right * horizontalSize, Vector2.zero, Vector2.up * verticalSize});
            }
        } else {
            var verticalSize = (v3 - v0).magnitude;
            var horizontalSize = (v1 - v0).magnitude;
            if (IsAbove(compVec[1], compVec[2])) {
                uvs.AddRange(new[] {Vector2.right * horizontalSize, Vector2.zero, Vector2.up * verticalSize, Vector2.up * verticalSize + Vector2.right * horizontalSize});
            } else {
                uvs.AddRange(new[] {Vector2.up * verticalSize, Vector2.up * verticalSize + Vector2.right * horizontalSize, Vector2.right * horizontalSize, Vector2.zero});
            }
        }

        if (uvSettings.HasFlag(MeshGenerator.UVSettings.FlipHorizontal)) {
            Swap(1, 2, uvs);
            Swap(0, 3, uvs);
        }

        if (uvSettings.HasFlag(MeshGenerator.UVSettings.FlipVertical)) {
            Swap(0, 1, uvs);
            Swap(2, 3, uvs);
        }

        if (uvSettings.HasFlag(MeshGenerator.UVSettings.Rotate90)) {
            var t0 = uvs[0];
            for (int i = 0; i < 3; i++) uvs[i] = uvs[i + 1];
            uvs[3] = t0;
        }

        return uvs;
    }

    private static bool IsAboveBelow(Vector2 a, Vector2 b) {
        return Mathf.Abs(a.x - b.x) < Mathf.Abs(a.y - b.y);
    }

    private static bool IsRightLeft(Vector2 a, Vector2 b) {
        return !IsAboveBelow(a, b);
    }

    private static bool IsAbove(Vector2 a, Vector2 b) {
        return IsAboveBelow(a, b) && a.y > b.y;
    }

    private static bool IsRight(Vector2 a, Vector2 b) {
        return IsRightLeft(a, b) && a.x > b.x;
    }

    private static void Swap<T>(int a, int b, List<T> list) {
        var tA = list[a];
        list[a] = list[b];
        list[b] = tA;
    }
}