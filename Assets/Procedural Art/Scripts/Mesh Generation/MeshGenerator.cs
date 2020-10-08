using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeshGenerator {
    protected MeshData meshData;
    protected Vector3 position;
    protected Quaternion rotation;
    protected Dictionary<string, dynamic> defaultParameters;

    protected abstract void SetDefaultSettings();
    protected abstract void DeconstructSettings(Dictionary<string, dynamic> parameters);
    protected virtual void ApplyCustomSettings() { }
    protected abstract void Generate();

    public static MeshData GetMesh<T>(Vector3 position, Quaternion rotation, Dictionary<string, dynamic> parameters) where T : MeshGenerator, new() {
        var generator = new T();
        return generator.GetMesh(position, rotation, parameters);
    }

    private MeshData GetMesh(Vector3 position, Quaternion rotation, Dictionary<string, dynamic> parameters) {
        this.position = position;
        this.rotation = rotation;
        meshData = new MeshData();
        SetDefaultSettings();
        DeconstructSettings(parameters);
        ApplyCustomSettings();
        Generate();
        ApplyTransformation();
        return meshData;
    }

    private void ApplyTransformation() {
        for (var i = 0; i < meshData.Vertices.Count; i++) {
            meshData.Vertices[i] = rotation * meshData.Vertices[i] + position * GlobalSettings.Instance.GridSize;
        }
    }

    protected void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int submeshIndex, bool flip = false, UVSettings uvSettings = Default) {
        var quadIndex = meshData.Vertices.Count;
        if (flip) {
            meshData.Vertices.Add(v3);
            meshData.Vertices.Add(v2);
            meshData.Vertices.Add(v1);
            meshData.Vertices.Add(v0);
        } else {
            meshData.Vertices.Add(v0);
            meshData.Vertices.Add(v1);
            meshData.Vertices.Add(v2);
            meshData.Vertices.Add(v3);
        }

        // var verticalSize = (v1 - v0).magnitude;
        // var horizontalSize = (v3 - v0).magnitude;
        // var uvs = new List<Vector2> {Vector2.zero, Vector2.up * verticalSize, Vector2.up * verticalSize + Vector2.right * horizontalSize, Vector2.right * horizontalSize};
        var uvs = UVUtils.QuadUVS(v0, v1, v2, v3, uvSettings);
        meshData.UVs.AddRange(uvs);

        if (!meshData.Triangles.ContainsKey(submeshIndex)) meshData.Triangles[submeshIndex] = new List<int>();
        meshData.Triangles[submeshIndex].Add(quadIndex);
        meshData.Triangles[submeshIndex].Add(quadIndex + 1);
        meshData.Triangles[submeshIndex].Add(quadIndex + 2);
        meshData.Triangles[submeshIndex].Add(quadIndex);
        meshData.Triangles[submeshIndex].Add(quadIndex + 2);
        meshData.Triangles[submeshIndex].Add(quadIndex + 3);
    }

    protected void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, int submeshIndex, bool flip = false, UVSettings uvSettings = Default) {
        var triangleIndex = meshData.Vertices.Count;
        if (flip) {
            meshData.Vertices.Add(v2);
            meshData.Vertices.Add(v1);
            meshData.Vertices.Add(v0);
        } else {
            meshData.Vertices.Add(v0);
            meshData.Vertices.Add(v1);
            meshData.Vertices.Add(v2);
        }

        
        // TODO: FIX THIS. MAKE SAME AS QUAD
        var verticalSize = (v1 - v0).magnitude;
        var horizontalSize = (v2 - v0).magnitude;
        meshData.UVs.AddRange(new List<Vector2> {Vector2.zero + new Vector2(position.z, 0), Vector2.up * verticalSize + new Vector2(position.z, 0), Vector2.right * horizontalSize + new Vector2(position.z, 0)});
        if (!meshData.Triangles.ContainsKey(submeshIndex)) meshData.Triangles[submeshIndex] = new List<int>();
        meshData.Triangles[submeshIndex].Add(triangleIndex);
        meshData.Triangles[submeshIndex].Add(triangleIndex + 1);
        meshData.Triangles[submeshIndex].Add(triangleIndex + 2);
    }

    [Flags]
    public enum UVSettings {
        None = 0,
        Rotate90 = 1,
        FlipHorizontal = 2,
        FlipVertical = 4,
        TriangleMiddle = 8
    }

    public const UVSettings Default = UVSettings.None;
}