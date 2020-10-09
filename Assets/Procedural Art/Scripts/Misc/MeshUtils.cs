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
}