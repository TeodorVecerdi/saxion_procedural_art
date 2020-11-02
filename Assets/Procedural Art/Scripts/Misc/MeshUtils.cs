using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils {
    public static MeshData Combine(params MeshData[] lists) {
        var combined = new MeshData();
        foreach (var meshData in lists) {
            combined.MergeMeshData(meshData);
        }
        return combined;
    }
    
    public static MeshData Combine(List<MeshData> lists) {
        var combined = new MeshData();
        foreach (var meshData in lists) {
            combined.MergeMeshData(meshData);
        }
        return combined;
    }

    public static MeshData Translate(MeshData data, Vector3 translation) {
        var clone = Combine(data);
        for (int i = 0; i < data.Vertices.Count; i++) {
            clone.Vertices[i] += translation;
        }

        return clone;
    }
}