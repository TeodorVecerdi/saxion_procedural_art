using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)),ExecuteInEditMode]
public class SplineMeshGenerator : MonoBehaviour {
    [Range(2, 200)] public int Resolution = 8;
    public float Thickness = 0.25f;
    public float Extrusion = 0.25f;
    public SplineComponent SplineComponent;

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;
    
    private void Awake() {
        var meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;
    }

    public void Generate() {
        if(SplineComponent == null) return;
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int i = 0; i < SplineComponent.points.Count - 3; i++) {
            var controlA = SplineComponent.points[i];
            var pointA = SplineComponent.points[i+1];
            var pointB = SplineComponent.points[i+2];
            var controlB = SplineComponent.points[i+3];
            
            
            var p = 0f;
            var start = Interpolate(controlA, pointA, pointB, controlB, p);
            var step = 1f / Resolution;
            do {
                p += step;
                var here = Interpolate(controlA, pointA, pointB, controlB, p);
                var next = Interpolate(controlA, pointA, pointB, controlB, p+step);
                AddSplinePos(start, here, next, p+step > 1);
                start = here;
            } while (p + step <= 1);
        }
        
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
    }

    private void AddSplinePos(Vector3 from, Vector3 to, Vector3 next, bool discardLast = false) {
        var diff = to - from;
        var diff2 = next - to;
        var perp1 = Vector3.Cross(diff,diff + Vector3.down).normalized;
        var perp2 = Vector3.Cross(diff + Vector3.down, diff).normalized;
        var perp12 = Vector3.Cross(diff2,diff2 + Vector3.down).normalized;
        var perp22 = Vector3.Cross(diff2 + Vector3.down, diff2).normalized;

        var fromThickA = from + Vector3.down * (Thickness / 2f) + perp1 * (Extrusion / 2f);
        var fromThickB = from + Vector3.up * (Thickness / 2f) + perp1 * (Extrusion / 2f);
        var toThickA = to + Vector3.down * (Thickness / 2f) + perp1 * (Extrusion / 2f);
        var toThickB = to + Vector3.up * (Thickness / 2f) + perp1 * (Extrusion / 2f);
        
        var fromThickA2 = from + Vector3.down * (Thickness / 2f) + perp2 * (Extrusion / 2f);
        var fromThickB2 = from + Vector3.up * (Thickness / 2f) + perp2 * (Extrusion / 2f);
        var toThickA2 = to + Vector3.down * (Thickness / 2f) + perp2 * (Extrusion / 2f);
        var toThickB2 = to + Vector3.up * (Thickness / 2f) + perp2 * (Extrusion / 2f);

        var nextThickA = to + Vector3.down * (Thickness / 2f) + perp12 * (Extrusion / 2f);
        var nextThickB = to + Vector3.up * (Thickness / 2f) + perp12 * (Extrusion / 2f);
        var nextThickA2 = to + Vector3.down * (Thickness / 2f) + perp22 * (Extrusion / 2f);
        var nextThickB2 = to + Vector3.up * (Thickness / 2f) + perp22 * (Extrusion / 2f);

        AddQuad(fromThickA, toThickA, toThickB, fromThickB, true);
        AddQuad(fromThickA2, toThickA2, toThickB2, fromThickB2);
        AddQuad(fromThickA, toThickA, toThickA2, fromThickA2);
        AddQuad(fromThickB, toThickB, toThickB2, fromThickB2, true);
        if (!discardLast) {
            AddQuad(toThickA2, nextThickA2, nextThickB2, toThickB2);
            
        }
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
    
    private float X(float t, Vector3 controlA, Vector3 pointA, Vector3 pointB, Vector3 controlB) {
        var tSq = t * t;
        var tCu = tSq * t;
        var cX = 3 * (controlA.x - pointA.x);
        var bX = 3 * (controlB.x - controlA.x) - cX;
        var aX = pointB.x - pointA.x - cX - bX;
        return aX * tCu + bX * tSq + cX * t + pointA.x;
    }
    
    private float Y(float t, Vector3 controlA, Vector3 pointA, Vector3 pointB, Vector3 controlB) {
        var tSq = t * t;
        var tCu = tSq * t;
        var cX = 3 * (controlA.z - pointA.z);
        var bX = 3 * (controlB.z - controlA.z) - cX;
        var aX = pointB.z - pointA.z - cX - bX;
        return aX * tCu + bX * tSq + cX * t + pointA.z;
    }
    
    private Vector3 Interpolate(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u) {
        return (
            0.5f *
            (
                (-a + 3f * b - 3f * c + d) *
                (u * u * u) +
                (2f * a - 5f * b + 4f * c - d) *
                (u * u) +
                (-a + c) *
                u + 2f * b
            )
        );
    }
}

[CustomEditor(typeof(SplineMeshGenerator))]
public class SplineMeshGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate")) {
            ((SplineMeshGenerator)target).Generate();
        }
    }
}