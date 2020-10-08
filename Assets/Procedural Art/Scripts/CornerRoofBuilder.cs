using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CornerRoofBuilder : MonoBehaviour {
    public float Width;
    public float Length;
    public float Height;
    public float Thickness;
    public float Extrusion;
    public Vector3 VertexOffset;
    public Vector3 CapOffset;
    public Quaternion Rotation;

    public bool FlipX;
    public bool FlipZ;
    public bool AddCap;
    public bool JoinCaps;

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
        var cap10 = new Vector3(0, Thickness, 0);
        var cap13 = new Vector3(0, 0, 0);
        var cap11 = new Vector3(FlipX ? -Width : Width, Thickness, 0);
        var cap12 = new Vector3(FlipX ? -Width : Width, 0, 0);
        var cap20 = new Vector3(0, Thickness, FlipZ ? -Length : Length);
        var cap23 = new Vector3(0, 0, FlipZ ? -Length : Length);
        var cap21 = new Vector3(0, Thickness, 0);
        var cap22 = new Vector3(0, 0, 0);
        var cap30 = new Vector3(CapOffset.z + (JoinCaps ? -Thickness : 0), CapOffset.y, FlipZ ? Thickness : -Thickness + CapOffset.x);
        var cap31 = new Vector3(FlipX ? -Width : Width + CapOffset.z, CapOffset.y, FlipZ ? Thickness : -Thickness + CapOffset.x);
        var cap40 = new Vector3(FlipX ? Thickness : -Thickness + CapOffset.x, CapOffset.y, FlipZ ? -Length : Length + CapOffset.z);
        var cap41 = new Vector3(FlipX ? Thickness : -Thickness + CapOffset.x, CapOffset.y, CapOffset.z + (JoinCaps ? -Thickness : 0));
        var cap50 = new Vector3(FlipX ? -Width : Width, Height, FlipZ ? -Length : Length);
        var cap51 = new Vector3(FlipX ? -Width : Width, Height - Thickness, FlipZ ? -Length : Length);
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(cap10, 0.03f);
        Gizmos.DrawWireSphere(cap11, 0.03f);
        Gizmos.DrawWireSphere(cap12, 0.03f);
        Gizmos.DrawWireSphere(cap13, 0.03f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(cap20, 0.03f);
        Gizmos.DrawWireSphere(cap21, 0.03f);
        Gizmos.DrawWireSphere(cap22, 0.03f);
        Gizmos.DrawWireSphere(cap23, 0.03f);
        if (AddCap) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cap30, 0.03f);
            Gizmos.DrawWireSphere(cap31, 0.03f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(cap40, 0.03f);
            Gizmos.DrawWireSphere(cap41, 0.03f);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(cap50, 0.03f);
        Gizmos.DrawWireSphere(cap51, 0.03f);

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
            Handles.Label(cap40, "4.0", guiStyle);
            Handles.Label(cap41, "4.1", guiStyle);
        }

        Handles.Label(cap50, "5.0", guiStyle);
        Handles.Label(cap51, "5.1", guiStyle);
    }

    private void Generate() {
        var meshData = MeshGenerator.GetMesh<CornerRoofGenerator>(VertexOffset, Rotation, new Dictionary<string, dynamic> {
            {"width", Width},
            {"height", Height},
            {"length", Length},
            {"thickness", Thickness},
            {"extrusion", Extrusion},
            {"addCap", AddCap},
            {"capOffset", CapOffset},
            {"flipX", FlipX},
            {"flipZ", FlipZ},
            {"joinCaps", JoinCaps},
        });
        var mesh = new Mesh {name = "Corner Roof"};
        mesh.SetVertices(meshData.Vertices);
        foreach(var key in meshData.Triangles.Keys)
            mesh.SetTriangles(meshData.Triangles[key], key);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}