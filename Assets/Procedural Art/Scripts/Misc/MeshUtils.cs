using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils {
    public static MeshData Combine(params MeshData[] meshes) {
        var meshData = new MeshData();
        foreach (var mesh in meshes) {
            meshData.MergeMeshData(mesh);
        }

        return meshData;
    }
    
    public static MeshData Combine(List<MeshData> meshes) {
        var meshData = new MeshData();
        foreach (var mesh in meshes) {
            meshData.MergeMeshData(mesh);
        }

        return meshData;
    }
}