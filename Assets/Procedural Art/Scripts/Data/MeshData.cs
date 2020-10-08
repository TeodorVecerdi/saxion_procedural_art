using System.Collections.Generic;
using UnityEngine;

public class MeshData {
    public readonly List<Vector3> Vertices;
    public readonly List<Vector2> UVs;
    public readonly Dictionary<int, List<int>> Triangles;
    
    public MeshData() {
        Vertices = new List<Vector3>();
        UVs = new List<Vector2>();
        Triangles = new Dictionary<int, List<int>>();
    }

    public void AppendMeshData(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, int submeshIndex) {
        FixTriangles(triangles);
        Vertices.AddRange(vertices);
        UVs.AddRange(uvs);
        if (!Triangles.ContainsKey(submeshIndex)) {
            Triangles[submeshIndex] = new List<int>();
        }

        Triangles[submeshIndex].AddRange(triangles);
    }

    public void AppendMeshData((List<Vector3> vertices, List<int> triangles) meshData, int submeshIndex) {
        AppendMeshData(meshData.vertices, new List<Vector2>(), meshData.triangles, submeshIndex);
    }

    public void AppendMeshData((List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) meshData, int submeshIndex) {
        AppendMeshData(meshData.vertices, meshData.uvs, meshData.triangles, submeshIndex);
    }

    public void MergeMeshData(MeshData meshData) {
        foreach (var key in meshData.Triangles.Keys) {
            FixTriangles(meshData.Triangles[key]);
            if (!Triangles.ContainsKey(key)) 
                Triangles[key] = new List<int>();
            Triangles[key].AddRange(meshData.Triangles[key]);
        }

        Vertices.AddRange(meshData.Vertices);
        UVs.AddRange(meshData.UVs);
    }

    private void FixTriangles(List<int> triangles) {
        var offset = Vertices.Count;
        for (var i = 0; i < triangles.Count; i++) {
            triangles[i] += offset;
        }
    }
}