using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LineBuilder : MonoBehaviour {
    public Vector3 Start = Vector3.zero;
    public Vector3 End = Vector3.up;
    public float Thickness = 0.1f;
    public float Extrusion = 0.1f;
    public bool ExtrusionOutwards = false;
    public bool RotateUV = false;

    private MeshFilter meshFilter;

    private void OnEnable() {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void Generate() {
        var meshData = MeshGenerator.GetMesh<LineGenerator>(Vector3.zero, Quaternion.identity, new Dictionary<string, dynamic> {
            {"start", Start},
            {"end", End},
            {"thickness", Thickness},
            {"extrusion", Extrusion},
            {"extrusionOutwards", ExtrusionOutwards},
            {"rotateUV", RotateUV}
        });
        var mesh = new Mesh{name = gameObject.name + " LineGenerator"};
        mesh.SetVertices(meshData.Vertices);
        mesh.SetUVs(0, meshData.UVs);
        mesh.subMeshCount = meshData.Triangles.Keys.Count;
        foreach (var key in meshData.Triangles.Keys) {
            mesh.SetTriangles(meshData.Triangles[key], key);
        }
        mesh.RecalculateNormals();
        if(meshFilter == null) OnEnable();
        if(meshFilter.sharedMesh != null)
            DestroyImmediate(meshFilter.sharedMesh);
        meshFilter.sharedMesh = mesh;
    }
}

[CustomEditor(typeof(LineBuilder))]
public class LineBuilderEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate")) {
            (target as LineBuilder).Generate();
        }
    }
}