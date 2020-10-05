using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TestArchBuilder : MonoBehaviour {
    public float Width = 1;
    public float Height = 1;
    public float Length = 1;
    [Range(3, 720)] public int Points = 2;
    private List<Vector3> vertices;
    private List<int> triangles;

    private Mesh mesh;
    private MeshFilter meshFilter;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;
    }

    private void OnDrawGizmos() {
        return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.blue;

        var step = 180f / (Points - 1) * Mathf.Deg2Rad;
        for (var val = 0f; val <= Mathf.PI + (step / 4f); val += step) {
            var x = ((Mathf.Cos(val) + 1) / 2f) * Width;
            var y = Mathf.Sin(val) * Height;
            Gizmos.DrawSphere(new Vector3(x, y, 0f), 0.1f);
        }

        Gizmos.color = Color.green;
        for (var val = 0f; val <= Mathf.PI + (step / 4f); val += step) {
            var x = ((Mathf.Cos(val) + 1) / 2f) * Width;
            var y = Mathf.Sin(val) * Height;
            Gizmos.DrawSphere(new Vector3(x, y, Length), 0.1f);
        }
    }

    public void Generate() {
        if (meshFilter == null || meshFilter.sharedMesh != mesh) {
            Awake();
        }
        mesh.Clear();
        vertices = new List<Vector3>();
        triangles = new List<int>();

        var b0 = new Vector3(0, 0, 0);
        var b1 = new Vector3(Width, 0, 0);
        var b2 = new Vector3(Width, 0, Length);
        var b3 = new Vector3(0, 0, Length);
        var c0 = new Vector3(0, Height, 0);
        var c1 = new Vector3(Width, Height, 0);
        var c2 = new Vector3(Width, Height, Length);
        var c3 = new Vector3(0, Height, Length);

        var step = 180f / (Points - 1) * Mathf.Deg2Rad;
        for (var val = 0f; val <= Mathf.PI/2f - (step / 4f); val += step) {
            var x = ((Mathf.Cos(val) + 1) / 2f) * Width - Width/2f;
            var y = 1 - Mathf.Sin(val) * Height;
            var x2 = ((Mathf.Cos(val + step) + 1) / 2f) * Width-Width/2f;
            var y2 = 1 - Mathf.Sin(val + step) * Height;

            var p0 = new Vector3(x, y, 0);
            var p1 = new Vector3(x2, y2, 0);
            var p2 = new Vector3(x2, y2, Length);
            var p3 = new Vector3(x, y, Length);
            AddQuad(p3, p2, p1, p0, true);
        }
        for (var val = Mathf.PI/2f; val <= Mathf.PI - (step / 4f); val += step) {
            var x = ((Mathf.Cos(val) + 1) / 2f) * Width + Width/2f;
            var y = 1 - Mathf.Sin(val) * Height;
            var x2 = ((Mathf.Cos(val + step) + 1) / 2f) * Width+Width/2f;
            var y2 = 1 - Mathf.Sin(val + step) * Height;

            var p0 = new Vector3(x, y, 0);
            var p1 = new Vector3(x2, y2, 0);
            var p2 = new Vector3(x2, y2, Length);
            var p3 = new Vector3(x, y, Length);
            AddQuad(p3, p2, p1, p0, true);
            
            /*
            var cross = Vector3.Cross(p1 - p0, p3 - p0).normalized;
            var p02 = new Vector3(x, y, 0) - cross * Length;
            var p12 = new Vector3(x2, y2, 0) - cross * Length;
            var p22 = new Vector3(x2, y2, Length) - cross * Length;
            var p32 = new Vector3(x, y, Length) - cross * Length;
            AddQuad(p32, p22, p12, p02);*/
        }

        
        for (var val = 0f; val <= Mathf.PI/2f - (step / 4f); val += step) {
            var x = ((Mathf.Cos(val) + 1) / 2f) * Width - Width/2f;
            var y = Mathf.Sin(Mathf.PI - val) * (Height);
            var x2 = ((Mathf.Cos(val + step) + 1) / 2f) * Width-Width/2f;
            var y2 = Mathf.Sin(Mathf.PI - val + step) * (Height);

            var p0 = new Vector3(x, y - Length, 0);
            var p1 = new Vector3(x2, y2 - Length, 0);
            var p2 = new Vector3(x2, y2 - Length, Length);
            var p3 = new Vector3(x, y - Length, Length);
            AddQuad(p3, p2, p1, p0, true);
        }

        // AddQuad(c0, c3, c2, c1);
        // AddQuad(b0, b3, c3, c0);
        // AddQuad(b1, c1, c2, b2);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        
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

[CustomEditor(typeof(TestArchBuilder))]
public class TestArchBuilderEditor : Editor {
    private SerializedProperty points;
    private SerializedProperty width;
    private SerializedProperty height;
    private SerializedProperty length;

    private void OnEnable() {
        points = serializedObject.FindProperty("Points");
        width = serializedObject.FindProperty("Width");
        height = serializedObject.FindProperty("Height");
        length = serializedObject.FindProperty("Length");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        if (GUILayout.Button("Build Mesh")) {
            (target as TestArchBuilder)?.Generate();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(width);
        EditorGUILayout.PropertyField(height);
        EditorGUILayout.PropertyField(length);
        EditorGUILayout.PropertyField(points);
        if (EditorGUI.EndChangeCheck()) {
            serializedObject.ApplyModifiedProperties();
            (target as TestArchBuilder)?.Generate();
        }
    }
}