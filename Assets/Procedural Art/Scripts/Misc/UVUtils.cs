using System;
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

        if (uvSettings.HasFlag(MeshGenerator.UVSettings.Rotate)) {
            var t0 = uvs[0];
            for (int i = 0; i < 3; i++) uvs[i] = uvs[i + 1];
            uvs[3] = t0;
        }

        return uvs;
    }

    public static List<Vector2> TriangleUVS(Vector3 v0, Vector3 v1, Vector3 v2, MeshGenerator.UVSettings uvSettings, bool flip) {
        var compVec = new List<Vector2>();
        var v0_1 = v0 - v0;
        var v1_1 = v1 - v0;
        var v2_1 = v2 - v0;
        var triangleNormal = Vector3.Cross(v1_1 - v0_1, v2_1 - v0_1).Abs();

        if (triangleNormal.x >= triangleNormal.y && triangleNormal.x >= triangleNormal.z) {
            v0_1.x = 0;
            v1_1.x = 0;
            v2_1.x = 0;
            compVec.AddRange(new[] {new Vector2(v0_1.y, v0_1.z), new Vector2(v1_1.y, v1_1.z), new Vector2(v2_1.y, v2_1.z)});
        } else if (triangleNormal.y >= triangleNormal.x && triangleNormal.y >= triangleNormal.z) {
            v0_1.y = 0;
            v1_1.y = 0;
            v2_1.y = 0;
            compVec.AddRange(new[] {new Vector2(v0_1.x, v0_1.z), new Vector2(v1_1.x, v1_1.z), new Vector2(v2_1.x, v2_1.z)});
        } else if (triangleNormal.z >= triangleNormal.x && triangleNormal.z >= triangleNormal.y) {
            v0_1.z = 0;
            v1_1.z = 0;
            v2_1.z = 0;
            compVec.AddRange(new[] {new Vector2(v0_1.x, v0_1.y), new Vector2(v1_1.x, v1_1.y), new Vector2(v2_1.x, v2_1.y)});
        }

        var v20 = (v2 - v0).sqrMagnitude;
        var v10 = (v1 - v0).sqrMagnitude;
        var v21 = (v2 - v1).sqrMagnitude;
        var sortedVertices = new List<Vector3>();
        if (Math.Abs(v21 - (v10 + v20)) < 0.01f) {
            Debug.Log("YEET");
            sortedVertices.AddRange(new[] {v0, v1, v2});
        } else if (Math.Abs(v10 - (v21 + v20)) < 0.01f) {
            sortedVertices.AddRange(new[] {v0, v1, v2});
            // sortedVertices.AddRange(new[] {v2, v0, v1});
        } else {
            sortedVertices.AddRange(new[] {v0, v1, v2});
            // sortedVertices.AddRange(new[] {v1, v2, v0});
        }

        if (flip) {
            Debug.Log("Flippity flop");
            sortedVertices.Reverse();
        }

        var uvs = new List<Vector2>();
        float horizontalSize;
        float verticalSize;
        if (IsAboveBelow(sortedVertices[0], sortedVertices[1])) {
            verticalSize = (v1 - v0).magnitude;
            horizontalSize = (v2 - v0).magnitude;
            if (IsRight(sortedVertices[2], sortedVertices[0])) {
                Debug.Log("OPTION A");
                uvs.AddRange(new[] {Vector2.zero, Vector2.up * verticalSize, Vector2.right * horizontalSize});
            } else {
                Debug.Log("OPTION B");
                uvs.AddRange(new[] {Vector2.up * verticalSize + Vector2.right * horizontalSize, Vector2.right * horizontalSize, Vector2.up * verticalSize});
                if(flip) uvs[1] = Vector2.zero;
            }
        } else {
            verticalSize = (v2 - v0).magnitude;
            horizontalSize = (v1 - v0).magnitude;
            if (IsAbove(sortedVertices[2], sortedVertices[0])) {
                Debug.Log("OPTION C");
                uvs.AddRange(new[] {Vector2.right * horizontalSize, Vector2.zero, Vector2.up * verticalSize});
            } else {
                Debug.Log("OPTION D");
                uvs.AddRange(new[] {Vector2.up * verticalSize, Vector2.up * verticalSize + Vector2.right * horizontalSize, Vector2.zero});
            }
        }

        if (uvSettings.HasFlag(MeshGenerator.UVSettings.Rotate))
            RotateTriangle(uvs, verticalSize, horizontalSize);
        if (uvSettings.HasFlag(MeshGenerator.UVSettings.FlipHorizontal))
            FlipTriangleHorizontal(uvs, horizontalSize);

        if (uvSettings.HasFlag(MeshGenerator.UVSettings.FlipVertical))
            FlipTriangleVertical(uvs, verticalSize);

        if (uvSettings.HasFlag(MeshGenerator.UVSettings.TriangleMiddle))
            TriangleMiddle(uvs);

        return uvs;
    }

    public static List<Vector2> TriangleUVS2(Vector3 v0, Vector3 v1, Vector3 v2, MeshGenerator.UVSettings uvSettings, Vector3 position) {
        var sizeHorizontal = (v2 - v0).magnitude;
        var sizeVertical = (v1 - v0).magnitude;
        
        var uvs = new List<Vector2>();
        uvs.AddRange(new []{Vector2.zero,Vector2.up * sizeVertical,Vector2.right * sizeHorizontal });
        
        if (uvSettings.HasFlag(MeshGenerator.UVSettings.FlipHorizontal))
            FlipTriangleHorizontal(uvs, sizeHorizontal);

        if (uvSettings.HasFlag(MeshGenerator.UVSettings.FlipVertical))
            FlipTriangleVertical(uvs, sizeVertical);

        if (uvSettings.HasFlag(MeshGenerator.UVSettings.FlipTopPart)) {
            uvs[1] += Vector2.right * sizeHorizontal;
        }
        if (uvSettings.HasFlag(MeshGenerator.UVSettings.FlipBottomPart)) {
            uvs[0] += Vector2.right * sizeHorizontal;
            uvs[2] -= Vector2.right * sizeHorizontal;
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

    private static void FlipTriangleHorizontal(List<Vector2> uvs, float horizontalSize) {
        for (int i = 0; i < 3; i++) {
            var uv = uvs[i];
            if (Math.Abs(uv.x - 0) < 0.01f) uv.x = horizontalSize;
            else uv.x = 0;
            uvs[i] = uv;
        }
    }

    private static void FlipTriangleVertical(List<Vector2> uvs, float verticalSize) {
        for (int i = 0; i < 3; i++) {
            var uv = uvs[i];
            if (Math.Abs(uv.y - 0) < 0.01f) uv.y = verticalSize;
            else uv.y = 0;
            uvs[i] = uv;
        }
    }

    private static void RotateTriangle(List<Vector2> uvs, float verticalSize, float horizontalSize) {
        var offsets = new List<Vector2>();
        if (uvs[0] == Vector2.zero)
            offsets.AddRange(new[] {Vector2.up * verticalSize, Vector2.right * horizontalSize, Vector2.left * horizontalSize});
        else if (uvs[0] == Vector2.up * verticalSize)
            offsets.AddRange(new[] {Vector2.right * horizontalSize, Vector2.down * verticalSize, Vector2.up * verticalSize});
        else if (uvs[0] == Vector2.right * horizontalSize)
            offsets.AddRange(new[] {Vector2.left * horizontalSize, Vector2.up * verticalSize, Vector2.down * verticalSize});
        else
            offsets.AddRange(new[] {Vector2.down * verticalSize, Vector2.left * horizontalSize, Vector2.right * horizontalSize});
        uvs[0] += offsets[0];
        uvs[1] += offsets[1];
        uvs[2] += offsets[2];
    }

    private static void TriangleMiddle(List<Vector2> uvs) {
        if (IsAboveBelow(uvs[0], uvs[1])) uvs[1] = new Vector2((uvs[0].x + uvs[2].x) / 2f, uvs[1].y);
        else uvs[1] = new Vector2(uvs[1].x, (uvs[0].y + uvs[2].y) / 2f);
    }
}