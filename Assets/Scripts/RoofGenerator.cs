using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoofGenerator : MonoBehaviour {
    [Header("Base")]
    public float RoofWidth = 1f;
    public float RoofLength = 1f;
    public float RoofHeight = 1f;
    public float RoofThickness = 0.25f;
    public bool ThicknessBasedOnRoofAngle = true;
    [Header("Cap")]
    public bool AddCap = true;
    public Vector3 CapOffset;
    [Space]
    public bool Flip;
    public Vector3 MeshOffset;

    private MeshFilter meshFilter;
    private List<Vector3> vertices;
    private List<int> triangles;
    private Mesh mesh;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    public void InitializeAndBuild(float roofWidth, float roofHeight, float roofLength, float roofThickness, Vector3 capOffset, bool thicknessBasedOnRoofAngle = true, bool addCap = false, bool flip = false) {
        RoofWidth = roofWidth;
        RoofLength = roofLength;
        RoofHeight = roofHeight;
        RoofThickness = roofThickness;
        ThicknessBasedOnRoofAngle = thicknessBasedOnRoofAngle;
        AddCap = addCap;
        CapOffset = capOffset;
        Flip = flip;
        
        Generate();
    }

    private void OnDrawGizmos() {
        var cap10 = new Vector3(0, RoofThickness, 0);
        var cap11 = new Vector3(Flip ? -RoofWidth : RoofWidth, RoofThickness, 0);
        var cap12 = new Vector3(Flip ? -RoofWidth : RoofWidth, 0, 0);
        var cap13 = new Vector3(0, 0, 0);
        var cap20 = new Vector3(0, RoofHeight - RoofThickness, RoofLength);
        var cap21 = new Vector3(Flip ? -RoofWidth : RoofWidth, RoofHeight - RoofThickness, RoofLength);
        var cap22 = new Vector3(Flip ? -RoofWidth : RoofWidth, RoofHeight, RoofLength);
        var cap23 = new Vector3(0, RoofHeight, RoofLength);
        var cap30 = new Vector3(0 + CapOffset.z, CapOffset.y, -RoofThickness + CapOffset.x);
        var cap31 = new Vector3(Flip ? -RoofWidth : RoofWidth + CapOffset.z, CapOffset.y, -RoofThickness + CapOffset.x);

        if (ThicknessBasedOnRoofAngle) {
            var v1 = cap11 - cap12;
            var v2 = cap21 - cap12;
            var angle = Mathf.Deg2Rad * Vector3.Angle(v1, v2);
            var multiplier = 1f / Mathf.Sin(angle);
            var actualRoofThickness = RoofThickness * multiplier;
            cap10.y = actualRoofThickness;
            cap11.y = actualRoofThickness;
            cap20.y = RoofHeight - actualRoofThickness;
            cap21.y = RoofHeight - actualRoofThickness;
            cap30.z = -actualRoofThickness + CapOffset.x;
            cap31.z = -actualRoofThickness + CapOffset.x;
        }

        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(cap10, 0.05f);
        Gizmos.DrawWireSphere(cap11, 0.05f);
        Gizmos.DrawWireSphere(cap12, 0.05f);
        Gizmos.DrawWireSphere(cap13, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(cap20, 0.05f);
        Gizmos.DrawWireSphere(cap21, 0.05f);
        Gizmos.DrawWireSphere(cap22, 0.05f);
        Gizmos.DrawWireSphere(cap23, 0.05f);
        Gizmos.color = Color.green;
        if (AddCap) {
            Gizmos.DrawWireSphere(cap30, 0.05f);
            Gizmos.DrawWireSphere(cap31, 0.05f);
        }

        var guiStyle = new GUIStyle {fontSize = 16, fontStyle = FontStyle.Bold, normal = {textColor = Color.white}};
        Handles.matrix = transform.localToWorldMatrix;
        Handles.Label(cap10, "1.0", guiStyle);
        Handles.Label(cap11, "1.1", guiStyle);
        Handles.Label(cap12, "1.2", guiStyle);
        Handles.Label(cap13, "1.3", guiStyle);

        Handles.Label(cap20, "2.0", guiStyle);
        Handles.Label(cap21, "2.1", guiStyle);
        Handles.Label(cap22, "2.2", guiStyle);
        Handles.Label(cap23, "2.3", guiStyle);

        if (AddCap) {
            Handles.Label(cap30, "3.0", guiStyle);
            Handles.Label(cap31, "3.1", guiStyle);
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
            Generate();
    }

    private void Generate() {
        mesh.Clear();
        vertices = new List<Vector3>();
        triangles = new List<int>();

        var cap10 = new Vector3(0, RoofThickness, 0);
        var cap11 = new Vector3(Flip ? -RoofWidth : RoofWidth, RoofThickness, 0);
        var cap12 = new Vector3(Flip ? -RoofWidth : RoofWidth, 0, 0);
        var cap13 = new Vector3(0, 0, 0);
        var cap20 = new Vector3(0, RoofHeight - RoofThickness, RoofLength);
        var cap21 = new Vector3(Flip ? -RoofWidth : RoofWidth, RoofHeight - RoofThickness, RoofLength);
        var cap22 = new Vector3(Flip ? -RoofWidth : RoofWidth, RoofHeight, RoofLength);
        var cap23 = new Vector3(0, RoofHeight, RoofLength);
        var cap30 = new Vector3(0 + CapOffset.z, CapOffset.y, -RoofThickness + CapOffset.x);
        var cap31 = new Vector3(Flip ? -RoofWidth : RoofWidth + CapOffset.z, CapOffset.y, -RoofThickness + CapOffset.x);

        if (ThicknessBasedOnRoofAngle) {
            var v1 = cap11 - cap12;
            var v2 = cap21 - cap12;
            var angle = Mathf.Deg2Rad * Vector3.Angle(v1, v2);
            var multiplier = 1f / Mathf.Sin(angle);
            var actualRoofThickness = RoofThickness * multiplier;
            cap10.y = actualRoofThickness;
            cap11.y = actualRoofThickness;
            cap20.y = RoofHeight - actualRoofThickness;
            cap21.y = RoofHeight - actualRoofThickness;
            cap30.z = -actualRoofThickness + CapOffset.x;
            cap31.z = -actualRoofThickness + CapOffset.x;
        }

        AddQuad(cap10, cap11, cap12, cap13, Flip);
        AddQuad(cap20, cap21, cap22, cap23, Flip);
        AddQuad(cap13, cap12, cap21, cap20, Flip);
        AddQuad(cap12, cap11, cap22, cap21, Flip);
        AddQuad(cap10, cap13, cap20, cap23, Flip);
        AddQuad(cap11, cap10, cap23, cap22, Flip);

        if (AddCap) {
            AddTriangle(cap12, cap31, cap11, Flip);
            AddTriangle(cap13, cap10, cap30, Flip);
            AddQuad(cap12, cap13, cap30, cap31, Flip);
            AddQuad(cap10, cap11, cap31, cap30, Flip);
        }

        for (var i = 0; i < vertices.Count; i++) {
            vertices[i] += MeshOffset;
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, bool flip) {
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

    private void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, bool flip) {
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