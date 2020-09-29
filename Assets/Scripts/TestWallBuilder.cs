using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TestWallBuilder : MonoBehaviour {
    public float Width;
    public float Height;
    public float Thickness;
    public Vector3 VertexOffset;
    public Quaternion Rotation;

    public bool ThicknessInwards;
    public bool ThicknessOutwards;

    private MeshFilter meshFilter;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Generate();
        }
    }

    private void Generate() {
        var (vertices, triangles) = WallGenerator.Generate(Width, Height, Thickness, VertexOffset, Rotation, ThicknessInwards, ThicknessOutwards);
        var mesh = new Mesh {vertices = vertices.ToArray(), triangles = triangles.ToArray()};
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}