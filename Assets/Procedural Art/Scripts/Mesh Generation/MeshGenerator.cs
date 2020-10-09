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
    protected virtual void ApplyCustomSettings(){}
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
    
    protected void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int submesh, bool flip = false, UVSettings uvSettings = Default) {
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

        var uvs = UVUtils.QuadUVS(v0, v1, v2, v3, uvSettings);
        meshData.UVs.AddRange(uvs);

        if(!meshData.Triangles.ContainsKey(submesh)) meshData.Triangles[submesh] = new List<int>();
        meshData.Triangles[submesh].Add(quadIndex);
        meshData.Triangles[submesh].Add(quadIndex + 1);
        meshData.Triangles[submesh].Add(quadIndex + 2);
        meshData.Triangles[submesh].Add(quadIndex);
        meshData.Triangles[submesh].Add(quadIndex + 2);
        meshData.Triangles[submesh].Add(quadIndex + 3);
    }

    protected void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, int submesh, bool flip = false, UVSettings uvSettings = Default) {
        var triangleIndex = meshData.Vertices.Count;
        
        if (flip) {
            meshData.Vertices.Add(v2);
            meshData.Vertices.Add(v1);
            meshData.Vertices.Add(v0);
            // var uvs = UVUtils.TriangleUVS2(v2, v1, v0, uvSettings, flip);
            // meshData.UVs.AddRange(uvs);
        } else {
            meshData.Vertices.Add(v0);
            meshData.Vertices.Add(v1);
            meshData.Vertices.Add(v2);
            
        }

        var uvs = UVUtils.TriangleUVS2(v0, v1, v2, uvSettings, position);
        meshData.UVs.AddRange(uvs);
        if(!meshData.Triangles.ContainsKey(submesh)) meshData.Triangles[submesh] = new List<int>();
        meshData.Triangles[submesh].Add(triangleIndex);
        meshData.Triangles[submesh].Add(triangleIndex + 1);
        meshData.Triangles[submesh].Add(triangleIndex + 2);
    }
    
    [Flags]
    public enum UVSettings {
        None = 0,
        FlipHorizontal = 1,
        FlipVertical = 2,
        Rotate = 4,
        TriangleMiddle = 8,
        FlipTopPart = 16,
        FlipBottomPart = 32
    }

    public const UVSettings Default = UVSettings.None;
}