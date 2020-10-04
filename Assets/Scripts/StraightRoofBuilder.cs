using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class StraightRoofBuilder : MonoBehaviour {
    public float Width;
    public float Length;
    public float Height;
    public float Thickness;
    public float Extrusion;
    public Vector3 VertexOffset;
    public Vector3 CapOffset;
    public Quaternion Rotation;

    public bool Flip;
    public bool AddCap;
    public bool CloseRoof;
    public bool ExtrusionRight;
    public bool ExtrusionLeft;

    private MeshFilter meshFilter;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Generate();
        }
    }

    private void OnDrawGizmos() {
        var cap10 = new Vector3(ExtrusionLeft ? (Flip ? Extrusion : -Extrusion) : 0, Thickness, 0);
        var cap11 = new Vector3(Flip ? -Width - (ExtrusionRight ? Extrusion : 0) : Width + (ExtrusionRight ? Extrusion : 0), Thickness, 0);
        var cap12 = new Vector3(Flip ? -Width - (ExtrusionRight ? Extrusion : 0) : Width + (ExtrusionRight ? Extrusion : 0), 0, 0);
        var cap13 = new Vector3(ExtrusionLeft ? (Flip ? Extrusion : -Extrusion) : 0, 0, 0);
        var cap20 = new Vector3(ExtrusionLeft ? (Flip ? Extrusion : -Extrusion) : 0, Height - Thickness, Length);
        var cap21 = new Vector3(Flip ? -Width - (ExtrusionRight ? Extrusion : 0) : Width + (ExtrusionRight ? Extrusion : 0), Height - Thickness, Length);
        var cap22 = new Vector3(Flip ? -Width - (ExtrusionRight ? Extrusion : 0) : Width + (ExtrusionRight ? Extrusion : 0), Height, Length);
        var cap23 = new Vector3(ExtrusionLeft ? (Flip ? Extrusion : -Extrusion) : 0, Height, Length);
        var cap30 = new Vector3(ExtrusionLeft ? (Flip ? Extrusion : -Extrusion) : 0 + CapOffset.z, CapOffset.y, -Thickness + CapOffset.x);
        var cap31 = new Vector3((Flip ? -Width - (ExtrusionRight ? Extrusion : 0) : Width + (ExtrusionRight ? Extrusion : 0)) + CapOffset.z, CapOffset.y, -Thickness + CapOffset.x);
        
        var cap40 = new Vector3(Flip ? -Width : Width, 0, 0);
        var cap41 = new Vector3(Flip ? -Width : Width, Height - Thickness, Length);
        var cap42 = new Vector3(Flip ? -Width : Width, 0, Length);
        
        var cap50 = new Vector3(0, 0, 0);
        var cap51 = new Vector3(0, Height - Thickness, Length);
        var cap52 = new Vector3(0, 0, Length);

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

        Gizmos.color = Color.cyan;
        if (CloseRoof) {
            Gizmos.DrawWireSphere(cap40, 0.05f);
            Gizmos.DrawWireSphere(cap41, 0.05f);
            Gizmos.DrawWireSphere(cap42, 0.05f);
            Gizmos.DrawWireSphere(cap50, 0.05f);
            Gizmos.DrawWireSphere(cap51, 0.05f);
            Gizmos.DrawWireSphere(cap52, 0.05f);
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

    private void Generate() {
        var (vertices, triangles) = MeshGenerator.GetMesh<StraightRoofGenerator>(VertexOffset, Rotation, new Dictionary<string, dynamic> {
            {"width", Width},
            {"height", Height},
            {"length", Length},
            {"thickness", Thickness},
            {"extrusion", Extrusion},
            {"extrusionLeft", ExtrusionLeft},
            {"extrusionRight", ExtrusionRight},
            {"addCap", AddCap},
            {"capOffset", CapOffset},
            {"flip", Flip},
            {"closeRoof", CloseRoof}
        });
        var mesh = new Mesh {vertices = vertices.ToArray(), triangles = triangles.ToArray()};
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}