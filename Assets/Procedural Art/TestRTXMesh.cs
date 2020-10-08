using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TestRTXMesh : MonoBehaviour {
    public float Size = 1f;

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;
    private MeshFilter meshFilter;

    public void FixRef() {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void Generate() {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        var lowerLeftFront = new Vector3(0f, 0f, 0f);
        var lowerRightFront = new Vector3(Size, 0f, 0f);
        var upperLeftFront = new Vector3(0, Size, 0);
        var upperRightFront = new Vector3(Size, Size, 0);

        var lowerLeftBack = new Vector3(0f, 0f, Size);
        var lowerRightBack = new Vector3(Size, 0f, Size);
        var upperLeftBack = new Vector3(0, Size, Size);
        var upperRightBack = new Vector3(Size, Size, Size);
        
        AddQuad(lowerLeftFront,upperLeftFront, upperRightFront, lowerRightFront);
        AddQuad(lowerLeftBack,upperLeftBack, upperRightBack, lowerRightBack);
        
        
        mesh = new Mesh {name = "TestRTXMesh"};
        meshFilter.sharedMesh = mesh;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    private void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, bool flip = false) {
        var quadIndex = vertices.Count;
        if (flip) {
            vertices.Add(v3);
            vertices.Add(v2);
            vertices.Add(v1);
            vertices.Add(v0);
        } else {
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
        }

        triangles.Add(quadIndex);
        triangles.Add(quadIndex + 1);
        triangles.Add(quadIndex + 2);
        triangles.Add(quadIndex);
        triangles.Add(quadIndex + 2);
        triangles.Add(quadIndex + 3);
    }

    private void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, bool flip = false) {
        var triangleIndex = vertices.Count;
        if (flip) {
            vertices.Add(v2);
            vertices.Add(v1);
            vertices.Add(v0);
        } else {
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
        }

        triangles.Add(triangleIndex);
        triangles.Add(triangleIndex + 1);
        triangles.Add(triangleIndex + 2);
    }
}