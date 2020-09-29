using System.Collections.Generic;
using UnityEngine;

public class StraightRoofGenerator {
    private float roofWidth = 1f;
    private float roofLength = 1f;
    private float roofHeight = 1f;
    private float roofThickness = 0.25f;
    private bool thicknessBasedOnRoofAngle = true;
    
    private bool addCap = true;
    private Vector3 capOffset;
    
    private bool flip;
    private Vector3 vertexOffset;
    private Quaternion rotation;

    private List<Vector3> vertices;
    private List<int> triangles;

    public static (List<Vector3> vertices, List<int> triangles) Generate(float roofWidth, float roofHeight, float roofLength, float roofThickness, Vector3 vertexOffset, Quaternion rotation, Vector3 capOffset, bool thicknessBasedOnRoofAngle = false, bool addCap = false, bool flip = false) {
        var generator = new StraightRoofGenerator(roofWidth, roofHeight, roofLength, roofThickness, vertexOffset, rotation, capOffset, thicknessBasedOnRoofAngle, addCap, flip);
        return (generator.vertices, generator.triangles);
    }
    
    public StraightRoofGenerator(float roofWidth, float roofHeight, float roofLength, float roofThickness, Vector3 vertexOffset, Quaternion rotation, Vector3 capOffset, bool thicknessBasedOnRoofAngle = false, bool addCap = false, bool flip = false) {
        this.roofWidth = roofWidth;
        this.roofLength = roofLength;
        this.roofHeight = roofHeight;
        this.roofThickness = roofThickness;
        this.vertexOffset = vertexOffset;
        this.rotation = rotation;
        this.thicknessBasedOnRoofAngle = thicknessBasedOnRoofAngle;
        this.addCap = addCap;
        this.capOffset = capOffset;
        this.flip = flip;
        
        Generate();
    }

    /*
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
    */


    private void Generate() {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        var cap10 = new Vector3(0, roofThickness, 0);
        var cap11 = new Vector3(flip ? -roofWidth : roofWidth, roofThickness, 0);
        var cap12 = new Vector3(flip ? -roofWidth : roofWidth, 0, 0);
        var cap13 = new Vector3(0, 0, 0);
        var cap20 = new Vector3(0, roofHeight - roofThickness, roofLength);
        var cap21 = new Vector3(flip ? -roofWidth : roofWidth, roofHeight - roofThickness, roofLength);
        var cap22 = new Vector3(flip ? -roofWidth : roofWidth, roofHeight, roofLength);
        var cap23 = new Vector3(0, roofHeight, roofLength);
        var cap30 = new Vector3(0 + capOffset.z, capOffset.y, -roofThickness + capOffset.x);
        var cap31 = new Vector3(flip ? -roofWidth : roofWidth + capOffset.z, capOffset.y, -roofThickness + capOffset.x);

        if (thicknessBasedOnRoofAngle) {
            var v1 = cap11 - cap12;
            var v2 = cap21 - cap12;
            var angle = Mathf.Deg2Rad * Vector3.Angle(v1, v2);
            var multiplier = 1f / Mathf.Sin(angle);
            var actualRoofThickness = roofThickness * multiplier;
            cap10.y = actualRoofThickness;
            cap11.y = actualRoofThickness;
            cap20.y = roofHeight - actualRoofThickness;
            cap21.y = roofHeight - actualRoofThickness;
            cap30.z = -actualRoofThickness + capOffset.x;
            cap31.z = -actualRoofThickness + capOffset.x;
        }

        AddQuad(cap10, cap11, cap12, cap13, flip);
        AddQuad(cap20, cap21, cap22, cap23, flip);
        AddQuad(cap13, cap12, cap21, cap20, flip);
        AddQuad(cap12, cap11, cap22, cap21, flip);
        AddQuad(cap10, cap13, cap20, cap23, flip);
        AddQuad(cap11, cap10, cap23, cap22, flip);

        if (addCap) {
            AddTriangle(cap12, cap31, cap11, flip);
            AddTriangle(cap13, cap10, cap30, flip);
            AddQuad(cap12, cap13, cap30, cap31, flip);
            AddQuad(cap10, cap11, cap31, cap30, flip);
        }

        // Rotation and Translation
        for (var i = 0; i < vertices.Count; i++) {
            vertices[i] = rotation * vertices[i];
            vertices[i] += vertexOffset;
        }
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