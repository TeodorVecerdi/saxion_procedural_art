using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CornerRoofGenerator : MonoBehaviour {
    [Header("Base")]
    public float RoofWidth = 1f;
    public float RoofLength = 1f;
    public float RoofHeight = 1f;
    public float RoofThickness = 0.25f;
    public bool ThicknessBasedOnRoofAngle = true;
    [Header("Cap")]
    public bool AddCap = true;
    public Vector3 CapOffset;
    public float CornerCapOffset = 0f;
    [Space]
    public bool FlipX;
    public bool FlipZ;

    private MeshFilter meshFilter;
    private List<Vector3> vertices;
    private List<int> triangles;
    private Mesh mesh;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }
    
    public void InitializeAndBuild(float roofWidth, float roofHeight, float roofLength, float roofThickness, Vector3 capOffset, bool thicknessBasedOnRoofAngle = true, bool addCap = false, bool flipX = false, bool flipZ = false) {
        RoofWidth = roofWidth;
        RoofLength = roofLength;
        RoofHeight = roofHeight;
        RoofThickness = roofThickness;
        ThicknessBasedOnRoofAngle = thicknessBasedOnRoofAngle;
        AddCap = addCap;
        CapOffset = capOffset;
        FlipX = flipX;
        FlipZ = flipZ;
        
        Generate();
    }

    private void OnDrawGizmos() {
        var cap10 = new Vector3(0, RoofThickness, 0);
        var cap13 = new Vector3(0, 0, 0);
        var cap11 = new Vector3(FlipX ? -RoofWidth : RoofWidth, RoofThickness, 0);
        var cap12 = new Vector3(FlipX ? -RoofWidth : RoofWidth, 0, 0);

        var cap20 = new Vector3(0, RoofThickness, FlipZ ? -RoofLength : RoofLength);
        var cap23 = new Vector3(0, 0, FlipZ ? -RoofLength : RoofLength);
        var cap21 = new Vector3(0, RoofThickness, 0);
        var cap22 = new Vector3(0, 0, 0);

        var cap30 = new Vector3(0 + CapOffset.z, CapOffset.y, FlipZ ? RoofThickness : -RoofThickness + CapOffset.x);
        var cap31 = new Vector3(FlipX ? -RoofWidth : RoofWidth + CapOffset.z, CapOffset.y, FlipZ ? RoofThickness : -RoofThickness + CapOffset.x);

        var cap40 = new Vector3(FlipX ? RoofThickness : -RoofThickness + CapOffset.x, CapOffset.y, FlipZ ? -RoofLength : RoofLength + CapOffset.z);
        var cap41 = new Vector3(FlipX ? RoofThickness : -RoofThickness + CapOffset.x, CapOffset.y, 0 + CapOffset.z);

        var cap50 = new Vector3(FlipX ? -RoofWidth : RoofWidth, RoofHeight, FlipZ ? -RoofLength : RoofLength);
        var cap51 = new Vector3(FlipX ? -RoofWidth : RoofWidth, RoofHeight - RoofThickness, FlipZ ? -RoofLength : RoofLength);

        if (ThicknessBasedOnRoofAngle) {
            var v1 = cap11 - cap12;
            var v2 = cap51 - cap12;
            var angle = Mathf.Deg2Rad * Vector3.Angle(v1, v2);
            var multiplier = 1f / Mathf.Sin(angle);
            var actualRoofThickness = RoofThickness * multiplier;
            cap10.y = actualRoofThickness;
            cap11.y = actualRoofThickness;
            cap20.y = actualRoofThickness;
            cap21.y = actualRoofThickness;
            cap30.z = FlipZ ? actualRoofThickness : -actualRoofThickness + CapOffset.x;
            cap31.z = FlipZ ? actualRoofThickness : -actualRoofThickness + CapOffset.x;
            cap40.x = FlipX ? actualRoofThickness : -actualRoofThickness + CapOffset.x;
            cap41.x = FlipX ? actualRoofThickness : -actualRoofThickness + CapOffset.x;
            cap51.y = RoofHeight - actualRoofThickness;
        }

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

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
            Generate();
    }

    private void Generate() {
        mesh.Clear();
        vertices = new List<Vector3>();
        triangles = new List<int>();

        var cap10 = new Vector3(0, RoofThickness, 0);
        var cap13 = new Vector3(0, 0, 0);
        var cap11 = new Vector3(FlipX ? -RoofWidth : RoofWidth, RoofThickness, 0);
        var cap12 = new Vector3(FlipX ? -RoofWidth : RoofWidth, 0, 0);

        var cap20 = new Vector3(0, RoofThickness, FlipZ ? -RoofLength : RoofLength);
        var cap23 = new Vector3(0, 0, FlipZ ? -RoofLength : RoofLength);
        var cap21 = new Vector3(0, RoofThickness, 0);
        var cap22 = new Vector3(0, 0, 0);

        var cap30 = new Vector3(0 + CapOffset.z, CapOffset.y, FlipZ ? RoofThickness : -RoofThickness + CapOffset.x);
        var cap31 = new Vector3(FlipX ? -RoofWidth : RoofWidth + CapOffset.z, CapOffset.y, FlipZ ? RoofThickness : -RoofThickness + CapOffset.x);

        var cap40 = new Vector3(FlipX ? RoofThickness : -RoofThickness + CapOffset.x, CapOffset.y, FlipZ ? -RoofLength : RoofLength + CapOffset.z);
        var cap41 = new Vector3(FlipX ? RoofThickness : -RoofThickness + CapOffset.x, CapOffset.y, 0 + CapOffset.z);

        var cap50 = new Vector3(FlipX ? -RoofWidth : RoofWidth, RoofHeight, FlipZ ? -RoofLength : RoofLength);
        var cap51 = new Vector3(FlipX ? -RoofWidth : RoofWidth, RoofHeight - RoofThickness, FlipZ ? -RoofLength : RoofLength);

        if (ThicknessBasedOnRoofAngle) {
            var v1 = cap11 - cap12;
            var v2 = cap51 - cap12;
            var angle = Mathf.Deg2Rad * Vector3.Angle(v1, v2);
            var multiplier = 1f / Mathf.Sin(angle);
            var actualRoofThickness = RoofThickness * multiplier;
            cap10.y = actualRoofThickness;
            cap11.y = actualRoofThickness;
            cap20.y = actualRoofThickness;
            cap21.y = actualRoofThickness;
            cap30.z = FlipZ ? actualRoofThickness : -actualRoofThickness + CapOffset.x;
            cap31.z = FlipZ ? actualRoofThickness : -actualRoofThickness + CapOffset.x;
            cap40.x = FlipX ? actualRoofThickness : -actualRoofThickness + CapOffset.x;
            cap41.x = FlipX ? actualRoofThickness : -actualRoofThickness + CapOffset.x;
            cap51.y = RoofHeight - actualRoofThickness;
        }
        AddQuad(cap10, cap11, cap12, cap13, FlipX ^ FlipZ);
        AddQuad(cap20, cap21, cap22, cap23, FlipX ^ FlipZ);
        AddTriangle(cap10, cap50, cap11, FlipX ^ FlipZ);
        AddTriangle(cap20, cap50, cap21, FlipX ^ FlipZ);
        AddQuad(cap11, cap50, cap51, cap12, FlipX ^ FlipZ);
        AddQuad(cap50, cap20, cap23, cap51, FlipX ^ FlipZ);
        AddTriangle(cap22, cap12, cap51, FlipX ^ FlipZ /*^*/);
        AddTriangle(cap22, cap51, cap23, FlipX ^ FlipZ /*^*/);

        if (AddCap) {
            AddTriangle(cap12, cap31, cap11, FlipX ^ FlipZ);
            AddQuad(cap12, cap13, cap30, cap31, FlipX ^ FlipZ);
            AddQuad(cap10, cap11, cap31, cap30, FlipX ^ FlipZ);
            AddTriangle(cap23, cap20, cap40, FlipX ^ FlipZ);
            AddQuad(cap22, cap23, cap40, cap41, FlipX ^ FlipZ);
            AddQuad(cap20, cap21, cap41, cap40, FlipX ^ FlipZ);
            AddTriangle(cap21, cap30, cap41, FlipX ^ FlipZ);
            AddTriangle(cap41, cap30, cap22, FlipX ^ FlipZ);
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